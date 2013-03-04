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
using System.Windows.Forms;

namespace eReading
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : Window
    {
        public static string downloadPath = @"d:\e-book";
        public static String cookies = @"msign_dsr=1357453002333; JSESSIONID=0FC50001CC4874481FB88BB2F7D25238.gharea35; testcookie=yes; duxiu=userName%5fdsr%2c%3dgdzsdx%2c%21userid%5fdsr%2c%3d22%2c%21char%5fdsr%2c%3d%u676f%2c%21metaType%2c%3d265%2c%21dsr%5ffrom%2c%3d1%2c%21logo%5fdsr%2c%3dlogo0408%2ejpg%2c%21logosmall%5fdsr%2c%3dsmall0408%2ejpg%2c%21title%5fdsr%2c%3d%u4e2d%u5c71%u5927%u5b66%2c%21url%5fdsr%2c%3debook%2c%21compcode%5fdsr%2c%3d1023%2c%21province%5fdsr%2c%3d%u5e7f%u4e1c%u7701%2c%21readDom%2c%3d0%2c%21isdomain%2c%3d11%2c%21showcol%2c%3d0%2c%21hu%2c%3d0%2c%21uscol%2c%3d0%2c%21isfirst%2c%3d0%2c%21istest%2c%3d0%2c%21cdb%2c%3d0%2c%21og%2c%3d0%2c%21testornot%2c%3d1%2c%21remind%2c%3d0%2c%21datecount%2c%3d343%2c%21userIPType%2c%3d2%2c%21lt%2c%3d0%2c%21enc%5fdsr%2c%3dF199FA81938E726E4B3E2CC9A5BA4C8B; AID_dsr=17; uname=yaoyit; UID=15344866; nkname=yaoyit; uphoto=; enc=02594C4609D368A265B2E60D8F03C1B3; CNZZDATA2088844=cnzz_eid=2264698-1357449390-&ntime=1358819433&cnzz_a=13&retime=1358821318739&sin=&ltime=1358821318739&rtime=9";
        public static string searchUrlArg = @"/search?sw={0}&searchtype=1&channel=search&Pages={1}";
        public static string searchUrl;

        public static string[] platformHost = new string[] { @"http://book.szdnet.org.cn", @"http://book.duxiu.com" };
        public static bool isLoginWhenRun = false;
        public static bool isDefaultCookies = true;
        public static string userCookies;
        public static int platformIndex;

        private ConfigureHelper config;
        public bool isClose { get; set; }
        public Setting()
        {
            InitializeComponent();
            config = new ConfigureHelper("eReading.config");
            readConfigureFile();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isClose)
            {
                this.Hide();
                e.Cancel = true;
            }
        }
        public void writeConfigureFile()
        {
            config.WriteValue("downloadPath", downloadDir.Text);
            config.WriteValue("isDefaultCookies", defaultCookies.IsChecked.ToString());
            config.WriteValue("userCookies", Cookies.Text);
            config.WriteValue("platformIndex", Plaform.SelectedIndex.ToString());
            config.Save();
        }

        public void readConfigureFile()
        {

            downloadPath = config.ReadValue("downloadPath", "");
            isDefaultCookies = bool.Parse(config.ReadValue("isDefaultCookies",true.ToString()));
            userCookies = config.ReadValue("userCookies","");
            platformIndex = int.Parse(config.ReadValue("platformIndex","0"));
            initUI();
        }

        public void initUI()
        {
            this.downloadDir.Text = downloadPath;
            this.Cookies.Text = userCookies;
            this.defaultCookies.IsChecked = isDefaultCookies;
            this.Cookies.IsEnabled = !isDefaultCookies;
            this.Plaform.SelectedIndex = platformIndex;
            searchUrl = platformHost[platformIndex] + searchUrlArg;
            if (!isDefaultCookies)
                cookies = userCookies;
        }

        private void dirButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            this.downloadDir.Text = fbd.SelectedPath;
        }

        private void refreshDB_Click(object sender, RoutedEventArgs e)
        {
            BookDataBase.GetInstance().addNewItem(this.ssid.Text, this.str.Text);
        }
		
        private void defaultCookies_Click(object sender, RoutedEventArgs e)
        {
            bool ischecked = !(bool)this.defaultCookies.IsChecked;
            this.Cookies.IsEnabled = ischecked;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            writeConfigureFile();
            readConfigureFile();
        }

    }
}
