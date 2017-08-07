using System;
using System.Net;
using System.IO;
using System.Net.Mail;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;

namespace Ip2Mail
{
    class DisplayInformation : SenderRecipientConf
    {
        public void Display()
        {
            Console.Clear();
            Console.WriteLine("****************************************");
            Console.WriteLine("* Welcome to Ip-address sender to mail *");
            Console.WriteLine("****************************************");
            Console.WriteLine("| Curent IP - " + senderIP["CurrentIp"]);
            Console.WriteLine("| Ip was changed - " + senderIP["Date"]);
            Console.WriteLine("****************************************");
            Console.WriteLine("|   1. Setup the configuration file.   |");
            Console.WriteLine("|   2. Add recipients.                 |");
            Console.WriteLine("|   3. Delete recipients.              |");
            Console.WriteLine("|   Esc. Exit from program.            |");
            Console.WriteLine("****************************************");
        }
    }
    class SenderRecipientConf
    {
        public static Dictionary<string, string> sender = new Dictionary<string, string>();
        public static Dictionary<string, string> senderIP = new Dictionary<string, string>();
        List<string> recipients = new List<string>();

        public string FileManipulation(string choice)
        {
            string fileflag;
            fileflag = "";
            Mutex mutexfile = new Mutex();
            mutexfile.WaitOne();
            switch (choice)
            {
                case "Notification":
                    {
                        sender.Clear();
                        using (StreamReader sr = new StreamReader(@"config.cfg"))
                        {
                            while (sr.Peek() >= 0)
                            {
                                sender.Add("SenderAddress", sr.ReadLine());
                                sender.Add("SenderName", sr.ReadLine());
                                sender.Add("SenderSmtp", sr.ReadLine());
                                sender.Add("SenderLogin", sr.ReadLine());
                                sender.Add("SenderPass", sr.ReadLine());
                                sender.Add("SenderPort", sr.ReadLine());
                                
                            }
                        }
                        recipients.Clear();
                        using (StreamReader sr = new StreamReader(@"reclist.txt"))
                        {
                            while (sr.Peek() >= 0)
                            {
                                recipients.Add(sr.ReadLine());
                            }
                        }
                        fileflag = "OK";
                    } break;

                case "cfg":
                    {
                        using (StreamWriter sw = new StreamWriter(@"config.cfg", false, System.Text.Encoding.Default))
                        {
                            Console.WriteLine("<-Выбран пункт меню.");
                            Console.WriteLine("Начинаем установку параметров.");
                            Console.WriteLine("Введите адрес отправителя:");
                            sw.WriteLine(Console.ReadLine());
                            Console.WriteLine("Введите имя отправителя:");
                            sw.WriteLine(Console.ReadLine());
                            Console.WriteLine("Введите сервер SMTP:");
                            sw.WriteLine(Console.ReadLine());
                            Console.WriteLine("Введите логин почты:");
                            sw.WriteLine(Console.ReadLine());
                            Console.WriteLine("Введите пароль:");
                            sw.WriteLine(Console.ReadLine());
                            Console.WriteLine("Введите порт:");
                            sw.WriteLine(Console.ReadLine());
                        }
                        fileflag = "OK";
                    }
                    break;

                case "recadd": //добавляем адреса получателей
                    {
                        FileInfo recfile = new FileInfo(@"reclist.txt");
                        int number = 0;
                        List<string> tmp = new List<string>();
                        Console.WriteLine("<-Выбран пункт меню.");
                        if (recfile.Exists)
                        {
                            using (StreamReader sr = new StreamReader(@"reclist.txt"))
                            {
                                //считываем из файла адреса получателей, добавляем их в коллекцию и выводим на консоль
                                while (sr.Peek() >= 0)
                                {
                                    tmp.Add(sr.ReadLine());
                                    Console.WriteLine("Получатель № - {0} : "+tmp[number],number);
                                    number ++;
                                }
                            }
                        }
                        //пользователь вводит адрес получателя, записываем в коллекцию
                        Console.WriteLine("Введите адрес получателя:");
                        tmp.Add(Console.ReadLine());
                        //записываем в файл адреса получателей из коллекции
                        using (StreamWriter sw = new StreamWriter(@"reclist.txt", false, System.Text.Encoding.Default))
                        {
                            foreach (string i in tmp)
                            {
                                sw.WriteLine(i) ;
                            }
                        }
                        fileflag = "OK";
                    }
                    break;

                case "recdel": //удаляем адреса получателей
                    {
                        FileInfo recfile = new FileInfo(@"reclist.txt");
                        int number = 0;
                        List<string> tmp = new List<string>();
                        Console.WriteLine("<-Выбран пункт меню.");
                        if (recfile.Exists)
                        {
                            using (StreamReader sr = new StreamReader(@"reclist.txt"))
                            {
                                //считываем из файла адреса получателей, добавляем их в коллекцию и выводим на консоль
                                while (sr.Peek() >= 0)
                                {
                                    tmp.Add(sr.ReadLine());
                                    Console.WriteLine("Получатель № - {0} : " + tmp[number], number);
                                    number++;
                                }
                            }
                            //пользователь вводит адрес получателя, удаляем из коллекции
                            Console.WriteLine("Введите адрес получателя для удаления:");
                            tmp.RemoveAt(Convert.ToInt32(Console.ReadLine(), 10));
                            //записываем в файл адреса получателей из коллекции
                            using (StreamWriter sw = new StreamWriter(@"reclist.txt", false, System.Text.Encoding.Default))
                            {
                                foreach (string i in tmp)
                                {
                                    sw.WriteLine(i);
                                }
                            }
                            fileflag = "OK";
                        } else fileflag = "error";
                    }
                    break;

                case "CurrentIp":
                    {
                        senderIP.Clear();
                        using (StreamReader sr = new StreamReader(@"currentip.txt"))
                        {
                                fileflag = sr.ReadLine();
                        }
                        senderIP.Add("CurrentIp", fileflag);
                        senderIP.Add("Date"," ");
                    } break;
                case "NewIp":
                    {
                        using (StreamWriter sw = new StreamWriter(@"currentip.txt", false, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(senderIP["CurrentIp"]);
                        }
                        fileflag = "OK";
                    }
                    break;

            }
            mutexfile.ReleaseMutex();
            return fileflag;

        }

        public void Notification()
        {
            string error = this.FileManipulation("Notification");
            foreach (string recaddr in recipients)
            {
                if (error=="OK")
                {
                    // отправитель - устанавливаем адрес и отображаемое в письме имя
                    MailAddress from = new MailAddress(sender["SenderAddress"], sender["SenderName"]);
                    // кому отправляем
                    MailAddress to = new MailAddress(recaddr);
                    // создаем объект сообщения
                    MailMessage m = new MailMessage(from, to);
                    // тема письма
                    m.Subject = "Изменение IP для доступа к рабочему серверу";
                    // текст письма
                    m.Body = "<font size = \"3\" color = \"red\" face = \"Tahoma\"> Уважаемые пользователи, изменились коды доступа к серверам: </font>"
                           + "<br><font size = \"3\" color = \"green\" face = \"Tahoma\"> <u>MegaServer</u>  </font>" + senderIP["CurrentIp"] + ":50005"
                           + "<br><font size = \"3\" color = \"green\" face = \"Tahoma\"> <u>Server</u>  </font>" + senderIP["CurrentIp"] + ":50006";

                    // письмо представляет код html
                    m.IsBodyHtml = true;
                    // адрес smtp-сервера и порт, с которого будем отправлять письмо
                    SmtpClient smtp = new SmtpClient(sender["SenderSmtp"], Convert.ToInt32(sender["SenderPort"],10));
                    // логин и пароль
                    smtp.Credentials = new NetworkCredential(sender["SenderLogin"], sender["SenderPass"]);
                    smtp.EnableSsl = true;
                    smtp.Send(m);
                }
            }
            
        }

    }
   
    class IpCheck : DisplayInformation
    {
        public string CurrentIp;
        public CultureInfo culture = new CultureInfo("ru-RU");
        

        public void IpCompare()
        {
            while (true)
            {
                string newIp;
                newIp = this.Get();
                
                //Notification every morning in 08:00
                if (DateTime.Now.ToShortTimeString() == "8:00") { Notification(); }
                if ((newIp != senderIP["CurrentIp"]) && (newIp != "error"))
                {
                    senderIP["CurrentIp"] = newIp;
                    senderIP["Date"] = DateTime.Now.ToString();
                    FileManipulation("NewIp");
                    Display();
                    Notification();
                } 

                Thread.Sleep(60000);
            }
        }
        
        public string Get()
        {
            StreamReader reader;
            HttpWebRequest httpWebRequest;
            HttpWebResponse httpWebResponse;
            Mutex getip = new Mutex();

            getip.WaitOne();
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create("http://checkip.dyndns.org");
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                reader = new StreamReader(httpWebResponse.GetResponseStream());
                getip.ReleaseMutex();
                return System.Text.RegularExpressions.Regex.Match(reader.ReadToEnd(), @"(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})").Groups[1].Value;
            }
            catch
            {
                getip.ReleaseMutex();
                return "error";
            }

        }
    }
    class Program
    {
        
        static void Main(string[] args)
        {
            DisplayInformation info = new DisplayInformation();
            IpCheck MyIp = new IpCheck();
            SenderRecipientConf conf = new SenderRecipientConf();


            FileInfo ipfile = new FileInfo(@"currentip.txt");
            if (!ipfile.Exists) 
            {
                    Console.WriteLine("Determine the current Ip-adress... Please wait.");
                    do { MyIp.CurrentIp = MyIp.Get(); } while (MyIp.CurrentIp == "error");
                    using (StreamWriter sw = new StreamWriter(@"currentip.txt", false, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(MyIp.CurrentIp);
                    }
            }

            conf.FileManipulation("CurrentIp");

            info.Display();
            

              Thread checkthread = new Thread(MyIp.IpCompare);
              checkthread.IsBackground = true;
              checkthread.Start();



           ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey();

                switch (cki.Key)
                {
                    case ConsoleKey.D1:
                        if (conf.FileManipulation("cfg") == "OK")
                        {
                            Console.WriteLine("Configuration complete. Enter any key...");
                            Console.ReadLine();
                            info.Display();
                        }
                        break;
                    case ConsoleKey.D2:
                        if (conf.FileManipulation("recadd") == "OK")
                        {
                            Console.WriteLine("Addition succeeded. Enter any key...");
                            Console.ReadLine();
                            info.Display();
                        }
                        break;
                    case ConsoleKey.D3:
                        if (conf.FileManipulation("recdel") == "OK")
                        {
                            Console.WriteLine("Successfully deleted. Enter any key...");
                            Console.ReadLine();
                            info.Display();
                        }
                        else {
                               Console.WriteLine("Recipient file is empty. Enter any key...");
                               Console.ReadLine();
                               info.Display();
                             }
                        break;
                    case ConsoleKey.Escape:
                        break;
                    default:
                        info.Display();
                        Console.WriteLine("Input Error. Try again.");
                        break;
                }

            } while (cki.Key != ConsoleKey.Escape);

        }
    }
}
