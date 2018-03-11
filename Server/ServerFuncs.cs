﻿using MySql.Data.MySqlClient;
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
        public const int ERRONCLIENTSIDE = 9999;
    }
    public partial class Server
    {
        Socket listener;
        private static ArrayList m_aryClients = new ArrayList();	// Список подключенных клиентов
        static byte[] m_byBuff = new byte[1024]; // размер буфера
        public static MemoryStream stream = new MemoryStream(m_byBuff);
        static BinaryWriter writer = new BinaryWriter(stream);
        static BinaryReader reader = new BinaryReader(stream);
        static string EndofMessage = "<EOF>";
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
                    else client.Sock.Close();
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
            if (DB.SqlConnection != ConnectionState.Open)
            {
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Соединение с базой данной разорвано!") });
                Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("MySQL DataBase Status: {0}", DB.SqlConnection.ToString()) });
                return;
            }
            string sql = String.Format("SELECT * FROM operationsys WHERE systemid = {0} LIMIT 1", id);
            DB.SendQuery(sql, out MySqlDataReader dataReader);

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
                //cmd.ExecuteNonQuery();
                //cmd.Dispose();
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
                    DB.SendQuery(sql);
                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Добавлен новый компонент в таблицу.") });
                }
                catch
                {
                    Invoke(new AddMessageToConsole(AddNewConsoleMessage), new object[] { String.Format("Ошибка при добавлении нового компонента в таблицу.") });
                }
            }

            sql = String.Format("SELECT * FROM cpuunit WHERE systemid = {0} LIMIT 1", id);
            dataReader.Close();
            DB.SendQuery(sql, out dataReader);
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
                    DB.SendQuery(sql);
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
            DB.SendQuery(queryString, out MySqlDataReader reader);
            if (reader.HasRows)
            {
                reader.Read();
                id = reader.GetInt32(0);
            }
            reader.Close();
            return id;
        }
        private void RegisterNewClient(string name, string mac)
        {
            try
            {
                string queryString = String.Format("INSERT INTO systems (name, mac) VALUES('{0}', '{1}')", name, mac);
                DB.SendQuery(queryString);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

    }
}

