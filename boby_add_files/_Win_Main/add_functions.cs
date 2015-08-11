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
    public partial class MainWindow
    {
        private void info_text(string txt)
        {
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                tb_aion_dir.Text = txt;
            }));
        }

        private void _FlightPath()
        {
            DisableAllCB();
            string path = "";
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                path = tb_aion_dir.Text;
            }));
            string origin_file_path = path + @"\Data\FlightPath\FlightPath.pak";
            string mod_file_path = path + @"\Data\FlightPath\FlightPath.pak_boby";
            string zip_file_path = System.Windows.Forms.Application.StartupPath + @"\FlightPath.zip";
            string tmp_dir_path = System.Windows.Forms.Application.StartupPath + @"\tmp";
            string mod_dir_path = System.Windows.Forms.Application.StartupPath + @"\mod";

            info_text("remove tmp file...");

            try
            {
                Directory.Delete(mod_dir_path, true);
                Directory.Delete(tmp_dir_path, true);
            }
            catch
            { }

            info_text("move file...");

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\pak2zip.exe";
            process.StartInfo.Arguments = "\"" + origin_file_path + "\" \"" + zip_file_path + "\"";
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            info_text("extract file...");

            Directory.CreateDirectory(tmp_dir_path);
            using (ZipArchive zip = ZipFile.Open(zip_file_path, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                    if (entry.Name == "fly_path.xml")
                        entry.ExtractToFile(tmp_dir_path + @"\fly_path.xml");
            Directory.CreateDirectory(mod_dir_path);

            info_text("xml convert...");

            Process processXml = new Process();
            processXml.StartInfo.UseShellExecute = false;
            processXml.StartInfo.RedirectStandardOutput = false;
            processXml.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\Gibbed.Aion.ConvertXml.exe";
            processXml.StartInfo.Arguments = "\"" + tmp_dir_path + @"\fly_path.xml" + "\" \"" + mod_dir_path + @"\fly_path.xml" + "\"";
            processXml.StartInfo.CreateNoWindow = true;
            processXml.Start();
            processXml.WaitForExit();

            info_text("seq file create...");

            (new Thread(() =>
            {
                try
                {
                    File.Delete(zip_file_path);
                    Directory.Delete(tmp_dir_path, true);
                }
                catch
                { }
            }
            )).Start();

            List<string> filesList = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(mod_dir_path + @"\fly_path.xml");

            XmlNodeList xmlnode = xmlDoc.GetElementsByTagName("path_group");

            foreach (XmlNode v1node in xmlnode)
            {
                string file_name = "";
                string start_x = "";
                string start_y = "";
                string start_z = "";
                string end_x = "";
                string end_y = "";
                string end_z = "";

                XmlNodeList v2xmlnode = v1node.ChildNodes;
                foreach (XmlNode v2node in v2xmlnode)
                {
                    if (v2node.Name == "path")
                    {
                        XmlNodeList v3xmlnode = v2node.ChildNodes;
                        foreach (XmlNode v3node in v3xmlnode)
                        {
                            if (v3node.Name == "file")
                            {
                                file_name = v3node.InnerText;
                            }
                        }
                    }

                    if (v2node.Name == "start")
                    {
                        XmlNodeList v3xmlnode = v2node.ChildNodes;
                        foreach (XmlNode v3node in v3xmlnode)
                        {
                            if (v3node.Name == "x")
                            {
                                start_x = v3node.InnerText;
                            }
                            if (v3node.Name == "y")
                            {
                                start_y = v3node.InnerText;
                            }
                            if (v3node.Name == "z")
                            {
                                start_z = v3node.InnerText;
                            }
                        }
                    }

                    if (v2node.Name == "end")
                    {
                        XmlNodeList v3xmlnode = v2node.ChildNodes;
                        foreach (XmlNode v3node in v3xmlnode)
                        {
                            if (v3node.Name == "x")
                            {
                                end_x = v3node.InnerText;
                            }
                            if (v3node.Name == "y")
                            {
                                end_y = v3node.InnerText;
                            }
                            if (v3node.Name == "z")
                            {
                                end_z = v3node.InnerText;
                            }
                        }
                    }
                }

                if (file_name != "" && !file_name.Contains("_Q"))
                {
                    string file_content = "";
                    file_content += "<Sequence EndTime=\"0\" Flags=\"0\" Name=\"" + file_name + "\" StartTime=\"0\" TextFile=\"\" >";
                    file_content += "<Nodes>";
                    file_content += "<Node EntityClass=\"BasicEntity\" EntityClassId=\"4\" Pos=\"" + start_x + "," + start_y + "," + start_z + "\" GroupName=\"default\" Id=\"-1256010431\" Name=\"SoloSelf\" NodeClass=\"0\" Scale=\"1,1,1\" Type=\"1\" >";
                    file_content += "<Track Flags=\"0\" ParamId=\"1\">";
                    file_content += "<Key bias=\"1\" cont=\"-1\" ignorePhys=\"1\" time=\"0\" value=\"" + end_x + "," + end_y + "," + end_z + "\" />";
                    file_content += "</Track>";
                    file_content += "</Node>";
                    file_content += "</Nodes>";
                    file_content += "</Sequence>";

                    using (FileStream fs = File.Create(mod_dir_path + "\\" + file_name))
                    {
                        byte[] content = new UTF8Encoding(true).GetBytes(file_content);
                        fs.Write(content, 0, content.Length);
                    }

                    filesList.Add(file_name);
                }
            }

            info_text("pak file create...");

            if (!File.Exists(mod_file_path))
            {
                using (ZipArchive modFile = ZipFile.Open(mod_file_path, ZipArchiveMode.Create))
                {
                    foreach (var filename in filesList)
                    {
                        modFile.CreateEntryFromFile(mod_dir_path + "\\" + filename, filename, CompressionLevel.Fastest);
                    }
                }
            }

            try
            {
                Directory.Delete(mod_dir_path, true);
                Directory.Delete(tmp_dir_path, true);
            }
            catch
            { }

            info_text(path);

            EnableAllCB();
        }

        private void _Voices()
        {
            DisableAllCB();
            string path = "";
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                path = tb_aion_dir.Text;
            }));
            string origin_dir_path = path + @"\L10N";

            info_text("search langage...");

            List<string> dirs = new List<string>(Directory.EnumerateDirectories(origin_dir_path));

            foreach (var dir in dirs)
            {
                string dir_name = dir.Substring(dir.LastIndexOf("\\") + 1);

                info_text("appli in " + dir_name + " langage...");

                System.IO.File.Copy(path + @"\Sounds\voice\attack\attack.pak", path + @"\L10N\" + dir_name + @"\sounds\voice\attack\attack.pak_boby", true);
                System.IO.File.Copy(path + @"\Sounds\voice\cast\cast.pak", path + @"\L10N\" + dir_name + @"\sounds\voice\cast\cast.pak_boby", true);
                System.IO.File.Copy(path + @"\Sounds\voice\damage\damage.pak", path + @"\L10N\" + dir_name + @"\sounds\voice\damage\damage.pak_boby", true);
                System.IO.File.Copy(path + @"\Sounds\voice\defence\defence.pak", path + @"\L10N\" + dir_name + @"\sounds\voice\defence\defence.pak_boby", true);
                System.IO.File.Copy(path + @"\Sounds\voice\login\login.pak", path + @"\L10N\" + dir_name + @"\sounds\voice\login\login.pak_boby", true);
                System.IO.File.Copy(path + @"\Sounds\voice\social\social.pak", path + @"\L10N\" + dir_name + @"\sounds\voice\social\social.pak_boby", true);
            }

            info_text(path);
            EnableAllCB();
        }

        private void _Rank()
        {
            DisableAllCB();
            string path = "";
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                path = tb_aion_dir.Text;
            }));
            string origin_dir_path = path + @"\L10N";

            info_text("search langage...");

            List<string> dirs = new List<string>(Directory.EnumerateDirectories(origin_dir_path));

            foreach (var dir in dirs)
            {
                string dir_name = dir.Substring(dir.LastIndexOf("\\") + 1);

                info_text("extract " + dir_name + " data...");

                string origin_file_path = path + @"\L10N\" + dir_name + @"\data\data.pak";
                string mod_file_path = path + @"\L10N\" + dir_name + @"\data\data.pak_boby";
                string zip_file_path = System.Windows.Forms.Application.StartupPath + @"\data.zip";
                string tmp_dir_path = System.Windows.Forms.Application.StartupPath + @"\tmp";
                string mod_dir_path = System.Windows.Forms.Application.StartupPath + @"\mod";

                info_text(dir_name + " remove tmp file...");

                try
                {
                    Directory.Delete(mod_dir_path, true);
                    Directory.Delete(tmp_dir_path, true);
                }
                catch
                { }

                info_text(dir_name + " move file...");

                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\pak2zip.exe";
                process.StartInfo.Arguments = "\"" + origin_file_path + "\" \"" + zip_file_path + "\"";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();

                info_text(dir_name + " extract file...");

                Directory.CreateDirectory(tmp_dir_path);
                using (ZipArchive zip = ZipFile.Open(zip_file_path, ZipArchiveMode.Read))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                        if (entry.Name == "client_strings_ui.xml")
                            entry.ExtractToFile(tmp_dir_path + @"\client_strings_ui.xml");
                Directory.CreateDirectory(mod_dir_path);

                info_text(dir_name + " xml convert...");

                Process processXml = new Process();
                processXml.StartInfo.UseShellExecute = false;
                processXml.StartInfo.RedirectStandardOutput = false;
                processXml.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\Gibbed.Aion.ConvertXml.exe";
                processXml.StartInfo.Arguments = "\"" + tmp_dir_path + @"\client_strings_ui.xml" + "\" \"" + mod_dir_path + @"\client_strings_ui.xml" + "\"";
                processXml.StartInfo.CreateNoWindow = true;
                processXml.Start();
                processXml.WaitForExit();

                (new Thread(() =>
                {
                    try
                    {
                        File.Delete(zip_file_path);
                        Directory.Delete(tmp_dir_path, true);
                    }
                    catch
                    { }
                }
                )).Start();

                info_text(dir_name + " xml modification...");

                string star = ('\uE01F').ToString();
                string gene = ('\uE072').ToString();
                string ggen = ('\uE075').ToString();
                string comm = ('\uE077').ToString();
                string gouv = ('\uE005').ToString();

                string star1 = star;
                string star2 = star + star;
                string star3 = star + star + star;
                string star4 = star + star + star + star;
                string star5 = star + star + star + star + star;

                string general = gene + gene + gene + gene + gene + gene;
                string ggeneral = ggen + ggen + ggen + ggen + ggen + ggen + ggen;
                string commandant = comm + comm + comm + comm + comm + comm + comm + comm;
                string gouverneur = gouv + gouv + gouv + gouv + gouv + gouv + gouv + gouv + gouv;

                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(mod_dir_path + @"\client_strings_ui.xml");

                XmlNodeList xmlnode = xmlDoc.GetElementsByTagName("string");

                foreach (XmlNode v1node in xmlnode)
                {
                    XmlNodeList v2xmlnode = v1node.ChildNodes;
                    foreach (XmlNode v2node in v2xmlnode)
                    {
                        if (v2node.Name == "name")
                        {
                            switch (v2node.InnerText)
                            {
                                case "STR_MACRO_SHORTCUT":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = "/Q";
                                    break;
                                case "STR_MACRO_DELAY":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = "/D";
                                    break;
                                case "STR_MACRO_SETVARIABLE":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = "/V";
                                    break;
                                case "STR_MACRO_OPTION_VARIABLE":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = "[V%0]";
                                    break;
                                case "STR_Abyssrank_light_10":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star1;
                                    break;
                                case "STR_Abyssrank_light_11":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star2;
                                    break;
                                case "STR_Abyssrank_light_12":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star3;
                                    break;
                                case "STR_Abyssrank_light_13":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star4;
                                    break;
                                case "STR_Abyssrank_light_14":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star5;
                                    break;
                                case "STR_Abyssrank_light_15":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = general;
                                    break;
                                case "STR_Abyssrank_light_16":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = ggeneral;
                                    break;
                                case "STR_Abyssrank_light_17":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = commandant;
                                    break;
                                case "STR_Abyssrank_light_18":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = gouverneur;
                                    break;
                                case "STR_Abyssrank_dark_10":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star1;
                                    break;
                                case "STR_Abyssrank_dark_11":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star2;
                                    break;
                                case "STR_Abyssrank_dark_12":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star3;
                                    break;
                                case "STR_Abyssrank_dark_13":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star4;
                                    break;
                                case "STR_Abyssrank_dark_14":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = star5;
                                    break;
                                case "STR_Abyssrank_dark_15":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = general;
                                    break;
                                case "STR_Abyssrank_dark_16":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = ggeneral;
                                    break;
                                case "STR_Abyssrank_dark_17":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = commandant;
                                    break;
                                case "STR_Abyssrank_dark_18":
                                    foreach (XmlNode v3node in v2xmlnode)
                                        if (v3node.Name == "body")
                                            v3node.InnerText = gouverneur;
                                    break;
                            }
                        }
                    }
                }
                xmlDoc.Save(mod_dir_path + @"\client_strings_ui.xml");

                info_text(dir_name + " pak file create...");

                if (!File.Exists(mod_file_path))
                {
                    using (ZipArchive modFile = ZipFile.Open(mod_file_path, ZipArchiveMode.Create))
                    {
                        modFile.CreateEntry("strings/");
                        modFile.CreateEntryFromFile(mod_dir_path + "\\client_strings_ui.xml", "strings\\client_strings_ui.xml", CompressionLevel.Fastest);
                    }
                }

                try
                {
                    Directory.Delete(mod_dir_path, true);
                    Directory.Delete(tmp_dir_path, true);
                }
                catch
                { }
            }

            info_text(path);
            EnableAllCB();
        }

        private string replace_line(string origin_line, string[] search)
        {
            string line = origin_line;

            if (search.Any(x => origin_line.Contains(x)))
                line = "//#Comment By Boby#//" + line;

            return line;
        }

        private string[] weapon_search(string s1, string s2)
        {
            string[] weapon =
            {
                s1 + "noweapon" + s2,
                s1 + "1hand" + s2,
                s1 + "mace" + s2,
                s1 + "dagger" + s2,
                s1 + "orb" + s2,
                s1 + "book" + s2,
                s1 + "2hand" + s2,
                s1 + "polearm" + s2,
                s1 + "staff" + s2,
                s1 + "2weapon" + s2,
                s1 + "bow" + s2,
                s1 + "harp" + s2,
                s1 + "1gun" + s2,
                s1 + "2gun" + s2,
                s1 + "cannon" + s2,
                s1 + "keyblade" + s2
            };

            return weapon;
        }

        private void _Decompress(string cal)
        {
            
        }

        private void _Anim()
        {
            DisableAllCB();
            string path = "";
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                path = tb_aion_dir.Text;
            }));

            string origin_file_path_0 = path + @"\objects\pc\df\mesh\Mesh_Meshes_000.pak";
            string origin_file_path_1 = path + @"\objects\pc\lf\mesh\Mesh_Meshes_000.pak";
            string origin_file_path_2 = path + @"\objects\pc\dm\mesh\Mesh_Meshes_000.pak";
            string origin_file_path_3 = path + @"\objects\pc\lm\mesh\Mesh_Meshes_000.pak";
            string mod_file_path_0 = path + @"\objects\pc\df\mesh\Mesh_Meshes_000.pak_boby";
            string mod_file_path_1 = path + @"\objects\pc\lf\mesh\Mesh_Meshes_000.pak_boby";
            string mod_file_path_2 = path + @"\objects\pc\dm\mesh\Mesh_Meshes_000.pak_boby";
            string mod_file_path_3 = path + @"\objects\pc\lm\mesh\Mesh_Meshes_000.pak_boby";
            string zip_file_path_0 = System.Windows.Forms.Application.StartupPath + @"\Mesh_Meshes_000_0.zip";
            string zip_file_path_1 = System.Windows.Forms.Application.StartupPath + @"\Mesh_Meshes_000_1.zip";
            string zip_file_path_2 = System.Windows.Forms.Application.StartupPath + @"\Mesh_Meshes_000_2.zip";
            string zip_file_path_3 = System.Windows.Forms.Application.StartupPath + @"\Mesh_Meshes_000_3.zip";
            string tmp_dir_path = System.Windows.Forms.Application.StartupPath + @"\tmp";
            string mod_dir_path = System.Windows.Forms.Application.StartupPath + @"\mod";

            info_text("remove tmp file...");

            try
            {
                Directory.Delete(mod_dir_path, true);
                Directory.Delete(tmp_dir_path, true);
            }
            catch
            { }

            info_text("move file...");

            Thread t00 = new Thread(() =>
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\pak2zip.exe";
                process.StartInfo.Arguments = "\"" + origin_file_path_0 + "\" \"" + zip_file_path_0 + "\"";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            });
            Thread t01 = new Thread(() =>
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\pak2zip.exe";
                process.StartInfo.Arguments = "\"" + origin_file_path_1 + "\" \"" + zip_file_path_1 + "\"";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            });
            Thread t02 = new Thread(() =>
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\pak2zip.exe";
                process.StartInfo.Arguments = "\"" + origin_file_path_2 + "\" \"" + zip_file_path_2 + "\"";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            });
            Thread t03 = new Thread(() =>
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\pak2zip.exe";
                process.StartInfo.Arguments = "\"" + origin_file_path_3 + "\" \"" + zip_file_path_3 + "\"";
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            });

            t00.Start();
            t01.Start();
            t02.Start();
            t03.Start();

            t00.Join();
            t01.Join();
            t02.Join();
            t03.Join();

            info_text("extract file...");

            Directory.CreateDirectory(tmp_dir_path);

            Thread t10 = new Thread(() =>
            {
                using (ZipArchive zip = ZipFile.Open(zip_file_path_0, ZipArchiveMode.Read))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                        if (entry.Name == "df.cal")
                            entry.ExtractToFile(tmp_dir_path + @"\df.cal");
            });

            Thread t11 = new Thread(() =>
            {
                using (ZipArchive zip = ZipFile.Open(zip_file_path_1, ZipArchiveMode.Read))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                        if (entry.Name == "lf.cal")
                            entry.ExtractToFile(tmp_dir_path + @"\lf.cal");
            });

            Thread t12 = new Thread(() =>
            {
                using (ZipArchive zip = ZipFile.Open(zip_file_path_2, ZipArchiveMode.Read))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                        if (entry.Name == "dm.cal")
                            entry.ExtractToFile(tmp_dir_path + @"\dm.cal");
            });

            Thread t13 = new Thread(() =>
            {
                using (ZipArchive zip = ZipFile.Open(zip_file_path_3, ZipArchiveMode.Read))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                        if (entry.Name == "lm.cal")
                            entry.ExtractToFile(tmp_dir_path + @"\lm.cal");
            });

            t10.Start();
            t11.Start();
            t12.Start();
            t13.Start();

            t10.Join();
            t11.Join();
            t12.Join();
            t13.Join();

            (new Thread(() =>
            {
                try
                {
                    File.Delete(zip_file_path_0);
                }
                catch
                { }
            }
            )).Start();
            (new Thread(() =>
            {
                try
                {
                    File.Delete(zip_file_path_1);
                }
                catch
                { }
            }
            )).Start();
            (new Thread(() =>
            {
                try
                {
                    File.Delete(zip_file_path_2);
                }
                catch
                { }
            }
            )).Start();
            (new Thread(() =>
            {
                try
                {
                    File.Delete(zip_file_path_3);
                }
                catch
                { }
            }
            )).Start();

            info_text("mod file...");

            Directory.CreateDirectory(mod_dir_path);

            string[] base_search =
            {
                "nuseitem_succ_drinkcoke_run_001",
                "nuseitem_succ_drinkGrape_run_001",
                "nuseitem_succ_useup_001",
                "nuseitem_succ_useup_run_001",
                "nuseitem_succ_eat_001",
                "nuseitem_succ_eat_run_001",
                "nuseitem_succ_drink_001",
                "nuseitem_succ_drink_run_001",
                "Nuseitem_common_001",
                "Nuseitem_ride_001",
                "Nuseitem_succ_common_001",
                "Fuseitem_common_001",
                "Fuseitem_succ_common_001",
                "Ftakeoff_001",
                "Ftakeoff_run_001",
                "Ftakeoff_jump_001",
                "ngathering_fail_gathering_A_001",
                "ngathering_fail_gathering_B_001",
                "ngathering_succ_gathering_A_001",
                "ngathering_succ_gathering_B_001",
                "ngatheringstart_gathering_A_001",
                "ngatheringstart_gathering_B_001",
                "Fgathering_fail_aerialgathering_001",
                "Fgathering_succ_aerialgathering_001",
                "Fgatheringstart_aerialgathering_001",
                "Fgathering_fail_gathering_b_001",
                "Fgathering_succ_gathering_b_001",
                "Fgatheringstart_gathering_b_001",
                "ncraft_fail_alchemy_001",
                "ncraft_fail_Cooking_001",
                "ncraft_fail_handiwork_001",
                "ncraft_fail_tailoring_001",
                "ncraft_fail_WSmith_001",
                "ncraft_fail_asmith_001",
                "ncraft_succ_alchemy_001",
                "ncraft_succ_Cooking_001",
                "ncraft_succ_handiwork_001",
                "ncraft_succ_tailoring_001",
                "ncraft_succ_WSmith_001",
                "ncraft_succ_asmith_001",
                "ncraftstart_alchemy_001",
                "ncraftstart_Cooking_001",
                "ncraftstart_handiwork_001",
                "ncraftstart_tailoring_001",
                "ncraftstart_WSmith_001",
                "ncraftstart_ASmith_001",
                "ncraft_succ_menuisier_001",
                "ncraft_fail_menuisier_001",
                "ncraftstart_menuisier_001",
                "rsit_001",
                "nstand_001",
                "nresurrection_001",
                "fresurrection_001",
                "rsit_ninja_001",
                "nstand_ninja_001",
                "rsit_hovering_001",
                "nstand_hovering_001",
                "Zsit_001",
                "Mstand_001",
                "Mgathering_fail_gathering_A_001",
                "Mgathering_fail_gathering_B_001",
                "Mgathering_succ_gathering_A_001",
                "Mgathering_succ_gathering_B_001",
                "Mgatheringstart_gathering_A_001",
                "Mgatheringstart_gathering_B_001",
                "Agathering_fail_aerialgathering_001",
                "Aathering_succ_aerialgathering_001",
                "Agatheringstart_aerialgathering_001",
                "Agathering_fail_gathering_b_001",
                "Agathering_succ_gathering_b_001",
                "Agatheringstart_gathering_b_001",
                "Museitem_succ_drinkcoke_run_001",
                "Museitem_succ_drinkGrape_run_001",
                "Museitem_succ_useup_001",
                "Museitem_succ_useup_run_001",
                "Museitem_succ_eat_001",
                "Museitem_succ_eat_run_001",
                "Museitem_succ_drink_001",
                "Museitem_succ_drink_run_001",
                "Museitem_common_001",
                "Museiteme_common_001",
                "Museitem_ride_001",
                "Museitem_succ_common_001",
                "Auseitem_common_001",
                "Auseitem_succ_common_001",
                "Atakeoff_001",
                "Atakeoff_run_001",
                "Atakeoff_jump_001",
            };

            List<string> list_base_search = new List<string>(base_search);

            list_base_search.AddRange(weapon_search("nuseitem_succ_", "_drinkcoke_001"));
            list_base_search.AddRange(weapon_search("nuseitem_succ_", "_drinkGrape_001"));
            list_base_search.AddRange(weapon_search("nputin_", "_001"));
            list_base_search.AddRange(weapon_search("nputin_", "_run_001"));
            list_base_search.AddRange(weapon_search("Museitem_succ_", "_drinkcoke_001"));
            list_base_search.AddRange(weapon_search("Museitem_succ_", "_drinkGrape_001"));
            list_base_search.AddRange(weapon_search("Mputin_", "_001"));
            list_base_search.AddRange(weapon_search("Mputin_", "_run_001"));
            list_base_search.AddRange(weapon_search("cdraw_", "_001"));
            list_base_search.AddRange(weapon_search("Xtakeoff_", "_001"));
            list_base_search.AddRange(weapon_search("Xtakeoff_", "_run_001"));
            list_base_search.AddRange(weapon_search("Xtakeoff_", "_jump_001"));
            list_base_search.AddRange(weapon_search("Qtakeoff_", "_001"));
            list_base_search.AddRange(weapon_search("Qtakeoff_", "_run_001"));
            list_base_search.AddRange(weapon_search("Qtakeoff_", "_jump_001"));
            list_base_search.AddRange(weapon_search("xdraw_", "_001"));
            list_base_search.AddRange(weapon_search("xdraw_", "_run_001"));
            list_base_search.AddRange(weapon_search("Qdraw_", "_001"));
            list_base_search.AddRange(weapon_search("Qdraw_", "_run_001"));
            list_base_search.AddRange(weapon_search("fputin_", "_001"));
            list_base_search.AddRange(weapon_search("fputin_", "_run_001"));
            list_base_search.AddRange(weapon_search("Aputin_", "_001"));
            list_base_search.AddRange(weapon_search("Aputin_", "_run_001"));

            Thread t20 = new Thread(() =>
            {
                string line;

                using (System.IO.StreamReader source = new System.IO.StreamReader(tmp_dir_path + @"\df.cal"))
                using (System.IO.StreamWriter target = new System.IO.StreamWriter(mod_dir_path + @"\df.cal"))
                while ((line = source.ReadLine()) != null)
                {
                    target.WriteLine(replace_line(line, list_base_search.ToArray()));
                }
            });

            Thread t21 = new Thread(() =>
            {
                string line;

                using (System.IO.StreamReader source = new System.IO.StreamReader(tmp_dir_path + @"\lf.cal"))
                using (System.IO.StreamWriter target = new System.IO.StreamWriter(mod_dir_path + @"\lf.cal"))
                    while ((line = source.ReadLine()) != null)
                    {
                        target.WriteLine(replace_line(line, list_base_search.ToArray()));
                    }
            });

            Thread t22 = new Thread(() =>
            {
                string line;

                using (System.IO.StreamReader source = new System.IO.StreamReader(tmp_dir_path + @"\dm.cal"))
                using (System.IO.StreamWriter target = new System.IO.StreamWriter(mod_dir_path + @"\dm.cal"))
                    while ((line = source.ReadLine()) != null)
                    {
                        target.WriteLine(replace_line(line, list_base_search.ToArray()));
                    }
            });

            Thread t23 = new Thread(() =>
            {
                string line;

                using (System.IO.StreamReader source = new System.IO.StreamReader(tmp_dir_path + @"\lm.cal"))
                using (System.IO.StreamWriter target = new System.IO.StreamWriter(mod_dir_path + @"\lm.cal"))
                    while ((line = source.ReadLine()) != null)
                    {
                        target.WriteLine(replace_line(line, list_base_search.ToArray()));
                    }
            });

            t20.Start();
            t21.Start();
            t22.Start();
            t23.Start();

            t20.Join();
            t21.Join();
            t22.Join();
            t23.Join();

            info_text("pak file create...");

            if (!File.Exists(mod_file_path_0))
            {
                using (ZipArchive modFile = ZipFile.Open(mod_file_path_0, ZipArchiveMode.Create))
                {
                    modFile.CreateEntryFromFile(mod_dir_path + "\\" + "df.cal", "df.cal", CompressionLevel.Fastest);
                }
            }

            if (!File.Exists(mod_file_path_1))
            {
                using (ZipArchive modFile = ZipFile.Open(mod_file_path_1, ZipArchiveMode.Create))
                {
                    modFile.CreateEntryFromFile(mod_dir_path + "\\" + "lf.cal", "lf.cal", CompressionLevel.Fastest);
                }
            }

            if (!File.Exists(mod_file_path_2))
            {
                using (ZipArchive modFile = ZipFile.Open(mod_file_path_2, ZipArchiveMode.Create))
                {
                    modFile.CreateEntryFromFile(mod_dir_path + "\\" + "dm.cal", "dm.cal", CompressionLevel.Fastest);
                }
            }

            if (!File.Exists(mod_file_path_3))
            {
                using (ZipArchive modFile = ZipFile.Open(mod_file_path_3, ZipArchiveMode.Create))
                {
                    modFile.CreateEntryFromFile(mod_dir_path + "\\" + "lm.cal", "lm.cal", CompressionLevel.Fastest);
                }
            }

            try
            {
                Directory.Delete(mod_dir_path, true);
                Directory.Delete(tmp_dir_path, true);
            }
            catch
            { }

            info_text(path);

            EnableAllCB();
        }

        private void _Color()
        {
            DisableAllCB();
            string path = "";
            in_Win_Main.Dispatcher.Invoke((Action)(() =>
            {
                path = tb_aion_dir.Text;
            }));
            string origin_file_path = path + @"\Data\ui\ui.pak";
            string mod_file_path = path + @"\Data\ui\ui.pak_boby";
            string zip_file_path = System.Windows.Forms.Application.StartupPath + @"\ui.zip";
            string tmp_dir_path = System.Windows.Forms.Application.StartupPath + @"\tmp";
            string mod_dir_path = System.Windows.Forms.Application.StartupPath + @"\mod";

            info_text("remove tmp file...");

            try
            {
                Directory.Delete(mod_dir_path, true);
                Directory.Delete(tmp_dir_path, true);
            }
            catch
            { }

            info_text("move file...");

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\pak2zip.exe";
            process.StartInfo.Arguments = "\"" + origin_file_path + "\" \"" + zip_file_path + "\"";
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();

            info_text("extract file...");

            Directory.CreateDirectory(tmp_dir_path);
            using (ZipArchive zip = ZipFile.Open(zip_file_path, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                    if (entry.Name == "ui_colortable.xml")
                        entry.ExtractToFile(tmp_dir_path + @"\ui_colortable.xml");
            Directory.CreateDirectory(mod_dir_path);

            info_text("xml convert...");

            Process processXml = new Process();
            processXml.StartInfo.UseShellExecute = false;
            processXml.StartInfo.RedirectStandardOutput = false;
            processXml.StartInfo.FileName = System.Windows.Forms.Application.StartupPath + @"\Gibbed.Aion.ConvertXml.exe";
            processXml.StartInfo.Arguments = "\"" + tmp_dir_path + @"\ui_colortable.xml" + "\" \"" + mod_dir_path + @"\ui_colortable.xml" + "\"";
            processXml.StartInfo.CreateNoWindow = true;
            processXml.Start();
            processXml.WaitForExit();

            info_text("mod file...");

            (new Thread(() =>
            {
                try
                {
                    File.Delete(zip_file_path);
                    Directory.Delete(tmp_dir_path, true);
                }
                catch
                { }
            }
            )).Start();

            IniFile Config = new IniFile("./boby_add_file.ini");

            string red = Config.IniReadValue("boby", "red");
            string green = Config.IniReadValue("boby", "green");
            string blue = Config.IniReadValue("boby", "blue");

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(mod_dir_path + @"\ui_colortable.xml");

            XmlNodeList xmlnode = xmlDoc.GetElementsByTagName("color");

            for (int i = 0; i < xmlnode.Count; i++)
            {
                XmlAttributeCollection xmlattrc = xmlnode[i].Attributes;

                String strNameValue = xmlattrc[0].Value;

                if (strNameValue == "RadarEnemyPC" || strNameValue == "TitlePCEnemyPlayer")
                {
                    xmlattrc[1].Value = red + "," + green + "," + blue + ",255";
                }
            }

            xmlDoc.Save(mod_dir_path + @"\ui_colortable.xml");

            info_text("pak file create...");

            if (!File.Exists(mod_file_path))
            {
                using (ZipArchive modFile = ZipFile.Open(mod_file_path, ZipArchiveMode.Create))
                {
                    modFile.CreateEntryFromFile(mod_dir_path + "\\" + @"\ui_colortable.xml", "ui_colortable.xml", CompressionLevel.Fastest);
                }
            }

            try
            {
                Directory.Delete(mod_dir_path, true);
                Directory.Delete(tmp_dir_path, true);
            }
            catch
            { }

            info_text(path);

            EnableAllCB();
        }
    }
}
