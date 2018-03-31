using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Server
{
    public partial class ErrorsListForm : Form
    {
        //-------------Events & Delegates---------------
        public delegate void RemoveQuery();
        public static event RemoveQuery RemoveQueryHandle;
        public delegate void UpdateClients();
        public static event UpdateClients UpdateAllCients;
        public delegate void AddQueryEvent();
        public static event AddQueryEvent AddQueryHandle;
        //----------------------------------------------

        public static LinkedList<QueryElement> link = new LinkedList<QueryElement>();
        public static int SysErrors = 0;
        public static int DBErrors = 0;
        public static int ClientErrors = 0;
        public static int ErrorsCounter = 0;
        public ErrorsListForm()
        {
            InitializeComponent();
            AddRows();
        }
        public void AddRows()
        {
            LinkedListNode<QueryElement> node;
            for (node = link.Last; node != null; node = node.Previous)
            {
                switch (node.Value.GetQType())
                {
                    case QueryElement.QueryType.None: break;
                    case QueryElement.QueryType.Error: break;
                    case QueryElement.QueryType.DBError:
                        {
                            SocketClient client = node.Value.GetSocket();
                            string name = client.name;
                            int id = client.Clientid;
                            dataGridView1.Rows.Add(Properties.Resources.CacheWarning_16x, node.Value.GetIndex(),node.Value.GetTime(), "DBError", node.Value.GetMessage(), name, id, "Выполнить перезапись в базу данных", "Исправить", "Пропустить");
                            break;
                        }
                    case QueryElement.QueryType.SysError:
                        {
                            dataGridView1.Rows.Add(Properties.Resources.Important_16x, node.Value.GetIndex(), node.Value.GetTime(), "SysError", node.Value.GetMessage(), "", "", "", "", "Пропустить");
                            break;
                        }
                    case QueryElement.QueryType.ClientError:
                        {
                            SocketClient client = node.Value.GetSocket();
                            string name = client.name;
                            int id = client.Clientid;
                            dataGridView1.Rows.Add(Properties.Resources.RouteServiceError_16x, node.Value.GetIndex(), node.Value.GetTime(), "ClientError", node.Value.GetMessage(), name, id, "", "", "Пропустить");
                            break;
                        }
                    case QueryElement.QueryType.ClientWarning:
                        {
                            dataGridView1.Rows.Add(Properties.Resources.AddUser_16x, node.Value.GetIndex(), node.Value.GetTime(), "ClientWarning", node.Value.GetMessage(), "", "", "", "Исправить", "Пропустить");
                            break;
                        }
                }
            }
            dataGridView1.AutoResizeColumns();
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            LinkedListNode<QueryElement> node;
            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if (e.ColumnIndex == IgnoreButton.DisplayIndex)
                {
                    int indx = Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[index.DisplayIndex].Value.ToString());
                    for (node = link.Last; node != null; node = node.Previous)
                    {
                        if (indx == node.Value.GetIndex())
                        {
                            switch (node.Value.GetQType())
                            {
                                case QueryElement.QueryType.DBError:
                                    DBErrors--;
                                    break;
                                case QueryElement.QueryType.SysError:
                                    SysErrors--;
                                    break;
                                case QueryElement.QueryType.ClientError:
                                    ClientErrors--;
                                    break;
                                case QueryElement.QueryType.Error:
                                    break;
                                case QueryElement.QueryType.None:
                                    break;
                                case QueryElement.QueryType.ClientWarning:
                                    ClientErrors--;
                                    break;
                            }
                            node.Value.Dispose();
                            node.Value = null;
                            link.Remove(node);
                            dataGridView1.Rows.RemoveAt(e.RowIndex);
                            RemoveQueryHandle();
                            break;
                            
                        }
                    }
                }
                else if (e.ColumnIndex == Completebutton.DisplayIndex)
                {
                    int indx = Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[index.DisplayIndex].Value.ToString());
                    for (node = link.Last; node != null; node = node.Previous)
                    {
                        if (indx == node.Value.GetIndex())
                        {
                            if (node.Value.GetQType() == QueryElement.QueryType.DBError)
                            {
                                if (!String.IsNullOrEmpty(node.Value.GetQuery()))
                                {
                                    MySQLCon DB = new MySQLCon();
                                    DB.SendQuery(node.Value.GetQuery());
                                    node.Value.Dispose();
                                    node.Value = null;
                                    link.Remove(node);
                                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                                    DBErrors--;
                                    RemoveQueryHandle();
                                    break;
                                }
                            }
                            else if (node.Value.GetQType() == QueryElement.QueryType.SysError)
                            {

                            }
                            else if (node.Value.GetQType() == QueryElement.QueryType.ClientError)
                            {

                            }
                            else if (node.Value.GetQType() == QueryElement.QueryType.ClientWarning)
                            {
                                if (!String.IsNullOrEmpty(node.Value.GetQuery()))
                                {
                                    MySQLCon DB = new MySQLCon();
                                    DB.SendQuery(node.Value.GetQuery());
                                    node.Value.Dispose();
                                    node.Value = null;
                                    link.Remove(node);
                                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                                    ClientErrors--;
                                    RemoveQueryHandle();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void AddQuery(string mess, QueryElement.QueryType querytype)
        {
            switch (querytype)
            {
                case QueryElement.QueryType.SysError:
                    SysErrors++;
                    break;
                case QueryElement.QueryType.DBError:
                    DBErrors++;
                    break;
                case QueryElement.QueryType.ClientError:
                    ClientErrors++;
                    break;
                case QueryElement.QueryType.Error:
                    break;
                case QueryElement.QueryType.None:
                    break;
            }
            QueryElement query = new QueryElement(mess, querytype, DateTime.Now, ErrorsCounter++);
            link.AddLast(query);
            AddQueryHandle();
        }
        public static void AddQuery(string mess, SocketClient sock, QueryElement.QueryType QType)
        {
            switch (QType)
            {
                case QueryElement.QueryType.SysError:
                    SysErrors++;
                    break;
                case QueryElement.QueryType.DBError:
                    DBErrors++;
                    break;
                case QueryElement.QueryType.ClientError:
                    ClientErrors++;
                    break;
                case QueryElement.QueryType.Error:
                    break;
                case QueryElement.QueryType.None:
                    break;
            }
            QueryElement query = new QueryElement(mess, sock, QType, DateTime.Now, ErrorsCounter++);
            link.AddLast(query);
            AddQueryHandle();
        }
        public static void AddQuery(string mess, SocketClient sock, QueryElement.QueryType QType, string sql)
        {
            switch (QType)
            {
                case QueryElement.QueryType.SysError:
                    SysErrors++;
                    break;
                case QueryElement.QueryType.DBError:
                    DBErrors++;
                    break;
                case QueryElement.QueryType.ClientError:
                    ClientErrors++;
                    break;
                case QueryElement.QueryType.Error:
                    break;
                case QueryElement.QueryType.None:
                    break;
            }
            QueryElement query = new QueryElement(mess, sock, QType, sql, DateTime.Now, ErrorsCounter++);
            link.AddLast(query);
            AddQueryHandle();
        }
        public static void AddQuery(string mess, QueryElement.QueryType QType, string sql)
        {
            switch (QType)
            {
                case QueryElement.QueryType.SysError:
                    SysErrors++;
                    break;
                case QueryElement.QueryType.DBError:
                    DBErrors++;
                    break;
                case QueryElement.QueryType.ClientError:
                    ClientErrors++;
                    break;
                case QueryElement.QueryType.Error:
                    break;
                case QueryElement.QueryType.None:
                    break;
                case QueryElement.QueryType.ClientWarning:
                    ClientErrors++;
                    break;
            }
            QueryElement query = new QueryElement(mess, QType, sql, DateTime.Now, ErrorsCounter++);
            link.AddLast(query);
            AddQueryHandle();
        }
        private void ErrorsListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateAllCients();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

}
