using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace Client
{
    class Client
    {
        private Socket _socket;
        private Dictionary<string, string> _dictionary = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            Client program = new Client();
            program.CreateDictionary(); //建立字典

            program.Connect();
            

            Console.WriteLine(program.Receive());

            Console.ReadKey();
        }

        //與server進行連線
        private void Connect()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));

                Console.WriteLine("連線成功");
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

            }
            catch
            {
                Console.WriteLine("receive error");
                return "error";
            }
        }

        // 建立字典
        private void CreateDictionary()
        {
            //黑
            _dictionary.Add("A", "將");
            _dictionary.Add("B", "士");
            _dictionary.Add("C", "象");
            _dictionary.Add("D", "車");
            _dictionary.Add("E", "馬");
            _dictionary.Add("F", "包");
            _dictionary.Add("G", "卒");

            //紅
            _dictionary.Add("a", "帥");
            _dictionary.Add("b", "仕");
            _dictionary.Add("c", "相");
            _dictionary.Add("d", "俥");
            _dictionary.Add("e", "傌");
            _dictionary.Add("f", "炮");
            _dictionary.Add("g", "兵");
        }
    }
}
