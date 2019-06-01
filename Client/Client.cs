using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Client
{
    class Client
    {
        private Socket _socket;

        static void Main(string[] args)
        {
            Client program = new Client();
            program.Connect();

            Console.ReadKey();
        }

        //與server進行連線
        private void Connect()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));
            }
            catch
            {
                Console.WriteLine("connect error");
            }

        }

        //取得Server送來的訊息
        private string Receive()
        {
            try
            {
                long IntAcceptData;

                byte[] clientData = new byte[20];

                // 程式會被 hand 在此, 等待接收來自 Server 端傳來的資料
                IntAcceptData = _socket.Receive(clientData);

                string message = Encoding.Default.GetString(clientData);
                return message;

                //Console.WriteLine(S);
            }
            catch
            {
                Console.WriteLine("receive error");
                return "error";
            }
        }

    }
}
