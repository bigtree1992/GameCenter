using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace QConnection
{
    //同屏客户端，放到游戏启动器里面
    public class QScreenClient
    {
        private Socket m_ClientSocket = null;
        private Thread m_StartThread;
        private string m_ServerIP;
        private int m_Port;

        public Action<Bitmap> OnScreenChange;

        private bool m_Running = false;
        //private int m_LogTimes = 5;

        public void Start(string ip,int port)
        {
            try
            {
                Stop();

                
                m_ServerIP = ip;
                m_Port = port;

                m_Running = true;
                m_StartThread = new Thread(OnStartReceive);
                m_StartThread.Start();

            }
            catch (Exception ex)
            {
                Log.Error("[QScreenClient] Start Error:" + ex.Message);
            }
        }

        public void Stop()
        {
            m_Running = false;
         
            try
            {
                if (m_StartThread != null )
                {
                    m_StartThread.Abort();
                }
            }
            catch(Exception e)
            {
                Log.Error("[QScreenClient] Stop Error : " + e.ToString());
            }
            finally
            {
                m_StartThread = null;
            }

            try
            {               
                if (m_ClientSocket != null && m_ClientSocket.Connected)
                {
                    
                    m_ClientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error("[QScreenClient] Stop Error " + e.Message);
            }
            finally
            {               
                m_ClientSocket = null;                
            }
        }

        private void OnStartReceive()
        {
            int count = 10;
            while (count-- > 0)
            {
                Thread.Sleep(10);
                if (!m_Running)
                {
                    return;
                }
                
                StartReceive(OnScreenChange);
            }

            Log.Error("[QScreenClient] OnStartReceive Can't Connect To ScreenServer.");
        }

        private void StartReceive(Action<Bitmap> onscreenchange)
        {            
            m_Running = true;
            onscreenchange = OnScreenChange;

            try
            {
                m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                m_ClientSocket.ReceiveBufferSize = 1024 * 1024 * 1;
                m_ClientSocket.SendBufferSize = 1024 * 1024 * 1;
                var endPoint = new IPEndPoint(IPAddress.Parse(m_ServerIP), m_Port);

                m_ClientSocket.Connect(endPoint);
            }
            catch (Exception)
            {
                //Log.Error("[QScreenClient] Connect Failed:" + m_ServerIP + "   Error : " + e.ToString());
                return;
            }

            var dataSizeBuffer = new byte[4];
            int receivedSize = 0;
            int dataSize = 0;
            var bodyBuffer = new byte[1024 * 1024 * 4];

            while (m_Running)
            {             
                try
                {
                    ////首先接受头部4字节表示缓冲区长度
                    receivedSize = 0;
                    dataSize = 4;

                    while (m_Running && dataSize - receivedSize > 0)
                    {
                        try
                        {
                            receivedSize += m_ClientSocket.Receive(
                          dataSizeBuffer, receivedSize, dataSize - receivedSize, SocketFlags.None);
                        }
                        catch(Exception)
                        {
                            break;
                        }
                      
                    }

                    dataSize = BitConverter.ToInt32(dataSizeBuffer, 0);
                    if(dataSize > 1024 * 1024 * 4)
                    {
                        Log.Debug("DataSize  > 4M.");
                        break;
                    }
                    
                    //接受Body Buff
                    receivedSize = 0;                    

                    if(m_Running == false)
                    {
                        break;
                    }

                    while (m_Running && dataSize - receivedSize != 0)
                    {
                        int left = dataSize - receivedSize;
                        var c = m_ClientSocket.Receive(
                                bodyBuffer, receivedSize, left, SocketFlags.None);
                        receivedSize += c;
                    }
                   
                    if (!m_Running)
                    {
                        break;
                    }

                    if (dataSize <= 0 || dataSize > bodyBuffer.Length)
                    {
                        break;
                    }

                    var memoryStream = new MemoryStream();
                    memoryStream.Write(bodyBuffer, 0, dataSize);
                    memoryStream.Position = 0;
                    
                    Bitmap bitmap = null;
                    try
                    {
                        bitmap = new Bitmap(memoryStream);
                        //用回调函数
                        if (OnScreenChange != null && bitmap != null)
                        {
                            OnScreenChange(bitmap);
                        }
                    }
                    catch (Exception)
                    {
                        //continue;
                        break;
                    }
                    finally
                    {
                        if (memoryStream != null)
                        {
                            memoryStream.Close();
                        }

                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                        }
                    }
                }
                catch
                {
                    //Log.Error("[QScreenClient] StartReceive error: " + ex);
                    break;
                }
            }

            m_StartThread = null;
            //Log.Debug("[QScreenClient] Stop : " + m_ServerIP);
        }

    }
}
