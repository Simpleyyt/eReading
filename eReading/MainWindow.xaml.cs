using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace eReading
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread subThread;
        private int _curpage;
        private String _cursearch;
        private Setting _setting;
		private Storyboard storyboard;
        private static MainWindow Instance;

        public MainWindow()
        {
            InitializeComponent();
            _setting = new Setting();
            bookDetail1.downloadClicked += new RoutedEventHandler(downloadClicked);
            bookDetail1.close.MouseUp += new MouseButtonEventHandler(close_MouseUp);
			storyboard = (Storyboard)this.Resources["Storyboard1"];
            Instance = this;
        }

        void close_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.bookDetail1.Visibility = Visibility.Hidden;
        }

        private void searchButton_Clicked(object sender, RoutedEventArgs e)
        {
			this.searchScroll.ScrollToVerticalOffset(0);
			this.help.Visibility = Visibility.Hidden;
            this.searchResult.Children.Clear();
            _curpage = 1;
            this.loadingLabel.Content = "加载中...";
            this.loadingLabel.Visibility = Visibility.Visible;
            subThread = new Thread(new ThreadStart(getSearchResult));
            subThread.Start();
            
        }

        private void getSearchResult()
        {
            try
            {
                _cursearch = "";
                this.Dispatcher.Invoke(new Action(() =>
                {
                    _cursearch = this.searchBox.Text.Replace(" ", "+").Replace("+", "%2B");
                }));
                BookList booklist = new BookList();
                booklist.AddNewBook(_cursearch, _curpage);
                this.loadingLabel.Dispatcher.Invoke(new Action(() =>
                    {

                        if (booklist.Count == 0)
                        {
                            this.loadingLabel.Content = "找不到相关图书";
                            this.loadingLabel.Visibility = Visibility.Visible;
                        }
                        else
                            this.loadingLabel.Visibility = Visibility.Hidden;
                    }));
                for (int i = 0; i < booklist.Count; i++)
                {
                    this.searchResult.Dispatcher.Invoke(new Action(() =>
                        {
                            BookElement be = new BookElement(booklist[i]);
                            be.MouseUp += new MouseButtonEventHandler(be_MouseUp);
                            be.downloadClicked += new RoutedEventHandler(downloadClicked);
                            searchResult.Children.Add(be);
                        }));
                }
                if (booklist.GetMoreable)
                {
                    this.searchResult.Dispatcher.Invoke(new Action(() =>
                        {
                            LoadingMoreElement load = new LoadingMoreElement();
                            load.MouseUp += new MouseButtonEventHandler(load_MouseUp);
                            searchResult.Children.Add(load);
                        }));
                }
            }
            catch
            {
                this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.loadingLabel.Content = "加载失败";
                        this.loadingLabel.Visibility = Visibility.Visible;
                    }));
            }
        }

        private void downloadClicked(object sender, RoutedEventArgs e)
        {
            IBookInfo be = (IBookInfo)sender;
            downloadTaskList.StartNewTask(be.book);
			ShowMessage("已添加到下载列表");
        }

        private void getMoreBook()
        {
            try
            {
                BookList booklist = new BookList();
                booklist.AddNewBook(_cursearch, ++_curpage);
                for (int i = 0; i < booklist.Count; i++)
                {
                    this.searchResult.Dispatcher.Invoke(new Action(() =>
                    {

                        BookElement be = new BookElement(booklist[i]);
                        be.MouseUp += new MouseButtonEventHandler(be_MouseUp);
                        be.downloadClicked += new RoutedEventHandler(downloadClicked);
                        int count = searchResult.Children.Count;
                        searchResult.Children.Insert(count - 1, be);
                    }));
                }
                this.searchResult.Dispatcher.Invoke(new Action(() =>
                    {
                        int count = searchResult.Children.Count;
                        LoadingMoreElement load = (LoadingMoreElement)searchResult.Children[count - 1];
                        load.changeToGetting();
                        if (!booklist.GetMoreable)
                            searchResult.Children.Remove(load);
                    }));
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                    return;
                this.searchResult.Dispatcher.Invoke(new Action(() =>
                    {
                        int count = searchResult.Children.Count;
                        LoadingMoreElement load = (LoadingMoreElement)searchResult.Children[count - 1];
                        load.changeToFailed();
                    }));
            }
        }

        void load_MouseUp(object sender, MouseButtonEventArgs e)
        {
            LoadingMoreElement load = (LoadingMoreElement)sender;
            if (!load.isLoading)
            {
                load.changeToLoading();
                subThread = new Thread(new ThreadStart(getMoreBook));
                subThread.Start();
            }
        }

        void be_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BookElement be = (BookElement)sender;
			bookDetail1.GetMoreDetail(be.book);
        }

        private void downloadList_Click(object sender, RoutedEventArgs e)
        {
            downloadTaskList.Visibility = Visibility.Visible;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            _setting.Show();
            _setting.Activate();
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
			help.Visibility = Visibility.Visible;
			searchResult.Children.Clear();
			this.searchScroll.ScrollToVerticalOffset(0);
        }
		
		public static void ShowMessage(String msg)
		{
            Instance.message.Content = msg;
			Instance.storyboard.Begin();
		}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _setting.isClose = true;
            _setting.Close();
            if(subThread!=null && subThread.ThreadState == ThreadState.Running)
                subThread.Abort();
            if (subThread != null && subThread.ThreadState == ThreadState.Running)
                subThread.Join();
            downloadTaskList.StopTask();
            //downloadTaskList.SaveToFile();
        }
  
    }
}
