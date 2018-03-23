using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class SettingsClass
    {
        string DB_USER;
        string DB_PASS;
        string DB_HOST;
        string DB_BASE;
        int SPort;
        int SBufferSize;
        bool LocalWork;
        int MaxClients;



        public SettingsClass()
        {

        }

        public bool LoadSettings()
        {
            return true;
        }
        public bool SaveSettings()
        {
            return true;
        }
        public bool CheckSettingsFile()
        {
            return true;
        }

    }
}
