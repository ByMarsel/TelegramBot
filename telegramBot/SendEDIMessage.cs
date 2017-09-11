using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace telegramBot
{
    class SendEDIMessage
    {
        string GLN;
        string messageType;

        public SendEDIMessage(string _GLN, string _messageType) {
            GLN = _GLN;
            messageType = _messageType;
        }
        public bool Send() {

            FTPClient ftpClient = new FTPClient();
            PrepareXML nd = new PrepareXML(this.GLN, "");
            try
            {
                ftpClient.UploadFile(nd.path);
            }
            catch(Exception) {
                return false;
            }
            File.Delete(nd.path);
            return true; 
        }
    }
}
