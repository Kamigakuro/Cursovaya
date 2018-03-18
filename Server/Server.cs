//using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace Server
{
    public partial class Server : Form
    {
        Thread TimerThread;
        public MySQLCon DB = new MySQLCon();
        
        public Server()
        {
            InitializeComponent();
            notifyIcon1 = new NotifyIcon
            {
                Icon = SystemIcons.Asterisk,
                Visible = true
            };
            notifyIcon1.Click += NotifyIcon1_Click;
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeMySQL();
            IntializeSocket();
            ErrorsListForm.AddQueryHandle += this.UpdateQueryCounts;
            ErrorsListForm.RemoveQueryHandle += this.UpdateQueryCounts;
            TimerThread = new Thread(UpdateTimer);
            TimerThread.Start(); //запускаем поток
            
        }
        public delegate void UpdateTimeEx();
        public void UpdateTime()
        {
            ArrayList clients = m_aryClients;
            foreach (SocketManagment client in clients)
            {
                int id = client.Clientid;
                long time = DateTime.Now.Ticks - client.time.Ticks;
                DateTime watch = new DateTime();
                watch = watch.AddTicks(time);
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells[0].Value.Equals(id))
                    {
                        row.Cells["Time"].Value = String.Format("{0:mm:ss}", watch);
                        break;
                    }
                }
            }
        }
        private void UpdateTimer()
        {
            while (true)
            {
                try
                {
                    Invoke(new UpdateTimeEx(UpdateTime), new object[] { });
                }
                catch { }
                Thread.Sleep(1000);
            }
        }

        public void UpdateQueryCounts()
        {
            dberrorslabel.BeginInvoke((MethodInvoker)(() => dberrorslabel.Text = ErrorsListForm.DBErrors.ToString()));
            syserorslabel.BeginInvoke((MethodInvoker)(() => syserorslabel.Text = ErrorsListForm.SysErrors.ToString()));
            clienterrorlabel.BeginInvoke((MethodInvoker)(() => clienterrorlabel.Text = ErrorsListForm.ClientErrors.ToString()));
        }
        private void NotifyIcon1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Left)
            {
                this.Show();
                //WindowState = FormWindowState.Normal;
            }
        }

   /*    private void OnButtonActionClick(object sender, ListViewColumnMouseEventArgs e)//Кнопка в листе
        {
            SocketManagment client = null;
            foreach (SocketManagment clients in m_aryClients)
            {
                if (clients.Sock.RemoteEndPoint.ToString() == e.Item.SubItems[1].Text) client = clients;
                break;
            }
            if (client == null) return;
            string text = String.Format("\t\t\tИнформация по клиенту - {0}\n\n" +
                "IP-адрес: {1}\n" +
                "Время подключения: {2}\n" +
                "\t\t\tИнформация о системе:\n\n" +
                "Операционная система: {3}\n\n" +
                "Дата установки: {4}\t\t Количество пользователей: {5}\n" +
                "Серийный номер: {6}\n\n" +
                "Процессор: {7}\n" +
                "Количество ядер: {8}\t\t Количество логических ядер: {9}\n" +
                "L2 Cache: {10} \t L3 Cache: {11}\t Socket Designation: {12}\n" +
                "Максимальная частота: {13}\t Роль: {14}\n" +
                "Id процессора: {15}\t Тип процессора: {16}\t Ревизия: {17}\n\n" +
                "Видеокарта: Geforce GTX 760\n" +
                "Название: Gigabyte\n" +
                "Частота: 6008 MHz\n" +
                "Объем памяти: 2048 MB\n\n" +
                "Материнская плата: M2N32-SLI DELUXE\n" +
                "Тип: Base Board\n" +
                "Производитель: ASUSTeK Computer INC\n" +
                "Серийный номер: 123456789000\n" +
                "", e.Item.Text, client.Sock.RemoteEndPoint.ToString(), client.time.ToString(),  client.OperationSistem[0] + "  " + client.OperationSistem[2] + "  " + client.OperationSistem[1],
                client.OperationSistem[3], client.OperationSistem[5], client.OperationSistem[6],
                client.CPUUNIT[5] + "  " + client.CPUUNIT[0] + " " + client.CPUUNIT[1], client.CPUUNIT[6], client.CPUUNIT[7], client.CPUUNIT[2], client.CPUUNIT[3], client.CPUUNIT[12],
                client.CPUUNIT[4], client.CPUUNIT[11], client.CPUUNIT[8], client.CPUUNIT[9], client.CPUUNIT[10]);
            MessageBox.Show(this, text, e.Item.SubItems[1].Text);
        }*/        

        public delegate void AddNewClientDelegate(string name, string ip, int id);
        public void AddNewClient(string name, string ip, int id)
        {
            label6.Text = Convert.ToString(m_aryClients.Count);
            dataGridView1.Rows.Add(id, name, ip, "00:00", "Подробнее");
            AddNewConsoleMessage(String.Format("Подключен новый клиент: [{0}] {1}", name, ip));
        }
        public delegate void GetClientsList();
        public void GetAllClients()
        {
            string sql = "SELECT * FROM systems";
            DB.SendQuery(sql, out MySqlDataReader reader);
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (!reader.GetBoolean(3))
                    {
                        string mess = String.Format("Не подтвержденный клиент - {0}({1})!", reader.GetString(1), reader.GetInt32(0));
                        string db = String.Format("UPDATE systems SET isConfirm = True WHERE id = {0}", reader.GetInt32(0));
                        ErrorsListForm.AddQuery(mess, QueryElement.QueryType.ClientWarning, db);
                    }
                    dataGridView2.Rows.Add(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), "Удалить");
                }
            }
            else
            {

            }
            reader.Close();
        }

        public delegate void AddMessageToConsole(string text);
        public void AddNewConsoleMessage(string text)
        {
            textBox1.AppendText(">> " + text + "\n");
        }

        private delegate void DeleteClientFromList(SocketManagment client);
        private void DeleteClient(SocketManagment client)
        {
            string ip = client.Sock.RemoteEndPoint.ToString();
            int id = client.Clientid;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value.Equals(id))
                {
                    dataGridView1.Rows.RemoveAt(row.Index);
                    break;
                }
            }
            label6.Text = Convert.ToString(m_aryClients.Count);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            TimerThread.Abort();
            try
            {
                if (listener != null)
                {
                    listener.Blocking = false;
                    listener.Close();
                }
            }
            catch (Exception es)
            {
                MessageBox.Show(es.ToString());
            }
            DB.CloseConnection();
        }

        bool cancelevent = true;
        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = cancelevent;
            this.Hide();
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cancelevent = false;
            Application.Exit();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                foreach (SocketManagment client in m_aryClients)
                {
                    if (client.Clientid == Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[0].Value))
                    {
                        AboutClientForm f2 = new AboutClientForm(client.OperationSistem, client.CPUUNIT);
                        f2.Show();
                        break;
                    }
                }
            }
        }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if (MessageBox.Show("Вы действительно хотите удалить клиента с именем " + senderGrid.Rows[e.RowIndex].Cells[CName.DisplayIndex].Value + "?\nВсе данные будут удалены безвозвратно, ключая компоненты.", "Подтвердите удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string sql = String.Format("DELETE FROM `cpuunit` WHERE systemid = {0}", Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[ID.DisplayIndex].Value));
                    DB.SendQuery(sql);
                    sql = String.Format("DELETE FROM `operationsys` WHERE systemid = {0}", Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[ID.DisplayIndex].Value));
                    DB.SendQuery(sql);
                    sql = String.Format("DELETE FROM `gpuunit` WHERE systemid = {0}", Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[ID.DisplayIndex].Value));
                    DB.SendQuery(sql);
                    sql = String.Format("DELETE FROM `systems` WHERE id = {0}", Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[ID.DisplayIndex].Value));
                    DB.SendQuery(sql);
                    dataGridView2.Rows.RemoveAt(e.RowIndex);
                }
                else
                {
                    // user clicked no
                }
            }
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void querylist_Click(object sender, EventArgs e)
        {
            if (ErrorsListForm.link != null && ErrorsListForm.link.Count == 0) { MessageBox.Show("Очередь пуста!"); return; }
            ErrorsListForm form = new ErrorsListForm();
            form.Show();
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void Server_Shown(object sender, EventArgs e)
        {
            GetAllClients();
        }
    }

}
