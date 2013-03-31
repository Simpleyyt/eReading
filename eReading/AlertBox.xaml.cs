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
	/// AlertBox.xaml 的交互逻辑
	/// </summary>
	public partial class AlertBox : UserControl
	{
        public static AlertBox Instance ;
        public bool IsSubmitClicked { get; set; }
        public static ManualResetEvent mre;
		public AlertBox()
		{
			this.InitializeComponent();
            Instance = this;
            mre = new ManualResetEvent(false);
		}

        public static ManualResetEvent Alert(String title, String content)
        {
            mre.Reset();
            Instance.title.Content = title;
            Instance.Msg.Text = content;
            Instance.Visibility = Visibility.Visible;
            Instance.IsSubmitClicked = false;
            return mre;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            IsSubmitClicked = true;
            mre.Set();
        }
        
	}
}