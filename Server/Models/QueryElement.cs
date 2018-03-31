using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class QueryElement : IDisposable
    {
        public enum QueryType
        {
            None = 0,
            DBError = 1,
            SysError = 2,
            Error = 3,
            ClientError = 4,
            ClientWarning = 5,
        }
        private string Message = String.Empty;
        private SocketClient socket;
        private QueryType Type = QueryType.None;
        private string sqlquery = String.Empty;
        private DateTime time;
        private int index;

        public QueryElement(string Mess, SocketClient sock, QueryType QType, DateTime tie, int indx)
        {
            Message = Mess;
            socket = sock;
            Type = QType;
            time = tie;
            index = indx;
        }
        public QueryElement(string Mess, QueryType QType, DateTime tie, int indx)
        {
            Message = Mess;
            Type = QType;
            time = tie;
            index = indx;
        }
        public QueryElement(string Mess, SocketClient sock, QueryType QType, string sql, DateTime tie, int indx)
        {
            Message = Mess;
            socket = sock;
            Type = QType;
            sqlquery = sql;
            time = tie;
            index = indx;
        }
        public QueryElement(string Mess, QueryType QType, string sql, DateTime tie, int indx)
        {
            Message = Mess;
            Type = QType;
            sqlquery = sql;
            time = tie;
            index = indx;
        }
        public string GetMessage() { return Message; }
        public string GetQuery() { return sqlquery; }
        public SocketClient GetSocket() { return socket; }
        public DateTime GetTime() { return time; }
        public QueryType GetQType() { return Type; }
        public int GetIndex() { return index; }
        public void Dispose()
        {
            Message = null;
            socket = null;
            sqlquery = null;
        }

        ~QueryElement()
        {
            Message = null;
            socket = null;
        }

    }
}
