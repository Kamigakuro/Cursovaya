using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Server
{
    public struct IRC_QUERIES
    {
        public const int REQ_AUTH = 1;
        const int RES_AUTH = 2;
        const int TEST = 8888;
        public const int ERROR_IRC = 0;
        public const int OPSYS = 3;
        public const int CPUUNIT = 4;
        public const int GPUUNIT = 5;
        public const int Board = 6;
        public const int RAM = 7;
        public const int Products = 8;
        public const int ERRONCLIENTSIDE = 9999;
    }
    public partial class Server
    {
        //Socket listener;
        private static ArrayList m_aryClients = new ArrayList();	// Список подключенных клиентов
        static byte[] m_byBuff = new byte[1024]; // размер буфера
        public static MemoryStream stream = new MemoryStream(m_byBuff);
        static BinaryWriter writer = new BinaryWriter(stream);
        static BinaryReader reader = new BinaryReader(stream);
        static string EndofMessage = "<EOF>";
        public SSocket socket;
        //LinkedList<QueryElement> link = new LinkedList<QueryElement>();

        /// <summary>
        /// Соединение с базой данных
        /// </summary>
        private void InitializeMySQL()
        {
            DB.OpenConnection("user10870", "0lwHqEJe4X75", "user10870", "137.74.4.167");
            if (DB.SqlConnection == ConnectionState.Open)
            {
                dbStatusLabel.Text = "Подключено ";
                dbStatusLabel.ForeColor = Color.Green;
                DB.CheckBaseIntegrity("user10870");

            }
            else
            {
                dbStatusLabel.Text = "Отключено";
                dbStatusLabel.ForeColor = Color.Red;
            }
        }
        /// <summary>
        /// Получение внешнего IP-адреса
        /// </summary>
        /// <returns>IP-адрес ввиде строки</returns>
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
        /// <summary>
        /// Метод получения локального IP-адреса
        /// </summary>
        /// <returns></returns>
        private IPAddress GetLocalIP()
        {
            try
            {
                String strHostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostByName(strHostName);
                IPAddress aryLocalAddr = ipEntry.AddressList[0];
                return aryLocalAddr;
            }
            catch (Exception e)
            {
                string mess = String.Format("Ошибка при попытке получить локальный адрес: {0}", e.Message);
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                return null;
            }
        }
        /// <summary>
        /// Включение прослушивания сокетом по порту
        /// </summary>
        private void IntializeSocket()
        {
            socket = new SSocket();
            const int nPortListen = 7777;
            IPAddress.TryParse(GetExternalIp(), out IPAddress aryLocalAddr);
            if (aryLocalAddr == null || aryLocalAddr == IPAddress.None) aryLocalAddr = GetLocalIP();
            if (aryLocalAddr == null)
            {
                string mess = String.Format("Невозможно получить адрес сервера.");
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                label4.Text = "Отключено";
                label4.ForeColor = Color.Red;
                label10.Text = "";
                return;
            }
            if (socket == null)
            {
                socket = new SSocket();
                if (!socket.StartSocket(SettingsClass.SPort, aryLocalAddr, 10))
                {
                    string mess = String.Format("Невозможно подключить службу сообщений.");
                    ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                    label4.Text = "Отключено";
                    label4.ForeColor = Color.Red;
                    label10.Text = "";
                }
                else
                {
                    string localip = "" + GetLocalIP().ToString();
                    label4.Text = "Подключено";
                    label4.ForeColor = Color.Green;
                    label10.Text = aryLocalAddr + ":" + nPortListen + " (" + localip + ")";
                }
            }
   
            /*try
            {
                listener = new Socket(aryLocalAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, nPortListen));
                listener.Listen(100);
                listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                string mess = String.Format("Ошибка при попытке получить локальный адрес: {0}", e.Message);
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
            }*/
        }
        /// <summary>
        /// Обработчик событий при получении каких-либо данных
        /// </summary>
        /// <param name="ar"></param>
        public void OnConnectRequest(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            if (!listener.Blocking) return;
            listener.Blocking = false;
            OnNewConnection(listener.EndAccept(ar));
            listener.Blocking = true;
            listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
        }
        /// <summary>
        /// Метод вызываемый при подключении новых соединений
        /// </summary>
        /// <param name="sockClient"></param>
        public void OnNewConnection(Socket sockClient)
        {
            SocketClient client = new SocketClient(sockClient);
            m_aryClients.Add(client);
            stream.Position = 0;
            writer.Write(IRC_QUERIES.REQ_AUTH);
            writer.Write(EndofMessage);
            client.Sock.Send(m_byBuff);
            client.SetupRecieveCallback(this);
        }
        /// <summary>
        /// Метод, вызываемый после получения данных от клиента
        /// </summary>
        /// <param name="ar"></param>
        public void OnRecievedData(IAsyncResult ar)
        {
            SocketClient client = (SocketClient)ar.AsyncState;
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
            catch (Exception e)
            {
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Принят битый пакет от: [{0}].", client.Sock.RemoteEndPoint) });
                string mess = String.Format("Принят битый пакет от: {0}. {1}", client.Sock.RemoteEndPoint, e.ToString());
                ErrorsListForm.AddQuery(mess, client, QueryElement.QueryType.ClientError);

            }
            switch (irc)
            {
                #region Авторизация
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
                           // stream.Position = 0;
                           // writer.Write(IRC_QUERIES.OPSYS);
                           // client.Sock.Send(m_byBuff);
                            break;
                        }
                        else
                        {
                            RegisterNewClient(clientname, mac);
                            id = CheckRegister(mac);
                            //id = cons;
                            client.Clientid = id;
                            client.name = clientname;
                            client.time = DateTime.Now;
                            Invoke(new AddNewClientDelegate(AddNewClient), new object[] { clientname, client.Sock.RemoteEndPoint.ToString(), id });
                            //stream.Position = 0;
                            //writer.Write(IRC_QUERIES.OPSYS);
                            //client.Sock.Send(m_byBuff);
                            break;
                        }
                    }
                    else client.Sock.Close();
                    break;
                #endregion
                #region ОС
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
                    CheckMySQLInformation(client.Clientid, client, 0);
                    break;
                #endregion
                #region Процессор
                case IRC_QUERIES.CPUUNIT:
                    for (int i = 0; i < 15; i++)
                    {
                        string rd = read.ReadString();
                        if (rd == EndofMessage) break;
                        client.CPUUNIT[i] = rd;
                    }
                    stream.Position = 0;
                    writer.Write(IRC_QUERIES.GPUUNIT);
                    writer.Write(EndofMessage);
                    client.Sock.Send(m_byBuff);
                    CheckMySQLInformation(client.Clientid, client, 1);
                    break;
                #endregion
                #region Видео
                case IRC_QUERIES.GPUUNIT:
                    for (int i = 0; i < 15; i++)
                    {
                        string rd = read.ReadString();
                        if (rd == EndofMessage) break;
                        client.GPUUNIT[i] = rd;
                    }
                    stream.Position = 0;
                    writer.Write(IRC_QUERIES.Board);
                    writer.Write(EndofMessage);
                    client.Sock.Send(m_byBuff);
                    CheckMySQLInformation(client.Clientid, client, 2);
                    break;
                #endregion
                #region Материнка
                case IRC_QUERIES.Board:
                    for (int i = 0; i < 12; i++)
                    {
                        string rd = read.ReadString();
                        if (rd == EndofMessage) break;
                        client.Board[i] = rd;
                    }
                    stream.Position = 0;
                    writer.Write(IRC_QUERIES.RAM);
                    writer.Write(EndofMessage);
                    client.Sock.Send(m_byBuff);
                    CheckMySQLInformation(client.Clientid, client, 3);
                    break;
                #endregion
                #region RAM
                case IRC_QUERIES.RAM:
                    int count = read.ReadInt32();
                    for (int k = 0; k < count; k++)
                    {
                        object[] array = new object[16];
                        for (int i = 0; i < 16; i++)
                        {
                            string rd = read.ReadString();
                            if (rd == EndofMessage) break;
                            array[i] = rd;
                        }
                        client.RAM.Rows.Add(array);
                    }
                    stream.Position = 0;
                    writer.Write(IRC_QUERIES.Products);
                    writer.Write(EndofMessage);
                    client.Sock.Send(m_byBuff);
                    CheckMySQLInformation(client.Clientid, client, 4);
                    break;
                #endregion
                #region Products
                case IRC_QUERIES.Products:
                    object[] arrayr = new object[5];
                    for (int i = 0; i < 5; i++)
                    {
                        string rd = read.ReadString();
                        if (rd == EndofMessage)
                        {
                            CheckMySQLInformation(client.Clientid, client, 5);
                            client.SetupRecieveCallback(this);
                            return;
                        }
                        arrayr[i] = rd;
                    }
                    client.Products.Rows.Add(arrayr);
                    stream.Position = 0;
                    writer.Write(IRC_QUERIES.Products);
                    writer.Write(EndofMessage);
                    client.Sock.Send(m_byBuff);
                    break;
                #endregion
                case IRC_QUERIES.ERROR_IRC:
                    break;
                default:
                    break;
            }
            client.SetupRecieveCallback(this);
        }
        /// <summary>
        /// Метод проверки данных клиента с базой данных сервера
        /// </summary>
        /// <param name="id"></param>
        /// <param name="client"></param>
        /// <param name="check">Указать что проверять! 0 - ОС, 1 - Процессор, 2 - Видео</param>
        public void CheckMySQLInformation(int id, SocketClient client, int check)
        {
            //--------------------------------Проверка соединения с БД ----------------------------------------
            if (DB.SqlConnection != ConnectionState.Open)
            {
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Соединение с базой данной разорвано!") });
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("MySQL DataBase Status: {0}", DB.SqlConnection.ToString()) });
                return;
            }
            //-------------------------------------------------------------------------------------------------
            string sql;
            DataTable dataReader;
            switch (check)
            {
                case 0:
                    {
                        sql = String.Format("SELECT * FROM operationsys WHERE systemid = {0} LIMIT 1", id);
                        
                        dataReader = DB.SendTQuery(sql);
                        if (dataReader.Rows.Count > 0)
                        {
                            foreach (DataRow row in dataReader.Rows)
                            {
                                string s = row.Field<string>(1);
                                if (s != client.OperationSistem[0])
                                {
                                    ErrorsListForm.AddQuery("Несовпадение значений!\tБыло: " + s + " Стало: " + client.OperationSistem[0], client, QueryElement.QueryType.DBError, String.Format("UPDATE operationsys SET Name = '{0}' WHERE systemid = {1}", client.OperationSistem[0], id));
                                }
                                if (row.Field<string>(2) != client.OperationSistem[1]) { ErrorsListForm.AddQuery("operationsys: Несовпадение значений Version!\tБыло: " + row.Field<string>(2) + " Стало: " + client.OperationSistem[1], client, QueryElement.QueryType.DBError, String.Format("UPDATE operationsys SET Version = '{0}' WHERE systemid = {1}", client.OperationSistem[1], id)); }
                                if (row.Field<string>(3) != client.OperationSistem[2]) { ErrorsListForm.AddQuery("operationsys: Несовпадение значений CDVersion!\tБыло: " + row.Field<string>(3) + " Стало: " + client.OperationSistem[2], client, QueryElement.QueryType.DBError, String.Format("UPDATE operationsys SET CDVersion = '{0}' WHERE systemid = {1}", client.OperationSistem[2], id)); }
                                if (row.Field<string>(4) != client.OperationSistem[3]) { ErrorsListForm.AddQuery("operationsys: Несовпадение значений InstallDate!\tБыло: " + row.Field<string>(4) + " Стало: " + client.OperationSistem[3], client, QueryElement.QueryType.DBError, String.Format("UPDATE operationsys SET InstallDate = '{0}' WHERE systemid = {1}", client.OperationSistem[3], id)); }
                                if (row.Field<string>(5) != client.OperationSistem[4]) { ErrorsListForm.AddQuery("operationsys: Несовпадение значений NumberOfProcesses!\tБыло: " + row.Field<string>(5) + " Стало: " + client.OperationSistem[4], client, QueryElement.QueryType.DBError, String.Format("UPDATE operationsys SET NumberOfProcesses = '{0}' WHERE systemid = {1}", client.OperationSistem[4], id)); }
                                if (row.Field<string>(6) != client.OperationSistem[5]) { ErrorsListForm.AddQuery("operationsys: Несовпадение значений NumberOfUsers!\tБыло: " + row.Field<string>(6) + " Стало: " + client.OperationSistem[5], client, QueryElement.QueryType.DBError, String.Format("UPDATE operationsys SET NumberOfUsers = '{0}' WHERE systemid = {1}", client.OperationSistem[5], id)); }
                                if (row.Field<string>(7) != client.OperationSistem[6]) { ErrorsListForm.AddQuery("operationsys: Несовпадение значений SerialNumber!\tБыло: " + row.Field<string>(7) + " Стало: " + client.OperationSistem[6], client, QueryElement.QueryType.DBError, String.Format("UPDATE operationsys SET SerialNumber = '{0}' WHERE systemid = {1}", client.OperationSistem[6], id)); }
                            }
                        }
                        else
                        {
                            dataReader.Clear();
                            dataReader.Dispose();
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
                                DB.SendNonQuery(sql);
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                                break;
                            }
                            catch (MySqlException me)
                            {
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                                string mess = String.Format("Ошибка при добавлении нового компонента в таблицу. {0}", me.ToString());
                                ErrorsListForm.AddQuery(mess, client, QueryElement.QueryType.ClientError);
                            }
                        }
                        dataReader.Clear();
                        dataReader.Dispose();
                        break;
                    }
                case 1:
                    {
                        sql = String.Format("SELECT * FROM cpuunit WHERE systemid = {0} LIMIT 1", id);
                        dataReader = DB.SendTQuery(sql);
                        if (dataReader.Rows.Count > 0)
                        {
                            foreach (DataRow row in dataReader.Rows)
                            {
                                if (row.Field<string>(1) != client.CPUUNIT[5]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений Name!\tБыло: " + row.Field<string>(1) + " Стало: " + client.CPUUNIT[5], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET Name = '{0}' WHERE systemid = {1}", client.CPUUNIT[5], id)); }
                                if (row.Field<string>(2) != client.CPUUNIT[0]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений Description!\tБыло: " + row.Field<string>(2) + " Стало: " + client.CPUUNIT[0], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET Description = '{0}' WHERE systemid = {1}", client.CPUUNIT[0], id)); }
                                if (row.Field<string>(3) != client.CPUUNIT[1]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений DeviceID!\tБыло: " + row.Field<string>(3) + " Стало: " + client.CPUUNIT[1], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET DeviceID = '{0}' WHERE systemid = {1}", client.CPUUNIT[1], id)); }
                                if (row.Field<string>(4) != client.CPUUNIT[2]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений L2CacheSize!\tБыло: " + row.Field<string>(4) + " Стало: " + client.CPUUNIT[2], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET L2CacheSize = '{0}' WHERE systemid = {1}", client.CPUUNIT[2], id)); }
                                if (row.Field<string>(5) != client.CPUUNIT[3]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений L3CacheSize!\tБыло: " + row.Field<string>(5) + " Стало: " + client.CPUUNIT[3], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET L3CacheSize = '{0}' WHERE systemid = {1}", client.CPUUNIT[3], id)); }
                                if (row.Field<string>(6) != client.CPUUNIT[4]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений MaxClockSpeed!\tБыло: " + row.Field<string>(6) + " Стало: " + client.CPUUNIT[4], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET MaxClockSpeed = '{0}' WHERE systemid = {1}", client.CPUUNIT[4], id)); }
                                if (row.Field<string>(7) != client.CPUUNIT[6]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений NumberOfCores!\tБыло: " + row.Field<string>(7) + " Стало: " + client.CPUUNIT[6], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET NumberOfCores = '{0}' WHERE systemid = {1}", client.CPUUNIT[6], id)); }
                                if (row.Field<string>(8) != client.CPUUNIT[7]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений NumberOfLogicalProcessors!\tБыло: " + row.Field<string>(8) + " Стало: " + client.CPUUNIT[7], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET NumberOfLogicalProcessors = '{0}' WHERE systemid = {1}", client.CPUUNIT[7], id)); }
                                if (row.Field<string>(9) != client.CPUUNIT[8]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений ProcessorId!\tБыло: " + row.Field<string>(9) + " Стало: " + client.CPUUNIT[8], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET ProcessorId = '{0}' WHERE systemid = {1}", client.CPUUNIT[8], id)); }
                                if (row.Field<string>(10) != client.CPUUNIT[9]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений ProcessorType!\tБыло: " + row.Field<string>(10) + " Стало: " + client.CPUUNIT[9], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET ProcessorType = '{0}' WHERE systemid = {1}", client.CPUUNIT[9], id)); }
                                if (row.Field<string>(11) != client.CPUUNIT[10]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений Revision!\tБыло: " + row.Field<string>(11) + " Стало: " + client.CPUUNIT[10], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET Revision = '{0}' WHERE systemid = {1}", client.CPUUNIT[10], id)); }
                                if (row.Field<string>(12) != client.CPUUNIT[11]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений Role!\tБыло: " + row.Field<string>(12) + " Стало: " + client.CPUUNIT[11], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET Role = '{0}' WHERE systemid = {1}", client.CPUUNIT[11], id)); }
                                if (row.Field<string>(13) != client.CPUUNIT[12]) { ErrorsListForm.AddQuery("cpuunit: Несовпадение значений SocketDesignation!\tБыло: " + row.Field<string>(13) + " Стало: " + client.CPUUNIT[12], client, QueryElement.QueryType.DBError, String.Format("UPDATE cpuunit SET SocketDesignation = '{0}' WHERE systemid = {1}", client.CPUUNIT[12], id)); }
                            }
                        }
                        else
                        {
                            dataReader.Clear();
                            dataReader.Dispose();
                            //   cmd.ExecuteNonQuery();
                            //   cmd.Dispose();
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
                                DB.SendNonQuery(sql);
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                                break;
                            }
                            catch (MySqlException me)
                            {
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                                string mess = String.Format("Ошибка при добавлении нового компонента в таблицу. {0}", me.ToString());
                                ErrorsListForm.AddQuery(mess, client, QueryElement.QueryType.ClientError);
                            }
                        }
                        dataReader.Clear();
                        dataReader.Dispose();
                        break;
                    }
                case 2:
                    {
                        sql = String.Format("SELECT * FROM gpuunit WHERE systemid = {0} LIMIT 1", id);
                        dataReader = DB.SendTQuery(sql);
                        if (dataReader.Rows.Count > 0)
                        {
                            foreach (DataRow row in dataReader.Rows)
                            {
                                if (row.Field<string>(1) != client.GPUUNIT[12]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений Name!\tБыло: " + row.Field<string>(1) + " Стало: " + client.GPUUNIT[12], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET Name = '{0}' WHERE systemid = {1}", client.GPUUNIT[12], id)); }
                                if (row.Field<string>(2) != client.GPUUNIT[5]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений Description!\tБыло: " + row.Field<string>(2) + " Стало: " + client.GPUUNIT[5], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET Description = '{0}' WHERE systemid = {1}", client.GPUUNIT[5], id)); }
                                if (row.Field<string>(3) != client.GPUUNIT[6]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений DeviceID!\tБыло: " + row.Field<string>(3) + " Стало: " + client.GPUUNIT[6], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET DeviceID = '{0}' WHERE systemid = {1}", client.GPUUNIT[6], id)); }
                                if (row.Field<string>(4) != client.GPUUNIT[0]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений AdapterRAM!\tБыло: " + row.Field<string>(4) + " Стало: " + client.GPUUNIT[0], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET AdapterRAM = '{0}' WHERE systemid = {1}", client.GPUUNIT[0], id)); }
                                if (row.Field<string>(5) != client.GPUUNIT[1]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений Availability!\tБыло: " + row.Field<string>(5) + " Стало: " + client.GPUUNIT[1], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET Availability = '{0}' WHERE systemid = {1}", client.GPUUNIT[1], id)); }
                                if (row.Field<string>(6) != client.GPUUNIT[2]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений Caption!\tБыло: " + row.Field<string>(6) + " Стало: " + client.GPUUNIT[2], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET Caption = '{0}' WHERE systemid = {1}", client.GPUUNIT[2], id)); }
                                if (row.Field<string>(7) != client.GPUUNIT[3]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений CurrentRefreshRate!\tБыло: " + row.Field<string>(7) + " Стало: " + client.GPUUNIT[3], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET CurrentRefreshRate = '{0}' WHERE systemid = {1}", client.GPUUNIT[3], id)); }
                                if (row.Field<string>(8) != client.GPUUNIT[4]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений CurrentScanMode!\tБыло: " + row.Field<string>(8) + " Стало: " + client.GPUUNIT[4], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET CurrentScanMode = '{0}' WHERE systemid = {1}", client.GPUUNIT[4], id)); }
                                if (row.Field<string>(9) != client.GPUUNIT[7]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений DriverDate!\tБыло: " + row.Field<string>(9) + " Стало: " + client.GPUUNIT[7], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET DriverDate = '{0}' WHERE systemid = {1}", client.GPUUNIT[7], id)); }
                                if (row.Field<string>(10) != client.GPUUNIT[8]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений DriverVersion!\tБыло: " + row.Field<string>(10) + " Стало: " + client.GPUUNIT[8], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET DriverVersion = '{0}' WHERE systemid = {1}", client.GPUUNIT[8], id)); }
                                if (row.Field<string>(11) != client.GPUUNIT[9]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений MaxRefreshRate!\tБыло: " + row.Field<string>(11) + " Стало: " + client.GPUUNIT[9], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET MaxRefreshRate = '{0}' WHERE systemid = {1}", client.GPUUNIT[9], id)); }
                                if (row.Field<string>(12) != client.GPUUNIT[10]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений MinRefreshRate!\tБыло: " + row.Field<string>(12) + " Стало: " + client.GPUUNIT[10], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET MinRefreshRate = '{0}' WHERE systemid = {1}", client.GPUUNIT[10], id)); }
                                if (row.Field<string>(13) != client.GPUUNIT[11]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений Monochrome!\tБыло: " + row.Field<string>(13) + " Стало: " + client.GPUUNIT[11], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET Monochrome = '{0}' WHERE systemid = {1}", client.GPUUNIT[11], id)); }
                                if (row.Field<string>(14) != client.GPUUNIT[13]) { ErrorsListForm.AddQuery("gpuunit: Несовпадение значений VideoProcessor!\tБыло: " + row.Field<string>(14) + " Стало: " + client.GPUUNIT[13], client, QueryElement.QueryType.DBError, String.Format("UPDATE gpuunit SET VideoProcessor = '{0}' WHERE systemid = {1}", client.GPUUNIT[13], id)); }
                            }
                        }
                        else
                        {
                            dataReader.Clear();
                            dataReader.Dispose();
                            sql = String.Format("INSERT INTO gpuunit(`Name`, `Description`, `DeviceID`, `AdapterRAM`, `Availability`, `Caption`, `CurrentRefreshRate`, `CurrentScanMode`, `DriverDate`, `DriverVersion`, `MaxRefreshRate`, `MinRefreshRate`, `Monochrome`, `VideoProcessor`, `systemid`) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}')",
                                client.GPUUNIT[12],
                                client.GPUUNIT[5],
                                client.GPUUNIT[6],
                                 client.GPUUNIT[0],
                                  client.GPUUNIT[1],
                                   client.GPUUNIT[2],
                                    client.GPUUNIT[3],
                                     client.GPUUNIT[4],
                                      client.GPUUNIT[7],
                                       client.GPUUNIT[8],
                                        client.GPUUNIT[9],
                                         client.GPUUNIT[10],
                                          client.GPUUNIT[11],
                                          client.GPUUNIT[13],
                                          id);
                            try
                            {
                                DB.SendNonQuery(sql);
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                                break;
                            }
                            catch (MySqlException me)
                            {
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                                string mess = String.Format("Ошибка при добавлении нового компонента в таблицу. {0}", me.ToString());
                                ErrorsListForm.AddQuery(mess, client, QueryElement.QueryType.ClientError);
                            }
                        }
                        dataReader.Clear();
                        dataReader.Dispose();
                        break;
                    }
                case 3:
                    {
                        sql = String.Format("SELECT * FROM boards WHERE systemid = {0} LIMIT 1", id);
                        dataReader = DB.SendTQuery(sql);
                        if (dataReader.Rows.Count > 0)
                        {
                            foreach (DataRow row in dataReader.Rows)
                            {
                                if (row.Field<string>(1) != client.Board[5]) { ErrorsListForm.AddQuery("boards: Несовпадение значений Name!\tБыло: " + row.Field<string>(1) + " Стало: " + client.Board[12], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET Name = '{0}' WHERE systemid = {1}", client.Board[5], id)); }
                                if (row.Field<string>(2) != client.Board[0]) { ErrorsListForm.AddQuery("boards: Несовпадение значений Description!\tБыло: " + row.Field<string>(2) + " Стало: " + client.Board[5], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET Description = '{0}' WHERE systemid = {1}", client.Board[0], id)); }
                                if (row.Field<string>(3) != client.Board[1]) { ErrorsListForm.AddQuery("boards: Несовпадение значений HostingBoard!\tБыло: " + row.Field<string>(3) + " Стало: " + client.Board[6], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET HostingBoard = '{0}' WHERE systemid = {1}", client.Board[1], id)); }
                                if (row.Field<string>(4) != client.Board[2]) { ErrorsListForm.AddQuery("boards: Несовпадение значений HotSwappable!\tБыло: " + row.Field<string>(4) + " Стало: " + client.Board[0], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET HotSwappable = '{0}' WHERE systemid = {1}", client.Board[2], id)); }
                                if (row.Field<string>(5) != client.Board[3]) { ErrorsListForm.AddQuery("boards: Несовпадение значений Manufacturer!\tБыло: " + row.Field<string>(5) + " Стало: " + client.Board[1], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET Manufacturer = '{0}' WHERE systemid = {1}", client.Board[3], id)); }
                                if (row.Field<string>(6) != client.Board[4]) { ErrorsListForm.AddQuery("boards: Несовпадение значений Model!\tБыло: " + row.Field<string>(6) + " Стало: " + client.Board[2], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET Model = '{0}' WHERE systemid = {1}", client.Board[4], id)); }
                                if (row.Field<string>(7) != client.Board[6]) { ErrorsListForm.AddQuery("boards: Несовпадение значений OtherIdentifyingInfo!\tБыло: " + row.Field<string>(7) + " Стало: " + client.Board[3], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET OtherIdentifyingInfo = '{0}' WHERE systemid = {1}", client.Board[6], id)); }
                                if (row.Field<string>(8) != client.Board[7]) { ErrorsListForm.AddQuery("boards: Несовпадение значений Product!\tБыло: " + row.Field<string>(8) + " Стало: " + client.Board[4], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET Product = '{0}' WHERE systemid = {1}", client.Board[7], id)); }
                                if (row.Field<string>(9) != client.Board[8]) { ErrorsListForm.AddQuery("boards: Несовпадение значений SerialNumber!\tБыло: " + row.Field<string>(9) + " Стало: " + client.Board[7], client, QueryElement.QueryType.DBError, String.Format("UPDATE boards SET SerialNumber = '{0}' WHERE systemid = {1}", client.Board[8], id)); }

                            }
                        }
                        else
                        {
                            dataReader.Clear();
                            dataReader.Dispose();
                            sql = String.Format("INSERT INTO boards(`Name`, `Description`, `HostingBoard`, `HotSwappable`, `Manufacturer`, `Model`, `OtherIdentifyingInfo`, `Product`, `SerialNumber`, `systemid`) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}', '{8}', '{9}')",
                                client.Board[5],
                                client.Board[0],
                                client.Board[1],
                                 client.Board[2],
                                  client.Board[3],
                                   client.Board[4],
                                    client.Board[6],
                                     client.Board[7],
                                      client.Board[8],
                                          id);
                            try
                            {
                                DB.SendNonQuery(sql);
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                                break;
                            }
                            catch (MySqlException me)
                            {
                                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                                string mess = String.Format("Ошибка при добавлении нового компонента в таблицу. {0}", me.ToString());
                                ErrorsListForm.AddQuery(mess, client, QueryElement.QueryType.ClientError);
                            }
                        }
                        dataReader.Clear();
                        dataReader.Dispose();
                        break;
                    }
                case 4:
                    {
                        sql = String.Format("SELECT * FROM rams WHERE systemid = {0}", id);
                        dataReader = DB.SendTQuery(sql);
                        int records = 0;
                        if (dataReader.Rows.Count > 0)
                        {
                            foreach (DataRow row in dataReader.Rows)
                            {
                                int rid = row.Field<int>(0);
                                if (row.Field<string>(1) != client.RAM.Rows[records][0].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений BankLabel!\tБыло: " + row.Field<string>(1) + " Стало: " + client.RAM.Rows[records][0].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET BankLabel = '{0}' WHERE id = {1}", client.RAM.Rows[records][0].ToString(), rid)); }
                                if (row.Field<string>(2) != client.RAM.Rows[records][1].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений Capacity!\tБыло: " + row.Field<string>(2) + " Стало: " + client.RAM.Rows[records][1].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET Capacity = '{0}' WHERE id = {1}", client.RAM.Rows[records][1].ToString(), rid)); }
                                if (row.Field<string>(3) != client.RAM.Rows[records][2].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений DataWidth!\tБыло: " + row.Field<string>(3) + " Стало: " + client.RAM.Rows[records][2].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET DataWidth = '{0}' WHERE id = {1}", client.RAM.Rows[records][2].ToString(), rid)); }
                                if (row.Field<string>(4) != client.RAM.Rows[records][3].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений Description!\tБыло: " + row.Field<string>(4) + " Стало: " + client.RAM.Rows[records][3].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET Description = '{0}' WHERE id = {1}", client.RAM.Rows[records][3].ToString(), rid)); }
                                if (row.Field<string>(5) != client.RAM.Rows[records][4].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений DeviceLocator!\tБыло: " + row.Field<string>(5) + " Стало: " + client.RAM.Rows[records][4].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET DeviceLocator = '{0}' WHERE id = {1}", client.RAM.Rows[records][4].ToString(), rid)); }
                                if (row.Field<string>(6) != client.RAM.Rows[records][5].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений FormFactor!\tБыло: " + row.Field<string>(6) + " Стало: " + client.RAM.Rows[records][5].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET FormFactor = '{0}' WHERE id = {1}", client.RAM.Rows[records][5].ToString(), rid)); }
                                if (row.Field<string>(7) != client.RAM.Rows[records][6].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений MemoryType!\tБыло: " + row.Field<string>(7) + " Стало: " + client.RAM.Rows[records][6].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET MemoryType = '{0}' WHERE id = {1}", client.RAM.Rows[records][6].ToString(), rid)); }
                                if (row.Field<string>(8) != client.RAM.Rows[records][7].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений Model!\tБыло: " + row.Field<string>(8) + " Стало: " + client.RAM.Rows[records][7].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET Model = '{0}' WHERE id = {1}", client.RAM.Rows[records][7].ToString(), rid)); }
                                if (row.Field<string>(9) != client.RAM.Rows[records][8].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений Name!\tБыло: " + row.Field<string>(9) + " Стало: " + client.RAM.Rows[records][8].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET Name = '{0}' WHERE id = {1}", client.RAM.Rows[records][8].ToString(), rid)); }
                                if (row.Field<string>(10) != client.RAM.Rows[records][9].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений OtherIdentifyingInfo!\tБыло: " + row.Field<string>(10) + " Стало: " + client.RAM.Rows[records][9].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET OtherIdentifyingInfo = '{0}' WHERE id = {1}", client.RAM.Rows[records][9].ToString(), rid)); }
                                if (row.Field<string>(11) != client.RAM.Rows[records][10].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений PartNumber!\tБыло: " + row.Field<string>(11) + " Стало: " + client.RAM.Rows[records][10].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET PartNumber = '{0}' WHERE id = {1}", client.RAM.Rows[records][10].ToString(), rid)); }
                                if (row.Field<string>(12) != client.RAM.Rows[records][11].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений PositionInRow!\tБыло: " + row.Field<string>(12) + " Стало: " + client.RAM.Rows[records][11].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET PositionInRow = '{0}' WHERE id = {1}", client.RAM.Rows[records][11].ToString(), rid)); }
                                if (row.Field<string>(13) != client.RAM.Rows[records][12].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений SerialNumber!\tБыло: " + row.Field<string>(13) + " Стало: " + client.RAM.Rows[records][12].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET SerialNumber = '{0}' WHERE id = {1}", client.RAM.Rows[records][12].ToString(), rid)); }
                                if (row.Field<string>(14) != client.RAM.Rows[records][13].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений Speed!\tБыло: " + row.Field<string>(14) + " Стало: " + client.RAM.Rows[records][13].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET Speed = '{0}' WHERE id = {1}", client.RAM.Rows[records][13].ToString(), rid)); }
                                if (row.Field<string>(15) != client.RAM.Rows[records][14].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений Status!\tБыло: " + row.Field<string>(15) + " Стало: " + client.RAM.Rows[records][14].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET Status = '{0}' WHERE id = {1}", client.RAM.Rows[records][14].ToString(), rid)); }
                                if (row.Field<string>(16) != client.RAM.Rows[records][15].ToString()) { ErrorsListForm.AddQuery("rams: Несовпадение значений Version!\tБыло: " + row.Field<string>(16) + " Стало: " + client.RAM.Rows[records][15].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE rams SET Version = '{0}' WHERE id = {1}", client.RAM.Rows[records][15].ToString(), rid)); }
                                records++;
                            }
                        }
                        else
                        {
                            dataReader.Clear();
                            dataReader.Dispose();
                            for (int i = 0; i < client.RAM.Rows.Count; i++)
                            {
                                sql = String.Format("INSERT INTO rams(`BankLabel`, `Capacity`, `DataWidth`, `Description`, `DeviceLocator`, `FormFactor`, `MemoryType`, `Model`, `Name`, `OtherIdentifyingInfo`, `PartNumber`, `PositionInRow`, `SerialNumber`, `Speed`, `Status`, `Version`, `systemid`) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}')",
                                client.RAM.Rows[i][0].ToString(),
                                client.RAM.Rows[i][1].ToString(),
                                client.RAM.Rows[i][2].ToString(),
                                 client.RAM.Rows[i][3].ToString(),
                                  client.RAM.Rows[i][4].ToString(),
                                   client.RAM.Rows[i][5].ToString(),
                                    client.RAM.Rows[i][6].ToString(),
                                     client.RAM.Rows[i][7].ToString(),
                                      client.RAM.Rows[i][8].ToString(),
                                      client.RAM.Rows[i][9].ToString(),
                                      client.RAM.Rows[i][10].ToString(),
                                      client.RAM.Rows[i][11].ToString(),
                                      client.RAM.Rows[i][12].ToString(),
                                      client.RAM.Rows[i][13].ToString(),
                                      client.RAM.Rows[i][14].ToString(),
                                      client.RAM.Rows[i][15].ToString(),
                                          id);
                                try
                                {
                                    DB.SendNonQuery(sql);
                                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                                    //break;
                                }
                                catch (MySqlException me)
                                {
                                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                                    string mess = String.Format("Ошибка при добавлении нового компонента в таблицу. {0}", me.ToString());
                                    ErrorsListForm.AddQuery(mess, client, QueryElement.QueryType.ClientError);
                                }
                            }
                        }
                        dataReader.Clear();
                        dataReader.Dispose();
                        break;
                    }
                case 5:
                    {
                        sql = String.Format("SELECT * FROM products WHERE systemid = {0}", id);
                        dataReader = DB.SendTQuery(sql);
                        //int records = 0;
                        if (dataReader.Rows.Count > 0)
                        {
                            /* foreach (DataRow row in dataReader.Rows)
                             {
                                 int rid = row.Field<int>(0);
                                 if (row.Field<string>(1) != client.Products.Rows[records][0].ToString()) { ErrorsListForm.AddQuery("products: Несовпадение значений DisplayName!\tБыло: " + row.Field<string>(1) + " Стало: " + client.Products.Rows[records][0].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE products SET DisplayName = '{0}' WHERE id = {1}", client.Products.Rows[records][0].ToString(), rid)); }
                                 if (row.Field<string>(2) != client.Products.Rows[records][1].ToString()) { ErrorsListForm.AddQuery("products: Несовпадение значений DisplayVersion!\tБыло: " + row.Field<string>(2) + " Стало: " + client.Products.Rows[records][1].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE products SET DisplayVersion = '{0}' WHERE id = {1}", client.Products.Rows[records][1].ToString(), rid)); }
                                 if (row.Field<string>(3) != client.Products.Rows[records][2].ToString()) { ErrorsListForm.AddQuery("products: Несовпадение значений InstallDate!\tБыло: " + row.Field<string>(3) + " Стало: " + client.Products.Rows[records][2].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE products SET InstallDate = '{0}' WHERE id = {1}", client.Products.Rows[records][2].ToString(), rid)); }
                                 if (row.Field<string>(4) != client.Products.Rows[records][3].ToString()) { ErrorsListForm.AddQuery("products: Несовпадение значений Publisher!\tБыло: " + row.Field<string>(4) + " Стало: " + client.Products.Rows[records][3].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE products SET Publisher = '{0}' WHERE id = {1}", client.Products.Rows[records][3].ToString(), rid)); }
                                 if (row.Field<string>(5) != client.Products.Rows[records][4].ToString()) { ErrorsListForm.AddQuery("products: Несовпадение значений IdentifyingNumber!\tБыло: " + row.Field<string>(5) + " Стало: " + client.Products.Rows[records][4].ToString(), client, QueryElement.QueryType.DBError, String.Format("UPDATE products SET IdentifyingNumber = '{0}' WHERE id = {1}", client.Products.Rows[records][4].ToString(), rid)); }
                                 records++;
                             }*/
                        }
                        else
                        {
                            dataReader.Clear();
                            dataReader.Dispose();
                            for (int i = 0; i < client.Products.Rows.Count; i++)
                            {
                                sql = String.Format("INSERT INTO products(`DisplayName`, `DisplayVersion`, `InstallDate`, `Publisher`, `IdentifyingNumber`, `systemid`) VALUES('{0}','{1}','{2}','{3}','{4}','{5}')",
                                client.Products.Rows[i][0].ToString(),
                                client.Products.Rows[i][1].ToString(),
                                client.Products.Rows[i][2].ToString(),
                                 client.Products.Rows[i][3].ToString(),
                                 " ",
                                  //client.Products.Rows[i][4].ToString(),
                                          id);
                                try
                                {
                                    DB.SendNonQuery(sql);
                                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                                    //break;
                                }
                                catch (MySqlException me)
                                {
                                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                                    string mess = String.Format("Ошибка при добавлении нового компонента в таблицу. {0}", me.ToString());
                                    ErrorsListForm.AddQuery(mess, client, QueryElement.QueryType.ClientError);
                                }
                            }
                            break;
                        }
                        dataReader.Clear();
                        dataReader.Dispose();
                        break;
                    }
                default:
                    break;
            }
        }
        /// <summary>
        /// Метод проверки клиента на наличие в базе данных
        /// </summary>
        /// <param name="mac">В качестве параметра сверки принимается MAC адрес</param>
        /// <returns></returns>
        private int CheckRegister(string mac)
        {
            int id = -1;
            string queryString = @"SELECT id FROM systems WHERE mac = '" + mac + "' LIMIT 1";
            DataTable reader = DB.SendTQuery(queryString);
            if (reader.Rows.Count > 0)
            {
                id = Convert.ToInt32(reader.Rows[0][0]);
            }
            reader.Clear();
            reader.Dispose();
            return id;
        }
        /// <summary>
        /// Регистрация клиента в случае его отсутствия в БД
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mac"></param>
        private void RegisterNewClient(string name, string mac)
        {
            try
            {
                string queryString = String.Format("INSERT INTO systems (name, mac, isConfirm) VALUES('{0}', '{1}', False)", name, mac);
                DB.SendTQuery(queryString);
                Invoke(new GetClientsList(GetAllClients), new object[] { });
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.ToString());
                string mess = String.Format("Ошибка при регистрации нового клиента в таблицу. Имя: {0}, MAC: {1}. {2}", name, mac, e.ToString());
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
            }
        }
        public void CheckAdresses(IPAddress BeginIP, IPAddress EndIP)
        {

        }
    }
}
