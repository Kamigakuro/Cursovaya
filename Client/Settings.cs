using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Client
{
    public class Settings
    {
        EventLogger Log = new EventLogger();
        public static int Port;
        public static string ServerAdressStr = String.Empty;
        public static System.Net.IPEndPoint ServerIP;
        public static System.Net.IPEndPoint ExternalServerIP;
        public static string ExternalAdressStr = String.Empty;
        public static bool LocalWorking = true;
        public static int BufferSize = 0;
        public static int ReconnectTime = 1000;
        public bool CheckSettingsFile()
        {
            if (File.Exists(@"preferences.xml")) return true;
            else return false;
        }
        public void CreateSettingsFile()
        {
            Log.AddLog("[SETTINGS] Создание файла настроек...");
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
                element = document.CreateElement("SocketPreferences");
                document.DocumentElement.AppendChild(element);

                XmlNode subElement1 = document.CreateElement("SocketPort");
                subElement1.InnerText = "7777";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("ServerAdress");
                subElement1.InnerText = "192.168.1.34";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("MaxBufferSize");
                subElement1.InnerText = "1024";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("LocalWorking");
                subElement1.InnerText = "True";
                element.AppendChild(subElement1);
                subElement1 = document.CreateElement("ExternalServerIP");
                subElement1.InnerText = "8.8.8.8";
                element.AppendChild(subElement1);


                //element = document.CreateElement("DataBasePrefences");
                //document.DocumentElement.AppendChild(element);

                //subElement1 = document.CreateElement("DBHost");
                //subElement1.InnerText = "localhost";
                //element.AppendChild(subElement1);
                //subElement1 = document.CreateElement("DBUser");
                //subElement1.InnerText = "root";
                //element.AppendChild(subElement1);
                //subElement1 = document.CreateElement("DBPass");
                //subElement1.InnerText = "    ";
                //element.AppendChild(subElement1);
                //subElement1 = document.CreateElement("DBBase");
                //subElement1.InnerText = "root";
                //element.AppendChild(subElement1);

                document.Save("preferences.xml");
                Log.AddLog("[SETTINGS] Создание завершено!");
            }
        }
        public void LoadSettings()
        {
            Log.AddLog("[SETTINGS] Начата загрузка настроек...");
            XDocument doc = XDocument.Load("preferences.xml");
            XNode node = doc.Root.FirstNode;
            while (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    XElement el = (XElement)node;
                    foreach (XElement element in el.Elements())
                    {
                        if (element.Name == "SocketPort") Port = Int32.Parse(element.Value);
                        else if (element.Name == "ServerAdress") ServerAdressStr = element.Value;
                        else if (element.Name == "MaxBufferSize") BufferSize = Int32.Parse(element.Value);
                        else if (element.Name == "LocalWorking") LocalWorking = Boolean.Parse(element.Value);
                        else if (element.Name == "ExternalServerIP") ExternalAdressStr = element.Value;
                    }
                }
                node = node.NextNode;
            }
            if (LocalWorking)
            {
                Log.AddLog("[SETTINGS] Режим работы определен как локальный.");
                ServerIP = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ServerAdressStr), Port);
            }
            else
            {
                Log.AddLog("[SETTINGS] Режим работы определен как внешний.");
                ServerIP = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ExternalAdressStr), Port);
            }
            Log.AddLog("[SETTINGS] Настройки загружены!");
        }
        public void SaveSettings()
        {
            Log.AddLog("[SETTINGS] Сохранение настроек...");
            XDocument doc = XDocument.Load("preferences.xml");
            XNode node = doc.Root.FirstNode;
            while (node != null)
            {
                if (node.NodeType == System.Xml.XmlNodeType.Element)
                {
                    XElement el = (XElement)node;
                    foreach (XElement element in el.Elements())
                    {
                        if (element.Name == "SocketPort") element.Value = Port.ToString();
                        else if (element.Name == "ServerAdress") element.Value = ServerAdressStr.ToString();
                        else if (element.Name == "MaxBufferSize") element.Value = BufferSize.ToString();
                        else if (element.Name == "LocalWorking") element.Value = LocalWorking.ToString();
                        else if (element.Name == "ExternalServerIP") element.Value = ExternalAdressStr;
                    }
                }
                node = node.NextNode;
            }
            doc.Save("preferences.xml");
            Log.AddLog("[SETTINGS] Настройки сохранены...");
        }

    }
}
