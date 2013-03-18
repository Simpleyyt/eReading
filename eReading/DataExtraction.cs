using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Threading.Tasks;

namespace eReading
{
    public static class DataExtraction
    {
        private static String readableDownloadUrl = @"http://img.sslibrary.com/n/{0}{1}?.&uf=ssr&zoom=0";
        private static String unreadableDownloadUrl = @"http://img.duxiu.com/n/{0}{1}?.&uf=ssr&zoom=0";
        private static String PdgPathSever = @"http://read.duxiu.com/setserverinfo/serverinfo.asp";

        public static void GetReadBookUrl(this BookInfo book)
        {
            if (!book.IsReadAll)
            {
                book.ReadBookUrl = book.ReadBookUrlTemp;
                return;
            }
            string source = HttpWebResponseUtility.GetHtmlByHttpWebRequest(book.ReadBookUrlTemp);
            string regexStr = @"window\.location\.href='(.*?)'";
            Match m = Regex.Match(source, regexStr);
            if (m.Success)
                book.ReadBookUrl = m.Groups[1].Value;
            else
                throw new Exception("获取在线读地址时出现错误！");
        }

        public static void AddNewBook(this BookList booklist, String keyword, int page)
        {
            String url = String.Format(Setting.searchUrl, UrlEncode(keyword), page);
            String regexStr = "url\" value=\"(.*?)\"[\\s\\S]*?封面 src='(.*?)'[\\s\\S]*?dxid\" value=\"(\\d*)[\\s\\S]*?ssid\" value=\"(\\d*)[\\s\\S]*?《(.*?)》</a>[\\s\\S]*?(/(?:gobaoku.jsp|readDetail.jsp).*?)\"[\\s\\S]*?(作者.*)";
            String html = HttpWebResponseUtility.GetHtmlByHttpWebRequest(url);

            MatchCollection m = Regex.Matches(html, regexStr);
            m.AsParallel();
            booklist.GetMoreable = html.Contains("下一页");
            string[] s = new string[10];
            Match[] ms = new Match[m.Count];
            m.CopyTo(ms,0);
            Parallel.ForEach(ms, match =>
                {
                    booklist.addBook(GetBriefBookInfo(match.Groups));
                });
        }

        private static string UrlEncode(String url)
        {
            byte[] bs = Encoding.GetEncoding("GB2312").GetBytes(url);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bs.Length; i++)
            {
                if (bs[i] < 128)
                    sb.Append((char)bs[i]);
                else
                {
                    sb.Append("%" + bs[i++].ToString("x").PadLeft(2, '0'));
                    sb.Append("%" + bs[i].ToString("x").PadLeft(2, '0'));
                }
            }
            return sb.ToString();
        }

        public static string RemoveHTMLTab(String str)
        {
            string regexStr = "<.*?>";
            Regex r = new Regex(regexStr);
            return r.Replace(str, "");
        }

        public static int GetPageNum(string str)
        {
            string regexStr = @"页数:(\d*)";
            Match m = Regex.Match(str, regexStr);
            if (m.Success)
                return int.Parse(m.Groups[1].Value);
            else
                throw new Exception("获取页数时出现错误！");
        }

        private static BookInfo GetBriefBookInfo(this GroupCollection gc)
        {
            BookInfo book = new BookInfo();
            book.DetailInfoUrl = gc[1].Value;
            book.SmallCoverImage = HttpWebResponseUtility.GetImage(gc[2].Value);
            book.DXID = gc[3].Value;
            book.SSID = gc[4].Value;
            book.Title = RemoveHTMLTab(gc[5].Value);
            book.ReadBookUrlTemp = Setting.platformHost[Setting.platformIndex] + gc[6].Value;
            book.IsReadAll = book.ReadBookUrlTemp.Contains("gobaoku");
            book.BriefInfo = RemoveHTMLTab(gc[7].Value.Replace("&nbsp;", " ").Replace("<br>", "\n"));
            book.PagesNum = GetPageNum(book.BriefInfo);
            return book;
        }

        public static ImageSource ImageToSource(Image image)
        {
            MemoryStream stream = new MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            System.Windows.Media.ImageBrush imageBrush = new System.Windows.Media.ImageBrush();
            System.Windows.Media.ImageSourceConverter imageSourceConverter = new System.Windows.Media.ImageSourceConverter();
            return (System.Windows.Media.ImageSource)imageSourceConverter.ConvertFrom(stream);
        }

        public static void GetDetailInfo(this BookInfo book)
        {
            if (book.isDetail)
                return;
            String coveReg = "(http://cover.*?)\"";
            string source = HttpWebResponseUtility.GetHtmlByHttpWebRequest(book.DetailInfoUrl);
            Match m = Regex.Match(source, coveReg);
            if (m.Success)
            {
                book.BigCoverImage = HttpWebResponseUtility.GetImage(m.Groups[1].Value);
                book.isDetail = true;
            }
            String regexStr = "<p>([\\s\\S]*?)</p>";
            m = Regex.Match(source, regexStr);
            while (m.Success)
            {
                book.DetailInfo += DataExtraction.RemoveHTMLTab(m.Groups[1].Value).Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(" ", "").Replace("&gt;","->") + "\n";
                m = m.NextMatch();
            }
        }

        public static void GetDownloadUrlByStr(this BookInfo book, String str)
        {
            string regex = @"img\d*/(.*?)/";
            Match m = Regex.Match(book.DownloadUrlTemp, regex);
            String pid = BookDataBase.GetInstance().GetPIDByStr(str);
            if(pid !=null && m.Success)
                book.DownloadUrl = book.DownloadUrlTemp.Replace(m.Groups[1].Value, pid);
        }

        public static bool GetDownloadUrl(this BookInfo book, DependencyObject dispatcher = null)
        {
            if (book.ReadBookUrl == null || book.ReadBookUrl == "")
                GetReadBookUrl(book);
            string source = null;
            if (book.IsReadAll)
                source = HttpWebResponseUtility.GetHtmlByWebBrowser(book.ReadBookUrl, dispatcher);
            else
                source = HttpWebResponseUtility.GetHtmlByHttpWebRequest(book.ReadBookUrl);

            string regexStr = "did = \"(.*?)\"[\\s\\S]*?PdgPath = \"(.*?)\"[\\s\\S]*?var str = \"(.*?)\"";
            Match m = Regex.Match(source, regexStr);
            if (!m.Success)
                throw new Exception("获取STR时出现错误！");
            book.did = m.Groups[1].Value;
            book.PdgPath = m.Groups[2].Value;
            String str = m.Groups[3].Value;
            if (!book.IsReadAll)
            {
                book.DownloadUrlTemp = String.Format(unreadableDownloadUrl,str,"{0}");
                string pid = null;
                try
                {
                    pid = BookDataBase.GetInstance().GetBookPID(book.DXID);
                }
                catch
                {
                    return false;
                }
                string regex = @"img\d*/(.*?)/";
                m = Regex.Match(str, regex);
                if (pid != null && m.Success)
                    book.DownloadUrl = book.DownloadUrlTemp.Replace(m.Groups[1].Value, pid);
                else
                    return false;
            }
            else
            {
                book.DownloadUrlTemp = String.Format(readableDownloadUrl, str, "{0}");
                book.DownloadUrl = book.DownloadUrlTemp;
            }
            return true ;
        }

        public static String GetPdgHost(String did)
        {
            String source = HttpWebResponseUtility.GetHtmlByHttpWebRequest(PdgPathSever);
            string regexStr = String.Format("{0}.*?<td>(.*?)</td>",did);
            Match m = Regex.Match(source, regexStr);
            if (m.Success)
                return m.Groups[1].Value;
            else
                return null;
        }
    }
}
