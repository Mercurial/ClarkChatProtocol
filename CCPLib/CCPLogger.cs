using System;
using System.Net.Sockets;

namespace ClarkChatProtocol
{
    public class CCPLogger
    {
        public static void Log(string msg)
        {
            Console.WriteLine($"[{DateTime.Now.ToString()}] : {msg.Replace("\r\n", string.Empty)}");
        }
    }
}
