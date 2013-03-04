using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace eReading
{
	namespace DownloadInfo
	{
        public class BodyPagesInfo
        {
            public int beginPage;
            public int endPage;
            public int curPage;
            public override String ToString()
            {
                return String.Format("{0},{1},{2}", beginPage, endPage, curPage);
            }

            public void FromString(String str)
            {
                String[] strlist = str.Split(',');
                beginPage = Int32.Parse(strlist[0]);
                endPage = Int32.Parse(strlist[1]);
                curPage = Int32.Parse(strlist[2]);
            }
        }

        public class FrontPagesInfo
        {
            public int Part;
            public int[] curPage = new int[5];
            public int DownloadedPages;
            public override String ToString()
            {
                String str = Part + ",";
                for (int i = 0; i < 5; i++)
                {
                    str += curPage[i] + ",";
                }
                str += DownloadedPages;
                return str;
            }
            public void FromString(String str)
            {
                String[] strlist = str.Split(',');
                Part = Int32.Parse(strlist[0]);
                int i;
                for (i = 0; i < 5; i++)
                {
                    curPage[i] = Int32.Parse(strlist[i + 1]);
                }
                DownloadedPages = Int32.Parse(strlist[i+1]);
                
            }
        }
	}
}