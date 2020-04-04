using System;
using System.Net.Sockets;
using System.Threading;

namespace ClarkChatProtocol.EventArgs
{
    public class ServerEventArgs : System.EventArgs
    {
        public string Address { get; set; }
        public int Port { get; set; }
    }
}