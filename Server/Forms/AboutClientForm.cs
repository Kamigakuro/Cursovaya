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
        public AboutClientForm(string [] OperationSistem, string[] CPUUNIT)
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

            //---------------Видеокарта----------------

            //-----------------------------------------

        }

    }
}
