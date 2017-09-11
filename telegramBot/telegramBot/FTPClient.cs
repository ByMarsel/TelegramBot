using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram;

namespace telegramBot
{
    class FTPClient
    {
        //поля
        //поле для хранения имени фтп-сервера
        private string _Host = "ftp-edi.kontur.ru";

        //поле для хранения логина
        private string _UserName = "2000000000787";

        //поле для хранения пароля
        private string _Password = "1j3bfww70492fkq";

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
        public FileStruct[] ListDirectory()
        {
            Program.log.Info("Загружаем список файлов на FTP"); 
            //Создаем объект запроса
            try
            {
                ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://" + _Host + @"/inbox/");
                //логин и пароль
                ftpRequest.Credentials = new NetworkCredential(_UserName,_Password);
                //команда фтп LIST
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                ftpRequest.EnableSsl = _UseSSL;
                //Получаем входящий поток
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
               
            }
            catch (Exception e) {
                Program.log.Error("Не смогли загрузить список файлов FTP!");
                return new FileStruct[] {new FileStruct() };
               
            }
            //переменная для хранения всей полученной информации
            string content = "";

            StreamReader sr = new StreamReader(ftpResponse.GetResponseStream(), System.Text.Encoding.ASCII);
            content = sr.ReadToEnd();
            sr.Close();
            ftpResponse.Close();

            DirectoryListParser parser = new DirectoryListParser(content);
            Program.log.Info($"Cписок файлов FTP загружен! Количество {parser.FullListing.Length}");
            return parser.FullListing;
        }
        public void DownloadFile(string path, string fileName)
        {

            Program.log.Info($"Начали загрузку файла: {fileName}");
            ftpRequest = (FtpWebRequest)WebRequest.Create(@"ftp://" + _Host + @"/Inbox/" + fileName);

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
            Program.log.Info($"Файл загружен: {fileName}");
            ftpResponse.Close();
            downloadedFile.Close();
            responseStream.Close();
        }
        public void DeleteFile(string fileName)
        {
            if (fileName != null)
            {
                Program.log.Info($"Начали удаление файла: {fileName}");
                ftpRequest = (FtpWebRequest)WebRequest.Create(@"ftp://" + _Host + @"/Inbox/" + fileName);
                ftpRequest.Credentials = new NetworkCredential(_UserName, _Password);
                ftpRequest.EnableSsl = _UseSSL;
                ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                Program.log.Info($"Файл удален: {fileName}");
                ftpResponse.Close();
            }
        }
        //метод протокола FTP STOR для загрузки файла на FTP-сервер
        public void UploadFile(string fileName)
        {
            Program.log.Info($"Начали выгружать файл на FTP: {fileName}");
            //для имени файла
            string shortName = fileName.Remove(0, fileName.LastIndexOf("\"") + 1);


            FileStream uploadedFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);

            ftpRequest = (FtpWebRequest)WebRequest.Create(@"ftp://" + _Host + @"/Outbox/" + Path.GetFileName(shortName));
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
            Program.log.Info($"Файл на выгружен FTP: {fileName}");
            writer.Close();
        }
        public struct FileStruct
        {
            public string Flags;
            public string Owner;
            public bool IsDirectory;
            public string CreateTime;
            public string Name;
        }

        public enum FileListStyle
        {
            UnixStyle,
            WindowsStyle,
            Unknown
        }
        //Класс для парсинга
        public class DirectoryListParser
        {
            private List<FileStruct> _myListArray;

            public FileStruct[] FullListing
            {
                get
                {
                    return _myListArray.ToArray();
                }
            }

            public FileStruct[] FileList
            {
                get
                {
                    List<FileStruct> _fileList = new List<FileStruct>();
                    foreach (FileStruct thisstruct in _myListArray)
                    {
                        if (!thisstruct.IsDirectory)
                        {
                            _fileList.Add(thisstruct);
                        }
                    }
                    return _fileList.ToArray();
                }
            }

            public FileStruct[] DirectoryList
            {
                get
                {
                    List<FileStruct> _dirList = new List<FileStruct>();
                    foreach (FileStruct thisstruct in _myListArray)
                    {
                        if (thisstruct.IsDirectory)
                        {
                            _dirList.Add(thisstruct);
                        }
                    }
                    return _dirList.ToArray();
                }
            }

            public DirectoryListParser(string responseString)
            {
                _myListArray = GetList(responseString);
            }

            private List<FileStruct> GetList(string datastring)
            {
                List<FileStruct> myListArray = new List<FileStruct>();
                string[] dataRecords = datastring.Split('\n');
                //Получаем стиль записей на сервере
                FileListStyle _directoryListStyle = GuessFileListStyle(dataRecords);
                foreach (string s in dataRecords)
                {
                    if (_directoryListStyle != FileListStyle.Unknown && s != "")
                    {
                        FileStruct f = new FileStruct();
                        f.Name = "..";
                        switch (_directoryListStyle)
                        {
                            case FileListStyle.UnixStyle:
                                f = ParseFileStructFromUnixStyleRecord(s);
                                break;
                            case FileListStyle.WindowsStyle:
                                f = ParseFileStructFromWindowsStyleRecord(s);
                                break;
                        }
                        if (f.Name != "" && f.Name != "." && f.Name != "..")
                        {
                            myListArray.Add(f);
                        }
                    }
                }
                return myListArray;
            }

           
            private FileStruct ParseFileStructFromWindowsStyleRecord(string Record)
            {
                //Предположим стиль записи 02-03-04  07:46PM       <DIR>     Append
                FileStruct f = new FileStruct();
                string processstr = Record.Trim();
                //Получаем дату
                string dateStr = processstr.Substring(0, 8);
                processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
                //Получаем время
                string timeStr = processstr.Substring(0, 7);
                processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
                f.CreateTime = dateStr + " " + timeStr;
                //Это папка или нет
                if (processstr.Substring(0, 5) == "<DIR>")
                {
                    f.IsDirectory = true;
                    processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
                }
                else
                {
                    string[] strs = processstr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    processstr = strs[1];
                    f.IsDirectory = false;
                }
                //Остальное содержмое строки представляет имя каталога/файла
                f.Name = processstr;
                return f;
            }
            //Получаем на какой ОС работает фтп-сервер - от этого будет зависеть дальнейший парсинг
            public FileListStyle GuessFileListStyle(string[] recordList)
            {
                foreach (string s in recordList)
                {
                    //Если соблюдено условие, то используется стиль Unix
                    if (s.Length > 10
                        && Regex.IsMatch(s.Substring(0, 10), "(-|d)((-|r)(-|w)(-|x)){3}"))
                    {
                        return FileListStyle.UnixStyle;
                    }
                    //Иначе стиль Windows
                    else if (s.Length > 8
                        && Regex.IsMatch(s.Substring(0, 8), "[0-9]{2}-[0-9]{2}-[0-9]{2}"))
                    {
                        return FileListStyle.WindowsStyle;
                    }
                }
                return FileListStyle.Unknown;
            }
            //Если сервер работает на nix-ах
            private FileStruct ParseFileStructFromUnixStyleRecord(string record)
            {
                //Предположим. тчо запись имеет формат dr-xr-xr-x   1 owner    group    0 Nov 25  2002 bussys
                FileStruct f = new FileStruct();
                if (record[0] == '-' || record[0] == 'd')
                {// правильная запись файла
                    string processstr = record.Trim();
                    f.Flags = processstr.Substring(0, 9);
                    f.IsDirectory = (f.Flags[0] == 'd');
                    processstr = (processstr.Substring(14)).Trim();
                    //отсекаем часть строки
                    _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
                    f.Owner = _cutSubstringFromStringWithTrim(ref processstr, ' ', 0);
                    f.CreateTime = getCreateTimeString(processstr);
                    //Индекс начала имени файла
                    int fileNameIndex = processstr.IndexOf(f.CreateTime) + f.CreateTime.Length;
                    //Само имя файла
                    f.Name = processstr.Substring(18).Trim();
                }
                else
                {
                    f.Name = "";
                }
                return f;
            }
            private string _cutSubstringFromStringWithTrim(ref string s, char c, int startIndex)
            {
                int pos1 = s.IndexOf(c, startIndex);
                string retString = s.Substring(0, pos1);
                s = (s.Substring(pos1)).Trim();
                return retString;
            }
            private string getCreateTimeString(string record)
            {
                //Получаем время
                string month = "(jan|feb|mar|apr|may|jun|jul|aug|sep|oct|nov|dec)";
                string space = @"(\040)+";
                string day = "([0-9]|[1-3][0-9])";
                string year = "[1-2][0-9]{3}";
                string time = "[0-9]{1,2}:[0-9]{2}";
                Regex dateTimeRegex = new Regex(month + space + day + space + "(" + year + "|" + time + ")", RegexOptions.IgnoreCase);
                Match match = dateTimeRegex.Match(record);
                return match.Value;
            }
            //метод протокола FTP DELE для удаления файла с FTP-сервера 


            //метод протокола FTP MKD для создания каталога на FTP-сервере 
        }
    }
}
    //Для парсинга полученного детального списка каталогов фтп-сервера
    //Структура для хранения детальной информации о файде или каталоге
   
    

