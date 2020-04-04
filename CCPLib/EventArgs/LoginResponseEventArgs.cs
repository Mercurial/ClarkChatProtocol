using System;
using System.Net.Sockets;
using System.Threading;

namespace ClarkChatProtocol.EventArgs
{
    public class LoginResponseEventArgs : System.EventArgs
    {
        public PayloadResponseStatus Status { get; set; }
    }
}