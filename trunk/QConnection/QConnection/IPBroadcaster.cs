using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System;

namespace QConnection
{
    public class IPBroadcaster
    {
        private Thread m_BroadCastThread;
        private Socket m_BroadCastSocket;
        private string m_ServerIP;
        private bool m_Running;
        //private int m_Count;
        private int m_Port;

        public void Start(string ip,int port)  //开始广播ip
        {
            m_Port = port;
            m_ServerIP = ip;
            Stop();
            try
            {
                //udp协议 无限连接 利用现有线路进行传递
                m_BroadCastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                m_BroadCastThread = new Thread(OnBroadCast);
                m_BroadCastThread.Start();
            }
            catch(Exception e)
            {
                Log.Error("[IPBroadcaster] Can't BroadCast. " + e);
            }
        }

        private void OnBroadCast()  //广播ip的线程方法
        {
            var iep = new IPEndPoint(IPAddress.Broadcast, m_Port);
            m_BroadCastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            byte[] buffer = Encoding.ASCII.GetBytes(string.Format("ip:{0}:end", m_ServerIP));
            m_Running = true;
            Log.Debug($"[IPBroadcast] Start BroadCasting {Encoding.ASCII.GetString(buffer)} ...");

            while (m_Running)
            {
                if (m_BroadCastSocket != null)
                {
                    m_BroadCastSocket.SendTo(buffer, iep);
                }
                else
                {
                    break;
                }

             
                Thread.Sleep(10);
            }
        }

        public void Stop()  //停止广播ip
        {
            try
            {
                if (m_BroadCastThread != null && m_BroadCastThread.IsAlive)
                {
                    m_Running = false;
                    m_BroadCastThread.Abort();
                    m_BroadCastThread = null;
                }

                if (m_BroadCastSocket != null && m_BroadCastSocket.Connected)
                {
                    m_BroadCastSocket.Close();
                    m_BroadCastSocket = null;
                }
            }
            catch(Exception e)
            {
                Log.Error("[IPBroadcaster] Close Error." + e);
            }
            finally
            {
                m_BroadCastThread = null;
                m_BroadCastSocket = null;
            }
        }
    }
}
