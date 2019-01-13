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
    public class IPBroadcastReceiver
    {
        private Socket m_GetServerIPSocket;
        private Thread m_GetServerIPThread;        
        private bool m_GetServerRunning = true;
        private int m_Port;
        public void StartGetServerIP(Action<string> OnServerIPGeted,int port)
        {
            m_Port = port;
            if (OnServerIPGeted == null)
            {
                return;
            }

            Stop();

            m_GetServerRunning = true;

            m_GetServerIPThread = new Thread(() =>
            {
                while (m_GetServerRunning)
                {
                    CloseSocket();

                    Thread.Sleep(1000);

                    try
                    {
                        GetServerIP(OnServerIPGeted);
                    }
                    catch (Exception e)
                    {
                        Log.Error("[IPBroadcastGeter] StartGetServerIP " + e);
                    }
                    
                }
            });
            m_GetServerIPThread.Start();
        }

        private void GetServerIP(Action<string> OnServerIPGeted)
        {
            var iep = new IPEndPoint(IPAddress.Any, m_Port);
            m_GetServerIPSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_GetServerIPSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            m_GetServerIPSocket.Bind(iep);

            var buffer = new byte[32];
            var ep = (EndPoint)iep;

            Log.Debug("[IPBroadcastGeter] Searching ServerIP... ");
            while (m_GetServerRunning)
            {
                try 
                {
                    m_GetServerIPSocket.ReceiveFrom(buffer, ref ep);
                }
                catch
                {
                    break;
                }               

                string ip = Encoding.ASCII.GetString(buffer);
                ip = ip.Trim();

                string ServerIP = ip;

                if (ip.StartsWith("ip:"))
                {
                    ip = ip.Replace("ip:", "");

                    if (string.IsNullOrEmpty(ip))
                    {
                        continue;
                    }

                    int index = ip.IndexOf(":end");
                    if (index != -1)
                    {
                        ServerIP = ip.Remove(index, ip.Length - index);
                        Log.Debug("[IPBroadcastGeter] ServerIP : " + ServerIP);
                        m_GetServerRunning = false;

                        try 
                        {
                            OnServerIPGeted(ServerIP);
                        }
                        catch(Exception e)
                        {
                            Log.Error("[IPBroadcastGeter] OnServerIPGeted Error : " + e);
                        }
                        
                        break;
                    }
                    else
                    {
                        Log.Error("[GetServerIP] IP Format Error:" + ServerIP);
                    }
                }
                else
                {
                    Log.Error("[GetServerIP] IP Format Error:" + ServerIP);
                }
            }
        }

        public void Stop()
        {
            try
            {
                m_GetServerRunning = false;
                
                if (m_GetServerIPThread != null && m_GetServerIPThread.IsAlive)
                {
                    //m_GetServerIPThread.Abort();
                }
            }
            catch (Exception e)
            {
                Log.Error("[IPBroadcastGeter] Stop Error :" + e);
            }
            finally
            {
                m_GetServerIPThread = null;
            }

            CloseSocket();
        }

        private void CloseSocket()
        {
            try
            {
                //不能加入Connected条件因为即使在接收中UDP也是未连接状态
                if (m_GetServerIPSocket != null /* && m_GetServerIPSocket.Connected*/)
                {
                    m_GetServerIPSocket.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error("[IPBroadcastGeter] Close Error :" + e.Message);
            }
            finally
            {
                m_GetServerIPSocket = null;
            }
        }
    }
}
