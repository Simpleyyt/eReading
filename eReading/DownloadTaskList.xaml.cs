using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using System.Collections;
using System.Configuration;

namespace eReading
{
    /// <summary>
    /// DownLoadManager.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadTaskList : UserControl
    {
        public int CurrentTask
        {
            get { return (int)GetValue(CurrentTaskProperty); }
            set
            {
                SetValue(CurrentTaskProperty, value);
            }
        }

        public bool IsEmpty
        {
            get { return (bool)GetValue(IsEmptyProperty); }
            set { SetValue(IsEmptyProperty, value); }
        }

        public static readonly DependencyProperty IsEmptyProperty =
            DependencyProperty.Register("IsEmpty",
                     typeof(bool),
                     typeof(DownloadTaskList),
                     new UIPropertyMetadata(false));

        public static readonly DependencyProperty CurrentTaskProperty =
           DependencyProperty.Register("CurrentTask",
                    typeof(int),
                    typeof(DownloadTaskList),
                    new PropertyMetadata(0));

        public DownloadTaskList()
        {
            InitializeComponent();
            CurrentTask = 0;
            IsEmpty = (taskList.Children.Count == 0);
            //ReadFromFile();
        }

        public void Exception(DownloadTaskElement sender)
        {
            StartOneToDownload();
            MainWindow.ShowMessage(String.Format("《{0}》下载出错", sender.Book.Title));
        }

        public void Finish(DownloadTaskElement sender)
        {
            CurrentTask--;
            StartOneToDownload();
            MainWindow.ShowMessage(String.Format("《{0}》下载完成",sender.Book.Title));
        }

        public void StartNewTask(BookInfo book)
        {
            DownloadTaskElement de = new DownloadTaskElement(book, this);
            AddTaskElement(de);
            StartOneToDownload();
        }

        public void StartOneToDownload()
        {
            DownloadTaskElement de = null;
            for (int i = 0; i < taskList.Children.Count; i++)
            {
                de = (DownloadTaskElement)taskList.Children[i];
                if (de.Status == Status.Downloading || de.Status == Status.GettingSTR)
                    break;
                if (de.Status == Status.Waiting)
                {
                    de.StartDownload();
                    break;
                }
            }
        }

        public void StopTask()
        {
            for (int i = 0; i < taskList.Children.Count; i++)
            {
                if (taskList.Children[i] is DownloadTaskElement)
                {
                    DownloadTaskElement downloadelement = (DownloadTaskElement)taskList.Children[i];
                    downloadelement.StopTask();
                }
            }
            //SaveToFile();
        }

        private void close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        public void SaveToFile()
        {
            ConfigureHelper config = new ConfigureHelper("DownloadTaskList.config");
            config.CreatGrouop("DownloadList");
            foreach (DownloadTaskElement de in this.taskList.Children)
            {
                de.SavePathAndBook(config);
                de.SaveToFile();
            }
            config.Save();
        }

        public void AddTaskElement(DownloadTaskElement de)
        {
            taskList.Children.Add(de);
            if (de.isWaiting || de.isDownloading || de.isGettingSTR)
                CurrentTask++;
            IsEmpty = (taskList.Children.Count == 0);
        }

        public void RemoveTaskElement(DownloadTaskElement de)
        {
            taskList.Children.Remove(de);
            if (de.isWaiting || de.isDownloading || de.isGettingSTR || de.isError)
                CurrentTask--;
            if (de.isDownloading || de.isError || de.isGettingSTR)
                StartOneToDownload();
            IsEmpty = (taskList.Children.Count == 0);
        }

        public void ReadFromFile()
        {
            ConfigureHelper config = new ConfigureHelper("DownloadTaskList.config");
            ConfigurationSectionCollection datas = config.GetValue("DownloadList");
            int count = datas.Count;
            for (int i = 0; i < count; i++)
            {
                ConfigSectionData data = (ConfigSectionData)datas["add"+i];
                BookInfo book = new BookInfo();
                book.FromString(data.BookInfo);
                DownloadTaskElement de = new DownloadTaskElement(book, this);
                AddTaskElement(de);
                de.FromFile(data.Path);
            }
        }

    }
}
