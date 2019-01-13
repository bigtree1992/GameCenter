using System;
using System.IO;
using System.Net;
using System.Threading;


namespace QConnection
{
    public class QFileClient
    {
        private const string ServerUserName = "FantasyForest";
        private const string ServerPassWord = "";

        public double Progress
        {
            get;
            set;
        }

        public Action<float> OnProgress;

        public Action<string> OnError;

        public Action OnFinshed;

        private string m_SourcePath;
        private string m_TargetPath;
        private string m_ServerIP;

        private Thread m_DownloadThread;
        private bool m_Running;

        private Stream m_FTPStream = null;
        private FtpWebRequest m_FTPRequest;
        private FileStream m_FileStream = null;
        private FtpWebResponse m_FTPResponse = null;
        private System.Timers.Timer m_ProgressTimer = null;

        public QFileClient()
        {
        }

        /// <summary>
        /// 开始下载，Path为相对FTP服务器跟目录的路径
        /// </summary>
        public void Start(string ip, string sourcePath , string targetPath)
        {
            m_ServerIP = ip;
            m_SourcePath = sourcePath;
            m_TargetPath = targetPath;

            if (!string.IsNullOrEmpty(m_ServerIP) &&
                !string.IsNullOrEmpty(m_SourcePath) &&
                !string.IsNullOrEmpty(m_TargetPath))
            {
                m_DownloadThread = new Thread(OnDownloading);
                m_DownloadThread.Start();
            }
            else
            {
                Log.Debug("[QFileClient] ip or file path empty.");
            }
        }

        /// <summary>
        /// 停止下载
        /// </summary>
        public void Stop()
        {
            m_Running = false;

            ClearResource();

            try
            {
                if (m_DownloadThread != null)
                {
                    m_DownloadThread.Abort();                   
                }
            }
            catch (Exception e)
            {
                Log.Error("[QFileClient] Stop Error " + e.Message);
            }
            finally
            {
                m_DownloadThread = null;
            }
        }

        private void OnDownloading()
        {
            m_Running = true;

            try
            {
                var remoteFullPath = $"ftp://{m_ServerIP}/{m_SourcePath}";

                m_FTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(remoteFullPath));
                m_FTPRequest.UseBinary = true;
                m_FTPRequest.KeepAlive = true;
                m_FTPRequest.UsePassive = true;

                string directory = Path.GetDirectoryName(m_TargetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string targetPath = m_TargetPath;

                if (string.IsNullOrEmpty(Path.GetFileName(m_TargetPath)))
                {
                    targetPath = $"{m_TargetPath}/{Path.GetFileName(m_SourcePath)}";
                }

                m_FileStream = new FileStream(targetPath, FileMode.Create);
                m_FTPRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                m_FTPRequest.Credentials = new NetworkCredential(ServerUserName, ServerPassWord);
                m_FTPRequest.EnableSsl = false;

                m_FTPResponse = (FtpWebResponse)m_FTPRequest.GetResponse();
                m_FTPStream = m_FTPResponse.GetResponseStream();

                int bufferSize = 2048;
                byte[] buffer = new byte[bufferSize];
                long fileSize = GetFileSize(remoteFullPath);
                long totalDownloadedSize = 0;

                if (null != OnProgress)//定时反馈下载进度
                {
                    m_ProgressTimer = new System.Timers.Timer();
                    m_ProgressTimer.Enabled = true;
                    m_ProgressTimer.Interval = 500;
                    m_ProgressTimer.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
                    {
                        OnProgress(1.0f * totalDownloadedSize / fileSize);
                    });
                }

                int readCount = 0;
                do
                {
                    readCount = m_FTPStream.Read(buffer, 0, bufferSize);

                    totalDownloadedSize += readCount;

                    m_FileStream.Write(buffer, 0, readCount);

                } while (m_Running && readCount > 0);

                OnFinshed?.Invoke();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
            finally
            {
                ClearResource();
            }

            m_Running = false;
        }

        private void ClearResource()
        {
            try { m_ProgressTimer?.Close(); } finally { m_ProgressTimer = null; }
            try { m_FTPResponse?.Close(); } finally { m_FTPResponse = null; }
            try { m_FileStream?.Close(); } finally { m_FileStream = null; }
            try { m_FTPStream?.Close(); } finally { m_FTPStream = null; }                             
        }

        private long GetFileSize(string fileFullPath, long contentOffset = 0)
        {
            long fileSize = 0;
            Stream ftpStream = null;
            FtpWebRequest ftpRequest = null;
            FtpWebResponse ftpResponse = null;
            try
            {
                ftpRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri( $"ftp://{m_ServerIP}/{m_SourcePath}"));
                ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                ftpRequest.UseBinary = true;
                ftpRequest.KeepAlive = false;

                if (contentOffset > 0)
                {
                    ftpRequest.ContentOffset = contentOffset;
                }

                ftpRequest.Credentials = new NetworkCredential(ServerUserName, ServerPassWord);
                ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                ftpStream = ftpResponse.GetResponseStream();
                fileSize = ftpResponse.ContentLength;
            }
            catch (Exception e)
            {
                OnError?.Invoke(e.Message);
            }
            finally
            {
                if (null != ftpStream) {
                    ftpStream.Close();
                    ftpStream = null;
                }
                if (null != ftpResponse) {
                    ftpResponse.Close();
                    ftpResponse = null;
                }
            }
            return fileSize;
        }
    }
}
