using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;

using Microsoft.Win32;

using System.Xml;
using System.IO.Compression;


using Ini;

namespace boby_add_files
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow   in_Win_Main                   = null;
		public Style        Style_ShugoLoading            = null;
		public string       g_path                        = "";
		public string       g_red                         = "";
		public string       g_green                       = "";
		public string       g_blue                        = "";

        public Thread       g_thread                      = null;

        public bool         start                         = true;

		public MainWindow()
		{
			InitializeComponent();
			in_Win_Main = this;

			tb_aion_dir.Text = "Check Update";

			IniFile Config = new IniFile("./boby_add_file.ini");

			string path = Config.IniReadValue("boby", "path");
			string type = Config.IniReadValue("boby", "type");
			string other = Config.IniReadValue("boby", "other");
			string lang = Config.IniReadValue("boby", "lang");

            if (VerifPathAion(path))
			{
				g_path = path;
			}
			else if (VerifPathAion(VerifRegAion()))
			{
				g_path = VerifRegAion();
				Config.IniWriteValue("boby", "path", g_path);
			}

            if (type == "NC")
            {
                rb_NC.IsChecked = true;
            }
            else
            {
                rb_GF.IsChecked = true;
            }

            if (other != "")
                tb_other.Text = other.Replace("<br/>", "\n");
            else
                tb_other.Text = "-ncping -noweb -noauthgg -st\n-charnamemenu -litelauncher\n-ingamebrowser -webshopevent:0\n-f2p -lbox";

            if (g_path != "")
            {
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(g_path + @"\L10N"));

                List<string> dir_name_list = new List<string>();

                foreach (var dir in dirs)
                {
                    string dir_name = dir.Substring(dir.LastIndexOf("\\") + 1);
                    dir_name_list.Add(dir_name);
                }

                this.cb_lang.ItemsSource = dir_name_list;

                if (lang != "" && dir_name_list.Contains(lang))
                    this.cb_lang.SelectedItem = lang;
                else
                    this.cb_lang.SelectedIndex = 0;
            }

            VerifOption();
			start = false;
        }

        private void VerifOption()
		{
			string path = tb_aion_dir.Text;

            IniFile Config = new IniFile("./boby_add_file.ini");
            string lang = Config.IniReadValue("boby", "lang");

            bool fly = false;
            bool anim = false;
            bool color = false;
            bool voice = false;
            bool rank = false;

            if (System.IO.File.Exists(path + @"\Data\flightpath\FlightPath.pak_boby"))
			{
                fly = true;
			}

            if (System.IO.File.Exists(path + @"\Objects\pc\df\mesh\Mesh_Meshes_000.pak_boby") &&
                System.IO.File.Exists(path + @"\Objects\pc\dm\mesh\Mesh_Meshes_000.pak_boby") &&
                System.IO.File.Exists(path + @"\Objects\pc\lf\mesh\Mesh_Meshes_000.pak_boby") &&
                System.IO.File.Exists(path + @"\Objects\pc\lm\mesh\Mesh_Meshes_000.pak_boby"))
            {
                anim = true;
            }

            if (System.IO.File.Exists(path + @"\Data\ui\ui.pak_boby"))
            {
                color = true;
            }

            string origin_dir_path = path + @"\L10N";

            List<string> dirs = new List<string>(Directory.EnumerateDirectories(origin_dir_path));

            List<string> dir_name_list = new List<string>();

            foreach (var dir in dirs)
            {
                string dir_name = dir.Substring(dir.LastIndexOf("\\") + 1);
                dir_name_list.Add(dir_name); 

                if (File.Exists(path + @"\L10N\" + dir_name + @"\sounds\voice\attack\attack.pak_boby"))
                    voice = true;
                if (File.Exists(path + @"\L10N\" + dir_name + @"\data\data.pak_boby"))
                    rank = true;
			}

            this.cb_lang.ItemsSource = dir_name_list;

            if (lang != "" && dir_name_list.Contains(lang))
                this.cb_lang.SelectedItem = lang;
            else
                this.cb_lang.SelectedIndex = 0;

            this.cb_fly.IsChecked = fly;
            this.cb_anim.IsChecked = anim;
            this.cb_color.IsChecked = color;
            this.cb_voice.IsChecked = voice;
            this.cb_rank.IsChecked = rank;
        }

		private string VerifRegAion()
		{
			RegistryKey Nkey = Registry.LocalMachine;
			string BaseDirAion = "";

			try
			{
                RegistryKey keyNC = Nkey.OpenSubKey(@"Software\Wow6432Node\NCWest\AION", true);
                try
                {
                    BaseDirAion = (string)keyNC.GetValue("BaseDir").ToString();
                    rb_NC.IsChecked = true;
                }
                finally
                {
                    keyNC.Close();
                }
                RegistryKey keyGF = Nkey.OpenSubKey(@"Software\Wow6432Node\GameForge\AION-LIVE", true);
				try
				{
					BaseDirAion = (string)keyGF.GetValue("BaseDir").ToString();
                    rb_GF.IsChecked = true;
                }
				finally
				{
                    keyGF.Close();
				}
			}
			catch (Exception)
			{
				//---
			}
			finally
			{
				Nkey.Close();
			}
			return BaseDirAion;
		}

		private bool VerifPathAion(string dirPath)
		{
			try
			{
				string[] files = System.IO.Directory.GetDirectories(dirPath);
				foreach (string j in files)
				{
					if (System.IO.Path.GetFileName(j.ToLower()) == "bin32")
					{
						tb_aion_dir.Text = dirPath;
						tb_aion_dir.SelectionStart = tb_aion_dir.Text.Length - 1;

						cb_fly.IsEnabled = true;
						cb_voice.IsEnabled = true;
						cb_rank.IsEnabled = true;
						cb_anim.IsEnabled = true;
						cb_color.IsEnabled = true;

                        start = true;
                        VerifOption();
                        start = false;

                        return true;
					}
				}
			}
			catch
			{}
			return false;
		}

		private void Launch_Aion(string type)
		{
			string path = tb_aion_dir.Text;

            string parameters = "";

            parameters = tb_other.Text.Replace("\n", " ");
            if (!tb_other.Text.Contains("-ip"))
            {
                if (rb_NC.IsChecked == true)
                    parameters = "-ip:64.25.35.103 -port:2106 -cc:1 -loginex -pwd16" + parameters;
                else
                    parameters = "-ip:79.110.83.80 -port:2106 " + parameters;
            }

            if (!tb_other.Text.Contains("-lang"))
            {
                parameters += " -lang:" + cb_lang.Text;
            }

            try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.CreateNoWindow = false;
				startInfo.UseShellExecute = false;
				startInfo.FileName = path + @"\bin" + type + @"\aion.bin";
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				startInfo.Arguments = parameters;
				Process.Start(startInfo);
				this.Close();
			}
			catch (Exception)
			{
				System.Windows.MessageBox.Show("Erreur de lancement");
			}
		}

		#region _Event gui_
		private bool In_Close = false;

		public void Full_Close()
		{
			if (In_Close == false)
			{
				In_Close = true;
				Environment.Exit(0);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Full_Close();
		}

		private void Bt_Close_Click(object sender, RoutedEventArgs e)
		{
			Full_Close();
		}

		private void Bt_Minimise_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void Rt_Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}
		#endregion

		private void bt_bin32_Click(object sender, RoutedEventArgs e)
		{
			Launch_Aion("32");
		}

		private void bt_bin64_Click(object sender, RoutedEventArgs e)
		{
			Launch_Aion("64");
		}

		private void TextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "aion.bin"; // Default file extension
            dlg.Filter = " |aion.bin"; // Filter files by extension
            //FolderBrowserDialog fbd = new FolderBrowserDialog();
            //fbd.Description = "Select the directory of AION (with the folders \"Bin32\" and \"Bin64\").";
            //fbd.ShowNewFolderButton = false;
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string path = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(dlg.FileName)).ToString();
                if (!VerifPathAion(path))
                {
                    System.Windows.MessageBox.Show("It's not aion dir!");
                }
                else
                {
                    g_path = path;
                    IniFile Config = new IniFile("./boby_add_file.ini");
                    Config.IniWriteValue("boby", "path", g_path);
                }
			}
		}

        private void DisableAllCB()
        {
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                cb_anim.IsEnabled = false;
                cb_color.IsEnabled = false;
                cb_fly.IsEnabled = false;
                cb_rank.IsEnabled = false;
                cb_voice.IsEnabled = false;
                bt_bin32.IsEnabled = false;
                bt_bin64.IsEnabled = false;
            }));
        }
        private void EnableAllCB()
        {
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                cb_anim.IsEnabled = true;
                cb_color.IsEnabled = true;
                cb_fly.IsEnabled = true;
                cb_rank.IsEnabled = true;
                cb_voice.IsEnabled = true;
                bt_bin32.IsEnabled = true;
                bt_bin64.IsEnabled = true;
            }));
        }

        private void cb_fly_Checked(object sender, RoutedEventArgs e)
		{
            if (!start && (g_thread == null || !g_thread.IsAlive))
            {
                g_thread = new Thread(_FlightPath);
                g_thread.Start();
            }
        }

		private void cb_fly_Unchecked(object sender, RoutedEventArgs e)
		{
            if (!start)
            {
                string path = tb_aion_dir.Text;
                string file_path = path + @"\Data\FlightPath\FlightPath.pak_boby";

                try
                {
                    if (File.Exists(file_path))
                        File.Delete(file_path);
                }
                catch
                {
                    System.Windows.MessageBox.Show("File open in other application.", "Error");
                    start = true;
                    cb_fly.IsChecked = true;
                    start = false;
                }
            }
		}

		private void cb_voice_Checked(object sender, RoutedEventArgs e)
		{
            if (!start && (g_thread == null || !g_thread.IsAlive))
            {
                g_thread = new Thread(_Voices);
                g_thread.Start();
            }
        }

		private void cb_voice_Unchecked(object sender, RoutedEventArgs e)
		{
            if (!start)
            {
                string path = tb_aion_dir.Text;
                string origin_dir_path = path + @"\L10N";

                List<string> dirs = new List<string>(Directory.EnumerateDirectories(origin_dir_path));

                foreach (var dir in dirs)
                {
                    string dir_name = dir.Substring(dir.LastIndexOf("\\") + 1);

                    try
                    {
                        if (File.Exists(path + @"\L10N\" + dir_name + @"\sounds\voice\attack\attack.pak_boby"))
                            File.Delete(path + @"\L10N\" + dir_name + @"\sounds\voice\attack\attack.pak_boby");
                        if (File.Exists(path + @"\L10N\" + dir_name + @"\sounds\voice\cast\cast.pak_boby"))
                            File.Delete(path + @"\L10N\" + dir_name + @"\sounds\voice\cast\cast.pak_boby");
                        if (File.Exists(path + @"\L10N\" + dir_name + @"\sounds\voice\damage\damage.pak_boby"))
                            File.Delete(path + @"\L10N\" + dir_name + @"\sounds\voice\damage\damage.pak_boby");
                        if (File.Exists(path + @"\L10N\" + dir_name + @"\sounds\voice\defence\defence.pak_boby"))
                            File.Delete(path + @"\L10N\" + dir_name + @"\sounds\voice\defence\defence.pak_boby");
                        if (File.Exists(path + @"\L10N\" + dir_name + @"\sounds\voice\login\login.pak_boby"))
                            File.Delete(path + @"\L10N\" + dir_name + @"\sounds\voice\login\login.pak_boby");
                        if (File.Exists(path + @"\L10N\" + dir_name + @"\sounds\voice\social\social.pak_boby"))
                            File.Delete(path + @"\L10N\" + dir_name + @"\sounds\voice\social\social.pak_boby");
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("File open in other application.", "Error");
                        start = true;
                        cb_voice.IsChecked = true;
                        start = false;
                        return;
                    }
                }
            }
		}

		private void cb_rank_Checked(object sender, RoutedEventArgs e)
		{
            if (!start && (g_thread == null || !g_thread.IsAlive))
            {
                g_thread = new Thread(_Rank);
                g_thread.Start();
            }
        }

		private void cb_rank_Unchecked(object sender, RoutedEventArgs e)
		{
            if (!start)
            {
                string path = tb_aion_dir.Text;
                string origin_dir_path = path + @"\L10N";

                List<string> dirs = new List<string>(Directory.EnumerateDirectories(origin_dir_path));

                foreach (var dir in dirs)
                {
                    string dir_name = dir.Substring(dir.LastIndexOf("\\") + 1);

                    try
                    {
                        if (File.Exists(path + @"\L10N\" + dir_name + @"\data\data.pak_boby"))
                            File.Delete(path + @"\L10N\" + dir_name + @"\data\data.pak_boby");
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("File open in other application.", "Error");
                        start = true;
                        cb_rank.IsChecked = true;
                        start = false;
                        return;
                    }
                }
            }
		}

		private void cb_anim_Checked(object sender, RoutedEventArgs e)
		{
            if (!start && (g_thread == null || !g_thread.IsAlive))
            {
                g_thread = new Thread(_Anim);
                g_thread.Start();
            }
        }

		private void cb_anim_Unchecked(object sender, RoutedEventArgs e)
		{
            if (!start)
            {
                string path = tb_aion_dir.Text;
                string file_path1 = path + @"\Objects\pc\df\mesh\Mesh_Meshes_000.pak_boby";
                string file_path2 = path + @"\Objects\pc\dm\mesh\Mesh_Meshes_000.pak_boby";
                string file_path3 = path + @"\Objects\pc\lf\mesh\Mesh_Meshes_000.pak_boby";
                string file_path4 = path + @"\Objects\pc\lm\mesh\Mesh_Meshes_000.pak_boby";

                try
                {
                    if (File.Exists(file_path1))
                        File.Delete(file_path1);
                    if (File.Exists(file_path2))
                        File.Delete(file_path2);
                    if (File.Exists(file_path3))
                        File.Delete(file_path3);
                    if (File.Exists(file_path4))
                        File.Delete(file_path4);
                }
                catch
                {
                    System.Windows.MessageBox.Show("File open in other application.", "Error");
                    cb_anim.IsChecked = true;
                }
            }
		}

		private void cb_color_Checked(object sender, RoutedEventArgs e)
		{
			if (!start)
			{
				string path = tb_aion_dir.Text;

				if (!System.IO.File.Exists(path + @"\Data\ui\ui.pak_boby"))
				{
					WinColor win_color = new WinColor(this);
					win_color.Show();
				}
			}
		}

        public void color_launch()
        {
            if (!start && (g_thread == null || !g_thread.IsAlive))
            {
                g_thread = new Thread(_Color);
                g_thread.Start();
            }
        }

		private void cb_color_Unchecked(object sender, RoutedEventArgs e)
		{
			string path = tb_aion_dir.Text;
			string file_path = path + @"\Data\ui\ui.pak_boby";

			try
			{
				if (File.Exists(file_path))
					File.Delete(file_path);
			}
			catch
			{
				System.Windows.MessageBox.Show("File open in other application.", "Error");
				cb_color.IsChecked = true;
			}

			//this.pb_color.Value = 0;
		}

        private void rb_GF_Checked(object sender, RoutedEventArgs e)
        {
            IniFile Config = new IniFile("./boby_add_file.ini");
            Config.IniWriteValue("boby", "type", "GF");
        }

        private void rb_NC_Checked(object sender, RoutedEventArgs e)
        {
            IniFile Config = new IniFile("./boby_add_file.ini");
            Config.IniWriteValue("boby", "type", "NC");
        }

        private void tb_other_TextChanged(object sender, TextChangedEventArgs e)
        {
            IniFile Config = new IniFile("./boby_add_file.ini");
            Config.IniWriteValue("boby", "other", tb_other.Text.Replace("\n", "<br/>"));
        }

        private void cb_lang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!start)
            {
                IniFile Config = new IniFile("./boby_add_file.ini");
                if (cb_lang.SelectedItem != null)
                    Config.IniWriteValue("boby", "lang", cb_lang.SelectedItem.ToString());
            }
        }
    }
}
