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
using System.Windows.Threading;
using System.Threading;

namespace eReading
{
    /// <summary>
    /// BookElement.xaml 的交互逻辑
    /// </summary>
    public partial class BookElement : UserControl,IBookInfo
    {
        public BookInfo book { get; set; }
        public static readonly RoutedEvent downloadEvent = EventManager.RegisterRoutedEvent("BookElementDownloadEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(BookElement));
        public event RoutedEventHandler downloadClicked
        {
            add { base.AddHandler(downloadEvent, value); }
            remove { base.RemoveHandler(downloadEvent, value); }
        }
		
		public BookElement()
        {
            InitializeComponent();
        }
		
        public BookElement(BookInfo book)
        {
            InitializeComponent();
            this.book = book;
            this.title.Content = book.Title;
            this.cover.Source = DataExtraction.ImageToSource(book.SmallCoverImage);
            this.brief.Text = book.BriefInfo;
        }

        private void downLoadBook_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(BookElement.downloadEvent, this));
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            g.Background = new SolidColorBrush(Colors.Purple);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid g = (Grid)sender;
            g.Background = new SolidColorBrush(Colors.Blue);
        }
    }
}