using System;
using System.Net.Sockets;
using System.Threading;
using ClarkChatProtocol.EventArgs;

namespace ClarkChatProtocol
{
    public class CCPClient : IDisposable
    {
        public event EventHandler<RegisterResponseEventArgs> RegisterResponse;
        public event EventHandler<LoginResponseEventArgs> LoginResponse;
        public event EventHandler<WhisperEventArgs> ReceiveWhisper;
        private TcpClient _Client { get; set; } = new TcpClient();
        private NetworkStream _NetworkStream { get; set; }
        private string _Address { get; set; }
        private int _Port { get; set; }
        public CCPClient(string _address, int _port)
        {
            _Address = _address;
            _Port = _port;
        }

        public void Connect()
        {
            try
            {
                _Client.Connect(_Address, _Port);

                if (_Client.Connected)
                {
                    _NetworkStream = _Client.GetStream();
                    ProcessMessages();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Disconnect()
        {
            _Client.Close();
        }

        public void Register(string username, string password)
        {
            byte[] msgBuffer = StringToBytes($"\t{username}\t{password}\r\n");
            byte[] payloadBuffer = new byte[msgBuffer.Length + 1];
            payloadBuffer[0] = (byte)PayloadCommand.REGISTER;
            msgBuffer.CopyTo(payloadBuffer, 1);
            _NetworkStream.Write(payloadBuffer, 0, payloadBuffer.Length);
        }

        public void Login(string username, string password)
        {
            byte[] msgBuffer = StringToBytes($"\t{username}\t{password}\r\n");
            byte[] payloadBuffer = new byte[msgBuffer.Length + 1];
            payloadBuffer[0] = (byte)PayloadCommand.LOGIN;
            msgBuffer.CopyTo(payloadBuffer, 1);
            _NetworkStream.Write(payloadBuffer, 0, payloadBuffer.Length);
        }

        public void Whisper(string receiver, string message)
        {
            byte[] msgBuffer = StringToBytes($"\t{receiver}\t{message}\r\n");
            byte[] payloadBuffer = new byte[msgBuffer.Length + 1];
            payloadBuffer[0] = (byte)PayloadCommand.WHISPER;
            msgBuffer.CopyTo(payloadBuffer, 1);
            _NetworkStream.Write(payloadBuffer, 0, payloadBuffer.Length);
        }

        public void Logout()
        {
            byte[] payloadBuffer = new byte[3];
            payloadBuffer[0] = (byte)PayloadCommand.LOGOUT;
            payloadBuffer[1] = (byte)'\r';
            payloadBuffer[2] = (byte)'\n';
            _NetworkStream.Write(payloadBuffer, 0, payloadBuffer.Length);
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void ProcessMessages()
        {
            new Thread(new ThreadStart(() =>
            {
                byte[] bytes = new byte[256];
                string data = string.Empty;
                int i = 0;
                try
                {
                    while ((i = _NetworkStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        if (data.EndsWith("\r\n"))
                        {
                            data = data.Replace("\r\n", string.Empty);
                            ProcessMessage(data);
                            data = string.Empty;
                        }
                    }
                }
                catch
                {
                    CCPLogger.Log("Disconnected");
                }
            })).Start();
        }

        private byte[] StringToBytes(string s)
        {
            return System.Text.Encoding.ASCII.GetBytes(s);
        }

        private void ProcessMessage(string msg)
        {
            var command = (PayloadCommand)msg[0];
            var rawParams = new string[0];
            switch (command)
            {
                case PayloadCommand.REGISTER:
                    rawParams = msg.Split('\t');
                    RegisterResponse?.Invoke(this, new RegisterResponseEventArgs
                    {
                        Status = (PayloadResponseStatus)rawParams[1][0]
                    });
                    break;
                case PayloadCommand.LOGIN:
                    rawParams = msg.Split('\t');
                    LoginResponse?.Invoke(this, new LoginResponseEventArgs
                    {
                        Status = (PayloadResponseStatus)rawParams[1][0]
                    });
                    break;
                case PayloadCommand.WHISPER:
                    rawParams = msg.Split('\t');
                    ReceiveWhisper?.Invoke(this, new WhisperEventArgs
                    {
                        Sender = rawParams[1],
                        Message = rawParams[2]
                    });
                    break;
            }
        }
    }
}