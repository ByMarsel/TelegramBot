using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Telegram;

namespace telegramBot
{
    class PrepareXML
    {
       
        public string path;
        public PrepareXML(string GLN, string type, string desadvName ) {
            Program.log.Info($"Начали подготавливать файл для поставщика: {GLN} тип сообщения {type}");
            XmlDocument document = new XmlDocument();
            path = Directory.GetCurrentDirectory();
            document.Load(path+ $"\\{type}.xml");
            var recipient = document.GetElementsByTagName("recipient");
            var messageID = document.GetElementsByTagName("eDIMessage");
            messageID.Item(0).Attributes.Item(0).Value = Guid.NewGuid().ToString();
            recipient.Item(0).InnerText = GLN;
            var seller = document.GetElementsByTagName("seller");
            seller.Item(0).ChildNodes.Item(0).InnerText =  GLN;
            path  += $"\\{type}_{Guid.NewGuid()}.xml";
            if (type == "RECADV") {
                XmlDocument desadvXML = new XmlDocument();
                desadvXML.Load(Path.GetDirectoryName(path) + "\\" + desadvName);
                var despatchAdvice = desadvXML.GetElementsByTagName("despatchAdvice");
                var despatchIdentificator = document.GetElementsByTagName("despatchIdentificator");
                var senderDesadvGLN = desadvXML.GetElementsByTagName("sender").Item(0).InnerText;
                var sellerDesadvGLN = desadvXML.GetElementsByTagName("seller").Item(0).ChildNodes.Item(0).InnerText;
                var shipFromDesadv = desadvXML.GetElementsByTagName("shipFrom").Item(0).ChildNodes.Item(0).InnerText;
                document.GetElementsByTagName("shipFrom").Item(0).ChildNodes.Item(0).InnerText = shipFromDesadv;
                recipient.Item(0).InnerText = senderDesadvGLN;
                seller.Item(0).ChildNodes.Item(0).InnerText = sellerDesadvGLN;
                var desadvNumber = despatchAdvice.Item(0).Attributes.Item(0).Value.ToString();
                var desadvDate = despatchAdvice.Item(0).Attributes.Item(1).Value.ToString();
                despatchIdentificator.Item(0).Attributes.Item(0).Value = desadvNumber;
                despatchIdentificator.Item(0).Attributes.Item(1).Value = desadvDate;


            }
            Program.log.Info($"Сохраняем файл: {Path.GetFileName(path)}");
            document.Save(path);
        }
        //document("");
        //XmlNode element = document.CreateElement("element");
        //document.DocumentElement.AppendChild(element); // указываем родителя
        //XmlAttribute attribute = document.CreateAttribute("number"); // создаём атрибут
        //attribute.Value = 1; // устанавливаем значение атрибута
        //element.Attributes.Append(attribute);
    }
}
