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
        public const int ProductBL = 9;
        public const int ERRONCLIENTSIDE = 9999;
    }
    public partial class Server
    {
        public SSocket socket;
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
            //socket = new SSocket();
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
                socket = new SSocket(this);
                if (!socket.StartSocket(SettingsClass.SPort, aryLocalAddr, 100))
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
        public void CheckAdresses(IPAddress BeginIP, IPAddress EndIP)
        {

        }
    }
}
