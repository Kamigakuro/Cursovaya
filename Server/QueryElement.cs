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
            ClientError = 4
        }
        private string Message = String.Empty;
        private SocketManagment socket;
        private QueryType Type = QueryType.None;
        private string sqlquery = String.Empty;
        private DateTime time;

        public QueryElement(string Mess, SocketManagment sock, QueryType QType, DateTime tie)
        {
            Message = Mess;
            socket = sock;
            Type = QType;
            time = tie;
        }
        public QueryElement(string Mess, QueryType QType, DateTime tie)
        {
            Message = Mess;
            Type = QType;
            time = tie;
        }
        public QueryElement(string Mess, SocketManagment sock, QueryType QType, string sql, DateTime tie)
        {
            Message = Mess;
            socket = sock;
            Type = QType;
            sqlquery = sql;
            time = tie;
        }
        public string GetMessage() { return Message; }
        public string GetQuery() { return sqlquery; }
        public SocketManagment GetSocket() { return socket; }
        public DateTime GetTime() { return time; }
        public QueryType GetQType() { return Type; }

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
