using QProtocols;
using System;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace QConnection
{
    public class QClientBase : QConnBase
    {
        private Timer m_Timer;
        private Socket m_ClientSocket;
        private QClientConfig m_Config;
        private ReceiveEventArgs m_ReceiveEvent;
        private SocketAsyncEventArgs m_ConnectEvent = new SocketAsyncEventArgs();
        private bool m_Connected;

        public QClientBase(QClientConfig config)
        {
            m_Config = config;
            m_ConnectEvent = new SocketAsyncEventArgs();
            InitPool(1, m_Config.ReceiveBufferSize, m_Config.DecodeBufferSize);
        }

        public virtual void Start(IPEndPoint remoteEndPoint)
        {
            OnRegisterProtocols();
            Stop();

            m_Timer = new Timer();
            m_Timer.AutoReset = true;
            m_Timer.Elapsed += OnChecking;
            m_Timer.Interval = m_Config.Timeout * 1000;
            
            m_ClientSocket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            m_ConnectEvent.RemoteEndPoint = remoteEndPoint;
            m_ConnectEvent.Completed -= OnConnectCompleted;
            m_ConnectEvent.Completed += OnConnectCompleted;
            Log.Debug("[QClientBase] Start Connecting...");
            StartConnect(m_ConnectEvent);
            m_Timer.Start();
        }

        public virtual void Stop()
        {
            m_Connected = false;
            try
            {
                if(m_Timer != null)
                {
                    m_Timer.Stop();
                    m_Timer = null;
                }
               
            }
            catch(Exception e)
            {
                Log.Error("[QClientBase] StopTimer Error : " + e.Message);
            }

            try
            {
                if(m_ClientSocket != null)
                {
                    m_ClientSocket.Close();
                    m_ClientSocket = null;
                }
            }
            catch (Exception e)
            {
                Log.Error("[QClientBase] StopSocket Error : " + e.Message);
            }
        }

        private void StartConnect(SocketAsyncEventArgs evt)
        {            
            if (m_ClientSocket == null)
            {
                return;
            }

            bool willRaiseEvent = m_ClientSocket.ConnectAsync(evt);

            if (!willRaiseEvent)
            {
                ProcessConnect(evt);
            }
        }

        private void OnConnectCompleted(object sender, SocketAsyncEventArgs evt)
        {
            ProcessConnect(evt);
        }

        private void ProcessConnect(SocketAsyncEventArgs evt)
        {
            if (evt.LastOperation != SocketAsyncOperation.Connect)
            {
                Log.Error("[QClientBase] ProcessConnect Failed: Not Connect.");
                OnClientConnect(null);
                return;
            }

            if (evt.ConnectSocket == null)
            {
                Log.Error("[QClientBase] Connect Failed : " + evt.SocketError);
                OnClientTimeout(null);
                return;
            }

            m_Connected = true;

            //当服务器收到一个客户端连接以后，立即把这个Socket保存到这个Event里
            m_ReceiveEvent = m_ReceiveEventPool.Pop();
            m_ReceiveEvent.Socket = evt.ConnectSocket;
            m_ReceiveEvent.Address = (evt.ConnectSocket.RemoteEndPoint as IPEndPoint).Address;
            m_ReceiveEvent.Reset();
            var state = new P_ClientState();
            state.Time = Utils.DateTimeToUnixTimestamp( DateTime.Now );
            m_ReceiveEvent.SendProtocol(state);
            
            //向外部传播用户连接的信息
            try
            {
                OnClientConnect(m_ReceiveEvent);
            }
            catch (Exception e)
            {
                Log.Error("[QClientBase] OnUserConnect Error:" + e.Message);
            }
            //当客户端连接后，开始监听这个客户端收到的数据
            bool willRaiseEvent = m_ReceiveEvent.Socket.ReceiveAsync(m_ReceiveEvent);
            if (!willRaiseEvent)
            {
                ProcessReceive(m_ReceiveEvent);
            }
        }

        public void SendProtocol(Protocol protocol)
        {
            OnSendProtocol(m_ClientSocket, protocol);
        }

        internal override void OnClientClosed(ClientEvent clientEvent)
        {
            m_Connected = false;
        }

        private void OnChecking(object sender, ElapsedEventArgs evt)
        {
            if (m_Connected)
            {
                var span = DateTime.Now - m_ReceiveEvent.ActiveTime;
                if (span.TotalSeconds > m_Config.Timeout * 2)
                {
                    OnClientTimeout(m_ReceiveEvent);
                    m_ReceiveEvent.KickOut();
                }
                else
                {                   
                    m_ReceiveEvent.SendProtocol(new P_ClientState());
                }
            }
            else
            {
                try
                {
                    m_ClientSocket.Close();
                }
                catch(Exception e)
                {
                    Log.Error("[QClientBase] CloseSocket Error : " + e.Message);
                }                
            }
        }
    }
}
