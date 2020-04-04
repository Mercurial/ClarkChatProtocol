using System;
using System.Net.Sockets;
using System.Threading;

namespace ClarkChatProtocol.EventArgs
{
    public class RegisterEventArgs : ServerEventArgs
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public NetworkStream NetworkStream { get; set; }

        public void Success()
        {
            CCPServer.SendCommandResponse(NetworkStream, PayloadCommand.REGISTER, PayloadResponseStatus.Success);
        }

        public void Failed()
        {
            CCPServer.SendCommandResponse(NetworkStream, PayloadCommand.REGISTER, PayloadResponseStatus.Failed);
        }
    }
}
