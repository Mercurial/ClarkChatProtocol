using System;
using System.Net.Sockets;
using System.Threading;

namespace ClarkChatProtocol.EventArgs
{
    public class WhisperEventArgs : ServerEventArgs
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Message { get; set; }
        public NetworkStream NetworkStream { get; set; }
    }
}
