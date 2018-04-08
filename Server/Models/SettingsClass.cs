using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Server
{
    public class SettingsClass
    {
        public static string DB_USER;
        public static string DB_PASS;
        public static string DB_HOST;
        public static string DB_BASE;
        public static int SPort;
        public static int SBufferSize;
        public static bool LocalWork;
        public static int MaxClients;
        public static List<string> BlackPublish = new List<string>();
        public static List<string> BlackNames = new List<string>();
        public static string ClientVersion;
        public static string oldVersion;

        public SettingsClass()
        {

        }

        public bool LoadSettings()
        {
            XDocument doc = XDocument.Load("preferences.xml");
            XNode node = doc.Root.FirstNode;
            while (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    XElement el = (XElement)node;
                    foreach (XElement element in el.Elements())
                    {
                        if (element.Name == "SocketPort") SPort = Int32.Parse(element.Value);
                        else if (element.Name == "MaxClients") MaxClients = Int32.Parse(element.Value);
                        else if (element.Name == "MaxBufferSize") SBufferSize = Int32.Parse(element.Value);
                        else if (element.Name == "LocalWorking") LocalWork = Boolean.Parse(element.Value);
                        else if (element.Name == "DBHost") DB_HOST = element.Value;
                        else if (element.Name == "DBUser") DB_USER = element.Value;
                        else if (element.Name == "DBPass") DB_PASS = element.Value;
                        else if (element.Name == "DBBase") DB_BASE = element.Value;
                        else if (element.Name == "Version") oldVersion = element.Value;
                        else if (element.Name == "ClientVersion") ClientVersion = element.Value;
                    }
                }
                node = node.NextNode;
            }
            return true;
        }
        public bool SaveSettings()
        {
            XDocument doc = XDocument.Load("preferences.xml");
            XNode node = doc.Root.FirstNode;
            while (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    XElement el = (XElement)node;
                    foreach (XElement element in el.Elements())
                    {
                        if (element.Name == "SocketPort") element.Value = SPort.ToString();
                        else if (element.Name == "MaxClients") element.Value = MaxClients.ToString();
                        else if (element.Name == "MaxBufferSize") element.Value = SBufferSize.ToString();
                        else if (element.Name == "LocalWorking") element.Value = LocalWork.ToString();
                        else if (element.Name == "DBHost") element.Value = DB_HOST;
                        else if (element.Name == "DBUser") element.Value = DB_USER;
                        else if (element.Name == "DBPass") element.Value = DB_PASS;
                        else if (element.Name == "DBBase") element.Value = DB_BASE;
                        else if (element.Name == "ClientVersion") element.Value = ClientVersion;
                        else if (element.Name == "Version") element.Value = System.Windows.Forms.Application.ProductVersion;
                    }
                }
                node = node.NextNode;
            }
            doc.Save("preferences.xml");
            return true;
        }
        public bool CheckSettingsFile()
        {
            if (File.Exists("preferences.xml")) return true;
            return false;
        }
        public void CreateSettingsFile()
        {
            XmlTextWriter textWritter = new XmlTextWriter(@"preferences.xml", Encoding.UTF8);
            textWritter.WriteStartDocument();//writer
            textWritter.WriteStartElement("root");//root
            textWritter.WriteEndElement();//root
            textWritter.Close();//writer
            
            if (File.Exists("preferences.xml"))
            {
                XmlDocument document = new XmlDocument();
                document.Load("preferences.xml");
                XmlNode element = document.CreateElement("MainPreferences");
                document.DocumentElement.AppendChild(element);

                XmlNode subElementz = document.CreateElement("Version");
                subElementz.InnerText = System.Windows.Forms.Application.ProductVersion;
                element.AppendChild(subElementz);


                element = document.CreateElement("SocketPreferences");
                document.DocumentElement.AppendChild(element);

                XmlNode subElement1 = document.CreateElement("SocketPort");
                subElement1.InnerText = "7777";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("MaxClients");
                subElement1.InnerText = "100";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("MaxBufferSize");
                subElement1.InnerText = "1024";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("LocalWorking");
                subElement1.InnerText = "True";
                element.AppendChild(subElement1);


                element = document.CreateElement("DataBasePreferences");
                document.DocumentElement.AppendChild(element);

                subElement1 = document.CreateElement("DBHost");
                subElement1.InnerText = "localhost";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("DBUser");
                subElement1.InnerText = "root";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("DBPass");
                subElement1.InnerText = "    ";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("DBBase");
                subElement1.InnerText = "root";
                element.AppendChild(subElement1);


                element = document.CreateElement("ClientPreferenses");
                document.DocumentElement.AppendChild(element);

                subElement1 = document.CreateElement("ClientVersion");
                subElement1.InnerText = "1.0.0.0";
                element.AppendChild(subElement1);

                document.Save("preferences.xml");
            }

        }
        public void ReBuildSettings()
        {
            LoadSettings();
            CreateSettingsFile();
            SaveSettings();
        }
        public bool CheckProductList()
        {
            if (File.Exists("appsblacklist.txt")) return true;
            return false;
        }
        public void CreateProductList()
        {
            File.Create("appsblacklist.txt").Close();
            using (StreamWriter output = new StreamWriter(@"appsblacklist.txt"))
            {
                output.WriteLine("// Список приложений, которые собирать с клиентов не требуется.");
                output.WriteLine("// Комментирование строк производится двойным слэшем ('//'), это значит, что сервер будет игнорировать данную строку.");
                output.WriteLine("// Перед названием приложения укажите к чему запись относится");
                output.WriteLine("// Name: - конкретное название приложения");
                output.WriteLine("// Publisher: - поставщик приложения");
                output.WriteLine("Publisher: Microsoft Corporation");
                output.WriteLine("Name: Notepad++ (64-bit x64)");
                output.Close();
            }
        }
        public void LoadProductList()
        {
            using (StreamReader fs = new StreamReader(@"appsblacklist.txt"))
            {
                while (true)
                {
                    string temp = fs.ReadLine();
                    if (temp == null) break;
                    if (temp.Contains("//")) continue;
                    if (temp.Contains("Name:"))
                    {
                        string match = (new Regex(@"Name:\s(.*)"))
                             .Matches(temp)[0].ToString();
                        if (match != null) BlackNames.Add(match);
                    }
                    else if (temp.Contains("Publisher:"))
                    {
                        string match = (new Regex(@"Publisher:\s(.*)"))
                             .Matches(temp)[0].ToString();
                        if (match != null) BlackPublish.Add(match);
                    }
                }
            }
        }
        public void ClearBlackList()
        {
            BlackPublish.Clear();
            BlackNames.Clear();
        }
        public void DeleteBlackList()
        {
            ClearBlackList();
            if (File.Exists("appsblacklist.txt")) File.Delete(@"appsblacklist.txt");
        }
    }
}
