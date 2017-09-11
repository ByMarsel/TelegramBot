using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            PrepareXML nd = new PrepareXML(this.GLN, messageType,"");
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
        public static void SendRecadv() {
            PrepareXML nd;
            while (true)
            {
                FTPClient ftpClient = new FTPClient();
                FTPClient.FileStruct[] files = ftpClient.ListDirectory();
                foreach (FTPClient.FileStruct file in files) {
                    if (file.Name!=null&&file.Name.StartsWith("DESADV"))
                    {
                        try
                        {
                            ftpClient.DownloadFile(Directory.GetCurrentDirectory(), file.Name);
                            nd = new PrepareXML("", "RECADV", file.Name);
                            ftpClient.UploadFile(nd.path);
                            ftpClient.DeleteFile(file.Name);
                            File.Delete(nd.path);
                        }
                        catch (Exception ex) {
                            Telegram.Program.log.Error($"Сломались: {ex.Message}");
                        }
                       
                    }
                    else {
                        try
                        {
                            ftpClient.DeleteFile(file.Name);
                        }
                        catch (Exception ex) {
                            Telegram.Program.log.Error($"Сломались: {ex.Message}");
                        }
                    }
                    if (file.Name != null)
                    {
                        File.Delete(Directory.GetCurrentDirectory() + @"/" + Path.GetFileName(file.Name));
                    }
                    
                }
                Thread.Sleep(180000);
            }
        }
    }
}
