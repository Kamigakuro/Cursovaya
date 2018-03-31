using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace Server
{
    public class SSocket
    {
        private static Socket listener;
        public bool StartSocket(int Port, IPAddress aryLocalAddr, int listeners)
        {
            try
            {
                listener = new Socket(aryLocalAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, Port));
                listener.Listen(listeners);
                listener.BeginAccept(new AsyncCallback(OnConnectRequest), listener);
                return true;
            }
            catch (Exception e)
            {
                string mess = String.Format("Ошибка при попытке получить локальный адрес: {0}", e.Message);
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                return false;
            }
        }




    }
}
