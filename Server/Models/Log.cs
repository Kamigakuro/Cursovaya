using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Server
{
    public class Log
    {
        public Log()
        {

        }
        public void AddMessage(string message)
        {
            DateTime now = DateTime.Now;
            if (!File.Exists("Server.log")) File.Create("Server.log").Close();
            string mess = String.Format("{0} - {1}\n", now, message);
            File.AppendAllText("Server.log", mess);
            Server.TextBoxLog.BeginInvoke((System.Windows.Forms.MethodInvoker)(() => Server.TextBoxLog.AppendText(">> " + message + "\n")));
        }
    }
}
