using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace Server
{

    public struct Configuration
    {
        public const string DB_USER = "user10870";
        public const string DB_PASS = "0lwHqEJe4X75";
        public const string DB_BASE = "user10870";
        public const string DB_HOST = "137.74.4.167";
    }

    public struct IRC_QUERIES
    {
        public const int REQ_AUTH = 1;
        const int RES_AUTH = 2;
        const int TEST = 8888;
        public const int ERROR_IRC = 0;
        public const int OPSYS = 3;
        public const int CPUUNIT = 4;
        public const int ERRONCLIENTSIDE = 9999;
    }

    public partial class Server : Form
    {
        Socket listener;
        MySqlConnection dbHandle;
        private static ArrayList m_aryClients = new ArrayList();	// Список подключенных клиентов
        static byte[] m_byBuff = new byte[1024]; // размер буфера
        public static MemoryStream stream = new MemoryStream(m_byBuff);
        static BinaryWriter writer = new BinaryWriter(stream);
        static BinaryReader reader = new BinaryReader(stream);
        static string EndofMessage = "<EOF>";
        Thread TimerThread;
        
        public Server()
        {
            InitializeComponent();
            notifyIcon1 = new NotifyIcon
            {
                Icon = SystemIcons.Asterisk,
                Visible = true
            };
            notifyIcon1.Click += NotifyIcon1_Click;
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            listView1.FullRowSelect = true;
            ListViewExtender extender = new ListViewExtender(listView1);
            ListViewButtonColumn buttonAction = new ListViewButtonColumn(3);
            buttonAction.Click += OnButtonActionClick;
            buttonAction.FixedWidth = true;
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            extender.AddColumn(buttonAction);
            InitializeMySQL();
            IntializeSocket();
            TimerThread = new Thread(UpdateTimer);
            TimerThread.Start(); //запускаем поток
            
        }

        private void NotifyIcon1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
            {
                this.Show();
                //WindowState = FormWindowState.Normal;
            }
        }

        private void OnButtonActionClick(object sender, ListViewColumnMouseEventArgs e)//Кнопка в листе
        {
            SocketManagment client = null;
            foreach (SocketManagment clients in m_aryClients)
            {
                if (clients.Sock.RemoteEndPoint.ToString() == e.Item.SubItems[1].Text) client = clients;
                break;
            }
            if (client == null) return;
            string text = String.Format("\t\t\tИнформация по клиенту - {0}\n\n" +
                "IP-адрес: {1}\n" +
                "Время подключения: {2}\n" +
                "\t\t\tИнформация о системе:\n\n" +
                "Операционная система: {3}\n\n" +
                "Дата установки: {4}\t\t Количество пользователей: {5}\n" +
                "Серийный номер: {6}\n\n" +
                "Процессор: {7}\n" +
                "Количество ядер: {8}\t\t Количество логических ядер: {9}\n" +
                "L2 Cache: {10} \t L3 Cache: {11}\t Socket Designation: {12}\n" +
                "Максимальная частота: {13}\t Роль: {14}\n" +
                "Id процессора: {15}\t Тип процессора: {16}\t Ревизия: {17}\n\n" +
                "Видеокарта: Geforce GTX 760\n" +
                "Название: Gigabyte\n" +
                "Частота: 6008 MHz\n" +
                "Объем памяти: 2048 MB\n\n" +
                "Материнская плата: M2N32-SLI DELUXE\n" +
                "Тип: Base Board\n" +
                "Производитель: ASUSTeK Computer INC\n" +
                "Серийный номер: 123456789000\n" +
                "", e.Item.Text, client.Sock.RemoteEndPoint.ToString(), client.time.ToString(),  client.OperationSistem[0] + "  " + client.OperationSistem[2] + "  " + client.OperationSistem[1],
                client.OperationSistem[3], client.OperationSistem[5], client.OperationSistem[6],
                client.CPUUNIT[5] + "  " + client.CPUUNIT[0] + " " + client.CPUUNIT[1], client.CPUUNIT[6], client.CPUUNIT[7], client.CPUUNIT[2], client.CPUUNIT[3], client.CPUUNIT[12],
                client.CPUUNIT[4], client.CPUUNIT[11], client.CPUUNIT[8], client.CPUUNIT[9], client.CPUUNIT[10]);
            MessageBox.Show(this, text, e.Item.SubItems[1].Text);
        }

        public delegate void UpdateTimeEx();
        public  void UpdateTime()
        {
            ArrayList clients = m_aryClients;
            foreach (SocketManagment client in clients)
            {
                string ip = client.Sock.RemoteEndPoint.ToString();
                long time = DateTime.Now.Ticks - client.time.Ticks;
                DateTime watch = new DateTime();
                watch = watch.AddTicks(time);
                foreach (ListViewItem item in listView1.Items)
                {
                    if (item.SubItems[1].Text == ip)
                    {
                        item.SubItems[2].Text = String.Format("{0:mm:ss}", watch);
                        int indexit = item.Index;
                        break;
                    }
                    
                }
            }
        }
        public void UpdateTimer()
        {
            while(true)
            {
                try
                {
                    Invoke(new UpdateTimeEx(UpdateTime), new object[] { });
                }
                catch { }
                Thread.Sleep(1000);
            }
        }

        private void InitializeMySQL()
        {
            string CommandText = "Test Connection";
            dbHandle = new MySqlConnection("Database=" + Configuration.DB_BASE + ";Data Source=" +  Configuration.DB_HOST + ";User Id=" + Configuration.DB_USER + ";Password=" + Configuration.DB_PASS + ";charset = utf8");
            MySqlCommand myCommand = new MySqlCommand(CommandText, dbHandle);
            try
            {
                dbHandle.Open();
                
                dbStatusLabel.Text = "Подключено ";
                dbStatusLabel.ForeColor = Color.Green;
                CheckBaseIntegrity();
            }
            catch (MySqlException e)
            {
                MessageBox.Show(e.ToString());
                dbStatusLabel.Text = "Отключено";
                dbStatusLabel.ForeColor = Color.Red;
            }
        }

        public delegate void AddNewClientDelegate(string name, string ip, int id);
        public void AddNewClient(string name, string ip, int id)
        {

            ListViewItem lvi = new ListViewItem();
            ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem lvsp = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem lvsb = new ListViewItem.ListViewSubItem();
            ListViewGroup lvg = new ListViewGroup
            {
                Header = name,
                HeaderAlignment = HorizontalAlignment.Center
            };
            lvi.Text = name;
            lvsi.Text = ip;
            lvi.SubItems.Add(lvsi);
            lvsp.Text = "00:00";
            lvsb.Text = "Подробнее";
            lvi.SubItems.Add(lvsp);
            lvi.SubItems.Add(lvsb);
            listView1.Items.Add(lvi);
            listView1.Groups.Add(lvg);
            int lid = listView1.Items.Count;
            int gid = listView1.Groups.Count;
            listView1.Items[lid-1].Group = listView1.Groups[gid-1];
            label6.Text = (Convert.ToInt32(label6.Text) + 1).ToString();
            AddNewConsoleMessage(String.Format("Подключен новый клиент: [{0}] {1}", name, ip));
        }

        public delegate void AddMessageToConsole(string text);
        public void AddNewConsoleMessage(string text)
        {
            textBox1.AppendText(">> " + text + "\n");
        }

        private delegate void DeleteClientFromList(SocketManagment client);
        private void DeleteClient(SocketManagment client)
        {
            string ip = client.Sock.RemoteEndPoint.ToString();
            label6.Text = (Convert.ToInt32(label6.Text) - 1).ToString();
            ListViewGroup lvg;
            foreach (ListViewItem item in listView1.Items)
            {
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    if (subitem.Text == ip)
                    {
                        lvg = item.Group;
                        item.Remove();
                        listView1.Update();

                        return;
                    }
                }
                
            }
        }

        private string GetExternalIp()
        {
            try
            {
                string externalIP;
                externalIP = (new WebClient()).DownloadString("http://checkip.dyndns.org/");
                externalIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                             .Matches(externalIP)[0].ToString();
                return externalIP;
            }
            catch { return null; }
        }

        private void IntializeSocket()
        {
            const int nPortListen = 7777;
            String strHostName = "";
            IPAddress.TryParse(GetExternalIp(), out IPAddress aryLocalAddr);
            if (aryLocalAddr == null || aryLocalAddr == IPAddress.None)
            {
                try
                {
                    strHostName = Dns.GetHostName();
                    IPHostEntry ipEntry = Dns.GetHostByName(strHostName);
                    aryLocalAddr = ipEntry.AddressList[0];
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при попытке получить локальный адресс: " + ex.Message);
                }
            }
            if (aryLocalAddr == null)
            {
                MessageBox.Show("Невозможно получить локальный адрес");
                label4.Text = "Отключено";
                label4.ForeColor = Color.Red;
                label10.Text = "";
                return;
            }
            //MessageBox.Show("Активировано прослушивание: [" + strHostName +"] " + aryLocalAddr[0] + ":" + nPortListen);
            label4.Text = "Подключено";
            label4.ForeColor = Color.Green;
            label10.Text = aryLocalAddr + ":" + nPortListen;
            try
            {
                listener = new Socket(aryLocalAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, nPortListen));
                listener.Listen(100);
                listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void OnConnectRequest(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            if (!listener.Blocking) return;
            OnNewConnection(listener.EndAccept(ar));
            listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
        }

        public void OnNewConnection(Socket sockClient)
        {
            //SocketChatClient client = new SocketChatClient( listener.AcceptSocket() );
            SocketManagment client = new SocketManagment(sockClient);
            m_aryClients.Add(client);
            stream.Position = 0;
            writer.Write(IRC_QUERIES.REQ_AUTH);
            writer.Write(EndofMessage);
            client.Sock.Send(m_byBuff);
            client.SetupRecieveCallback(this);
        }

        public void OnRecievedData(IAsyncResult ar)
        {
            SocketManagment client = (SocketManagment)ar.AsyncState;
            byte[] cread = client.GetRecievedData(ar, out SocketError error);
            if (error != SocketError.Success)
            {
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Попытка получения данных от [{0}]", client.Sock.RemoteEndPoint) });
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Результат: {0}", error.ToString()) });
            }
            if (cread.Length < 1)
            {
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Клиент [{0}] отключен.", client.Sock.RemoteEndPoint) });
                Invoke(new DeleteClientFromList(DeleteClient), new object[] { client });
                client.Sock.Close();
                m_aryClients.Remove(client);
                //client.SetupRecieveCallback(this);
                return;
            }
            MemoryStream mem = new MemoryStream(cread);
            BinaryReader read = new BinaryReader(mem);
            int irc = -1;
            try
            {
                irc = read.ReadInt32();
            }
            catch { Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Принят битый пакет от: [{0}].", client.Sock.RemoteEndPoint) }); }
            switch (irc)
            {
                case IRC_QUERIES.REQ_AUTH:
                    string clientname = read.ReadString();
                    string key = read.ReadString();
                    string mac = read.ReadString();
                    if (key == "111111")
                    {
                        int id = CheckRegister(mac);
                        if (id != -1 && id != 0)
                        {
                            client.name = clientname;
                            client.Clientid = id;
                            client.time = DateTime.Now;
                            Invoke(new AddNewClientDelegate(AddNewClient), new object[] { clientname, client.Sock.RemoteEndPoint.ToString(), id });
                            stream.Position = 0;
                            writer.Write(IRC_QUERIES.OPSYS);
                            client.Sock.Send(m_byBuff);
                            break;
                        }
                        else
                        {
                            RegisterNewClient(clientname, mac);
                            id = CheckRegister(mac);
                            client.Clientid = id;
                            client.name = clientname;
                            client.time = DateTime.Now;
                            Invoke(new AddNewClientDelegate(AddNewClient), new object[] { clientname, client.Sock.RemoteEndPoint.ToString(), id });
                            stream.Position = 0;
                            writer.Write(IRC_QUERIES.OPSYS);
                            client.Sock.Send(m_byBuff);
                            break;
                        }
                    }
                    else  client.Sock.Close();
                    break;
                case IRC_QUERIES.OPSYS:
                    for (int i = 0; i < 7; i++)
                    {
                        string rd = read.ReadString();
                        if (rd == EndofMessage) break;
                        client.OperationSistem[i] = rd;
                    }
                    stream.Position = 0;
                    writer.Write(IRC_QUERIES.CPUUNIT);
                    writer.Write(EndofMessage);
                    client.Sock.Send(m_byBuff);
                    break;
                case IRC_QUERIES.CPUUNIT:
                    for (int i = 0; i < 15; i++)
                    {
                        string rd = read.ReadString();
                        if (rd == EndofMessage) break;
                        client.CPUUNIT[i] = rd;
                    }
                    CheckMySQLInformation(client.Clientid, client);
                    break;
                case IRC_QUERIES.ERROR_IRC:

                    break;
                default:
                    break;
            }
            client.SetupRecieveCallback(this);
        }

        public void CheckMySQLInformation(int id, SocketManagment client)
        {
            if (dbHandle.State.ToString() != "Open")
            {
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Соединение с базой данной разорвано!") });
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("MySQL DataBase Status: {0}", dbHandle.State.ToString()) });
                return;
            }
            string sql = String.Format("SELECT * FROM operationsys WHERE systemid = {0} LIMIT 1", id);
            MySqlCommand cmd = new MySqlCommand(sql, dbHandle);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    string s = dataReader.GetString(1);
                    if (s != client.OperationSistem[0]) { MessageBox.Show(s); }
                    if (dataReader.GetString(2) != client.OperationSistem[1]) { }
                    if (dataReader.GetString(3) != client.OperationSistem[2]) { }
                    if (dataReader.GetString(4) != client.OperationSistem[3]) { }
                    if (dataReader.GetString(5) != client.OperationSistem[4]) { }
                    if (dataReader.GetString(6) != client.OperationSistem[5]) { }
                    if (dataReader.GetString(7) != client.OperationSistem[6]) { }
                }
            }
            else
            {
                dataReader.Close();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                sql = String.Format("INSERT INTO operationsys(Name, Version, CDVersion, InstallDate, NumberOfProcesses, NumberOfUsers, SerialNumber, systemid) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}')",
                    client.OperationSistem[0],
                    client.OperationSistem[1],
                     client.OperationSistem[2],
                      client.OperationSistem[3],
                       client.OperationSistem[4],
                        client.OperationSistem[5],
                         client.OperationSistem[6],
                         id);
                try
                {
                    cmd = new MySqlCommand(sql, dbHandle);
                    cmd.ExecuteNonQuery();
                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                }
                catch
                {
                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                }
            }

            sql = String.Format("SELECT * FROM cpuunit WHERE systemid = {0} LIMIT 1", id);
            cmd = new MySqlCommand(sql, dbHandle);
            dataReader.Close();
            dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    if (dataReader.GetString(1) != client.CPUUNIT[5]) { }
                    if (dataReader.GetString(2) != client.CPUUNIT[0]) { }
                    if (dataReader.GetString(3) != client.CPUUNIT[1]) { }
                    if (dataReader.GetString(4) != client.CPUUNIT[2]) { }
                    if (dataReader.GetString(5) != client.CPUUNIT[3]) { }
                    if (dataReader.GetString(6) != client.CPUUNIT[4]) { }
                    if (dataReader.GetString(7) != client.CPUUNIT[6]) { }
                    if (dataReader.GetString(8) != client.CPUUNIT[7]) { }
                    if (dataReader.GetString(9) != client.CPUUNIT[8]) { }
                    if (dataReader.GetString(10) != client.CPUUNIT[9]) { }
                    if (dataReader.GetString(11) != client.CPUUNIT[10]) { }
                    if (dataReader.GetString(12) != client.CPUUNIT[11]) { }
                    if (dataReader.GetString(13) != client.CPUUNIT[12]) { }
                }
            }
            else
            {
                dataReader.Close();
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                sql = String.Format("INSERT INTO cpuunit(`Name`, `Description`, `DeviceID`, `L2CacheSize`, `L3CacheSize`, `MaxClockSpeed`, `NumberOfCores`, `NumberOfLogicalProcessors`, `ProcessorId`, `ProcessorType`, `Revision`, `Role`, `SocketDesignation`, `systemid`) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')",
                    client.CPUUNIT[5],
                    client.CPUUNIT[0],
                    client.CPUUNIT[1],
                     client.CPUUNIT[2],
                      client.CPUUNIT[3],
                       client.CPUUNIT[4],
                        client.CPUUNIT[6],
                         client.CPUUNIT[7],
                          client.CPUUNIT[8],
                           client.CPUUNIT[9],
                            client.CPUUNIT[10],
                             client.CPUUNIT[11],
                              client.CPUUNIT[12],
                              id);
                try
                {
                    cmd = new MySqlCommand(sql, dbHandle);
                    cmd.ExecuteNonQuery();
                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                }
                catch
                {
                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                }
            }
            
            dataReader.Close();
        }
        private int CheckRegister(string mac)
        {
            int id = -1;
            string queryString = @"SELECT id FROM systems WHERE mac = '" + mac + "' LIMIT 1";
            MySqlCommand com = new MySqlCommand(queryString, dbHandle);
            try
            {
                id = Convert.ToInt32(com.ExecuteScalar());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return id;
        }

        private void RegisterNewClient(string name, string mac)
        {
            try
            {
                string queryString = String.Format("INSERT INTO systems (name, mac) VALUES('{0}', '{1}')", name, mac);
                MySqlCommand com = new MySqlCommand(queryString, dbHandle);
                com.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            TimerThread.Abort();
            try
            {
                if (listener != null)
                {
                    listener.Blocking = false;
                    listener.Close();
                }
            }
            catch (Exception es)
            {
                MessageBox.Show(es.ToString());
            }
            if (dbHandle != null)
            {
                try
                {
                    dbHandle.Close();
                }
                catch (Exception es)
                {
                    MessageBox.Show(es.ToString());
                }
            }
        }

        private void ListView1_StyleChanged(object sender, EventArgs e)
        {

        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (listView1.SelectedItems.Count > 0)
            {
                string ip = listView1.SelectedItems[0].SubItems[1].Text;
                foreach (SocketManagment client in m_aryClients)
                {
                    if (client.Sock.RemoteEndPoint.ToString() == ip)
                    {
                        MessageBox.Show(client.OperationSistem[0] + client.OperationSistem[1] + client.OperationSistem[2]);
                        MessageBox.Show(client.CPUUNIT[0] + client.CPUUNIT[1] + client.CPUUNIT[2]);
                    }
                }
            }*/
        }

        bool cancelevent = true;
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = cancelevent;
            this.Hide();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cancelevent = false;
            Application.Exit();
        }

        private void CheckBaseIntegrity()
        {
            MySqlDataReader MyDataReader;
            int tablecount = 0;
            MySqlCommand com = new MySqlCommand("SHOW TABLES FROM `" + Configuration.DB_USER + "`", dbHandle);
            MyDataReader = com.ExecuteReader();
            string[] Tables = new String[10];
            if (MyDataReader.HasRows)
            {
                while (MyDataReader.Read())
                {
                    Tables[tablecount] = MyDataReader.GetString(0);
                    tablecount++;
                }
                if (!Tables.Contains("systems")) { }
                if (!Tables.Contains("operationsys")) { }
                if (!Tables.Contains("cpuunit")) { }
                //if (!Tables.Contains("system")) { }
            }
        }

    }




    public class SocketManagment
    {
        private Socket m_sock;                      // Connection to the client
        static byte[] m_byBuff = new byte[1024];		// Receive data buffer
        static MemoryStream mem = new MemoryStream(m_byBuff);
        static BinaryReader reader = new BinaryReader(mem);
        public string name = String.Empty;
        public DateTime time = new DateTime();
        /*[Name,Version, CDVersion, InstallDate, NumberOfProcesses, NumberOfUsers, SerialNumber]*/
        public string[] OperationSistem = new string[7];
        public string[] CPUUNIT = new string[15];
        public int Clientid = -1;
        public Socket Sock
        {
            get { return m_sock; }
        }
        public SocketManagment(Socket sock)
        {
            m_sock = sock;
        }
        public void SetupRecieveCallback(Server main)
        {

            try
            {
                AsyncCallback recieveData = new AsyncCallback(main.OnRecievedData);
                m_sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, this);
            }
            catch (Exception ex) { MessageBox.Show(String.Format("Не удалось подключить функцию получения собщений! {0}", ex.Message)); }
        }
        public byte[] GetRecievedData(IAsyncResult ar, out SocketError SockError)
        {
            int nBytesRec = 0;
            SockError = SocketError.Success;
            try
            {
                nBytesRec = m_sock.EndReceive(ar, out SockError);
            }
            catch { }
            byte[] byReturn = new byte[nBytesRec];
            Array.Copy(m_byBuff, byReturn, nBytesRec);
            /*
			int nToBeRead = m_sock.Available;
			if( nToBeRead > 0 )
			{
				byte [] byData = new byte[nToBeRead];
				m_sock.Receive( byData );
				
			}
			*/
            return byReturn;
        }

    }


    public class ListViewExtender : IDisposable
    {
        private readonly Dictionary<int, ListViewColumn> _columns = new Dictionary<int, ListViewColumn>();

        public ListViewExtender(ListView listView)
        {
            if (listView == null)
                throw new ArgumentNullException("listView");

            if (listView.View != View.Details)
                throw new ArgumentException(null, "listView");

            ListView = listView;
            ListView.OwnerDraw = true;
            ListView.DrawItem += OnDrawItem;
            ListView.DrawSubItem += OnDrawSubItem;
            ListView.DrawColumnHeader += OnDrawColumnHeader;
            ListView.MouseMove += OnMouseMove;
            ListView.MouseClick += OnMouseClick;

            Font = new Font(ListView.Font.FontFamily, ListView.Font.Size - 2);
        }

        public virtual Font Font { get; private set; }
        public ListView ListView { get; private set; }

        protected virtual void OnMouseClick(object sender, MouseEventArgs e)
        {
            ListViewColumn column = GetColumnAt(e.X, e.Y, out ListViewItem item, out ListViewItem.ListViewSubItem sub);
            if (column != null)
            {
                column.MouseClick(e, item, sub);
            }
        }

        public ListViewColumn GetColumnAt(int x, int y, out ListViewItem item, out ListViewItem.ListViewSubItem subItem)
        {
            subItem = null;
            item = ListView.GetItemAt(x, y);
            if (item == null)
                return null;

            subItem = item.GetSubItemAt(x, y);
            if (subItem == null)
                return null;

            for (int i = 0; i < item.SubItems.Count; i++)
            {
                if (item.SubItems[i] == subItem)
                    return GetColumn(i);
            }
            return null;
        }

        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
            ListViewColumn column = GetColumnAt(e.X, e.Y, out ListViewItem item, out ListViewItem.ListViewSubItem sub);
            if (column != null)
            {
                column.Invalidate(item, sub);
                return;
            }
            if (item != null)
            {
                ListView.Invalidate(item.Bounds);
            }
        }

        protected virtual void OnDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        protected virtual void OnDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            ListViewColumn column = GetColumn(e.ColumnIndex);
            if (column == null)
            {
                e.DrawDefault = true;
                return;
            }

            column.Draw(e);
        }

        protected virtual void OnDrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // do nothing
        }

        public void AddColumn(ListViewColumn column)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            column.Extender = this;
            _columns[column.ColumnIndex] = column;
        }

        public ListViewColumn GetColumn(int index)
        {
            return _columns.TryGetValue(index, out ListViewColumn column) ? column : null;
        }

        public IEnumerable<ListViewColumn> Columns
        {
            get
            {
                return _columns.Values;
            }
        }

        public virtual void Dispose()
        {
            if (Font != null)
            {
                Font.Dispose();
                Font = null;
            }
        }
    }

    public abstract class ListViewColumn
    {
        public event EventHandler<ListViewColumnMouseEventArgs> Click;

        protected ListViewColumn(int columnIndex)
        {
            if (columnIndex < 0)
                throw new ArgumentException(null, "columnIndex");

            ColumnIndex = columnIndex;
        }

        public virtual ListViewExtender Extender { get; protected internal set; }
        public int ColumnIndex { get; private set; }

        public virtual Font Font
        {
            get
            {
                return Extender?.Font;
            }
        }

        public ListView ListView
        {
            get
            {
                return Extender?.ListView;
            }
        }

        public abstract void Draw(DrawListViewSubItemEventArgs e);

        public virtual void MouseClick(MouseEventArgs e, ListViewItem item, ListViewItem.ListViewSubItem subItem)
        {
            Click?.Invoke(this, new ListViewColumnMouseEventArgs(e, item, subItem));
        }

        public virtual void Invalidate(ListViewItem item, ListViewItem.ListViewSubItem subItem)
        {
            if (Extender != null)
            {
                Extender.ListView.Invalidate(subItem.Bounds);
            }
        }
    }

    public class ListViewColumnMouseEventArgs : MouseEventArgs
    {
        public ListViewColumnMouseEventArgs(MouseEventArgs e, ListViewItem item, ListViewItem.ListViewSubItem subItem)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            Item = item;
            SubItem = subItem;
        }

        public ListViewItem Item { get; private set; }
        public ListViewItem.ListViewSubItem SubItem { get; private set; }
    }

    public class ListViewButtonColumn : ListViewColumn
    {
        private Rectangle _hot = Rectangle.Empty;

        public ListViewButtonColumn(int columnIndex)
            : base(columnIndex)
        {
        }

        public bool FixedWidth { get; set; }
        public bool DrawIfEmpty { get; set; }

        public override ListViewExtender Extender
        {
            get
            {
                return base.Extender;
            }
            protected internal set
            {
                base.Extender = value;
                if (FixedWidth)
                {
                    base.Extender.ListView.ColumnWidthChanging += OnColumnWidthChanging;
                }
            }
        }

        protected virtual void OnColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            if (e.ColumnIndex == ColumnIndex)
            {
                e.Cancel = true;
                e.NewWidth = ListView.Columns[e.ColumnIndex].Width;
            }
        }

        public override void Draw(DrawListViewSubItemEventArgs e)
        {
            if (_hot != Rectangle.Empty)
            {
                if (_hot != e.Bounds)
                {
                    ListView.Invalidate(_hot);
                    _hot = Rectangle.Empty;
                }
            }

            if ((!DrawIfEmpty) && (string.IsNullOrEmpty(e.SubItem.Text)))
                return;

            Point mouse = e.Item.ListView.PointToClient(Control.MousePosition);
            if ((ListView.GetItemAt(mouse.X, mouse.Y) == e.Item) && (e.Item.GetSubItemAt(mouse.X, mouse.Y) == e.SubItem))
            {
                ButtonRenderer.DrawButton(e.Graphics, e.Bounds, e.SubItem.Text, Font, true, PushButtonState.Hot);
                _hot = e.Bounds;
            }
            else
            {
                ButtonRenderer.DrawButton(e.Graphics, e.Bounds, e.SubItem.Text, Font, false, PushButtonState.Default);
            }
        }
    }



}
