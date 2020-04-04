using System;
using System.IO;
using System.Net;
using ClarkChatProtocol;
using ClarkChatProtocol.EventArgs;
using SQLite;

namespace server
{
    class Program
    {
        static CCPServer Server { get; set; }
        static SQLiteConnection DB { get; set; }

        static void Main(string[] args)
        {
            int port = 1338;
            Server = new CCPServer(IPAddress.Any.ToString(), port);
            Server.Register += OnRegisterEvent;
            Server.Login += OnLoginEvent;
            Server.Logout += OnLogoutEvent;
            Server.ClientDisconnect += OnLogoutEvent;
            Server.Whisper += OnWhisperEvent;
            Server.Listen();

            InitializeDB();

            Console.Clear();
            Console.WriteLine($"Welcome to Clark Chat Server, Listening at port {port}");
            while (true) Console.ReadKey();
        }

        static void InitializeDB()
        {
            DB = new SQLiteConnection(Path.Combine(Environment.CurrentDirectory, "chat.db"));
            DB.CreateTable<User>();
            DB.CreateTable<Session>();
        }

        static void OnRegisterEvent(object sender, RegisterEventArgs e)
        {
            var count = DB.Table<User>().Where((u) => u.Username == e.Username).Count();

            if (count <= 0)
            {
                DB.Insert(new User
                {
                    Username = e.Username,
                    Password = e.Password
                });
                e.Success();
                CCPLogger.Log($"User: {e.Username} succesfully registered.");
            }
            else
            {
                e.Failed();
                CCPLogger.Log($"User: {e.Username} registration failed.");
            }
        }

        static void OnLoginEvent(object sender, LoginEventArgs e)
        {
            var user = DB.Table<User>().Where((u) => u.Username == e.Username && u.Password == e.Password).FirstOrDefault();

            if (user != null)
            {
                DB.Table<Session>().Where(u => u.UserId == user.Id).Delete();
                DB.Insert(new Session
                {
                    UserId = user.Id,
                    LastAddress = e.Address,
                    LastPort = e.Port,
                    DateCreated = DateTime.Now
                });
                e.Success();
                CCPLogger.Log($"User: {e.Username} succesfully logged-in.");
            }
            else
            {
                e.Failed();
                CCPLogger.Log($"User: {e.Username} failed to log-in.");
            }
        }

        private static void OnWhisperEvent(object s, WhisperEventArgs e)
        {
            var senderSession = DB.Table<Session>().Where(s => s.LastAddress == e.Address && s.LastPort == e.Port).FirstOrDefault();
            if (senderSession != null)
            {
                var sender = DB.Table<User>().Where(u => u.Id == senderSession.UserId).FirstOrDefault();
                var receiver = DB.Table<User>().Where(u => u.Username == e.Receiver).FirstOrDefault();

                if (sender != null && receiver != null)
                {
                    var session = DB.Table<Session>().Where(s => s.UserId == receiver.Id).FirstOrDefault();
                    if (session != null)
                    {
                        Server.SendWhisper(new IPEndPoint(IPAddress.Parse(session.LastAddress), session.LastPort), sender.Username, e.Message);
                    }
                    // Send Back a response if receiver is offline
                }
            }
        }

        static void OnLogoutEvent(object sender, ServerEventArgs e)
        {
            DB.Table<Session>().Where(u => u.LastAddress == e.Address && u.LastPort == e.Port).Delete();
        }
    }
}
