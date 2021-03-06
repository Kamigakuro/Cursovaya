﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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


                element = document.CreateElement("DataBasePrefences");
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

                document.Save("preferences.xml");
            }

        }

    }
}
