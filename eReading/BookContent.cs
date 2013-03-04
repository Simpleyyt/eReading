using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComponentAce.Compression.ZipForge;
using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.Libs.ZLib;
using System.IO;
using System.Collections;

namespace eReading
{
    struct BookContentInfo
    {
        public String Title;
        public int Page;
        public String Lever;
        public int Type;
        public String Reserved;
    }

    class BookContentNode
    {
        public BookContentInfo Bookcontent { get; set; }
        public List<BookContentNode> Children;
        public BookContentNode Parent { get; set; }

        public BookContentNode()
        {
            Children = new List<BookContentNode>();
        }
    }

    class BookContent
    {
        public static Stream ZipUnCode(Stream stream)
        {
            return new ZInputStream(stream);
        }

        public static BookContentNode GetBookContent(String did, String codepdgpath)
        {
            String host = DataExtraction.GetPdgHost(did);
            String pdgpath = DecodePdgPath(codepdgpath);
            String path = "http://" + host + "/" + pdgpath + "BookContents.dat";
            Stream stream = HttpWebResponseUtility.CreateGetHttpResponse(path);
            stream.Read(new byte[0x28], 0, 0x28);
            StreamReader reader = new StreamReader(ZipUnCode(stream),Encoding.Default);
            BookContentNode root = new BookContentNode() { Parent = null };
            root.Bookcontent = new BookContentInfo()
                {
                    Title = "root",
                    Lever = "",
                    Page = 0,
                    Reserved = null,
                    Type = 0
                };
            BookContentNode curnode = root;
            while (!reader.EndOfStream)
            {
                BookContentNode newnode = new BookContentNode();
                String str = reader.ReadLine().Trim();
                String[] strlist = str.Split('|');
                newnode.Bookcontent = new BookContentInfo()
                {
                    Title = strlist[0].Trim(),
                    Lever = strlist[1].Trim(),
                    Page = Int32.Parse(strlist[2].Trim()),
                    Reserved = strlist[3].Trim(),
                    Type = Int32.Parse(strlist[4].Trim())
                };
                int newIndentation = newnode.Bookcontent.Lever.Length;
                int curIndentation = curnode.Bookcontent.Lever.Length;
                if(newIndentation <= curIndentation)
                {
                    curnode = curnode.Parent;
                    for (int i = newIndentation; i != curIndentation; i += 2)
                    {
                        curnode = curnode.Parent;
                    }
                }
                newnode.Parent = curnode;
                curnode.Children.Add(newnode);
                curnode = newnode;
            }
            return root ;
        }

        public static String DecodePdgPath(String pdgpath)
        {
            String temp = "";
            int[] bs = HexStrToInts(pdgpath);
            int code = bs[bs.Length - 1];
            foreach (int b in bs)
            {
                if (b <= code || System.Math.Abs(b - code) <= 32)
                    break;
                temp += (char)(b - code);
            }
            return temp;
        }

        public static int[] HexStrToInts(String str)
        {
            int[] result = new int[str.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                int k = Convert.ToInt32(str.Substring(i * 2, 2), 16);
                result[i] = k;
            }
            return result;
        }
    }
}
