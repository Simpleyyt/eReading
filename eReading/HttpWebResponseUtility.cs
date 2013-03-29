using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows;

namespace eReading
{
    public static class HttpWebResponseUtility
    {
        public static Stream CreateGetHttpResponse(String url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.MaximumAutomaticRedirections = 5;
            request.CookieContainer = new CookieContainer();
            request.Method = "GET";
            request.Headers.Add("Cookie", Setting.cookies);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.2) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.91 Safari/537.11";
            request.KeepAlive = true;
            request.Timeout = 30000;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            return response.GetResponseStream();
        }

        public static Image GetImage(String url)
        {
            Stream stream = CreateGetHttpResponse(url);
            stream.ReadTimeout = 30000;
            try
            {
                if (stream.CanRead)
                    return Image.FromStream(stream);
                else
                    throw new Exception("取图片错误");
            }
            catch (ArgumentException e)
            {
                return null;
            }
        }

        public static String GetHtmlByHttpWebRequest(String url)
        {
            StreamReader streamreader = new StreamReader(CreateGetHttpResponse(url), Encoding.GetEncoding("gbk"));
            return streamreader.ReadToEnd();
        }

        public static String GetHtmlByWebBrowser(String url, DependencyObject dispatcher)
        {
            String s = null;
            dispatcher.Dispatcher.Invoke(new Action(() =>
            {

                WebBrowser wb = new WebBrowser();
                wb.Navigate(url);
                while (wb.ReadyState != System.Windows.Forms.WebBrowserReadyState.Complete)
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                Encoding encoding = Encoding.GetEncoding(wb.Document.Encoding);
                StreamReader stream = new StreamReader(wb.DocumentStream, encoding);
                s = stream.ReadToEnd();
            }));
            return s;
        }



    }
}
