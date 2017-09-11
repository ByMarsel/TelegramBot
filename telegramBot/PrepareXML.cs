using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace telegramBot
{
    class PrepareXML
    {
        XmlDocument document = new XmlDocument();
        public string path;
        public PrepareXML(string GLN, string type) {
            path = Directory.GetCurrentDirectory() ;
            document.Load(path+ "\\ORDERS.xml");
            var recipient = document.GetElementsByTagName("recipient");
            recipient.Item(0).InnerText = GLN;
            var seller = document.GetElementsByTagName("seller");
            seller.Item(0).ChildNodes.Item(0).InnerText =  GLN ;
            path  += $"\\ORDERS_{Guid.NewGuid()}.xml";
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
