using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Diagnostics;
using eReading.DownloadInfo;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace eReading
{
    class Download
    {
        #region 常量

        private Color waterMark = Color.FromArgb(238, 238, 238);
        private static Hashtable prio = new Hashtable() { { '0', 0 }, { '!', 1 }, { 'f', 2 }, { 'l', 3 }, { 'b', 4 }, { 'c', 5 } };
        private const string arg = "?.&uf=ssr&zoom=0";
        private const string BodyPageFormat = "000000";
        private string[] FrontPageFormat = new string[] { "cov000", "bok000", "leg000", "fow000", "!00000" };
        private const int FrontPagesNum = 5;
        private const string ImageType = ".jpg";
        private const string PDFType = ".pdf";

        #endregion

        #region 委托

        public delegate void ProgressEventHandler(Download sender);
        public delegate void FinishedEventHandler(Download sender);
        public delegate void ExceptionEventHandler(Download sender, Exception e);

        #endregion

        #region 私有成员

        private ProgressEventHandler _progress;
        private FinishedEventHandler _finished;
        private ExceptionEventHandler _exception;
        private BookInfo _book;
        private string _imagepath;
        private object _lockthis;
        private Thread[] _threads;
        private int _completethread;
        private BodyPagesInfo[] _bodypagesinfo;
        private FrontPagesInfo _frontpagesinfo;

        #endregion

        #region 属性

        public int ThreadCount { get; set; }
        public bool isComplete { get; private set; }
        public bool isStop { get; private set; }
        public bool isError { get; private set; }
        public string DownloadPath { get; set; }
        public int DownloadedPage { get; private set; }
        public double FinishRate { get; private set; }
        public int MaxTryingTimesWhenFailed { get; set; }
        public string PDFFilePath { get; private set; }
        public int DownloadedPages { get; set; }
        public string ImagePath
        {
            get
            {
                return _imagepath;
            }
            set
            {
                _imagepath = value;
            }
        }

        #endregion

        #region 事件

        public event ProgressEventHandler Progress
        {
            add
            {
                this._progress += value;
            }
            remove
            {
                this._progress -= value;
            }
        }
        public event FinishedEventHandler Finish
        {
            add
            {
                this._finished += value;
            }
            remove
            {
                this._finished -= value;
            }
        }
        public event ExceptionEventHandler Exception
        {
            add
            {
                this._exception += value;
            }
            remove
            {
                this._exception -= value;
            }
        }

        #endregion

        public Download(BookInfo book, string path)
        {
            _book = book;
            ThreadCount = 5;
            isComplete = false;
            isStop = false;
            isError = false;
            _lockthis = new object();
            DownloadPath = path;
            _imagepath = Path.Combine(DownloadPath, book.Title);
            Directory.CreateDirectory(_imagepath);
            MaxTryingTimesWhenFailed = 5;
            PDFFilePath = Path.Combine(DownloadPath, _book.Title + PDFType);
            _bodypagesinfo = new BodyPagesInfo[ThreadCount - 1];
            for (int i = 0; i < ThreadCount - 1; i++)
                _bodypagesinfo[i] = new BodyPagesInfo();
            _frontpagesinfo = new FrontPagesInfo();
        }

        public void SetPath(String path)
        {
            DownloadPath = path;
            _imagepath = Path.Combine(DownloadPath, _book.Title);
            MaxTryingTimesWhenFailed = 10;
            PDFFilePath = Path.Combine(DownloadPath, _book.Title + PDFType);
        }

        public void Start()
        {
            if (ThreadCount < 2)
                throw new ArgumentException("The mininum of thread is 2. ");


            int average = _book.PagesNum / (ThreadCount - 1);
            int rest = _book.PagesNum % (ThreadCount - 1);

            for (int i = 1; i < ThreadCount; i++)
            {
                int beginPage = (i - 1) * average + 1;
                int endPage = beginPage + average - 1;
                if (i == ThreadCount - 1)
                    endPage += rest;
                _bodypagesinfo[i - 1].beginPage = beginPage;
                _bodypagesinfo[i - 1].endPage = endPage;
                _bodypagesinfo[i - 1].curPage = beginPage - 1;

            }
            StartThread();
        }

        private void StartThread()
        {
            _completethread = 0;
            _threads = new Thread[ThreadCount];
            _threads[0] = new Thread(new ThreadStart(FrontPagesDownload));
            _threads[0].Start();

            for (int i = 1; i < ThreadCount; i++)
            {
                _threads[i] = new Thread(new ParameterizedThreadStart(BodyPagesDownload));
                _threads[i].Start(i - 1);

            }
        }

        public void Stop()
        {
            if (isComplete)
                return;
            lock (_lockthis)
            {
                isStop = true;
            }
            if (_threads == null)
                return;
            for (int i = 0; i < _threads.Count(); i++)
            {
                if (_threads[i] != null && _threads[i] != Thread.CurrentThread && _threads[i].ThreadState == System.Threading.ThreadState.Running)
                    _threads[i].Abort();
                if (_threads[i] != null && _threads[i] != Thread.CurrentThread && _threads[i].ThreadState == System.Threading.ThreadState.Running)
                    _threads[i].Join();
            }
        }

        public void Continue()
        {
            isStop = false;
            isError = false;
            StartThread();
            OnProcess();
        }

        private void OnProcess()
        {
            if (isStop)
                return;
            FinishRate = 99.0 * DownloadedPages / (_book.PagesNum + FrontPagesNum);
            _progress.Invoke(this);
        }

        private void OnException(Exception e)
        {
            if (isError)
                return;
            Stop();
            isError = true;
            _exception.Invoke(this, e);
        }

        private void OnFinish()
        {
            if (isComplete)
                return;
            isComplete = true;
            _finished.Invoke(this);
        }

        private void FrontPagesDownload()
        {
            try
            {
                for (int part = _frontpagesinfo.Part; part < FrontPagesNum; part++, _frontpagesinfo.Part++)
                {
                    string downloadurl = null;
                    if (part == 0)
                        downloadurl = _book.DownloadUrlTemp;
                    else
                        downloadurl = _book.DownloadUrl;

                    int page = _frontpagesinfo.curPage[part] + 1;
                    string pageName = page.ToString(FrontPageFormat[part]);
                    string url = String.Format(_book.DownloadUrl, pageName);
                    string imagefileName = pageName + ImageType;
                    string path = Path.Combine(_imagepath, imagefileName);
                    try
                    {
                        while (TryToDownload(url, path))
                        {
                            page++;
                            _frontpagesinfo.curPage[part]++;
                            _frontpagesinfo.DownloadedPages++;
                            pageName = page.ToString(FrontPageFormat[part]);
                            url = String.Format(downloadurl, pageName);
                            imagefileName = pageName + ImageType;
                            path = Path.Combine(_imagepath, imagefileName);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is ThreadAbortException)
                            throw e;
                        lock (_lockthis)
                            OnException(e);
                        return;
                    }
                    if (!isStop)
                        lock (_lockthis) DownloadedPages++;
                    OnProcess();
                }
                if (!isStop)
                    ThreadComplete();
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                    return;
            }
        }

        private void BodyPagesDownload(object obj)
        {
            try
            {
                int id = (int)obj;

                for (int page = _bodypagesinfo[id].curPage + 1; page <= _bodypagesinfo[id].endPage; page++, _bodypagesinfo[id].curPage++)
                {
                    string pageName = page.ToString(BodyPageFormat);
                    string url = String.Format(_book.DownloadUrl, pageName);
                    string imagefileName = pageName + ImageType;
                    string path = Path.Combine(_imagepath, imagefileName);

                    try
                    {
                        int tryingtimes = 0;
                        while (!TryToDownload(url, path))
                        {
                            if (++tryingtimes > MaxTryingTimesWhenFailed)
                            {
                                string errorMsg = String.Format("页{0}是空的", page);
                                throw new Exception(errorMsg);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is ThreadAbortException)
                            throw e;
                        lock(_lockthis)
                            OnException(e);
                        return;
                    }
                    if (!isStop)
                        lock (_lockthis) DownloadedPages++;
                    OnProcess();
                }
                if (!isStop)
                    ThreadComplete();
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                    return;
            }
        }

        private void ThreadComplete()
        {
            lock (_lockthis)
            {
                if (++_completethread == ThreadCount)
                {
                    convertPDF();
                    OnFinish();
                }
            }
        }

        private bool TryToDownload(string url, string path)
        {
            Debug.WriteLine("file: {1} url:{0}", url, path);
            int currentTryingTimes = 0;
            bool success = false;
            bool hasContent = true;
            while (!success && !isStop)
            {
                try
                {
                    hasContent = DownloadPage(url, path);
                    success = true;
                }
                catch (Exception e)
                {
                    if (e is ThreadAbortException)
                        throw e;
                    success = false;
                    if (++currentTryingTimes > MaxTryingTimesWhenFailed)
                    {
                        string errorMsg = String.Format("下载错误", path, url);
                        throw new Exception(errorMsg);
                    }
                }
            }
            return hasContent;
        }

        private bool DownloadPage(string url, string path)
        {
            Bitmap image = new Bitmap(HttpWebResponseUtility.GetImage(url));
            System.Drawing.Size size = image.Size;
            if (size.Height == 1 && size.Width == 1)
                return false;
            removeMark(image);
            image.Save(path);
            return true;
        }

        private void removeMark(Bitmap bitmap)
        {
            for (int y = bitmap.Height / 2; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).Equals(waterMark))
                        bitmap.SetPixel(x, y, Color.White);
                }
        }

        private void convertPDF()
        {
            DirectoryInfo di = new DirectoryInfo(_imagepath);
            FileInfo[] files = di.GetFiles("*" + ImageType);
            Array.Sort(files, mySort);
            Document document = new Document(iTextSharp.text.PageSize.A4, 0, 0, 0, 0);
            try
            {
                PdfWriter pdfWrite = PdfWriter.GetInstance(document, new FileStream(PDFFilePath, FileMode.Create, FileAccess.ReadWrite));
                document.Open();
                iTextSharp.text.Image image;
                document.AddTitle(_book.Title);
                foreach (FileInfo file in files)
                {
                    image = iTextSharp.text.Image.GetInstance(file.FullName);
                    document.SetPageSize(new iTextSharp.text.Rectangle(image.Width, image.Height));
                    image.Alignment = iTextSharp.text.Image.ALIGN_MIDDLE;
                    document.NewPage();
                    document.Add(image);
                }
                try
                {
                    BookContentNode content = BookContent.GetBookContent(_book.did, _book.PdgPath);
                    PdfContentByte pdfcontent = pdfWrite.DirectContent;
                    AddBookContents(pdfWrite, pdfcontent.RootOutline, content);
                }
                catch(Exception e)
                {
                    if (e is ThreadAbortException)
                        throw e;
                }
                Directory.Delete(_imagepath, true);
            }
            catch(Exception e)
            {
                if (e is ThreadAbortException)
                    throw e;
                throw new Exception("转换PDF时出错");
            }
            finally
            {
                if (document!=null && document.IsOpen())
                    document.Close();
            }
        }

        public void AddBookContents(PdfWriter writer, PdfOutline outline, BookContentNode content)
        {
            foreach (BookContentNode contentnode in content.Children)
            {
                int page = contentnode.Bookcontent.Page;
                if (page > _book.PagesNum)
                    break;
                page += _frontpagesinfo.DownloadedPages;
                PdfAction action = PdfAction.GotoLocalPage(page, new PdfDestination(PdfDestination.FITB), writer);
                AddBookContents(writer, new PdfOutline(outline, action, contentnode.Bookcontent.Title), contentnode);
            }
        }

        public static int mySort(FileInfo file1, FileInfo file2)
        {
            if (file1.Name[0] == file2.Name[0])
            {
                return file1.Name.CompareTo(file2.Name);
            }
            else
                return ((int)prio[file2.Name[0]]).CompareTo((int)prio[file1.Name[0]]);
        }

        public override String ToString()
        {
            String str = "";
            str += ThreadCount + "|";
            str += DownloadedPages + "|";
            str += _frontpagesinfo + "|";
            foreach (BodyPagesInfo b in _bodypagesinfo)
                str += b + "|";
            return str;
        }

        public void FromString(String str)
        {
            String[] strlist = str.Split('|');
            int i = 0;
            ThreadCount = Int32.Parse(strlist[i++]);
            DownloadedPages = Int32.Parse(strlist[i++]);
            _frontpagesinfo = new FrontPagesInfo();
            _frontpagesinfo.FromString(strlist[i++]);

            for (int j = 0; j < ThreadCount - 1; j++)
            {
                _bodypagesinfo[j].FromString(strlist[i++]);
            }
        }
    }
}
