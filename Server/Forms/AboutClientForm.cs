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
    public partial class AboutClientForm : Form
    {
        public AboutClientForm(string [] OperationSistem, string[] CPUUNIT, DataTable RAM)
        {
            InitializeComponent();
            //-----------Процессор---------------------
            CPU.Text = CPUUNIT[5];
            sockettextlabel.Text = CPUUNIT[12];
            cores.Text = CPUUNIT[6] + "/" + CPUUNIT[7];
            l2l3.Text = CPUUNIT[2] + "/" + CPUUNIT[3];
            //-----------------------------------------
            
            //---------------ОС------------------------
            OperSys.Text = OperationSistem[0];
            OSVers.Text = OperationSistem[1] + " " + OperationSistem[2];
            OSInstDate.Text = OperationSistem[3];
            //-----------------------------------------

            //---------------RAM----------------
            if (RAM.Rows.Count > 1)
            {
                int count = 0;
                foreach (DataRow row in RAM.Rows)
                {
                    count++;
                    Label rams = new Label
                    {
                        Text = "RAM " + count,
                        AutoSize = true,
                        Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(204))),
                        Location = new Point(12, label5.Location.Y + (16*count)),
                        Name = "ramslbl" + count,
                        Size = new Size(41, 16),
                        TabIndex = 4
                    };
                    rams.Text = "RAM " + count;
                    Label ramsinfo = new Label
                    {
                        Text = RAM.Rows[count - 1][1].ToString(),
                        AutoSize = true,
                        Font = new Font("Verdana", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(204))),
                        Location = new Point(312, label5.Location.Y + (16 * count)),
                        Name = "ramslbl" + count,
                        Size = new Size(41, 16),
                        TabIndex = 4
                    };
                }
            }
            //-----------------------------------------

        }

    }
}
