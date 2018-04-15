using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public class SocketClient
    {
        private Socket m_sock;
        Log logger = new Log();
        public byte[] m_byBuff = new byte[SettingsClass.SBufferSize];
        public string macadr = String.Empty;
        public string name = String.Empty;
        public DateTime time = new DateTime();
        public string[] OperationSistem = new string[15];
        public string[] CPUUNIT = new string[15];
        public string[] GPUUNIT = new string[14];
        public string[] Board = new string[9];
        public System.Net.EndPoint endpoint;
        public DataTable RAM = new DataTable("RAM");
        public DataTable Products = new DataTable("Products");
        public int Clientid = -1;
        private bool RecieveUpped = false;
        private SSocket Main;
        private Server _Form;
        public bool Logged = false;
        public bool Spawned = false;
        public Socket Sock
        {
            get { return m_sock; }
        }
        public SocketClient(Socket sock, SSocket main, Server form)
        {
            m_sock = sock;
            Main = main;
            _Form = form;
            RAM.Columns.Add("BankLabel");
            RAM.Columns.Add("Capacity");
            RAM.Columns.Add("DataWidth");
            RAM.Columns.Add("Description");
            RAM.Columns.Add("DeviceLocator");
            RAM.Columns.Add("FormFactor");
            RAM.Columns.Add("MemoryType");
            RAM.Columns.Add("Model");
            RAM.Columns.Add("Name");
            RAM.Columns.Add("OtherIdentifyingInfo");
            RAM.Columns.Add("PartNumber");
            RAM.Columns.Add("PositionInRow");
            RAM.Columns.Add("SerialNumber");
            RAM.Columns.Add("Speed");
            RAM.Columns.Add("Status");
            RAM.Columns.Add("Version");

            Products.Columns.Add("DisplayName");
            Products.Columns.Add("DisplayVersion");
            Products.Columns.Add("InstallDate");
            Products.Columns.Add("Publisher");
            Products.Columns.Add("IdentifyingNumber");
            endpoint = m_sock.RemoteEndPoint;          

        }
        public void SetupRecieveCallback(SSocket main)
        {
            if (RecieveUpped) return;
            try
            {
                if (m_sock == null) return;
                AsyncCallback recieveData = new AsyncCallback(GetRecievedData);
                m_byBuff = new byte[m_byBuff.Length];
                m_sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, this);
                RecieveUpped = true;
            }
            catch (Exception ex) {
                m_byBuff = null;
                string mess = String.Format("Не удалось подключить функцию получения собщений! {0}", ex.Message);
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                Main.RemoveClient(this);
            }
        }
        public void GetRecievedData(IAsyncResult ar)
        {
            RecieveUpped = false;
            int nBytesRec = 0;
            //SockError = SocketError.Success;
            try
            {
                nBytesRec = m_sock.EndReceive(ar);
            }
            catch {
               // Main.RemoveClient(this);
                //return;
            }
            if (nBytesRec < 1)
            {
                m_byBuff = new byte[0];
                Main.ReciveArray.Add(this);
                return;
            }
            byte[] byReturn = new byte[nBytesRec];
            Array.Copy(m_byBuff, byReturn, nBytesRec);
            Main.ReciveArray.Add(this);
            //return byReturn;
        }
        public void Reconnect()
        {
            try
            {
                m_sock.Disconnect(true);
                m_sock.Connect(endpoint);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        bool SocketConnected()
        {
            if (m_sock == null) return false;
            bool part1 = m_sock.Poll(1000, SelectMode.SelectRead);
            bool part2 = (m_sock.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        public bool SendMessage(byte[] buffer)
        {
            if (SocketConnected())
            {
                try
                {
                    m_sock.Send(buffer);
                    return true;
                }
                catch (SocketException e)
                {
                    string mess = String.Format("Не удалось выполнить отправку сообщения! {0}", e.Message);
                    ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
