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
	/// SearchBox.xaml 的交互逻辑
	/// </summary>
	public partial class SearchBox : UserControl
	{
        public bool IsEmpty
        {
            get;
            set;
        }
        public String Text
        {
            get
            {
                return searchText.Text;
            }
            set
            {
                searchText.Text = value;
            }
        }
        public event RoutedEventHandler SearchButtonClicked
        {
            add
            {
                searchButton.Click += value;
            }
            remove
            {
                searchButton.Click -= value;
            }
        }

		public SearchBox()
		{
			this.InitializeComponent();
            IsEmpty = true;
		}

        private void searchText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (searchText.Text == "")
            {
                searchText.Text = "search...";
                searchText.FontStyle = FontStyles.Italic;
                searchText.Foreground = Brushes.LightGray;
                IsEmpty = true;
            }
        }

        private void searchText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsEmpty)
            {
                searchText.Text = "";
                searchText.FontStyle = FontStyles.Normal;
                searchText.Foreground = Brushes.Black;
                IsEmpty = false;
            }
        }
	}
}