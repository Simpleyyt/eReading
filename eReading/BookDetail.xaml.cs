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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace eReading
{
    /// <summary>
    /// BookDetail.xaml 的交互逻辑
    /// </summary>
    public partial class BookDetail : UserControl, IBookInfo
    {
        private Thread subThread;
        public BookInfo book { get; set; }

        public static readonly RoutedEvent downloadEvent = EventManager.RegisterRoutedEvent("BookDetailDownloadEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BookDetail));

        public event RoutedEventHandler downloadClicked
        {
            add { base.AddHandler(downloadEvent, value); }
            remove { base.RemoveHandler(downloadEvent, value); }
        }

        public BookDetail()
        {
            InitializeComponent();
        }

        private void SetValue(BookInfo book)
        {
            this.book = book;
            this.title.Content = book.Title;
            this.cover.Source = DataExtraction.ImageToSource(book.BigCoverImage);
            this.brief.Text = book.DetailInfo;
        }

        private void downLoadBook_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(BookDetail.downloadEvent, this));
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            StopTask();
        }

        private void StopTask()
        {
            if (subThread != null && subThread.ThreadState == ThreadState.Running)
                subThread.Abort();
            if (subThread != null && subThread.ThreadState == ThreadState.Running)
                subThread.Join();
            this.Visibility = Visibility.Hidden;
        }

        public void GetMoreDetail(BookInfo book)
        {
            this.title.Content = book.Title;
            Visibility = Visibility.Visible;
            loadingLabel.Visibility = Visibility.Visible;
            subThread = new Thread(new ParameterizedThreadStart(getDetail));
            subThread.Start(book);
        }

        private void getDetail(object obj)
        {
            try
            {
                BookInfo book = (BookInfo)obj;
                book.GetDetailInfo();
                this.Dispatcher.Invoke(new Action(() =>
                    {
                        SetValue(book);
                        loadingLabel.Visibility = Visibility.Hidden;
                    }));
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                    return;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    loadingLabel.Visibility = Visibility.Visible;
                    loadingLabel.Content = "获取失败";
                    StopTask();
                }));
            }
        }
    }
}
