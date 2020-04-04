using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClarkChatProtocol.EventArgs;

namespace ClarkChatProtocol
{
    public class CCPServer : IDisposable
    {
        public string Address { get; private set; }
        public int Port { get; private set; }
        public bool IsListening { get; private set; }
        private TcpListener _Listener { get; set; }
        public List<TcpClient> Clients { get; private set; } = new List<TcpClient>();
        public event EventHandler<RegisterEventArgs> Register;
        public event EventHandler<LoginEventArgs> Login;
        public event EventHandler<ServerEventArgs> Logout;
        public event EventHandler<ServerEventArgs> ClientDisconnect;
        public event EventHandler<WhisperEventArgs> Whisper;

        public CCPServer(string address, int port)
        {
            Address = address;
            Port = port;
            _Listener = new TcpListener(IPAddress.Parse(Address), Port);
        }

        public void Listen()
        {
            _Listener.Start();
            IsListening = true;
            ProcessMessages();
        }

        private void ProcessMessages()
        {
            new Thread(new ThreadStart(() =>
            {
                while (IsListening)
                {
                    TcpClient client = _Listener.AcceptTcpClient();
                    Clients.Add(client);
                    var endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                    new Thread(new ThreadStart(() =>
                    {
                        CCPLogger.Log($"Connected! {client.Client.RemoteEndPoint}");
                        byte[] bytes = new byte[256];
                        string data = string.Empty;
                        NetworkStream ns = client.GetStream();
                        int i = 0;

                        try
                        {
                            while ((i = ns.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                // Translate data bytes to a ASCII string.
                                data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                                if (data.EndsWith("\r\n"))
                                {
                                    data = data.Replace("\r\n", string.Empty);
                                    ProcessMessage(data, client);
                                    data = string.Empty;
                                }
                            }
                            CCPLogger.Log($"Client[{endpoint}] Disconnected");
                            ClientDisconnect?.Invoke(this, new ServerEventArgs
                            {
                                Address = endpoint.Address.ToString(),
                                Port = endpoint.Port
                            });
                            Clients.Remove(client);
                        }
                        catch (Exception ex)
                        {
                            CCPLogger.Log($"Client[{endpoint}] Disconnected");
                            ClientDisconnect?.Invoke(this, new ServerEventArgs
                            {
                                Address = endpoint.Address.ToString(),
                                Port = endpoint.Port
                            });
                            Clients.Remove(client);
                        }

                    })).Start();
                }
            })).Start();
        }

        private void ProcessMessage(string msg, TcpClient client)
        {
            var ns = client.GetStream();
            CCPLogger.Log($"Command: {msg}");
            var command = (PayloadCommand)msg[0];
            var rawParams = new string[0];
            switch (command)
            {
                case PayloadCommand.REGISTER:
                    rawParams = msg.Substring(1).Split('\t');

                    if (rawParams.Length > 3)
                    {
                        SendCommandResponse(ns, command, PayloadResponseStatus.Failed);
                        ErrorMessage(command);
                    }
                    else
                    {
                        Register?.Invoke(this, new RegisterEventArgs
                        {
                            Username = rawParams[1],
                            Password = rawParams[2],
                            NetworkStream = ns,
                            Address = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(),
                            Port = (client.Client.RemoteEndPoint as IPEndPoint).Port
                        });
                    }
                    break;
                case PayloadCommand.LOGIN:
                    rawParams = msg.Substring(1).Split('\t');

                    if (rawParams.Length > 3)
                    {
                        SendCommandResponse(ns, command, PayloadResponseStatus.Failed);
                        ErrorMessage(command);
                    }
                    else
                    {
                        Login?.Invoke(this, new LoginEventArgs
                        {
                            Username = rawParams[1],
                            Password = rawParams[2],
                            NetworkStream = ns,
                            Address = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(),
                            Port = (client.Client.RemoteEndPoint as IPEndPoint).Port
                        });
                    }
                    break;
                case PayloadCommand.WHISPER:
                    rawParams = msg.Substring(1).Split('\t');
                    var receiver = rawParams[1];
                    var message = rawParams[2];
                    Whisper?.Invoke(this, new WhisperEventArgs
                    {
                        Receiver = receiver,
                        Message = message,
                        NetworkStream = ns,
                        Address = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(),
                        Port = (client.Client.RemoteEndPoint as IPEndPoint).Port
                    });
                    break;
                case PayloadCommand.LOGOUT:
                    Logout?.Invoke(this, new ServerEventArgs
                    {
                        Address = (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString(),
                        Port = (client.Client.RemoteEndPoint as IPEndPoint).Port
                    });
                    break;
            }
        }

        public static void SendCommandResponse(NetworkStream ns, PayloadCommand cmd, PayloadResponseStatus status)
        {
            var rBuffer = new byte[5];
            rBuffer[0] = (byte)cmd;
            rBuffer[1] = (byte)'\t';
            rBuffer[2] = (byte)status;
            rBuffer[3] = (byte)'\r';
            rBuffer[4] = (byte)'\n';
            ns.Write(rBuffer, 0, rBuffer.Length);
        }

        public void SendWhisper(IPEndPoint endPoint, string sender, string message)
        {
            var client = SearchClientByAddressPort(endPoint.Address.ToString(), endPoint.Port.ToString());
            var ns = client.GetStream();

            var commandString = $"{sender}\t{message}\r\n";
            var commandBuffer = System.Text.Encoding.ASCII.GetBytes(commandString);

            var rBuffer = new byte[commandString.Length + 2];
            rBuffer[0] = (byte)PayloadCommand.WHISPER;
            rBuffer[1] = (byte)'\t';
            commandBuffer.CopyTo(rBuffer, 2);
            ns.Write(rBuffer, 0, rBuffer.Length);
        }

        private TcpClient SearchClientByAddressPort(string address, string port)
        {
            foreach (TcpClient c in Clients)
            {
                var ep = c.Client.RemoteEndPoint as IPEndPoint;
                if (ep.Address.ToString() == address && ep.Port.ToString() == port)
                {
                    return c;
                }
            }

            return null;
        }

        private void ErrorMessage(PayloadCommand cmd)
        {
            CCPLogger.Log($"{cmd} command is not VALID.");
        }

        public void Unlisten()
        {
            _Listener.Stop();
            IsListening = false;
        }

        public void Dispose()
        {
            Unlisten();
            _Listener = null;
        }
    }
}
