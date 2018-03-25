using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace Server
{
    public class MySQLCon
    {
        private static MySqlConnection dbHandle;
        public MySqlConnection GetHandle { get { return dbHandle; } }
        /// <summary>
        /// Получение статуса соединения
        /// </summary>
        public System.Data.ConnectionState SqlConnection
        {
            get { return dbHandle.State; }
        }
        /// <summary>
        /// Метод подключение к базе данных
        /// </summary>
        /// <param name="DB_USER">Имя пользователя</param>
        /// <param name="DB_PASS">Пароль</param>
        /// <param name="DB_BASE">Название базы</param>
        /// <param name="DB_HOST">Адрес подключения</param>
        public void OpenConnection(string DB_USER, string DB_PASS, string DB_BASE, string DB_HOST)
        {
            dbHandle = new MySqlConnection("Database=" + DB_BASE + ";Data Source=" + DB_HOST + ";User Id=" + DB_USER + ";Password=" + DB_PASS + ";charset = utf8");
            try
            {
                dbHandle.Open();
            }
            catch (MySqlException e)
            {
                string mess = String.Format("Не удалось установить соединение с базой данных! {0}", e.Message);
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                //MessageBox.Show(e.ToString());
            }
        }
        /// <summary>
        /// Отправка запроса в БД без получения результата выполнения
        /// </summary>
        /// <param name="command">текст запроса</param>
        public void SendNonQuery(string command)
        {
            try
            {
                MySqlCommand com = new MySqlCommand(command, dbHandle);
                com.ExecuteNonQuery();
            }
            catch(MySqlException me)
            {
                string mess = String.Format("Ошибка при выполнении запроса в БД. ({0}). {1}", command, me.ToString());
                ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
            }
        }
        /// <summary>
        /// Отправка запроса в БД с получением результата выполнения
        /// </summary>
        /// <param name="command">текст запроса</param>
        /// 
        private object threadLock = new object();
        public MySqlDataReader SendQuery(string command)
        {
            lock (threadLock)
            {
                MySqlDataReader reader;
                try
                {
                    MySqlCommand com = new MySqlCommand(command, dbHandle);
                    reader = com.ExecuteReader();
                }
                catch (MySqlException me)
                {
                    string mess = String.Format("Ошибка при выполнении запроса в БД. ({0}). {1}", command, me.ToString());
                    ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                    reader = null;
                }
                return reader;
            }
            
        }
        /// <summary>
        /// Закрытие подключения к базе данных
        /// </summary>
        /// <returns></returns>
        public bool CloseConnection()
        {
            if (dbHandle != null)
            {
                try
                {
                    dbHandle.Close();
                    return true;
                }
                catch (Exception es)
                {
                    //MessageBox.Show(es.ToString());
                    string mess = String.Format("Не удалось разорвать соединение с базой данных! {0}", es.Message);
                    ErrorsListForm.AddQuery(mess, QueryElement.QueryType.SysError);
                    //MessageBox.Show(e.ToString());
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Проверка целостности базы данных (Наличие необходимых таблиц)
        /// </summary>
        /// <param name="DB_USER"></param>
        public void CheckBaseIntegrity(string DB_USER)
        {
            MySqlDataReader MyDataReader;
            int tablecount = 0;
            MySqlCommand com = new MySqlCommand("SHOW TABLES FROM `" + DB_USER + "`", dbHandle);
            MyDataReader = com.ExecuteReader();
            string[] Tables = new String[10];
            if (MyDataReader.HasRows)
            {
                while (MyDataReader.Read())
                {
                    Tables[tablecount] = MyDataReader.GetString(0);
                    tablecount++;
                }
                MyDataReader.Close();

                if (!Tables.Contains("systems"))
                {
                    com.Dispose();
                    string command = String.Format("CREATE TABLE IF NOT EXISTS systems (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, name VARCHAR(30), mac VARCHAR(50), isConfirm BOOLEAN)");
                    com = new MySqlCommand(command, dbHandle);
                    com.ExecuteNonQuery();
                    MyDataReader.Close();
                }
                if (!Tables.Contains("operationsys"))
                {
                    com.Dispose();
                    string command = String.Format("CREATE TABLE IF NOT EXISTS operationsys (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(60), Version VARCHAR(30), CDVersion VARCHAR(30), InstallDate VARCHAR(30), NumberOfProcesses VARCHAR(4), NumberOfUsers VARCHAR(2), SerialNumber VARCHAR(30), systemid INT NOT NULL)");
                    com = new MySqlCommand(command, dbHandle);
                    com.ExecuteNonQuery();
                    MyDataReader.Close();
                }
                if (!Tables.Contains("cpuunit"))
                {
                    com.Dispose();
                    string command = String.Format("CREATE TABLE IF NOT EXISTS cpuunit (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(60), Description VARCHAR(60), DeviceID VARCHAR(30), L2CacheSize VARCHAR(30), L3CacheSize VARCHAR(30), MaxClockSpeed VARCHAR(30), NumberOfCores VARCHAR(30), NumberOfLogicalProcessors VARCHAR(30), ProcessorId VARCHAR(30), ProcessorType VARCHAR(30), Revision VARCHAR(30), Role VARCHAR(30), SocketDesignation VARCHAR(30), systemid INT NOT NULL)");
                    com = new MySqlCommand(command, dbHandle);
                    com.ExecuteNonQuery();
                    MyDataReader.Close();
                }
                if (!Tables.Contains("gpuuunit")) {
                     com.Dispose();                                                                                                                                                                                              
                     string command = String.Format("CREATE TABLE IF NOT EXISTS gpuunit (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(60), Description VARCHAR(60), DeviceID VARCHAR(30), AdapterRAM VARCHAR(30), Availability VARCHAR(30), Caption VARCHAR(30), CurrentRefreshRate VARCHAR(30), CurrentScanMode VARCHAR(30), DriverDate VARCHAR(30), DriverVersion VARCHAR(30), MaxRefreshRate VARCHAR(30), MinRefreshRate VARCHAR(30), Monochrome VARCHAR(30), VideoProcessor VARCHAR(30), systemid INT NOT NULL)");
                     com = new MySqlCommand(command, dbHandle);
                     com.ExecuteNonQuery();
                     MyDataReader.Close();
                 }
                if (!Tables.Contains("boards"))
                {
                    com.Dispose();
                    string command = String.Format("CREATE TABLE IF NOT EXISTS boards (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, Name VARCHAR(60), Description VARCHAR(60), HostingBoard VARCHAR(30), HotSwappable VARCHAR(30), Manufacturer VARCHAR(30), Model VARCHAR(30), OtherIdentifyingInfo VARCHAR(30), Product VARCHAR(30), SerialNumber VARCHAR(30), systemid INT NOT NULL)");
                    com = new MySqlCommand(command, dbHandle);
                    com.ExecuteNonQuery();
                    MyDataReader.Close();
                }
                if (!Tables.Contains("rams"))
                {
                    com.Dispose();
                    string command = String.Format("CREATE TABLE IF NOT EXISTS rams (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, BankLabel VARCHAR(30), Capacity VARCHAR(30), DataWidth VARCHAR(30), Description VARCHAR(60), DeviceLocator VARCHAR(30), FormFactor VARCHAR(30), MemoryType VARCHAR(30), Model VARCHAR(30), Name VARCHAR(60), OtherIdentifyingInfo VARCHAR(30), PartNumber VARCHAR(30), PositionInRow VARCHAR(30), SerialNumber VARCHAR(30), Speed VARCHAR(30), Status VARCHAR(30), Version VARCHAR(30), systemid INT NOT NULL)");
                    com = new MySqlCommand(command, dbHandle);
                    com.ExecuteNonQuery();
                    MyDataReader.Close();
                }
                if (!Tables.Contains("products"))
                {
                    com.Dispose();
                    string command = String.Format("CREATE TABLE IF NOT EXISTS products (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, DisplayName VARCHAR(60), DisplayVersion VARCHAR(60), InstallDate VARCHAR(30), Publisher VARCHAR(60), IdentifyingNumber VARCHAR(60), systemid INT NOT NULL)");
                    com = new MySqlCommand(command, dbHandle);
                    com.ExecuteNonQuery();
                    MyDataReader.Close();
                }
                //if (!Tables.Contains("system")) { }
            }
            // Тут надо сделать типа сообщения об отсутсвии таблиц и  предложить по новой создать
        }

    }
}
