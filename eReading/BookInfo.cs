using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;

namespace eReading
{
    public class BookInfo
    {
        public string DetailInfoUrl;
        public Image SmallCoverImage;
        public Image BigCoverImage;
        public string BriefInfo;
        public string DetailInfo;
        public string DXID;
        public string SSID;
        public string Title;
        public string ReadBookUrl;
        public string ReadBookUrlTemp;
        public string DownloadUrl;
        public string DownloadUrlTemp;
        public int PagesNum;
        public bool isDetail;
        public bool IsReadAll;
        public String did;
        public String PdgPath;
    }
}
