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
    public partial class Server
    {
        public SSocket socket;        
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

            IPAddress aryLocalAddr;
            if (!SettingsClass.LocalWork)
            {
                IPAddress.TryParse(GetExternalIp(), out aryLocalAddr);
            }
            else
            {
                aryLocalAddr = GetLocalIP();
            }
           // IPAddress.TryParse(GetExternalIp(), out IPAddress aryLocalAddr);
           // if (aryLocalAddr == null || aryLocalAddr == IPAddress.None) aryLocalAddr = GetLocalIP();
            if (aryLocalAddr == null || aryLocalAddr == IPAddress.None)
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
                    string localip = "" + aryLocalAddr.ToString();
                    label4.Text = "Подключено";
                    label4.ForeColor = Color.Green;
                    label10.Text = aryLocalAddr.ToString() + ":" + SettingsClass.SPort + " (" + localip + ")";
                }
            }
        }
        public void CheckAdresses(IPAddress BeginIP, IPAddress EndIP)
        {

        }
    }
}
