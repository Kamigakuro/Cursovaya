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

        public struct IRC_QUERIES
        {
            public const int REQ_AUTH = 1;
            const int RES_AUTH = 2;
            const int TEST = 8888;
            public const int ERROR_IRC = 0;
            public const string EndOfMessage = "<EOF>";
            public const int OPSYS = 3;
            public const int CPUUNIT = 4;
            public const int ERRONCLIENTSIDE = 9999;
            public const int GPUUNIT = 5;
            public const int Board = 6;
        }


        public Client()
        {
            InitializeComponent();
            Thread thread = new Thread(getExternalIp);
            thread.Start();
            GetComponets();
        }



        private void GetComponets()
        {
            if (!GetOpeationSystem()) return;
            if (!GetProcessUnit()) return;
            if (!GetGPU()) return;
            if (!GetBoard()) return;
            if (!GetRAM()) return;

        }

        private bool GetRAM()
        {

            return true;
        }

        private bool GetBoard()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection colItems = searcher.Get();
            foreach (ManagementObject queryObj in colItems)
            {
                try
                {
                    Board.Description = queryObj["Description"].ToString();
                }
                catch { }
                try
                {
                    Board.HostingBoard = queryObj["HostingBoard"].ToString();
                }
                catch { }
                try
                {
                    Board.HotSwappable = queryObj["HotSwappable"].ToString();
                }
                catch { }
                try
                {
                    Board.Manufacturer = queryObj["Manufacturer"].ToString();
                }
                catch { }
                try
                {
                    Board.Model = queryObj["Model"].ToString();
                }
                catch { }
                try
                {
                    Board.Name = queryObj["Name"].ToString();
                }
                catch { }
                try
                {
                    Board.OtherIdentifyingInfo = queryObj["OtherIdentifyingInfo"].ToString();
                }
                catch { }
                try
                {
                   Board.Product = queryObj["Product"].ToString();
                }
                catch { }
                try
                {
                    Board.SerialNumber = queryObj["SerialNumber"].ToString();
                }
                catch { }
                
            }
            return true;

        }

        private bool GetProcessUnit()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM CIM_Processor");
            ManagementObjectCollection colItems = searcher.Get();
            foreach (ManagementObject queryObj in colItems)
            {
                try { CPUUNIT.Description = queryObj["Description"].ToString(); }
                catch { }
                try { CPUUNIT.DeviceID = queryObj["DeviceID"].ToString(); }
                catch { }
                try { CPUUNIT.L2CacheSize = Convert.ToInt32(queryObj["L2CacheSize"]); }
                catch { }
                try { CPUUNIT.L3CacheSize = Convert.ToInt32(queryObj["L3CacheSize"]); }
                catch { }
                try { CPUUNIT.MaxClockSpeed = Convert.ToInt32(queryObj["MaxClockSpeed"]); }
                catch { }
                try { CPUUNIT.Name = queryObj["Name"].ToString(); }
                catch { }
                try { CPUUNIT.NumberOfCores = Convert.ToInt32(queryObj["NumberOfCores"]); }
                catch { }
                try { CPUUNIT.NumberOfLogicalProcessors = Convert.ToInt32(queryObj["NumberOfLogicalProcessors"]); }
                catch { }
                try { CPUUNIT.ProcessorId = queryObj["ProcessorId"].ToString(); }
                catch { }
                try { CPUUNIT.ProcessorType = Convert.ToInt32(queryObj["ProcessorType"]); }
                catch { }
                try { CPUUNIT.Revision = queryObj["Revision"].ToString(); }
                catch { }
                try { CPUUNIT.Role = queryObj["Role"].ToString(); }
                catch { }
                try { CPUUNIT.SocketDesignation = queryObj["SocketDesignation"].ToString(); }
                catch { }
                try { CPUUNIT.Status = queryObj["Status"].ToString(); }
                catch { }
                try { CPUUNIT.StatusInfo = Convert.ToInt32(queryObj["StatusInfo"]); }
                catch { }
            }
            return true;
        }

        private bool GetOpeationSystem()
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
                try
                {
                    GPUUNIT.AdapterRAM = queryObj["AdapterRAM"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.AdapterRAM = queryObj["Availability"].ToString();
                }
                catch { }
                try
                {
                   GPUUNIT.Caption = queryObj["Caption"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.CurrentRefreshRate = queryObj["CurrentRefreshRate"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.CurrentScanMode = queryObj["CurrentScanMode"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.Description = queryObj["Description"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.DeviceID = queryObj["DeviceID"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.DriverDate = queryObj["DriverDate"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.DriverVersion = queryObj["DriverVersion"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.MaxRefreshRate = queryObj["MaxRefreshRate"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.MinRefreshRate = queryObj["MinRefreshRate"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.Monochrome = queryObj["Monochrome"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.Name = queryObj["Name"].ToString();
                }
                catch { }
                try
                {
                    GPUUNIT.VideoProcessor = queryObj["VideoProcessor"].ToString();
                }
                catch { }
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
                MessageBox.Show("Ошибка при получении внешнего IP-адреса. Будет использован локальный адрес.");
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
                socket = new Socket(IPAddress.Parse(IpAdressString).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
                //sock.EndConnect( ar );
                if (sock.Connected)
                    SetupRecieveCallback(sock);
                else
                    sock.EndConnect(ar);
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
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;
            stream.Position = 0;
            // Check if we got any data
            try
            {
                
                int nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {

                    int irc = reader.ReadInt32();
                    switch (irc)
                    {
                        case IRC_QUERIES.REQ_AUTH:
                            stream.Position = 0;
                            writer.Write(IRC_QUERIES.REQ_AUTH);
                            writer.Write(Dns.GetHostName());
                            writer.Write("111111");
                            writer.Write(GetMACAddress());
                            writer.Write(IRC_QUERIES.EndOfMessage);
                            sock.Send(m_byBuff);
                            break;

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
                        case IRC_QUERIES.GPUUNIT:
                            break;
                        case IRC_QUERIES.Board:
                            break;
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
