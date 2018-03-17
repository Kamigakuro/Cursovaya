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
                QueryElement query = new QueryElement(mess, QueryElement.QueryType.SysError, DateTime.Now);
                ErrorsListForm.link.AddFirst(query);
                //MessageBox.Show(e.ToString());
            }
        }
        /// <summary>
        /// Отправка запроса в БД без получения результата выполнения
        /// </summary>
        /// <param name="command">текст запроса</param>
        public void SendQuery(string command)
        {
            MySqlCommand com = new MySqlCommand(command, dbHandle);
            com.ExecuteNonQuery();
        }
        /// <summary>
        /// Отправка запроса в БД с получением результата выполнения
        /// </summary>
        /// <param name="command">текст запроса</param>
        public void SendQuery(string command, out MySqlDataReader reader)
        {
            
            MySqlCommand com = new MySqlCommand(command, dbHandle);
            reader = com.ExecuteReader();
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
                    QueryElement query = new QueryElement(mess, QueryElement.QueryType.SysError, DateTime.Now);
                    ErrorsListForm.link.AddFirst(query);
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
                    string command = String.Format("CREATE TABLE IF NOT EXISTS systems (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, name VARCHAR(30), mac VARCHAR(50))");
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
                /* if (!Tables.Contains("gpuuunit")) {
                     com.Dispose();
                     string command = String.Format("CREATE TABLE IF NOT EXISTS systems (id INT NOT NULL PRIMARY KEY AUTO_INCREMENT, name VARCHAR(30), mac VARCHAR(50))");
                     com = new MySqlCommand(command, dbHandle);
                     com.ExecuteNonQuery();
                     MyDataReader.Close();
                 }*/
                //if (!Tables.Contains("system")) { }
            }
            // Тут надо сделать типа сообщения об отсутсвии таблиц и  предложить по новой создать
        }



    }
}
