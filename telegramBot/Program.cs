using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;
using System.Net;
using System.Threading;
using Telegram;
using static Telegram.Request;
using telegramBot;

namespace Telegram
{
    class Program
    {
       
        static void Main(string[] args)
        {
            TelegramRequest Tr = new TelegramRequest(Settings.Default.Token);
           
            Tr.MessageText += Tr_MessageText;
            
           
            //=====МЕТОДЫ=====
           
            //================
            Tr.GetUpdates();
        }
        private static void Tr_MessageText(object sender, MessageText e)
        {
            ApiMethods apiMethods = new ApiMethods(Settings.Default.Token);
            apiMethods.SendMessage("Привет! Вышли GLN поставщика :)(только цифарки)", int.Parse(e.chat.id));
            SendEDIMessage sendMessage = new SendEDIMessage(e.text, "");
            if (e.text != "/start")
            {
                if (sendMessage.Send())
                {
                    apiMethods.SendMessage("Cообщение успешно отправлено!", int.Parse(e.chat.id));
                }
                else
                {
                    apiMethods.SendMessage("Не получилось отправить сообщение, попробуйте позже :)", int.Parse(e.chat.id));
                }
            }
        }
    }
}
