using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace QConnection
{
    public class QServerBase : QConnBase
    {
        protected QServerConfig m_Config;
        private Socket m_ServerSocket;
        private Semaphore m_AcceptedClients;
        private ClientManager m_ClientManager;
        
        public ClientManager ClientManager
        {
            get { return m_ClientManager; }
        }
          
        public QServerBase(QServerConfig config)
        {
            m_Config = config;
            m_ClientManager = new ClientManager(this.OnClientTimeout);
            m_AcceptedClients = new Semaphore(m_Config.MaxConnections, m_Config.MaxConnections);
            InitPool(m_Config.MaxConnections, m_Config.ReceiveBufferSize, m_Config.DecodeBufferSize);
        }

        public virtual void Start(IPEndPoint localEndPoint)
        {
            OnRegisterProtocols();
            try
            {
                m_ServerSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                m_ServerSocket.Bind(localEndPoint);
                m_ServerSocket.Listen(m_Config.MaxConnections);
            }
            catch(Exception e)
            {
                Log.Error("[QServerBase] Start Failed" + e.Message);
                return;
            }
            
            var acceptEvent = new SocketAsyncEventArgs();
            acceptEvent.Completed += OnAcceptCompleted;

            Log.Debug("[QServerBase] Start Accepting...");

            StartAccept(acceptEvent);

            m_ClientManager.Start(m_Config.ClientTimeout);
        }

        public virtual void Stop()
        {
            try
            {
                m_ClientManager.Stop();
                m_ServerSocket.Close();
                m_ServerSocket = null;
                m_AcceptedClients.Release();

            }
            catch
            {

            }
        }

        private void StartAccept(SocketAsyncEventArgs evt)
        {
            //清空上一次收到的客户端连接的Socket
            evt.AcceptSocket = null;
            m_AcceptedClients.WaitOne();
            if(m_ServerSocket == null)
            {
                return;
            }

            bool willRaiseEvent = m_ServerSocket.AcceptAsync(evt);
            if (!willRaiseEvent)
            {
                ProcessAccept(evt);
            }
        }
        
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs evt)
        {
            ProcessAccept(evt);
        }

        private void ProcessAccept(SocketAsyncEventArgs evt)
        {
            if(evt.LastOperation != SocketAsyncOperation.Accept)
            {
                Log.Error("[QServerBase] ProcessAccept Failed.");
                return;
            }
            var endpoint = ((IPEndPoint)evt.AcceptSocket.RemoteEndPoint);
            if(endpoint == null)
            {
                Log.Error("[QServerBase] ProcessAccept Failed, No endpoint.");
                return;
            }
            //当服务器收到一个客户端连接以后，立即把这个Socket保存到这个Event的UserToken里
            var receiveEventArgs = m_ReceiveEventPool.Pop();
            receiveEventArgs.Socket = evt.AcceptSocket;
            receiveEventArgs.Address = endpoint.Address;
            receiveEventArgs.Reset();
            
            m_ClientManager.Add(receiveEventArgs);

            //向外部传播用户连接的信息
            try
            {
                OnClientConnect(receiveEventArgs);
            }
            catch(Exception e)
            {
                Log.Error("[QServerBase] ProcessAccept Error:" + e.Message);
            }
            //当客户端连接后，开始监听这个客户端收到的数据
            bool willRaiseEvent = receiveEventArgs.Socket.ReceiveAsync(receiveEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(receiveEventArgs);
            }

            //开始下一次等待客户端连接
            StartAccept(evt);
        }

        internal override void OnClientClosing(ClientEvent clientEvent)
        {
            //最先从用户列表移除
            m_ClientManager.Remove(clientEvent);
        }

        internal override void OnClientClosed(ClientEvent clientEvent)
        {
            //最后允许新用户进行连接
            m_AcceptedClients.Release();
        }
    } 
}
