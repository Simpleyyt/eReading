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

        public override String ToString()
        {
            String str = DetailInfo + ",";
            str += SSID + ",";
            str += Title + ",";
            str += ReadBookUrl + ",";
            str += ReadBookUrlTemp + ",";
            str += DownloadUrl + ",";
            str += DownloadUrlTemp + ",";
            str += PagesNum + ",";
            str += isDetail + ",";
            str += IsReadAll;
            return str;
        }

        public void FromString(String str)
        {
            String[] strlist = str.Split(',');
            DetailInfoUrl = strlist[0];
            SSID = strlist[1];
            Title = strlist[2];
            ReadBookUrl = strlist[3];
            ReadBookUrlTemp = strlist[4];
            DownloadUrl = strlist[5];
            DownloadUrlTemp = strlist[6];
            PagesNum = Int32.Parse(strlist[7]);
            isDetail = bool.Parse(strlist[8]);
            IsReadAll = bool.Parse(strlist[9]);
        }
    }
}
