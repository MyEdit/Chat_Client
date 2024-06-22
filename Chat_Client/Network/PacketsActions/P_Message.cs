using System;

namespace Chat_Client.Network.PacketsActions
{
    internal class P_Message
    {
        public static void onMessage()
        {
            string message = NetworkClient.getMessageFromServer();
            Console.WriteLine(message);
        }
    }
}
