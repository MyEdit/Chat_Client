using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using Chat_Client.Network.PacketsActions;

namespace Chat_Client.Network
{
    public enum PacketType
    {
        P_Message,
        P_Authorization,
        P_QueryWithoutResponce, //Для примера
        P_Query //Для примера
    }

    internal class NetworkClient
    {
        private static string ip = "127.0.0.1";
        private static int port = 1111;
        private static Socket serverSocket;

        public bool init(out string errorMessage)
        {
            IPEndPoint adress = new IPEndPoint(IPAddress.Parse(ip), port);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                serverSocket.Connect(adress);
                errorMessage = string.Empty;
            }
            catch (SocketException ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            Console.WriteLine("INFO: Connected to server");
            return true;
        }

        public void startListening()
        {
            if (serverSocket == null)
                throw new InvalidOperationException("init method must be called before startListening.");

            Task.Run(() => serverHandler());
        }

        private void serverHandler()
        {
            while (true)
            {
                packetHandler(getMessageFromServer<PacketType>());
            }
        }

        private void packetHandler(PacketType packetType)
        {
            switch (packetType)
            {
                case PacketType.P_Message:
                    {
                        P_Message.onMessage();
                        break;
                    }
                case PacketType.P_Authorization:
                    {
                        // Авторизовываем (Например: закрываем форму авторизации и открываем основную форму)
                        break;
                    }
                case PacketType.P_Query:
                    {
                        // Выполняем запрос в БД и возвращаем ответ
                        break;
                    }
                case PacketType.P_QueryWithoutResponce:
                    {
                        // Выполняем запрос в БД без ответа клиенту
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Server sent unknown packet type");
                        break;
                    }
            }
        }

        //Получение пакета от сервера
        public static T getMessageFromServer<T>()
        {
            byte[] buffer;

            //Создаем набор байтов по размеру типа данных T
            if (typeof(T).IsEnum)
                buffer = new byte[sizeof(int)];
            else
                buffer = new byte[Marshal.SizeOf<T>()];

            serverSocket.Receive(buffer);
            return byteArrayToObject<T>(buffer);
        }

        //Получение пакета string от сервера
        public static string getMessageFromServer()
        {
            int messageSize;
            string message;

            byte[] sizeBuffer = new byte[sizeof(int)];
            serverSocket.Receive(sizeBuffer);
            messageSize = BitConverter.ToInt32(sizeBuffer, 0);

            byte[] stringBuffer = new byte[messageSize];
            serverSocket.Receive(stringBuffer);
            message = Encoding.UTF8.GetString(stringBuffer);

            return message;
        }

        //Отправка пакета серверу
        public static void sendMessage<T>(T message)
        {
            serverSocket.Send(objectToByteArray(message));
        }

        //Отправка пакета string серверу
        public static void sendMessage(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            serverSocket.Send(lengthBytes);
            serverSocket.Send(messageBytes);
        }

        //Сериализатор. Серелиализует объекты string, int, double, Emun в набор байтов
        private static byte[] objectToByteArray<T>(T obj)
        {
            if (obj is string)
            {
                return Encoding.UTF8.GetBytes(obj as string);
            }

            if (typeof(T) == typeof(int))
            {
                return BitConverter.GetBytes((int)(object)obj);
            }

            if (typeof(T) == typeof(double))
            {
                return BitConverter.GetBytes((double)(object)obj);
            }

            if (typeof(T).IsEnum)
            {
                return BitConverter.GetBytes(Convert.ToInt32(obj));
            }

            throw new InvalidOperationException("Unsupported type");
        }

        //Десериализатор. Десерелиализует набор байтов в объекты string, int, double, Emun
        private static T byteArrayToObject<T>(byte[] byteArray)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)Encoding.UTF8.GetString(byteArray);
            }

            if (typeof(T) == typeof(int))
            {
                return (T)(object)BitConverter.ToInt32(byteArray, 0);
            }

            if (typeof(T) == typeof(double))
            {
                return (T)(object)BitConverter.ToDouble(byteArray, 0);
            }

            if (typeof(T).IsEnum)
            {
                return (T)Enum.ToObject(typeof(T), BitConverter.ToInt32(byteArray, 0));
            }

            throw new InvalidOperationException("Unsupported type");
        }
    }
}