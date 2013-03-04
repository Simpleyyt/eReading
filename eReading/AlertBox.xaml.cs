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

		public AlertBox()
		{
			this.InitializeComponent();
            Instance = this;
		}

        public static void Alert(String title, String content)
        {
            Instance.title.Content = title;
            Instance.Msg.Text = content;
            Instance.Visibility = Visibility.Visible;
            Instance.IsSubmitClicked = false;
            while(!Instance.IsSubmitClicked)
                System.Windows.Forms.Application.DoEvents();
            
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            IsSubmitClicked = true;
        }
        
	}
}