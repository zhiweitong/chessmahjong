using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Collections;

namespace Server
{
    class Program
    {
        private Socket[] _sockets;
        private List<string> _allCard = new List<string>();
        private Dictionary<string, string> _dictionary = new Dictionary<string, string>();
        int _playerNow = 0;//目前玩家(數字+1)

        static void Main(string[] args)
        {
            Console.WriteLine("Server啟動");
            Program program = new Program();

            program.Listen();

            program.CreateDictionary(); //建立字典
            Console.WriteLine("遊戲開始");

            while (true)
            {
                program.CreateCard();//建立牌庫

                program.Game();

                program.Change();
                Console.WriteLine("莊家換下一位，重新開始\n");
                program._playerNow = 0;
            }
        }

        private void Game()
        {
            string msg = "";
            int player = -1;

            Start();//發牌

            //看莊家有沒有天胡
            if (ReceiveOne(0).Split('_')[2].Equals("true"))//天胡
            {
                SendAll("Check_true_1_"); //廣播胡牌
                return;
            }
            else
                SendAll("Check_false_"); //廣播沒胡牌

            while (true)
            {
                msg = ReceiveOne(_playerNow);//收玩家出牌訊息
                player = Convert.ToInt32(msg.Split('_')[1]);
                Console.WriteLine("玩家 {0} 打出 {1} ", player + 1, _dictionary[msg.Split('_')[2]]);
                SendAll(string.Format("New_{0}_{1}_", player + 1, msg.Split('_')[2]));//廣播玩家出牌

                if (CheckWin())//所有玩家胡牌確認
                    return;
                else
                {
                    Console.WriteLine("沒有人胡牌");

                    if (_allCard.Count == 0)//牌庫沒牌
                    {
                        SendAll("Over_"); //廣播流局
                        Console.WriteLine("流局!");
                        return;
                    }

                    SendAll("Check_false_");
                }

                Console.WriteLine("現在是玩家{0}", _playerNow + 1);

                msg = ReceiveOne(_playerNow);//收下家要牌(不吃)or出牌(吃)訊息
                if (msg.Split('_')[1].Equals("true"))
                {
                    SendAll(string.Format("Player_{0}_getNewOne_", _playerNow + 1)); //廣播玩家摸牌
                    Console.WriteLine("玩家 {0} 選擇摸牌", _playerNow + 1);

                    SendOneCard(_playerNow);//給玩家一張牌

                    msg = ReceiveOne(_playerNow);//收自摸訊息

                    if (msg.Split('_')[2].Equals("true")) //自摸
                    {
                        Console.WriteLine("玩家 {0} 自摸胡牌", _playerNow + 1);
                        SendAll(string.Format("Check_true_{0}_", _playerNow + 1)); //廣播胡牌
                        return;
                    }
                    else
                        Console.WriteLine("玩家 {0} 沒有自摸", _playerNow + 1);
                }
                else
                {
                    SendAll(string.Format("Player_{0}_eat_", _playerNow + 1)); //廣播玩家吃牌
                    Console.WriteLine("玩家 {0} 選擇吃牌", _playerNow + 1);
                }
            }
        }

        // 開socket
        private void Listen()
        {
            // 用 Resize 的方式動態增加 Socket 的數目
            Array.Resize(ref _sockets, 1);

            Console.WriteLine("請輸入連線IP :");
            string input = Console.ReadLine();

            try
            {
                _sockets[0] = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _sockets[0].Bind(new IPEndPoint(IPAddress.Parse(input), 8888));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("等待連線...四人加入後自動開始");

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
            for (int i = 0; i < _sockets.Length; i++)
            {
                if (_sockets[i] != null && _sockets[i].Connected == true)
                {
                    Send(_sockets[i], message);
                }
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
            _dictionary.Add("d", "硨");//因"俥"字在CMD為亂碼，故用"硨"替代
            _dictionary.Add("e", "傌");
            _dictionary.Add("f", "炮");
            _dictionary.Add("g", "兵");
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
           
            //處理閒家
            for (int player = 1; player <= 3; player++)
            {
                //抽閒家的牌(四張)
                string four = "";
                for (int i = 0; i < 4; i++)

                {
                    int index = rnd.Next(0, _allCard.Count);
                    four = four + _allCard[index] + ".";
                    _allCard.RemoveAt(index);
                }

                //傳送訊息給閒家
                Send(_sockets[player], string.Format("Start_{0}_{1}", (player + 1), four));

            }

            //抽莊家的牌
            string five = "";
            for (int i = 0; i < 5; i++)
            {
                int index = rnd.Next(0, _allCard.Count);
                five = five + _allCard[index] + ".";
                _allCard.RemoveAt(index);
            }

            //傳送訊息給莊家(0)
            Send(_sockets[0], string.Format("Start_{0}_{1}", "1", five));

            Console.WriteLine("發牌完成");
        }

        //從牌庫拿出一張牌給玩家
        private void SendOneCard(int playerIndex)
        {
            Random rnd = new Random();
            int cardIndex = rnd.Next(0, _allCard.Count);
            string card = _allCard[cardIndex];
            _allCard.RemoveAt(cardIndex);

            Send(_sockets[playerIndex], string.Format("One_{0}_", card));//發一張牌

            Console.WriteLine("發給玩家{0}一張牌 {1} ，牌庫剩下{2}張牌", playerIndex + 1, _dictionary[card], _allCard.Count);
        }

        //收胡牌訊息
        private bool CheckWin()
        {
            byte[] data1 = new byte[20];
            byte[] data2 = new byte[20];
            byte[] data3 = new byte[20];
            byte[] data4 = new byte[20];

            _sockets[0].Receive(data1);
            _sockets[1].Receive(data2);
            _sockets[2].Receive(data3);
            _sockets[3].Receive(data4);

            string[] msg = new string[4];

            msg[0] = Encoding.Default.GetString(data1);
            msg[1] = Encoding.Default.GetString(data2);
            msg[2] = Encoding.Default.GetString(data3);
            msg[3] = Encoding.Default.GetString(data4);

            bool win = false;

            _playerNow++;//換下一位玩家了
            if (_playerNow == 4)
                _playerNow = 0;

            int k = _playerNow;

            for (int i = 0; i < 4; i++)
            {
                if (msg[k].Split('_')[2].Equals("true"))
                {
                    Console.WriteLine("玩家{0}胡牌了", k + 1);

                    //廣播胡牌
                    SendAll(string.Format("Check_true_{0}_", k + 1));

                    win = true;
                    break;
                }
                else
                {
                    k++;
                    if (k == 4)
                        k = 0;
                }
            }
            return win;
        }

        //收某一位玩家的訊息
        private string ReceiveOne(int player)
        {
            try
            {
                byte[] clientData = new byte[20];

                // 程式會被 hand 在此, 等待接收來自 Server 端傳來的資料
                _sockets[player].Receive(clientData);
                string message = Encoding.Default.GetString(clientData);

                return message;
            }
            catch
            {
                Console.WriteLine("receiveOne error");
                return "error";
            }
        }
        //莊家及閒家換人
        private void Change()
        {
            Socket[] NewSockets = new Socket[4];

            NewSockets[0] = _sockets[3];
            NewSockets[1] = _sockets[0];
            NewSockets[2] = _sockets[1];
            NewSockets[3] = _sockets[2];

            _sockets = NewSockets;
        }
        //傳送訊息
        private void Send(Socket sock,string msg)
        {
            ArrayList list = new ArrayList();
            list.Add(sock);

            Socket.Select(null, list, null, -1);

            ((Socket)list[list.Count - 1]).Send(Encoding.ASCII.GetBytes(msg));
        }
    }
}
