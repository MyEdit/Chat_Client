using Chat_Client.Network;
using System;

namespace Chat_Client
{
    internal class Program
    {
        static NetworkClient networkClient = new NetworkClient();

        static void Main(string[] args)
        {
            string errorMessage;
            if (!networkClient.init(out errorMessage))
            {
                Console.WriteLine($"ERROR: Unable to connect to server. {errorMessage}");
                PauseBeforeExit();
                return;
            }

            networkClient.startListening();
            auth();
            ListenForUserInput();

            PauseBeforeExit();
        }

        static void ListenForUserInput()
        {
            while (true)
            {
                string message = Console.ReadLine();
                NetworkClient.sendMessage(PacketType.P_Message);
                NetworkClient.sendMessage(message);
            }
        }

        static void auth()
        {
            Console.WriteLine("Введите свой ник");
            string nickName = Console.ReadLine();

            NetworkClient.sendMessage(PacketType.P_Authorization);
            NetworkClient.sendMessage(nickName);
        }

        static void PauseBeforeExit()
        {
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
