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
    public class SocketManagment
    {
        private Socket m_sock;
        static byte[] m_byBuff = new byte[1024];
        static MemoryStream mem = new MemoryStream(m_byBuff);
        static BinaryReader reader = new BinaryReader(mem);
        public string name = String.Empty;
        public DateTime time = new DateTime();
        public string[] OperationSistem = new string[7];
        public string[] CPUUNIT = new string[15];
        public string[] GPUUNIT = new string[14];
        public string[] Board = new string[9];
        public DataTable RAM = new DataTable("RAM");
        public DataTable Products = new DataTable("Products");
        public int Clientid = -1;
        public Socket Sock
        {
            get { return m_sock; }
        }
        public SocketManagment(Socket sock)
        {
            m_sock = sock;
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

        }
        public void SetupRecieveCallback(Server main)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(main.OnRecievedData);
                m_sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, this);
            }
            catch (Exception ex) {
                //MessageBox.Show(String.Format("Не удалось подключить функцию получения собщений! {0}", ex.Message));
                string mess = String.Format("Не удалось подключить функцию получения собщений! {0}", ex.Message);
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);

            }
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
            return byReturn;
        }

    }
}
