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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace eReading
{
    /// <summary>
    /// StrInputBox.xaml 的交互逻辑
    /// </summary>
    public partial class StrInputBox : UserControl
    {
        private static ManualResetEvent mre;
        public static StrInputBox Instance;
        public bool IsCancelClicked { get; set; }
        public bool IsSubmitClicked { get; set; }
        public StrInputBox()
        {
            InitializeComponent();
            Instance = this;
            mre = new ManualResetEvent(false);
        }

        public static ManualResetEvent showInputBox(BookInfo book)
        {
            mre.Reset();
            Instance.Visibility = Visibility.Visible;
            Instance.IsCancelClicked = false;
            Instance.IsSubmitClicked = false;
            Instance.STR.Text = "";
            Instance.title.Content = book.Title;
            Instance.ssid.Content = "SS号：" + book.SSID;
            return mre;
        }

        public static String GetString()
        {
            if (Instance.IsSubmitClicked)
                return Instance.STR.Text;
            return null;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            IsCancelClicked = true;
            this.Visibility = Visibility.Hidden;
            mre.Set();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            IsSubmitClicked = true;
            this.Visibility = Visibility.Hidden;
            mre.Set();
        }
    }
}
