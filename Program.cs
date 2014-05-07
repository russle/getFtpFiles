//-----------------------------------------------------------------
// <copyright file="Program.cs" company="SYSCOM">
//      copyright (c) 2013 Russle all rights reserved.
// </copyright>
//-----------------------------------------------------------------

namespace GetFtpFiles
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Net;

    /// <summary>
    /// get Ftp08 Files
    /// </summary>
    public class Program
    {       
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args">不需要</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("開始執行");

            string localDir = System.Configuration.ConfigurationManager.AppSettings["FtpFileSaveDir"];
            string isDownloadAndMove = System.Configuration.ConfigurationManager.AppSettings["isDownloadAndMove"];
            string fileName = System.Configuration.ConfigurationManager.AppSettings["FileName"];
            string fileExtension = System.Configuration.ConfigurationManager.AppSettings["FileExtension"];
            int filecnt = 0;

            // 得到目錄下檔案清單
            List<string> ftplist = GetFtpList();

            foreach (string filename in ftplist)
            {                
                // 判斷只下載檔名前置為fileName的檔案
                if (filename.IndexOf(fileName) < 0)
                {
                    continue;
                }

                // 判斷只下載附檔名為fileExtension的檔案
                if (filename.IndexOf("." + fileExtension) < 0)
                {
                    continue;
                }

                Console.WriteLine("下載檔案名稱：" + filename);
                ////Console.ReadKey();

                if (isDownloadAndMove == "Y")
                {
                    FtpDownloadAndMove(localDir + filename, filename);
                }
                else
                {
                    FtpDownload(localDir + filename, filename);
                }

                filecnt++;
            }

            if (filecnt == 0)
            {
                Console.WriteLine("無檔案");
            }
        }

        /// <summary>
        /// Get Ftp Files List
        /// </summary>
        /// <returns>FtpList</returns>
        public static List<string> GetFtpList()
        {
            List<string> strList = new List<string>();

            string urlAddress = ConfigurationManager.AppSettings["FtpLocation"].ToString();
            string username = ConfigurationManager.AppSettings["FtpUser"].ToString();
            string password = ConfigurationManager.AppSettings["FtpPwd"].ToString();

            FtpWebRequest f = (FtpWebRequest)WebRequest.Create(new Uri(urlAddress));
            f.Method = WebRequestMethods.Ftp.ListDirectory;
            f.UseBinary = true;
            f.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
            f.Credentials = new NetworkCredential(username, password);

            StreamReader sr = new StreamReader(f.GetResponse().GetResponseStream());

            string str = sr.ReadLine();
            while (str != null)
            {
                strList.Add(str);
                str = sr.ReadLine();
            }

            sr.Close();
            sr.Dispose();
            f = null;

            return strList;
        }

        /// <summary>
        /// 下載 檔案
        /// </summary>
        /// <param name="localfile">本地檔案 路徑名稱</param>
        /// <param name="remotefile">遠端檔案 名稱</param>
        public static void FtpDownload(string localfile, string remotefile)
        {
            try
            {
                string location = System.Configuration.ConfigurationManager.AppSettings["FtpLocation"];
                string user = System.Configuration.ConfigurationManager.AppSettings["FtpUser"];
                string pwd = System.Configuration.ConfigurationManager.AppSettings["FtpPwd"];

                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential(user, pwd);

                byte[] fileData = wc.DownloadData(location + @"/" + remotefile);
                FileStream file = File.Create(@localfile);
                file.Write(fileData, 0, fileData.Length);
                file.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 下載檔案 並 搬移原始檔案至backup資料夾
        /// </summary>
        /// <param name="localfile">本地檔案 路徑名稱</param>
        /// <param name="remotefile">遠端檔案 名稱</param>
        public static void FtpDownloadAndMove(string localfile, string remotefile)
        {
            try
            {
                string location = System.Configuration.ConfigurationManager.AppSettings["FtpLocation"];
                string baklocation = System.Configuration.ConfigurationManager.AppSettings["FtpBakLocation"];
                string relativebaklocation = System.Configuration.ConfigurationManager.AppSettings["FtpRelativeBakLocation"];
                string user = System.Configuration.ConfigurationManager.AppSettings["FtpUser"];
                string pwd = System.Configuration.ConfigurationManager.AppSettings["FtpPwd"];

                ////Console.Write(location + @"/" + remotefile);
                ////Console.Write(localfile);
                ////Console.ReadKey();

                // 下載
                WebClient wc = new WebClient();
                wc.Credentials = new NetworkCredential(user, pwd);
                byte[] fileData = wc.DownloadData(location + @"/" + remotefile);
                FileStream file = File.Create(@localfile);
                file.Write(fileData, 0, fileData.Length);
                file.Close();

                // 刪舊備份檔
                Uri fileToDelete = new Uri(baklocation + remotefile);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileToDelete);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(user, pwd);
                FtpWebResponse response;
                try
                {
                    response = (FtpWebResponse)request.GetResponse();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);                    
                }

                ////Console.Write("開始執行 移至備份資料夾");
                ////Console.Write("Uri: " + location + @"/" + remotefile);
                ////Console.Write("RenameTo: " + relativebaklocation + remotefile);
                ////Console.ReadKey();

                // 移至備份資料夾
                ////Uri fileToMove = new Uri(location + remotefile);
                Uri fileToMove = new Uri(location + @"/" +remotefile);
                request = (FtpWebRequest)WebRequest.Create(fileToMove);
                request.Method = WebRequestMethods.Ftp.Rename;
                request.RenameTo = relativebaklocation + remotefile;                
                request.Credentials = new NetworkCredential(user, pwd);
                response = (FtpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
