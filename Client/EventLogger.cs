using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Client
{
    public class EventLogger
    {
        private static List<string> Messages = new List<string> { };
        private static Thread writer;
        public EventLogger()
        {
            if (writer != null) return;
            else
            {
                writer = new Thread(WriteMessage);
                writer.Name = "Log Writer";
                writer.IsBackground = true;
                writer.Priority = ThreadPriority.BelowNormal;
                writer.Start();
            }
        }
        public object locker = new object();
        public void AddLog(string text)
        {
            lock (locker)
            {
                DateTime now = DateTime.Now;
                string message = String.Format("[{0}] - {1}\n", now, text);
                Messages.Add(message);
            }
        }
        private void WriteMessage()
        {
            while (true)
            {
                if (Messages.Count > 0)
                {
                    if (!File.Exists("client.log")) File.Create("client.log").Close();
                    foreach (string message in Messages.ToArray())
                    {
                        try
                        {
                            File.AppendAllText("client.log", message);
                            Messages.Remove(message);
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

    }
}
