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

namespace eReading
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingMoreElement : UserControl
    {
        public bool isLoading = false;
        public string loadingStr = "加载中......";
        public string getmoreStr = "点击加载更多";
        public string failedStr = "加载失败，点击重试";

        public LoadingMoreElement()
        {
            InitializeComponent();
        }

        public void changeToLoading()
        {
            this.state.Content = loadingStr;
            isLoading = true;
        }

        public void changeToGetting()
        {
            this.state.Content = getmoreStr;
            isLoading = false;
        }

        public void changeToFailed()
        {
            this.state.Content = failedStr;
            isLoading = false;
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
