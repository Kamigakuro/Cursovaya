﻿using System;
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
        public static LinkedList<QueryElement> link = new LinkedList<QueryElement>();
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
                            SocketManagment client = node.Value.GetSocket();
                            string name = client.name;
                            int id = client.Clientid;
                            dataGridView1.Rows.Add(Properties.Resources.CacheWarning_16x, "DBError",node.Value.GetTime(), node.Value.GetMessage(), name, id, "Выполнить перезапись в базу данных", "Исправить", "Пропустить");
                            break;
                        }
                    case QueryElement.QueryType.SysError: break;
                }
            }
            dataGridView1.AutoResizeColumns();
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if (e.ColumnIndex == IgnoreButton.DisplayIndex)
                {
                    string mess = senderGrid.Rows[e.RowIndex].Cells[Info.DisplayIndex].Value.ToString();
                    int id = Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[ClientId.DisplayIndex].Value);
                    LinkedListNode<QueryElement> node;
                    for (node = link.Last; node != null; node = node.Previous)
                    {
                        SocketManagment client = node.Value.GetSocket();
                        if (node.Value.GetMessage() == mess && id == client.Clientid)
                        {
                            node.Value.Dispose();
                            node.Value = null;
                            link.Remove(node);
                            dataGridView1.Rows.RemoveAt(e.RowIndex);
                            break;
                        }
                    }
                }
                if (e.ColumnIndex == Completebutton.DisplayIndex)
                {
                    string mess = senderGrid.Rows[e.RowIndex].Cells[Info.DisplayIndex].Value.ToString();
                    int id = Convert.ToInt32(senderGrid.Rows[e.RowIndex].Cells[ClientId.DisplayIndex].Value);
                    LinkedListNode<QueryElement> node;
                    for (node = link.Last; node != null; node = node.Previous)
                    {
                        SocketManagment clientr = node.Value.GetSocket();
                        if (node.Value.GetMessage() == mess && id == clientr.Clientid) break;   
                    }
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
                        }
                
                    }
                }
            }
        }

        private void ErrorsListForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}