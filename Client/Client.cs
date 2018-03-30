using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Client
{
    struct OperationSistem
    {
        public static string Name = String.Empty;
        public static string Version = String.Empty;
        public static string CDVersion = String.Empty;
        public static string InstallDate = String.Empty;
        public static int NumberOfProcesses = 0;
        public static int NumberOfUsers = 0;
        public static string SerialNumber = String.Empty;
    }
    struct CPUUNIT
    {
        public static string Description = String.Empty;
        public static string DeviceID = String.Empty;
        public static int L2CacheSize = 0;
        public static int L3CacheSize = 0;
        public static int MaxClockSpeed = 0;
        public static string Name = String.Empty;
        public static int NumberOfCores = 0;
        public static int NumberOfLogicalProcessors = 0;
        public static string ProcessorId = String.Empty;
        public static int ProcessorType = 0;
        public static string Revision = String.Empty;
        public static string Role = String.Empty;
        public static string SocketDesignation = String.Empty;
        public static string Status = String.Empty;
        public static int StatusInfo = 0;

    }
    struct GPUUNIT
    {
        public static string AdapterRAM = String.Empty;
        public static string Availability = String.Empty;
        public static string Caption = String.Empty;
        public static string CurrentRefreshRate = String.Empty;
        public static string CurrentScanMode = String.Empty;
        public static string Description = String.Empty;
        public static string DeviceID = String.Empty;
        public static string DriverDate = String.Empty;
        public static string DriverVersion = String.Empty;
        public static string MaxRefreshRate = String.Empty;
        public static string MinRefreshRate = String.Empty;
        public static string Monochrome = String.Empty;
        public static string Name = String.Empty;
        public static string VideoProcessor = String.Empty;
    }
    struct Board
    {
        public static string Description = String.Empty;
        public static string HostingBoard = String.Empty;
        public static string HotSwappable = String.Empty;
        public static string Manufacturer = String.Empty;
        public static string Model = String.Empty;
        public static string Name = String.Empty;
        public static string OtherIdentifyingInfo = String.Empty;
        public static string Product = String.Empty;
        public static string SerialNumber = String.Empty;
    }

    public partial class Client : Form
    {

        private Socket socket;
        static byte[] m_byBuff = new byte[1024];
        public static string IpAdressString = String.Empty;
        static MemoryStream stream = new MemoryStream(m_byBuff);
        static BinaryWriter writer = new BinaryWriter(stream);
        static BinaryReader reader = new BinaryReader(stream);
        DataTable rams = new DataTable("RAM");
        DataTable Programs = new DataTable("Programs");

        public struct IRC_QUERIES
        {
            public const int REQ_AUTH = 1; // Код запроса авторизации клиента 
            const int RES_AUTH = 2; // Код ответа на авторизацию
            const int TEST = 8888;
            public const int ERROR_IRC = 0; // Ошибка при обратоке запроса
            public const string EndOfMessage = "<EOF>"; // Метка конца сообщения
            public const int OPSYS = 3; // Код запроса операционной системы
            public const int CPUUNIT = 4; // Код запроса процессора
            public const int ERRONCLIENTSIDE = 9999; // Ошибка на стороне клиента
            public const int GPUUNIT = 5; // Код GPU
            public const int Board = 6; // Код материнской платы
            public const int RAM = 7; // Оперативная память
            public const int Products = 8; // Программы
        }

        public Client()
        {
            InitializeComponent();
            Thread thread = new Thread(getExternalIp);
            thread.Start();
            rams.Columns.Add("BankLabel");
            rams.Columns.Add("Capacity");
            rams.Columns.Add("DataWidth");
            rams.Columns.Add("Description");
            rams.Columns.Add("DeviceLocator");
            rams.Columns.Add("FormFactor");
            rams.Columns.Add("MemoryType");
            rams.Columns.Add("Model");
            rams.Columns.Add("Name");
            rams.Columns.Add("OtherIdentifyingInfo");
            rams.Columns.Add("PartNumber");
            rams.Columns.Add("PositionInRow");
            rams.Columns.Add("SerialNumber");
            rams.Columns.Add("Speed");
            rams.Columns.Add("Status");
            rams.Columns.Add("Version");

            Programs.Columns.Add("DisplayName");
            Programs.Columns.Add("DisplayVersion");
            Programs.Columns.Add("InstallDate");
            Programs.Columns.Add("Publisher");
            Programs.Columns.Add("IdentifyingNumber");
            GetComponets();
        }

        private void GetComponets()
        {
            // Здесь планируются дальнейшие действии при возникновении каких либо проблем
            if (!GetOperationSystem()) return;
            if (!GetProcessUnit()) return;
            if (!GetGPU()) return;
            if (!GetBoard()) return;
            if (!GetRAM()) return;
            if (!GetProducts()) return;
            InitializeSocket();
        }

        private bool GetProducts()
        {
            /*ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Product");
            ManagementObjectCollection colItems = searcher.Get();
            string d = "";
            foreach (ManagementObject queryObj in colItems)
            {
                string[] s = new string[8];
                if (queryObj["Vendor"] != null && (queryObj["Vendor"].ToString() != "Microsoft Corporation"))
                {
                    if (queryObj["Caption"] != null) s[0] = queryObj["Caption"].ToString();
                    if (queryObj["Description"] != null) s[1] = queryObj["Description"].ToString();
                    if (queryObj["IdentifyingNumber"] != null) s[2] = queryObj["IdentifyingNumber"].ToString();
                    if (queryObj["InstallDate"] != null) s[3] = queryObj["InstallDate"].ToString();
                    if (queryObj["Name"] != null) s[4] = queryObj["Name"].ToString();
                    if (queryObj["ProductID"] != null) s[5] = queryObj["ProductID"].ToString();
                    if (queryObj["Vendor"] != null) s[6] = queryObj["Vendor"].ToString();
                    if (queryObj["Version"] != null) s[7] = queryObj["Version"].ToString();
                    for (int i = 0; i < 8; i++)
                    {
                        d = d + s[i] + "\t";
                    }
                    d = d + "\n";
                    Programs.Rows.Add(s);
                }
            }*/
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    string[] s = new string[5];
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        if (subkey.GetValue("DisplayName") != null && !String.IsNullOrWhiteSpace(subkey.GetValue("DisplayName").ToString())) s[0] = subkey.GetValue("DisplayName").ToString();
                        else continue;
                        if (subkey.GetValue("DisplayVersion") != null) s[1] = subkey.GetValue("DisplayVersion").ToString();
                        if (subkey.GetValue("InstallDate") != null) s[2] = subkey.GetValue("InstallDate").ToString();
                        if (subkey.GetValue("Publisher") != null) s[3] = subkey.GetValue("Publisher").ToString();
                        s[4] = subkey.Name;
                    }
                    Programs.Rows.Add(s);
                }
            }
            return true;
        }
        private bool GetRAM()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM CIM_PhysicalMemory");
            ManagementObjectCollection colItems = searcher.Get();
            foreach (ManagementObject queryObj in colItems)
            {
                string[] s = new string[16];
                if (queryObj["BankLabel"] != null) s[0] = queryObj["BankLabel"].ToString();
                if (queryObj["Capacity"] != null) s[1] = queryObj["Capacity"].ToString();
                if (queryObj["DataWidth"] != null) s[2] = queryObj["DataWidth"].ToString();
                if (queryObj["Description"] != null) s[3] = queryObj["Description"].ToString();
                if (queryObj["DeviceLocator"] != null) s[4] = queryObj["DeviceLocator"].ToString();
                if (queryObj["FormFactor"] != null) s[5] = queryObj["FormFactor"].ToString();
                if (queryObj["MemoryType"] != null) s[6] = queryObj["MemoryType"].ToString();
                if (queryObj["Model"] != null) s[7] = queryObj["Model"].ToString();
                if (queryObj["Name"] != null) s[8] = queryObj["Name"].ToString();
                if (queryObj["OtherIdentifyingInfo"] != null) s[9] = queryObj["OtherIdentifyingInfo"].ToString();
                if (queryObj["PartNumber"] != null) s[10] = queryObj["PartNumber"].ToString();
                if (queryObj["PositionInRow"] != null) s[11] = queryObj["PositionInRow"].ToString();
                if (queryObj["SerialNumber"] != null) s[12] = queryObj["SerialNumber"].ToString();
                if (queryObj["Speed"] != null) s[13] = queryObj["Speed"].ToString();
                if (queryObj["Status"] != null) s[14] = queryObj["Status"].ToString();
                if (queryObj["Version"] != null) s[15] = queryObj["Version"].ToString();

                rams.Rows.Add(s);      
            }
            return true;
        }

        private bool GetBoard()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection colItems = searcher.Get();
            foreach (ManagementObject queryObj in colItems)
            {
                if (queryObj["Description"] != null) Board.Description = queryObj["Description"].ToString();
                if (queryObj["HostingBoard"] != null) Board.HostingBoard = queryObj["HostingBoard"].ToString();
                if (queryObj["HotSwappable"] != null) Board.HotSwappable = queryObj["HotSwappable"].ToString();
                if (queryObj["Manufacturer"] != null) Board.Manufacturer = queryObj["Manufacturer"].ToString();
                if (queryObj["Model"] != null) Board.Model = queryObj["Model"].ToString();
                if (queryObj["Name"] != null) Board.Name = queryObj["Name"].ToString();
                if (queryObj["OtherIdentifyingInfo"] != null) Board.OtherIdentifyingInfo = queryObj["OtherIdentifyingInfo"].ToString();
                if (queryObj["Product"] != null) Board.Product = queryObj["Product"].ToString();
                if (queryObj["SerialNumber"] != null) Board.SerialNumber = queryObj["SerialNumber"].ToString();
            }
            return true;

        }

        private bool GetProcessUnit()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM CIM_Processor");
            ManagementObjectCollection colItems = searcher.Get();
            foreach (ManagementObject queryObj in colItems)
            {
                if (queryObj["Description"] != null) CPUUNIT.Description = queryObj["Description"].ToString();
                if (queryObj["DeviceID"] != null) CPUUNIT.DeviceID = queryObj["DeviceID"].ToString();
                if (queryObj["L2CacheSize"] != null) CPUUNIT.L2CacheSize = Convert.ToInt32(queryObj["L2CacheSize"]);
                if (queryObj["L3CacheSize"] != null) CPUUNIT.L3CacheSize = Convert.ToInt32(queryObj["L3CacheSize"]);
                if (queryObj["MaxClockSpeed"] != null) CPUUNIT.MaxClockSpeed = Convert.ToInt32(queryObj["MaxClockSpeed"]);
                if (queryObj["Name"] != null) CPUUNIT.Name = queryObj["Name"].ToString();
                if (queryObj["NumberOfCores"] != null) CPUUNIT.NumberOfCores = Convert.ToInt32(queryObj["NumberOfCores"]);
                if (queryObj["NumberOfLogicalProcessors"] != null) CPUUNIT.NumberOfLogicalProcessors = Convert.ToInt32(queryObj["NumberOfLogicalProcessors"]);
                if (queryObj["ProcessorId"] != null) CPUUNIT.ProcessorId = queryObj["ProcessorId"].ToString();
                if (queryObj["ProcessorType"] != null) CPUUNIT.ProcessorType = Convert.ToInt32(queryObj["ProcessorType"]);
                if (queryObj["Revision"] != null) CPUUNIT.Revision = queryObj["Revision"].ToString();
                if (queryObj["Role"] != null) CPUUNIT.Role = queryObj["Role"].ToString();
                if (queryObj["SocketDesignation"] != null) CPUUNIT.SocketDesignation = queryObj["SocketDesignation"].ToString();
                if (queryObj["Status"] != null) CPUUNIT.Status = queryObj["Status"].ToString();
                if (queryObj["StatusInfo"] != null) CPUUNIT.StatusInfo = Convert.ToInt32(queryObj["StatusInfo"]);
            }
            return true;
        }

        private bool GetOperationSystem()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher( @"root\CIMV2", "SELECT * FROM CIM_OperatingSystem");
            ManagementObjectCollection colItems = searcher.Get();
            object vers = String.Empty, name = String.Empty, pack = String.Empty, istdate = String.Empty, NumberOfProcesses = 0, NumberOfUsers = 0, serial = String.Empty;
            foreach (ManagementObject item in colItems)
            {
                try
                {
                    vers = item["Version"];
                    name = item["Name"];
                    pack = item["CSDVersion"];
                    istdate = item["InstallDate"];
                    NumberOfProcesses = item["NumberOfProcesses"];
                    NumberOfUsers = item["NumberOfUsers"];
                    serial = item["SerialNumber"];
                }
                catch { return false; }
                //if (temp == null) continue;
                if (OperationSistem.Version == String.Empty) OperationSistem.Version = vers.ToString();
                if (OperationSistem.CDVersion == String.Empty) OperationSistem.CDVersion = pack.ToString();
                if (OperationSistem.Name == String.Empty)
                {
                    OperationSistem.Name = name.ToString();
                    OperationSistem.Name = OperationSistem.Name.Substring(0, OperationSistem.Name.IndexOf('|'));
                }
                if (OperationSistem.InstallDate == String.Empty)
                {
                    OperationSistem.InstallDate = istdate.ToString();
                    OperationSistem.InstallDate = OperationSistem.InstallDate.Substring(0, OperationSistem.InstallDate.IndexOf('.'));
                }
                if (OperationSistem.NumberOfProcesses == 0) OperationSistem.NumberOfProcesses = Convert.ToInt32(NumberOfProcesses);
                if (OperationSistem.NumberOfUsers == 0) OperationSistem.NumberOfUsers = Convert.ToInt32(NumberOfUsers.ToString());
                if (OperationSistem.SerialNumber == String.Empty) OperationSistem.SerialNumber = serial.ToString();

                item.Dispose();
            }
            return true;
        }

        private bool GetGPU()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_VideoController");
            ManagementObjectCollection colItems = searcher.Get();
            foreach (ManagementObject queryObj in colItems)
            {
                if (queryObj["AdapterRAM"] != null) GPUUNIT.AdapterRAM = queryObj["AdapterRAM"].ToString();
                if (queryObj["Availability"] != null) GPUUNIT.AdapterRAM = queryObj["Availability"].ToString();
                if (queryObj["Caption"] != null) GPUUNIT.Caption = queryObj["Caption"].ToString();
                if (queryObj["CurrentRefreshRate"] != null) GPUUNIT.CurrentRefreshRate = queryObj["CurrentRefreshRate"].ToString();
                if (queryObj["CurrentScanMode"] != null) GPUUNIT.CurrentScanMode = queryObj["CurrentScanMode"].ToString();
                if (queryObj["Description"] != null) GPUUNIT.Description = queryObj["Description"].ToString();
                if (queryObj["DeviceID"] != null) GPUUNIT.DeviceID = queryObj["DeviceID"].ToString();
                if (queryObj["DriverDate"] != null) GPUUNIT.DriverDate = queryObj["DriverDate"].ToString();
                if (queryObj["DriverVersion"] != null) GPUUNIT.DriverVersion = queryObj["DriverVersion"].ToString();
                if (queryObj["MaxRefreshRate"] != null) GPUUNIT.MaxRefreshRate = queryObj["MaxRefreshRate"].ToString();
                if (queryObj["MinRefreshRate"] != null) GPUUNIT.MinRefreshRate = queryObj["MinRefreshRate"].ToString();
                if (queryObj["Monochrome"] != null) GPUUNIT.Monochrome = queryObj["Monochrome"].ToString();
                if (queryObj["Name"] != null) GPUUNIT.Name = queryObj["Name"].ToString();
                if (queryObj["VideoProcessor"] != null) GPUUNIT.VideoProcessor = queryObj["VideoProcessor"].ToString();
            }
            return true;
        }

        private void getExternalIp()
        {
            try
            {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();
                IpAdressString = externalIP;
               
            }
            catch { return; }
        }

        private void InitializeSocket()
        {
            if (IpAdressString == String.Empty)
            {
                //MessageBox.Show("Ошибка при получении внешнего IP-адреса. Будет использован локальный адрес.");
                string strHostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostByName(strHostName);
                IpAdressString = Convert.ToString(ipEntry.AddressList[0]);
            }
            const int port = 7777;
            try
            {
                if (socket != null && socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    Thread.Sleep(100);
                    socket.Close();
                    button1.Text = "Подключиться";
                    return;
                }
                button1.Text = "Отключиться";
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //socket = new Socket(IPAddress.Parse(IpAdressString).AddressFamily, SocketType.Stream, ProtocolType.Tcp); Старая версия подключения
                IPEndPoint epServer = new IPEndPoint(IPAddress.Parse(adresslabel.Text), port);
                socket.Blocking = false;
                AsyncCallback onconnect = new AsyncCallback(OnConnect);
                socket.BeginConnect(epServer, onconnect, socket);
            }
            catch (Exception ex)
            {
                button1.Text = "Подключиться";
                MessageBox.Show(ex.Message, "Server Connect failed!");
            }
        }

        public void OnConnect(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            try
            {
                if (sock.Connected) SetupRecieveCallback(sock);
                else sock.EndConnect(ar);
            }
            catch (Exception ex)
            {
                if (button1.InvokeRequired) button1.Invoke(new Action(() => button1.Text = "Подключиться"));
                else button1.Text = "Подключиться";
                MessageBox.Show(ex.Message, "Unusual error during Connect!");
            }
        }

        public void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, sock);
            }
            catch (Exception ex)
            {
                button1.Text = "Отключиться";
                MessageBox.Show(ex.Message, "Setup Recieve Callback failed!");
            }
        }

        public void OnRecievedData(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            stream.Position = 0;
            try
            {
                int nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    int irc = reader.ReadInt32();
                    switch (irc)
                    {
                        #region Авторизация
                        case IRC_QUERIES.REQ_AUTH:
                            stream.Position = 0;
                            writer.Write(IRC_QUERIES.REQ_AUTH);
                            writer.Write(Dns.GetHostName());
                            writer.Write("111111");
                            Random rand = new Random();
                            writer.Write(GetMACAddress() + rand.Next(10000, 99999).ToString());
                            writer.Write(IRC_QUERIES.EndOfMessage);
                            sock.Send(m_byBuff);
                            break;
                        #endregion
                        #region OC
                        case IRC_QUERIES.OPSYS:
                            stream.Position = 0;
                            if (OperationSistem.Name == String.Empty || OperationSistem.CDVersion == String.Empty)
                            {
                                writer.Write(IRC_QUERIES.ERROR_IRC);
                                writer.Write(IRC_QUERIES.ERRONCLIENTSIDE);
                                writer.Write(IRC_QUERIES.EndOfMessage);
                                socket.Send(m_byBuff);
                                break;
                            }
                            writer.Write(IRC_QUERIES.OPSYS);
                            writer.Write(OperationSistem.Name);
                            writer.Write(OperationSistem.Version);
                            writer.Write(OperationSistem.CDVersion);
                            writer.Write(OperationSistem.InstallDate);
                            writer.Write(OperationSistem.NumberOfProcesses.ToString());
                            writer.Write(OperationSistem.NumberOfUsers.ToString());
                            writer.Write(OperationSistem.SerialNumber);
                            writer.Write(IRC_QUERIES.EndOfMessage);
                            socket.Send(m_byBuff);
                            break;
                        #endregion
                        #region CPU
                        case IRC_QUERIES.CPUUNIT:
                            stream.Position = 0;
                            if (CPUUNIT.Name == String.Empty || CPUUNIT.NumberOfCores == 0)
                            {
                                writer.Write(IRC_QUERIES.ERROR_IRC);
                                writer.Write(IRC_QUERIES.ERRONCLIENTSIDE);
                                writer.Write(IRC_QUERIES.EndOfMessage);
                                socket.Send(m_byBuff);
                                break;
                            }
                            writer.Write(IRC_QUERIES.CPUUNIT);
                            writer.Write(CPUUNIT.Description);
                            writer.Write(CPUUNIT.DeviceID);
                            writer.Write(CPUUNIT.L2CacheSize.ToString());
                            writer.Write(CPUUNIT.L3CacheSize.ToString());
                            writer.Write(CPUUNIT.MaxClockSpeed.ToString());
                            writer.Write(CPUUNIT.Name);
                            writer.Write(CPUUNIT.NumberOfCores.ToString());
                            writer.Write(CPUUNIT.NumberOfLogicalProcessors.ToString());
                            writer.Write(CPUUNIT.ProcessorId);
                            writer.Write(CPUUNIT.ProcessorType.ToString());
                            writer.Write(CPUUNIT.Revision);
                            writer.Write(CPUUNIT.Role);
                            writer.Write(CPUUNIT.SocketDesignation);
                            writer.Write(CPUUNIT.Status);
                            writer.Write(CPUUNIT.StatusInfo.ToString());
                            writer.Write(IRC_QUERIES.EndOfMessage);
                            socket.Send(m_byBuff);
                            break;
                        #endregion
                        #region GPU
                        case IRC_QUERIES.GPUUNIT:
                            stream.Position = 0;
                            if (GPUUNIT.Name == String.Empty || GPUUNIT.Description == String.Empty)
                            {
                                writer.Write(IRC_QUERIES.ERROR_IRC);
                                writer.Write(IRC_QUERIES.ERRONCLIENTSIDE);
                                writer.Write(IRC_QUERIES.EndOfMessage);
                                socket.Send(m_byBuff);
                                break;
                            }
                            writer.Write(IRC_QUERIES.GPUUNIT);
                            writer.Write(GPUUNIT.AdapterRAM);//0
                            writer.Write(GPUUNIT.Availability);
                            writer.Write(GPUUNIT.Caption);
                            writer.Write(GPUUNIT.CurrentRefreshRate);
                            writer.Write(GPUUNIT.CurrentScanMode);
                            writer.Write(GPUUNIT.Description);//5
                            writer.Write(GPUUNIT.DeviceID);
                            writer.Write(GPUUNIT.DriverDate);
                            writer.Write(GPUUNIT.DriverVersion);
                            writer.Write(GPUUNIT.MaxRefreshRate);
                            writer.Write(GPUUNIT.MinRefreshRate);//10
                            writer.Write(GPUUNIT.Monochrome);
                            writer.Write(GPUUNIT.Name);
                            writer.Write(GPUUNIT.VideoProcessor);
                            writer.Write(IRC_QUERIES.EndOfMessage);
                            socket.Send(m_byBuff);
                            break;
                        #endregion
                        #region Board
                        case IRC_QUERIES.Board:
                            stream.Position = 0;
                            if (Board.Name == String.Empty || Board.Description == String.Empty)
                            {
                                writer.Write(IRC_QUERIES.ERROR_IRC);
                                writer.Write(IRC_QUERIES.ERRONCLIENTSIDE);
                                writer.Write(IRC_QUERIES.EndOfMessage);
                                socket.Send(m_byBuff);
                                break;
                            }
                            writer.Write(IRC_QUERIES.Board);
                            writer.Write(Board.Description);//0
                            writer.Write(Board.HostingBoard);//1
                            writer.Write(Board.HotSwappable);
                            writer.Write(Board.Manufacturer);
                            writer.Write(Board.Model);
                            writer.Write(Board.Name);//5
                            writer.Write(Board.OtherIdentifyingInfo);
                            writer.Write(Board.Product);
                            writer.Write(Board.SerialNumber);
                            writer.Write(IRC_QUERIES.EndOfMessage);//9
                            socket.Send(m_byBuff);
                            break;
                        #endregion
                        #region RAM
                        case IRC_QUERIES.RAM:
                            stream.Position = 0;
                            if (rams.Rows.Count == 0)
                            {
                                writer.Write(IRC_QUERIES.ERROR_IRC);
                                writer.Write(IRC_QUERIES.ERRONCLIENTSIDE);
                                writer.Write(IRC_QUERIES.EndOfMessage);
                                socket.Send(m_byBuff);
                                break;
                            }
                            writer.Write(IRC_QUERIES.RAM);
                            writer.Write(rams.Rows.Count);
                            for (int i = 0; i < rams.Rows.Count; i++)
                            {
                                for (int k = 0; k < rams.Columns.Count; k++)
                                {
                                    writer.Write(rams.Rows[i][k].ToString());
                                }
                            }
                            writer.Write(IRC_QUERIES.EndOfMessage);//9
                            socket.Send(m_byBuff);
                            break;
                        #endregion
                        #region Products
                        case IRC_QUERIES.Products:
                            stream.Position = 0;
                            writer.Write(IRC_QUERIES.Products);
                            if (Programs.Rows.Count != 0)
                            {
                                for (int k = 0; k < Programs.Columns.Count; k++)
                                {
                                    writer.Write(Programs.Rows[0][k].ToString());
                                }
                                socket.Send(m_byBuff);
                                Programs.Rows.RemoveAt(0);
                                Thread.Sleep(200);
                                break;
                            }
                            writer.Write(IRC_QUERIES.EndOfMessage);
                            socket.Send(m_byBuff);
                            break;
                        #endregion
                        default:
                            break;
                    }
                    SetupRecieveCallback(sock);
                }
                else
                {
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                if (button1.InvokeRequired) button1.Invoke(new Action(() => button1.Text = "Подключиться"));
                else button1.Text = "Подключиться";
                MessageBox.Show(ex.Message, "Unusual error druing Recieve!");
            }
        }

        public string GetMACAddress()
        {
            string macAddress = String.Empty;
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_NetworkAdapterConfiguration");
            foreach (ManagementObject mo in mos.Get())
            {
                object tempMacAddrObj = mo["MacAddress"];
                if (tempMacAddrObj == null) continue; //Skip objects without a MACAddress
                if (macAddress == String.Empty) macAddress = tempMacAddrObj.ToString(); // only return MAC Address from first card that has a MAC Address
                mo.Dispose();
            }

            return macAddress;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            InitializeSocket();
        }

    }
}
