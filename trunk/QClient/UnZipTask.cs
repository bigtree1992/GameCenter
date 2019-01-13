using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using QConnection;
using System.Threading;
using QProtocols;

namespace QClientNS
{
    public class UnZipTask
    {
        class TaskParameter
        {
            public string ZipFilePath { get; set; }
            public string UnZipDir { get; set; }
        }

        public Action<Code,string, OpState, float> OnProgress;

        private ZipInputStream m_Stream = null;
        private FileStream m_StreamWriter = null;

        private Thread m_WorkThread = null;
        private System.Timers.Timer m_ProgressTimer = null;

        public void Start(string zipFilePath, string unZipDir)
        {           
            if (string.IsNullOrEmpty(zipFilePath) || 
                !File.Exists(zipFilePath))
            {
                OnProgress?.Invoke(Code.NotExist, "文件不存在",OpState.Done, -1);
                return;
            }

            if (Directory.Exists(unZipDir))
            {
                try
                {
                    Directory.Delete(unZipDir, true);
                }
                catch(Exception e)
                {
                    Log.Error($"[UnZipTask] Delete {unZipDir} Failed : {e}");
                    OnProgress?.Invoke(Code.Failed, "删除文件夹错误:" + e.Message,OpState.Done, -1);
                    return;
                }               
            }

            m_WorkThread = new Thread(new ParameterizedThreadStart(OnWorking));
            var param = new TaskParameter();
            param.UnZipDir = unZipDir;
            param.ZipFilePath = zipFilePath;
            m_WorkThread.Start(param);           
        }

        private void OnWorking(object param)
        {
            Log.Debug("[UnZipTask] Start.");

            var taskParameter = param as TaskParameter;

            int total = GetFileCount(taskParameter.ZipFilePath);

            m_Stream = null;
            try
            {
                int fileCount = 0;

                if (null != OnProgress)//定时反馈下载进度
                {
                    m_ProgressTimer = new System.Timers.Timer();
                    m_ProgressTimer.Enabled = true;
                    m_ProgressTimer.Interval = 500;
                    m_ProgressTimer.Elapsed += new System.Timers.ElapsedEventHandler((s, e) =>
                    {
                        try
                        {
                            OnProgress?.Invoke(Code.Success, "", OpState.Doing, 1.0f * fileCount / total);
                        }
                        catch (Exception ee)
                        {
                            Log.Error("[QClient] OnUnZipProgress Error 1 : " + ee.Message);
                        }
                    });
                }

                //解压文件夹为空时默认与压缩文件同一级目录下，跟压缩文件同名的文件夹  
                if (string.IsNullOrEmpty(taskParameter.UnZipDir))
                {
                    taskParameter.UnZipDir = taskParameter.ZipFilePath.Replace(
                        Path.GetFileName(taskParameter.ZipFilePath),
                        Path.GetFileNameWithoutExtension(taskParameter.ZipFilePath));
                }

                if (!taskParameter.UnZipDir.EndsWith("\\"))
                    taskParameter.UnZipDir += "\\";

                if (!Directory.Exists(taskParameter.UnZipDir))
                    Directory.CreateDirectory(taskParameter.UnZipDir);

                m_Stream = new ZipInputStream(File.OpenRead(taskParameter.ZipFilePath));
                ZipEntry entry;
                while ((entry = m_Stream.GetNextEntry()) != null)
                {
                    entry.IsUnicodeText = true;
                    string directoryName = Path.GetDirectoryName(entry.Name);
                    string fileName = Path.GetFileName(entry.Name);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(taskParameter.UnZipDir + directoryName);
                    }

                    if (!directoryName.EndsWith("\\"))
                    {
                        directoryName += "\\";
                    }

                    fileCount++;

                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    try
                    {
                        m_StreamWriter = File.Create(taskParameter.UnZipDir + entry.Name);

                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = m_Stream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                m_StreamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        fileCount--;
                        OnProgress(Code.Failed,"写入文件错误:" + e.Message, OpState.Done, -1);
                        Log.Error("[QClient] OnUnZipProgress Error 2 : " + e);
                    }
                    finally
                    {
                        if (m_StreamWriter != null)
                        {
                            m_StreamWriter.Close();
                            m_StreamWriter = null;
                            File.SetLastWriteTime(taskParameter.UnZipDir + entry.Name, entry.DateTime);
                        }
                    }
                }

                if (fileCount == total)
                {
                    OnProgress?.Invoke(Code.Success, "" , OpState.Done, 1.0f);
                }
            }
            catch (Exception e)
            {
                OnProgress(Code.Failed, "未知错误:" + e.Message, OpState.Done, -1);
                Log.Error("[QClient] OnUnZipProgress Error 3 : " + e);
            }
            finally
            {
                Clear();
            }
        }

        public void Stop()
        {
            Clear();

            try
            {
                if (m_WorkThread != null)
                {
                    m_WorkThread.Abort();
                }
            }
            finally
            {
                m_WorkThread = null;
            }
            Log.Debug("[UnZipTask] Stop.");
        }

        private void Clear()
        {
            try
            {
                if (m_ProgressTimer != null)
                {
                    m_ProgressTimer.Close();
                }
            }
            finally
            {
                m_ProgressTimer = null;
            }

            try
            {
                if (m_Stream != null)
                {
                    m_Stream.Close();
                }
            }
            finally
            {
                m_Stream = null;
            }

            try
            {
                if (m_StreamWriter != null)
                {
                    m_StreamWriter.Close();
                }
            }
            finally
            {
                m_StreamWriter = null;
            }
        }

        private int GetFileCount(string path)
        {
            int total = 0;
            ZipFile zipFiles = null;
            try
            {
                zipFiles = new ZipFile(path);
                total = (int)zipFiles.Count;
            }
            finally
            {
                if (zipFiles != null)
                {
                    zipFiles.Close();
                }
            }
            return total;
        }       
    }
}
