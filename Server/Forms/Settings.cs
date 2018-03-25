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
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            HostTB.Text = SettingsClass.DB_HOST;
            BaseTB.Text = SettingsClass.DB_BASE;
            LoginTB.Text = SettingsClass.DB_USER;
            PassTB.Text = SettingsClass.DB_PASS;
            ClientsTB.Text = SettingsClass.MaxClients.ToString();
            BufferTB.Text = SettingsClass.SBufferSize.ToString();
            SPortTB.Text = SettingsClass.SPort.ToString();
            if (SettingsClass.LocalWork) LocalCB.SelectedItem = 0;
            else LocalCB.SelectedItem = 1;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
