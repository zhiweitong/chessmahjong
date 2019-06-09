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
        private string[] _handCard;
        private int _position = -1;
        private int _playerNow = 1;
        private bool _gameOver = false;

        static void Main(string[] args)
        {
            Client program = new Client();
            program.CreateDictionary(); //建立字典

            program.Connect();

            while (!program._gameOver)
                program.Assignment(program.Receive());

            Console.WriteLine("遊戲結束");

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
                byte[] clientData = new byte[20];

                // 程式會被 hand 在此, 等待接收來自 Server 端傳來的資料
                long IntAcceptData = _socket.Receive(clientData);
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

        //取得開局手牌
        private void GetHandCard(string msg)
        {
            _handCard = msg.Split('.');
            Array.Resize(ref _handCard, _handCard.Length - 1);

            Array.Sort(_handCard, string.CompareOrdinal);

            Console.WriteLine("\n遊戲開始!\n");

            if (_position == 1)
            {
                Console.WriteLine("你是玩家 1 號，是莊家");

                //判斷天胡

                _socket.Send(Encoding.ASCII.GetBytes(string.Format("Check_{0}_false", (_position - 1))));//沒有天胡

                Console.Write("你的手牌: ");
                foreach (string card in _handCard)
                {
                    Console.Write("{0} ", card);
                }
                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("你是玩家 {0} 號，是閒家", _position);

                Console.Write("你的手牌: ");
                foreach (string card in _handCard)
                {
                    Console.Write("{0} ", card);
                }
                Console.WriteLine("");
            }
        }

        //出一張牌
        private void Discard()
        {
            Console.WriteLine("          0 1 2 3 4");
            Console.WriteLine("請出牌(輸入數字) :");
            int input = Convert.ToInt32(Console.ReadLine());
            _socket.Send(Encoding.ASCII.GetBytes(string.Format("New_{0}_{1}", (_position - 1),_handCard[input])));
        }

        //辨識訊息
        private void Assignment(string msg)
        {
            string[] str = msg.Split('_');

            switch (str[0])
            {
                case "Start":  //開局
                    _position = Convert.ToInt32(str[1]);
                    GetHandCard(str[2]);
                    break;

                case "New": //有人出牌的廣播
                    Console.WriteLine("玩家 {0} 打出 {1}", str[1], str[2]);
                    break;

                case "Check": //是否胡牌的通知
                    if (str[1].Equals("true"))
                    {
                        Console.WriteLine("玩家{0}胡牌了", str[2]);
                        _gameOver = true;
                    }
                    else
                    {
                        if (_position == _playerNow)
                            Discard();
                    }
                    break;

                case "One": //抽到一張牌

                    break;

                case "Over": //流局

                    break;
            }
        }

    }
}

