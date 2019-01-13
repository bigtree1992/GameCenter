using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QConnection
{
    public class QNetInfoClient
    {
        private Socket m_ClientSocket = null;
        private Thread m_StartThread;
        private string m_ServerIP;
        private int m_Port;

        private bool m_Running = false;


        public Action<string> NetInfo;
        public Action<string> SystemInfo;
        public Action<List<string>> ProcessInfo;
        public Action<List<string>> AppsInfo;

        private int m_SendProcessTimer;


        public QNetInfoClient()
        {

        }

        public void Start(string ip, int port)
        {
            Stop();
            m_ServerIP = ip;
            m_Port = port;

            m_Running = true;

            m_StartThread = new Thread(OnStartReceive);
            m_StartThread.Start();
        }

        private void OnStartReceive()
        {
            var count = 10;
            while (count-- > 0)
            {
                Thread.Sleep(10);
                if (!m_Running)
                {
                    return;
                }

                StartReceive();
            }

            Log.Error("[QScreenClient] OnStartReceive Can't Connect To ScreenServer.");
        }

        private void StartReceive()
        {
            m_Running = true;
            m_SendProcessTimer = 100;

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
                return;
            }

            while (m_Running)
            {
                try
                {
                    //接受NetInfo信息
                    OnReceiveData(NetInfo);
                    //接受SystemInfo信息
                    OnReceiveData(SystemInfo);

                    m_SendProcessTimer++;
                    if(m_SendProcessTimer > 30)
                    {
                        m_SendProcessTimer = 0;
                        OnReceiveData(ProcessInfo);
                    }

                    OnReceiveData(AppsInfo);
                }
                catch(Exception)
                {

                }

            }
        }

        private void OnReceiveData(Action<string> callback)
        {
            var dataSizeBuffer = new byte[4];
            var receivedSize = 0;
            var dataSize = 0;

            receivedSize = 0;
            dataSize = 4;
            while (m_Running && dataSize - receivedSize > 0)
            {
                try
                {
                    receivedSize += m_ClientSocket.Receive(
                  dataSizeBuffer, receivedSize, dataSize - receivedSize, SocketFlags.None);
                }
                catch (Exception)
                {
                    break;
                }
            }
            dataSize = BitConverter.ToInt32(dataSizeBuffer, 0);
            receivedSize = 0;

            var bodyBuffer = new byte[dataSize];
            while (m_Running && dataSize - receivedSize != 0)
            {
                receivedSize += m_ClientSocket.Receive(
                        bodyBuffer, receivedSize, dataSize - receivedSize, SocketFlags.None);
            }
            if (callback != null)
            {
                callback(Encoding.UTF8.GetString(bodyBuffer));
            }

        }


        private void OnReceiveData(Action<List<string>> callback)
        {
            var dataSizeBuffer = new byte[4];
            var receivedSize = 0;
            var dataSize = 0;

            receivedSize = 0;
            dataSize = 4;
            while (m_Running && dataSize - receivedSize > 0)
            {
                try
                {
                    receivedSize += m_ClientSocket.Receive(
                  dataSizeBuffer, receivedSize, dataSize - receivedSize, SocketFlags.None);
                }
                catch (Exception)
                {
                    break;
                }
            }

            dataSize = BitConverter.ToInt32(dataSizeBuffer, 0); //此时的dataSize即是list的长度

            var dataList = new List<string>();
            for(var i = 0; i < dataSize; i++)
            {
                var are = new AutoResetEvent(false);
                OnReceiveData((data)=> {
                    are.Set();
                    dataList.Add(data);
                });
                are.WaitOne(10);
            }

            var reslutList = new List<string>();
            for(var i = 0; i < dataList.Count;i++)
            {
                reslutList.Add(dataList[i]);
            }


            if(callback != null)
            {
                callback(reslutList);
            }

        }


        public void Stop()
        {
            try
            {
                if (m_StartThread != null)
                {
                    m_StartThread.Abort();
                }
            }
            catch (Exception e)
            {
                Log.Error("[QNetInfoClient] Stop Error : " + e.ToString());
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
                Log.Error("[QNetInfoClient] Stop Error " + e.Message);
            }
            finally
            {
                m_ClientSocket = null;
            }
            m_SendProcessTimer = 0;
        }


    }
}
