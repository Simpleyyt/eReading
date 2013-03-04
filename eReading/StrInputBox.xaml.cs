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

namespace eReading
{
    /// <summary>
    /// StrInputBox.xaml 的交互逻辑
    /// </summary>
    public partial class StrInputBox : UserControl
    {
        public static StrInputBox Instance;
        public bool IsCancelClicked { get; set; }
        public bool IsSubmitClicked { get; set; }
        public StrInputBox()
        {
            InitializeComponent();
            Instance = this;
        }

        public static String showInputBox(BookInfo book)
        {
            Instance.Visibility = Visibility.Visible;
            Instance.IsCancelClicked = false;
            Instance.IsSubmitClicked = false;
            Instance.STR.Text = "";
            Instance.title.Content = book.Title;
            Instance.ssid.Content = "SS号：" + book.SSID;

            while (!Instance.IsCancelClicked && !Instance.IsSubmitClicked)
                System.Windows.Forms.Application.DoEvents();

            if (Instance.IsSubmitClicked)
                return Instance.STR.Text;
            else
                return null;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            IsCancelClicked = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            IsSubmitClicked = true;
            this.Visibility = Visibility.Hidden;
        }


    }
}
