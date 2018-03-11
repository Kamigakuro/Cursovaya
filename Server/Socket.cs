using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Server
{
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
            return byReturn;
        }

    }
}
