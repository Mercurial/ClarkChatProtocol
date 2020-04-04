using System;
using System.Linq;
using ClarkChatProtocol;
using ClarkChatProtocol.EventArgs;
using Client;

namespace client
{
    class Program
    {
        static CCPClient Client;
        static string CurrentUsername;
        static bool IsLogged = false;

        static void Main(string[] args)
        {
            Console.Clear();
            Client = new CCPClient("127.0.0.1", 1338);
            Client.Connect();
            // Register Events
            Client.RegisterResponse += OnRegisterResponse;
            Client.LoginResponse += OnLoginResponse;
            Client.ReceiveWhisper += OnWhisperReceived;
            //
            Console.WriteLine("Welcome to Clark Chat Application v0.1!");
            while (true)
            {
                DisplayInputIndicator();
                ProcessInput(Console.ReadLine());
                ClearPrevConsoleLine();
            }
        }

        static void OnRegisterResponse(object sender, RegisterResponseEventArgs e)
        {
            ClearCurrentConsoleLine();
            CCPLogger.Log($"Register Response: {e.Status}");
            DisplayInputIndicator();
        }

        static void OnLoginResponse(object sender, LoginResponseEventArgs e)
        {
            ClearCurrentConsoleLine();
            IsLogged = e.Status == PayloadResponseStatus.Success;
            CCPLogger.Log($"Login Response: {e.Status}");
            DisplayInputIndicator();
        }

        private static void OnWhisperReceived(object sender, WhisperEventArgs e)
        {
            ClearCurrentConsoleLine();
            CCPLogger.Log($"[{e.Sender}]: {e.Message.Replace(":tab:", "\t")}");
            DisplayInputIndicator();
        }

        public static void DisplayInputIndicator()
        {
            if (IsLogged)
                Console.Write($"[{CurrentUsername}] > ");
            else
                Console.Write("> ");
        }

        static void ProcessInput(string input)
        {
            if (input.StartsWith(Command.Whisper))
            {

            }

            if (input.StartsWith(Command.Register))
            {
                var inputFragments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (inputFragments.Length < 3)
                {
                    CCPLogger.Log("Invalid Input");
                }
                else
                {
                    Client.Register(inputFragments[1], inputFragments[2]);
                }
            }

            if (input.StartsWith(Command.Login))
            {
                var inputFragments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (inputFragments.Length < 3)
                {
                    CCPLogger.Log("Invalid Input");
                }
                else
                {
                    Client.Login(inputFragments[1], inputFragments[2]);
                    CurrentUsername = inputFragments[1];
                }
            }

            if (input.StartsWith(Command.Logout))
            {
                IsLogged = false;
                Client.Logout();
            }

            if (input.StartsWith(Command.Whisper))
            {
                var inputFragments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (inputFragments.Length < 3)
                {
                    CCPLogger.Log("Invalid Input");
                }
                else
                {
                    var msg = string.Join(' ', inputFragments.Skip(2));
                    Client.Whisper(inputFragments[1], msg.Replace("\t", ":tab:"));
                }
            }

            if (input.StartsWith(Command.Exit))
            {
                Console.Clear();
                Environment.Exit(0);
            }

            if (input.StartsWith(Command.Clear))
            {
                Console.Clear();
            }
        }

        public static void ClearPrevConsoleLine()
        {
            if (Console.CursorTop > 0)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }

        public static void ClearCurrentConsoleLine()
        {

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }
}
