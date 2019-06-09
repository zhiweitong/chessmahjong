using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;

namespace Server
{
    class Program
    {
        private Socket[] _sockets;
        private List<string> _allCard = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Server啟動");
            Program program = new Program();

            program.CreateCard();//建立牌庫

            Console.WriteLine("等待連線...四人加入後自動開始");
            program.Listen();
            Console.WriteLine("遊戲開始");

            //遊戲開始
            program.Start();//發牌
            Console.ReadKey();
        }

        // 開socket
        private void Listen()
        {
            // 用 Resize 的方式動態增加 Socket 的數目
            Array.Resize(ref _sockets, 1);

            _sockets[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sockets[0].Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));

            // 其中 LocalIP 和 SPort 分別為 string 和 int 型態, 前者為 Server 端的IP, 後者為Server 端的Port
            _sockets[0].Listen(5); // 進行聆聽; Listen( )為允許 Client 同時連線的最大數
            SckSWaitAccept();   // 另外寫一個函數用來分配 Client 端的 Socket
        }

        // 找空的socket給client
        private void SckSWaitAccept()
        {
            while (true)
            {
                bool Finded = false;
                int socketIndex = -1;

                // 判斷目前是否有空的 Socket 可以提供給Client端連線
                for (int i = 1; i < _sockets.Length; i++)
                {
                    // SckSs[i] 若不為 null 表示已被實作過, 判斷是否有 Client 端連線
                    if (_sockets[i] != null)
                    {
                        // 如果Socket 沒有人連線, 便可提供給下一個 Client 進行連線
                        if (_sockets[i].Connected == false)
                        {
                            socketIndex = i;
                            Finded = true;

                            break;
                        }
                    }
                }

                // 如果 Finded 為 false 表示目前並沒有多餘的 Socket 可供 Client 連線
                if (Finded == false)
                {
                    if (_sockets.Length == 4)//滿四個人
                    {
                        try
                        {
                            //會停在這直到有 Client 端連上線
                            _sockets[0] = _sockets[0].Accept();
                            Console.WriteLine("第4個人加入");
                            return;
                        }
                        catch
                        {
                            Console.WriteLine("error");
                        }
                    }

                    // 增加 Socket 的數目以供下一個 Client 端進行連線
                    socketIndex = _sockets.Length;
                    Array.Resize(ref _sockets, socketIndex + 1);
                }

                try
                {
                    //會停在這直到有 Client 端連上線
                    _sockets[socketIndex] = _sockets[0].Accept();
                    Console.WriteLine("第" + (_sockets.Length - 1) + "個人加入");
                }
                catch
                {
                    Console.WriteLine("error");
                }
            }
        }

        // 廣播-Server傳送資料(message)給所有Client
        private void SendAll(string message)
        {
            for (int i = 1; i < _sockets.Length; i++)
            {
                if (_sockets[i] != null && _sockets[i].Connected == true)
                {
                    try
                    {
                        _sockets[i].Send(Encoding.ASCII.GetBytes(message));
                    }
                    catch
                    {
                        Console.WriteLine("send error");
                    }
                }
            }
        }

        //建立牌庫
        private void CreateCard()
        {
            _allCard.Clear();

            _allCard.Add("A");
            _allCard.Add("B");
            _allCard.Add("B");
            _allCard.Add("C");
            _allCard.Add("C");
            _allCard.Add("D");
            _allCard.Add("D");
            _allCard.Add("E");
            _allCard.Add("E");
            _allCard.Add("F");
            _allCard.Add("F");
            _allCard.Add("G");
            _allCard.Add("G");
            _allCard.Add("G");
            _allCard.Add("G");
            _allCard.Add("G");

            _allCard.Add("a");
            _allCard.Add("b");
            _allCard.Add("b");
            _allCard.Add("c");
            _allCard.Add("c");
            _allCard.Add("d");
            _allCard.Add("d");
            _allCard.Add("e");
            _allCard.Add("e");
            _allCard.Add("f");
            _allCard.Add("f");
            _allCard.Add("g");
            _allCard.Add("g");
            _allCard.Add("g");
            _allCard.Add("g");
            _allCard.Add("g");
        }

        //發牌(莊家五張，閒家四張)
        private void Start()
        {
            Random rnd = new Random();

            for (int player = 1; player <= 3; player++)
            {
                //抽閒家的牌(四張)
                string four = "";
                for (int i = 0; i < 4; i++)
                {
                    int index = rnd.Next(0, _allCard.Count);
                    four = four + _allCard[index];
                    _allCard.RemoveAt(index);
                }

                //傳送訊息給閒家
                try
                {
                    _sockets[player].Send(Encoding.ASCII.GetBytes(string.Format("Start_{0}_{1}", (player + 1), four)));
                }
                catch
                {
                    Console.WriteLine("send error");
                }
            }

            Console.WriteLine("發牌完成");

            //抽莊家的牌
            string five = "";
            for (int i = 0; i < 5; i++)
            {
                int index = rnd.Next(0, _allCard.Count);
                five = five + _allCard[index];
                _allCard.RemoveAt(index);
            }

            //傳送訊息給莊家(0)
            try
            {
                _sockets[0].Send(Encoding.ASCII.GetBytes(string.Format("Start_{0}_{1}", "1", five)));
            }
            catch
            {
                Console.WriteLine("send error");
            }

        }

    }
}
