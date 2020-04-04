using System;
using System.Net.Sockets;
using System.Threading;

namespace ClarkChatProtocol.EventArgs
{
    public class LoginEventArgs : ServerEventArgs
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public NetworkStream NetworkStream { get; set; }

        public void Success()
        {
            CCPServer.SendCommandResponse(NetworkStream, PayloadCommand.LOGIN, PayloadResponseStatus.Success);
        }

        public void Failed()
        {
            CCPServer.SendCommandResponse(NetworkStream, PayloadCommand.LOGIN, PayloadResponseStatus.Failed);
        }
    }
}
