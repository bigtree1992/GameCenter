using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace QConnection
{
    public class ClientManager
    {
        private Action<ClientEvent> OnTimeOut;
        private List<ClientEvent> m_ClientEvents;
        private Timer m_Timer;
        private int m_Second;

        public ClientManager(Action<ClientEvent> onTimeOut)
        {
            OnTimeOut = onTimeOut;
            m_ClientEvents = new List<ClientEvent>();
            m_Timer = new Timer();
        }

        public void Start(int second)
        {
            lock (this)
            {                
                m_Timer.AutoReset = true;
                m_Timer.Interval = second * 1000;
                m_Timer.Elapsed += this.OnChecking;
                m_Timer.Start();
                m_Second = second * 2;
            }            
        }

        public void Stop()
        {
            lock (this)
            {
                try
                {
                    m_ClientEvents.Clear();
                    m_Timer.Stop();
                }
                catch
                {

                }

            }
        }

        public void Add(ClientEvent client)
        {
            lock (this)
            {
                if(m_ClientEvents.Contains(client))
                {
                    //致命错误，不应该有重复的用户被添加进来
                    Log.Error("[ClientManager] AddClient Error : Dup Client.");
                }
                else
                {
                    m_ClientEvents.Add(client);
                }

            }
        }

        public void Remove(ClientEvent userToken)
        {
            lock (this)
            {
                m_ClientEvents.Remove(userToken);
            }            
        }        

        public int Count
        {
            get { return m_ClientEvents.Count; }
        }

        private void OnChecking(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                for(int i = 0; i < m_ClientEvents.Count; i++)
                {
                    var client = m_ClientEvents[i];
                    var span = DateTime.Now - client.ActiveTime;
                    if (span.TotalSeconds > m_Second)
                    {
                        //检测到有客户掉线了
                        OnTimeOut(client);
                        client.KickOut();
                       
                    }
                }
            }
        }
    }
}
