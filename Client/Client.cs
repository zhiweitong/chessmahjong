using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Collections;

namespace Client
{
    class Client
    {
        private Socket _socket;
        private Dictionary<string, string> _dictionary = new Dictionary<string, string>();
        private string[] _handCard;
        private int _position = -1; //玩家位置
        private int _playerNow = 1; //目前玩家
        private bool _gameOver = false;
        private string _lastCard = ""; //上一張被丟出的牌

        static void Main(string[] args)
        {
            Client program = new Client();
            program.CreateDictionary(); //建立字典

            program.Connect();//連線到server

            while (true)
            {
                while (!program._gameOver)
                    program.Assignment(program.Receive());

                Console.WriteLine("本局結束，莊家換人，開始下一場\n");

                program._position = -1;
                program._playerNow = 1;
                program._lastCard = "";
                program._gameOver = false;
                program._handCard = null;
            }
        }

        //與server進行連線
        private void Connect()
        {
            try
            {
                Console.WriteLine("請輸入連線IP :");
                string input = Console.ReadLine();

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _socket.Connect(new IPEndPoint(IPAddress.Parse(input), 8888));

                Console.WriteLine("連線成功");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        //傳送訊息
        private void Send(string msg)
        {
            ArrayList list = new ArrayList();
            list.Add(_socket);

            Socket.Select(null, list, null, -1);

            ((Socket)list[list.Count - 1]).Send(Encoding.ASCII.GetBytes(msg));
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
            _dictionary.Add("d", "硨");//因"俥"字在CMD為亂碼，故用"硨"替代
            _dictionary.Add("e", "傌");
            _dictionary.Add("f", "炮");
            _dictionary.Add("g", "兵");
        }
        //創建手牌字串陣列
        private string[] Create5CardArray(string[] four, string one)
        {
            string[] five = new string[5];

            for (int i = 0; i < 4; i++)
            {
                five[i] = four[i];
            }
            five[4] = one;

            Array.Sort(five, string.CompareOrdinal);

            return five;
        }

        //取得開局手牌
        private void GetHandCard(string msg)
        {
            _handCard = msg.Split('.');
            Array.Resize(ref _handCard, _handCard.Length - 1);

            Array.Sort(_handCard, string.CompareOrdinal); ///整理牌

            Console.WriteLine("\n遊戲開始!\n");

            if (_position == 1)
            {
                Console.WriteLine("你是玩家 1 號，是莊家");

                PrintHandCard();

                //判斷天胡
                if (WinCheck(_handCard) == true)
                {
                    Console.WriteLine("天胡!");
                    Send("Check_0_true_");
                    //_socket.Send(Encoding.ASCII.GetBytes("Check_0_true_"));//天胡
                }
                else
                    Send("Check_0_false_");

                //_socket.Send(Encoding.ASCII.GetBytes("Check_0_false_"));//沒有天胡
            }
            else
            {
                Console.WriteLine("你是玩家 {0} 號，是閒家", _position);

                PrintHandCard();
            }
        }

        //出一張牌
        private void Discard()
        {
            Console.WriteLine("           0  1  2  3  4");

            Console.WriteLine("請出牌(輸入數字) :");

            int input = -1;
            while (true)
            {
                string inputStr = Console.ReadLine();
                if (IsNumeric(inputStr))//是數字
                {
                    input = Convert.ToInt32(inputStr);
                    if (input >= 0 || input <= 4) //檢查數字
                        break;
                    else
                        Console.WriteLine("輸入錯誤，請重新輸入數字:");
                }
                else
                    Console.WriteLine("輸入錯誤，請重新輸入數字:");
            }

            try
            {
                Send(string.Format("Next_{0}_{1}_", (_position - 1), _handCard[input]));

                //_socket.Send(Encoding.ASCII.GetBytes(string.Format("Next_{0}_{1}_", (_position - 1), _handCard[input])));//發送出牌訊息
            }
            catch
            {
                Console.WriteLine("出一張牌時發生錯誤 無法傳送");
            }

            //將手牌調整回四張
            List<string> list = new List<string>(_handCard);
            list.RemoveAt(input);
            _handCard = list.ToArray();

            PrintHandCard();
        }

        //判斷胡牌
        private bool WinCheck(string[] cards)
        {
            string str = "";

            foreach (string card in cards)
            {
                str = str + card;
            }

            switch (str) //看看是否為六種胡牌牌型
            {
                case "ABCGG"://將士象卒卒
                    return true;

                case "abcgg"://帥仕相兵兵
                    return true;

                case "DEFGG"://車馬包卒卒
                    return true;

                case "defgg"://俥傌炮兵兵
                    return true;

                case "GGGgg"://卒卒卒兵兵
                    return true;

                case "GGggg"://卒卒兵兵兵
                    return true;

                default:
                    return false;
            }
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
                    Console.Write("玩家 {0} 打出 ", str[1]);

                    char[] c = str[2].ToCharArray();
                    if (c[0] >= 97 && c[0] <= 122)//小寫
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("{0}", _dictionary[str[2]]);
                    Console.ForegroundColor = ConsoleColor.White;

                    _lastCard = str[2];

                    //更新目前玩家
                    _playerNow++;
                    if (_playerNow == 5)
                        _playerNow = 1;

                    //判斷胡牌
                    if (WinCheck(Create5CardArray(_handCard, str[2])))
                        Send(string.Format("Check_{0}_true_", _position + 1));
                        //_socket.Send(Encoding.ASCII.GetBytes(string.Format("Check_{0}_true_", _position + 1)));//胡牌
                    else
                        Send(string.Format("Check_{0}_false_", _position + 1));
                        //_socket.Send(Encoding.ASCII.GetBytes(string.Format("Check_{0}_false_", _position + 1)));
                    break;

                case "Check": //是否胡牌的通知
                    if (str[1].Equals("true"))
                    {
                        Console.WriteLine("玩家 {0} 胡牌了", str[2]);
                        _gameOver = true;
                    }
                    else
                    {
                        if (_position == _playerNow)
                        {
                            if (_handCard.Length == 5) //莊家首局
                                Discard(); //出牌
                            else
                            {
                                PrintHandCard();

                                Console.WriteLine("要吃嗎? (請輸入數字) 不吃:0  吃:1");

                                int input = -1;
                                while (true)
                                {
                                    string inputStr = Console.ReadLine();
                                    if (IsNumeric(inputStr))//是數字
                                    {
                                        input = Convert.ToInt32(inputStr);
                                        if (input == 0 || input == 1) //檢查數字
                                            break;
                                        else
                                            Console.WriteLine("輸入錯誤，請重新輸入數字 (不吃:0  吃:1)");
                                    }
                                    else
                                        Console.WriteLine("輸入錯誤，請重新輸入數字 (不吃:0  吃:1)");
                                }

                                if (input == 0) //不吃
                                    Send("Want_true_");
                                //_socket.Send(Encoding.ASCII.GetBytes("Want_true_"));//要牌
                                else //吃
                                {
                                    //_socket.Send(Encoding.ASCII.GetBytes("Want_false_"));
                                    Send("Want_false_");

                                    _handCard = Create5CardArray(_handCard, _lastCard); //將上家打的牌放入手牌 
                                }
                            }
                        }
                    }
                    break;

                case "One": //抽到一張牌
                    Console.Write("拿一張牌，拿到 ");

                    char[] ch = str[1].ToCharArray();
                    if (ch[0] >= 97 && ch[0] <= 122)//小寫
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("{0}", _dictionary[str[1]]);
                    Console.ForegroundColor = ConsoleColor.White;

                    _handCard = Create5CardArray(_handCard, str[1]);

                    PrintHandCard();

                    //判斷自摸胡牌
                    if (WinCheck(Create5CardArray(_handCard, str[1])))//胡牌
                    {
                        Console.WriteLine("自摸!");
                        //_socket.Send(Encoding.ASCII.GetBytes(string.Format("Check_{0}_true_", _position + 1)));
                        Send(string.Format("Check_{0}_true_", _position + 1));
                    }
                    else
                    {
                        //_socket.Send(Encoding.ASCII.GetBytes(string.Format("Check_{0}_flase_", _position + 1)));
                        Send(string.Format("Check_{0}_flase_", _position + 1));
                        Discard();//打一張
                    }

                    break;

                case "Player":
                    if (str[2].Equals("eat"))
                    {
                        Console.WriteLine("玩家 {0} 選擇吃牌", str[1]);

                        string number = Convert.ToString(_position);

                        if (str[1].Equals(number))
                        {
                            PrintHandCard();

                            Discard();//出一張牌
                        }
                    }
                    else
                        Console.WriteLine("玩家 {0} 不吃牌，摸一張", str[1]);
                    break;

                case "Over": //流局
                    Console.WriteLine("流局!");
                    _gameOver = true;
                    break;
            }
        }

        //印出手牌(中文)
        private void PrintHandCard()
        {
            Console.Write("你的手牌: ");
            foreach (string card in _handCard) //印出手牌
            {
                char[] c = card.ToCharArray();

                if (c[0] >= 97 && c[0] <= 122)//小寫
                    Console.ForegroundColor = ConsoleColor.Red;

                Console.Write("{0} ", _dictionary[card]);
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("");
        }

        //判斷輸入值是否為數字
        public bool IsNumeric(string str)
        {
            try
            {
                int i = Convert.ToInt32(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

