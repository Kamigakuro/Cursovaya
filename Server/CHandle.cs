using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public class CHandle
    {
        private SocketClient client;
        private string command;
        private AsyncCallback callback;
        public CHandle(string cmd, AsyncCallback clbk, SocketClient clnt)
        {
            command = cmd;
            callback = clbk;
            client = clnt;
        }
        public SocketClient GetClient()
        {
            return client;
        }
        public AsyncCallback GetAsyncCallback()
        {
            return callback;
        }
        public string GetCommand()
        {
            return command;
        }
    }
}
