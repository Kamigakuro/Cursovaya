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
            if (SettingsClass.LocalWork) LocalCB.SelectedIndex = 0;
            else LocalCB.SelectedIndex = 1;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SettingsClass.DB_HOST = HostTB.Text;
            SettingsClass.DB_BASE = BaseTB.Text;
            SettingsClass.DB_USER = LoginTB.Text;
            SettingsClass.DB_PASS = PassTB.Text;
            SettingsClass.MaxClients = Convert.ToInt32(ClientsTB.Text);
            SettingsClass.SBufferSize = Convert.ToInt32(BufferTB.Text);
            SettingsClass.SPort = Convert.ToInt32(SPortTB.Text);
            if (LocalCB.SelectedIndex == 0) SettingsClass.LocalWork = true;
            else SettingsClass.LocalWork = false;
        }
    }
}
