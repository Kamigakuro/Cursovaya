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
        public byte[] m_byBuff = new byte[1024];
        public string macadr = String.Empty;
        public string name = String.Empty;
        public DateTime time = new DateTime();
        public string[] OperationSistem = new string[7];
        public string[] CPUUNIT = new string[15];
        public string[] GPUUNIT = new string[14];
        public string[] Board = new string[9];
        System.Net.EndPoint endpoint;
        public DataTable RAM = new DataTable("RAM");
        public DataTable Products = new DataTable("Products");
        public int Clientid = -1;
        private SSocket Main;
        private Server _Form;
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
            try
            {
                AsyncCallback recieveData = new AsyncCallback(GetRecievedData);
                m_sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, this);
            }
            catch (Exception ex) {
                //MessageBox.Show(String.Format("Не удалось подключить функцию получения собщений! {0}", ex.Message));
                string mess = String.Format("Не удалось подключить функцию получения собщений! {0}", ex.Message);
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);

            }
        }
        public void GetRecievedData(IAsyncResult ar)
        {
            int nBytesRec = 0;
            //SockError = SocketError.Success;
            try
            {
                nBytesRec = m_sock.EndReceive(ar);
                if (nBytesRec < 1)
                {
                    _Form.Invoke(new Server.AddMessageToConsole(_Form.AddNewConsoleMessage), new object[] { String.Format("Клиент [{0}] отключен.", Sock.RemoteEndPoint) });
                    logger.AddMessage("[CLIENT] " + String.Format("Клиент [{0}] отключен.", m_sock.RemoteEndPoint));
                    _Form.Invoke(new Server.DeleteClientFromList(_Form.DeleteClient), new object[] { this });      
                    m_sock.Close();
                    SSocket.m_aryClients.Remove(this);
                    return;
                }
            }
            catch { }
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
    }
}
