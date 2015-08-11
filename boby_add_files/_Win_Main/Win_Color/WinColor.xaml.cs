using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Ini;
using System.Xml;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace boby_add_files
{
    /// <summary>
    /// Logique d'interaction pour WinColor.xaml
    /// </summary>
    public partial class WinColor : Window
    {
        IniFile Config = null;
        MainWindow in_main_win = null;

        public WinColor(MainWindow in_main)
        {
            InitializeComponent();

            Config = new IniFile("./boby_add_file.ini");

            tb_red.Text = Config.IniReadValue("boby", "red");
            tb_green.Text = Config.IniReadValue("boby", "green");
            tb_blue.Text = Config.IniReadValue("boby", "blue");

            if (tb_red.Text == "")
                tb_red.Text = "255";
            if (tb_green.Text == "")
                tb_green.Text = "0";
            if (tb_blue.Text == "")
                tb_blue.Text = "0";
            if (Convert.ToInt32(tb_red.Text.Trim()) > 255)
                tb_red.Text = "255";
            if (Convert.ToInt32(tb_green.Text.Trim()) > 255)
                tb_green.Text = "255";
            if (Convert.ToInt32(tb_blue.Text.Trim()) > 255)
                tb_blue.Text = "255";

            var bc = new BrushConverter();
            rt_color.Fill = (Brush)bc.ConvertFrom("#FF" + Convert.ToInt32(tb_red.Text.Trim()).ToString("X2") + Convert.ToInt32(tb_green.Text.Trim()).ToString("X2") + Convert.ToInt32(tb_blue.Text.Trim()).ToString("X2"));
            in_main_win = in_main;
        }

        private void update_color()
        {
            if (tb_red.Text == "")
                tb_red.Text = "0";
            if (tb_green.Text == "")
                tb_green.Text = "0";
            if (tb_blue.Text == "")
                tb_blue.Text = "0";
            if (Convert.ToInt32(tb_red.Text.Trim()) > 255)
                tb_red.Text = "255";
            if (Convert.ToInt32(tb_green.Text.Trim()) > 255)
                tb_green.Text = "255";
            if (Convert.ToInt32(tb_blue.Text.Trim()) > 255)
                tb_blue.Text = "255";
            var bc = new BrushConverter();
            rt_color.Fill = (Brush)bc.ConvertFrom("#FF" + Convert.ToInt32(tb_red.Text.Trim()).ToString("X2") + Convert.ToInt32(tb_green.Text.Trim()).ToString("X2") + Convert.ToInt32(tb_blue.Text.Trim()).ToString("X2"));
        }

        private void rt_Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void bt_Close_Click(object sender, RoutedEventArgs e)
        {
            in_main_win.cb_color.IsChecked = false;
            this.Close();
        }

        private void bt_ok_Click(object sender, RoutedEventArgs e)
        {
            Config.IniWriteValue("boby", "red", tb_red.Text);
            Config.IniWriteValue("boby", "green", tb_green.Text);
            Config.IniWriteValue("boby", "blue", tb_blue.Text);

            in_main_win.color_launch();

            this.Close();
        }

        private void tb_red_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            update_color();
        }

        private void tb_green_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            update_color();
        }

        private void tb_blue_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            update_color();
        }
    }
}
