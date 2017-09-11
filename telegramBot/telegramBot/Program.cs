using System;
using System.Threading;
using static Telegram.Request;
using telegramBot;
using System.Text.RegularExpressions;
using NLog;
using System.Windows.Forms;

namespace Telegram
{
    class Program
    {
        public static Logger log;


        static void Main(string[] args)
        {
            try
            {
                log = LogManager.GetCurrentClassLogger();

                log.Trace("Version: {0}", Environment.Version.ToString());
                log.Trace("OS: {0}", Environment.OSVersion.ToString());
                log.Trace("Command: {0}", Environment.CommandLine.ToString());

                NLog.Targets.FileTarget tar = (NLog.Targets.FileTarget)LogManager.Configuration.FindTargetByName("run_log");
                tar.DeleteOldFileOnStartup = false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка работы с логом!n" + e.Message);
            }

            SendEDIMessage sendRecadvTask = new SendEDIMessage("","RECADV");
            Thread th = new Thread(new ThreadStart(SendEDIMessage.SendRecadv));
            th.Start();
            TelegramRequest Tr = new TelegramRequest(Settings.Default.Token);
           
            Tr.MessageText += Tr_MessageText;
            
           
            //=====МЕТОДЫ=====
           
            //================
            Tr.GetUpdates();
        }
        private static void Tr_MessageText(object sender, MessageText e)
        {
            ApiMethods apiMethods = new ApiMethods(Settings.Default.Token);        
           
            if (e.text != "/start")
            {
                Regex regex = new Regex(@"\d{13}");
                if (regex.IsMatch(e.text)&&e.text.Length==13)
                {
                    try
                    {
                        SendEDIMessage sendMessage = new SendEDIMessage(e.text, "ORDERS");
                        if (sendMessage.Send())
                        {
                            apiMethods.SendMessage("Cообщение успешно отправлено!", int.Parse(e.chat.id));
                        }
                        else
                        {
                            apiMethods.SendMessage("Не получилось отправить сообщение, попробуйте позже :)", int.Parse(e.chat.id));
                        }
                    }
                    catch (Exception ex) {
                        apiMethods.SendMessage($"Бот поломался: {ex.Message}", 64958128);
                    }
                }
                else {
                    apiMethods.SendMessage("Ты ошибся :( вышли корректный GLN", int.Parse(e.chat.id));
                }
            }
            else {
                apiMethods.SendMessage("Привет! Вышли GLN поставщика :)(только цифарки)", int.Parse(e.chat.id));
            }
        }
    }
}
