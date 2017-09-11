using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace telegramBot
{
    class FTPClient
    {
        //поля
        //поле для хранения имени фтп-сервера
        private string _Host = "ftp-edi.kontur.ru";

        //поле для хранения логина
        private string _UserName = "4607195499990";

        //поле для хранения пароля
        private string _Password = "zP3H3fE9X452o6s";

        //объект для запроса данных
        FtpWebRequest ftpRequest;

        //объект для получения данных
        FtpWebResponse ftpResponse;

        //флаг использования SSL
        private bool _UseSSL = false;

        //фтп-сервер
        public string Host 
        {
            get
            {
                return _Host;
            }
          
        }
        //логин
        public string UserName
        {
            get
            {
                return _UserName;
            }
           
          
        }
        //пароль
        public string Password
        {
            get
            {
                return _Password;
            }
        }
        //Для установки SSL-чтобы данные нельзя было перехватить
        public bool UseSSL
        {
            get
            {
                return _UseSSL;
            }
            set
            {
                _UseSSL = value;
            }
        }
 
        //метод протокола FTP RETR для загрузки файла с FTP-сервера
        public void DownloadFile(string fileName)
        {

            ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + _Host + "Inbox" + "/" + fileName);

            ftpRequest.Credentials = new NetworkCredential(_UserName, _Password);
            //команда фтп RETR
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            ftpRequest.EnableSsl = _UseSSL;
            //Файлы будут копироваться в кталог программы
            FileStream downloadedFile = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);

            ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            //Получаем входящий поток
            Stream responseStream = ftpResponse.GetResponseStream();

            //Буфер для считываемых данных
            byte[] buffer = new byte[1024];
            int size = 0;

            while ((size = responseStream.Read(buffer, 0, 1024)) > 0)
            {
                downloadedFile.Write(buffer, 0, size);

            }
            ftpResponse.Close();
            downloadedFile.Close();
            responseStream.Close();
        }
        //метод протокола FTP STOR для загрузки файла на FTP-сервер
        public void UploadFile(string fileName)
        {
            //для имени файла
            string shortName = fileName.Remove(0, fileName.LastIndexOf("\"" ) + 1);


            FileStream uploadedFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            ftpRequest = (FtpWebRequest)WebRequest.Create(@"ftp://" + _Host + @"/Outbox/" + Path.GetFileName( shortName));
            ftpRequest.Credentials = new NetworkCredential(_UserName, _Password);
            ftpRequest.EnableSsl = _UseSSL;
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            //Буфер для загружаемых данных
            byte[] file_to_bytes = new byte[uploadedFile.Length];
            //Считываем данные в буфер
            uploadedFile.Read(file_to_bytes, 0, file_to_bytes.Length);

            uploadedFile.Close();

            //Поток для загрузки файла 
            Stream writer = ftpRequest.GetRequestStream();

            writer.Write(file_to_bytes, 0, file_to_bytes.Length);
            writer.Close();
        }
        //метод протокола FTP DELE для удаления файла с FTP-сервера 
        public void DeleteFile(string path)
        {
            ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + "Inbox" + path);
            ftpRequest.Credentials = new NetworkCredential(_UserName, _Password);
            ftpRequest.EnableSsl = _UseSSL;
            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

            FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            ftpResponse.Close();
        }

        //метод протокола FTP MKD для создания каталога на FTP-сервере 
    }
    //Для парсинга полученного детального списка каталогов фтп-сервера
    //Структура для хранения детальной информации о файде или каталоге
   
    }

