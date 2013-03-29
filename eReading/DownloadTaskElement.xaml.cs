using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net;
using System.IO;
using System.Drawing;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using eReading.DownloadInfo;

namespace eReading
{
    /// <summary>
    /// DownloadElement.xaml 的交互逻辑
    /// </summary>
    public enum Status { Waiting, GettingSTR, Downloading, Completed, Error };
    public partial class DownloadTaskElement : UserControl
    {
        #region 私有成员

        public BookInfo _book;
        private DownloadTaskList _downloadmanager;
        private Download _download;
        private FinishEventHandler _finish;
        private ExceptionEventHandler _exception;
        private Thread _subthread;
        private Status _statuswhenerror;

        #endregion

        #region 委托

        public delegate void FinishEventHandler(DownloadTaskElement sender);
        public delegate void ExceptionEventHandler(DownloadTaskElement sender);

        #endregion

        #region 属性

        public bool isComplete
        {
            get
            {
                return Status == Status.Completed;
            }
        }
        public bool isError
        {
            get
            {
                return Status == Status.Error;
            }
        }
        public bool isWaiting
        {
            get
            {
                return Status == Status.Waiting;
            }
        }
        public bool isGettingSTR
        {
            get
            {
                return Status == Status.GettingSTR;
            }
        }
        public bool isDownloading
        {
            get
            {
                return Status == Status.Downloading;
            }
        }
        public Status Status { get; set; }
        public BookInfo Book
        {
            get 
            {
                return _book;
            }
        }

        #endregion

        #region 事件

        public event FinishEventHandler Finish
        {
            add
            {
                _finish += value;
            }
            remove
            {
                _finish -= value;
            }
        }
        public event ExceptionEventHandler Exception
        {
            add
            {
                _exception += value;
            }
            remove
            {
                _exception -= value;
            }
        }

        #endregion

        public DownloadTaskElement(BookInfo book, DownloadTaskList downloadmanager)
        {
            InitializeComponent();
            _book = book;
            _downloadmanager = downloadmanager;
            _download = new Download(book, Setting.downloadPath);
            _download.Finish += new Download.FinishedEventHandler(_download_Finish);
            _download.Exception += new Download.ExceptionEventHandler(_download_Exception);
            _download.Progress += new Download.ProgressEventHandler(_download_Progress);


            this.Exception += new ExceptionEventHandler(downloadmanager.Exception);
            this.Finish += new FinishEventHandler(downloadmanager.Finish);
            this.Status = Status.Waiting;
            initUI();
        }

        private void _download_Progress(Download sender)
        {
            this.Dispatcher.Invoke(new Action(() =>
                {
                    progress.Value = sender.FinishRate;
                    this.status.Content = String.Format("{0}%", (int)sender.FinishRate);
                }));
        }

        private void _download_Exception(Download sender, Exception e)
        {
            this.Dispatcher.Invoke(new Action(() => DownloadFailed(e.Message)));
        }

        private void GetStrThread()
        {
            try
            {
                if (!_book.GetDownloadUrl(this))
                {
                    String str = null;
                    this.Dispatcher.Invoke(new Action(() =>
                        {
                            str = StrInputBox.showInputBox(_book);

                        }));

                    if (str != null)
                    {
                        _book.GetDownloadUrlByStr(str);
                    }
                    else
                        throw new Exception("找不到STR");
                }
                this.Dispatcher.Invoke(new Action(() => status.Content = "正在下载"));
                Status = Status.Downloading;
                _download.Start();
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                    return;
                DownloadFailed(e.Message);
            }
        }

        private void _download_Finish(Download sender)
        {
            if (isError)
                return;
            Status = Status.Completed;
            this.Dispatcher.Invoke(new Action(() =>
                {
                    this.status.Content = "已完成";
                    this.progress.Value = 100;
                    openButtons.Visibility = Visibility.Visible;
                    _finish.Invoke(this);
                }));
        }

        public void initUI()
        {
            this.progress.Visibility = Visibility.Hidden;
            this.status.Content = "等待中...";
            title.Content = _book.Title;
        }

        public void StartDownload()
        {
            this.progress.Visibility = Visibility.Visible;
            Status = Status.GettingSTR;
            status.Content = "正在获取STR";
            progress.Visibility = Visibility.Visible;
            pausestartButton.IsEnabled = true;
            _subthread = new Thread(new ThreadStart(GetStrThread));
            _subthread.Start();
        }

        private void openFile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(_download.PDFFilePath);
        }

        private void openDir_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(_download.DownloadPath);
        }

        private void DownloadFailed(String errorMsg)
        {
            _statuswhenerror = Status;
            Status = Status.Error;
            this.Dispatcher.Invoke(new Action(() =>
                {
                    this.retry.Visibility = Visibility.Visible;
                    this.status.Content = errorMsg;
                    _exception.Invoke(this);
                    pausestartButton.IsEnabled = false;
                }));
        }

        public void StopTask()
        {
            if (_subthread != null)
                _subthread.Abort();
            _download.Stop();
        }

        private void delete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            StopTask();
            _downloadmanager.RemoveTaskElement(this);
        }

        public void Pause()
        {
            StopTask();
            _download.Stop();
        }

        public void Continue()
        {
            if (isGettingSTR)
                StartDownload();
            if (isDownloading)
                _download.Continue();
        }

        public void FromFile(String path)
        {
            _download.SetPath(path);
            ConfigureHelper config = new ConfigureHelper(System.IO.Path.Combine(_download.ImagePath, "DownloadInfo.config"));
            _download.FromString(config.ReadValue("_download"));
            Status = (Status)Int32.Parse(config.ReadValue("Status"));
            status.Content = config.ReadValue("status_content");
            progress.Value = double.Parse(config.ReadValue("progress_value"));
            if (isComplete)
            {
                this.openButtons.Visibility = Visibility.Visible;
            }
            if (isDownloading || isGettingSTR)
            {
                this.progress.Visibility = Visibility.Visible;
                this.pausestartButton.IsEnabled = true;
                this.pausestartButton.IsChecked = true;
            }

        }

        public void SaveToFile()
        {
            ConfigureHelper config = new ConfigureHelper(System.IO.Path.Combine(_download.ImagePath, "DownloadInfo.config"));
            config.WriteValue("_download", _download.ToString());
            config.WriteValue("Status", ((int)Status).ToString());
            config.WriteValue("status_content", status.Content.ToString());
            config.WriteValue("progress_value", progress.Value.ToString());
            config.Save();
        }

        public void SavePathAndBook(ConfigureHelper config)
        {
            config.AddValue("DownloadList", new ConfigSectionData() { Path = _download.DownloadPath, BookInfo = _book.ToString() });
        }

        private void pausestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (pausestartButton.IsChecked == true)
                Pause();
            if (pausestartButton.IsChecked == false)
                Continue();
        }

        private void retry_MouseUp(object sender, MouseButtonEventArgs e)
        {
            retry.Visibility = Visibility.Hidden;
            Status = _statuswhenerror;
            pausestartButton.IsEnabled = true;
            pausestartButton.IsChecked = false;
            this.Continue();
        }
    }
}