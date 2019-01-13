using QProtocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;

namespace QConnection
{
    public class QConnBase : ProtocolCenter
    {
        protected int m_ReceiveBufferSize;
        internal BufferManager m_BufferManager;
        internal SocketEventPool<ReceiveEventArgs> m_ReceiveEventPool;
        internal SocketEventPool<SendEventArgs> m_SendEventPool;

        protected virtual void OnClientConnect(ClientEvent clientEvent) { }
        protected virtual void OnClientDisconnect(ClientEvent clientEvent) { }
        protected virtual void OnClientTimeout(ClientEvent clientEvent) { }
        protected virtual void OnRegisterProtocols() { }
        internal virtual void OnClientClosing(ClientEvent clientEvent) { }
        internal virtual void OnClientClosed(ClientEvent clientEvent) { }

        public QConnBase()
        {
            try
            {
                ProtocolTable.LoadAssembly();
            }
            catch (Exception e)
            {
                Log.Error("[QConnBase] LoadAssembly Error: " + e.Message);
            }
        }

        protected void InitPool(int maxConnections,int receiveBufferSize,int decodeBufferSize)
        {
            m_ReceiveBufferSize = receiveBufferSize;

            m_BufferManager = new BufferManager(
                receiveBufferSize * maxConnections * 2, receiveBufferSize);

            m_ReceiveEventPool = new SocketEventPool<ReceiveEventArgs>(maxConnections);
            m_SendEventPool = new SocketEventPool<SendEventArgs>(maxConnections);

            for (int i = 0; i < maxConnections; i++)
            {
                var receiveEvent = new ReceiveEventArgs(decodeBufferSize, OnGetProtocol, OnSendProtocol);
                receiveEvent.Completed += OnReceiveCompleted;
                m_BufferManager.SetBuffer(receiveEvent);
                m_ReceiveEventPool.Push(receiveEvent);

                var sendEvent = new SendEventArgs();
                sendEvent.Completed += OnSendCompleted;
                m_BufferManager.SetBuffer(sendEvent);
                m_SendEventPool.Push(sendEvent);
            }
        }

        protected void OnReceiveCompleted(object sender, SocketAsyncEventArgs evt)
        {
            switch (evt.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(evt);
                    break;
                default:
                    CloseSocketWhenReceive(evt as ReceiveEventArgs,Error.UnKonw);
                    break;
            }
        }

        protected void OnSendCompleted(object sender, SocketAsyncEventArgs evt)
        {
            switch (evt.LastOperation)
            {
                case SocketAsyncOperation.Send:
                    ProcessSend(evt);
                    break;
                default:
                    CloseSocketWhenSend(evt as SendEventArgs, Error.UnKonw);
                    break;
            }
        }

        protected void ProcessReceive(SocketAsyncEventArgs evt)
        {
            var receiveEvent = evt as ReceiveEventArgs;
            if(receiveEvent == null)
            {
                return;
            } 

            switch (evt.SocketError)
            {
                case SocketError.Success:
                    if (evt.BytesTransferred > 0)
                    {
                        if (!receiveEvent.DecodeBuffer(evt.Buffer, evt.Offset, evt.BytesTransferred))
                        {
                            CloseSocketWhenReceive(receiveEvent, Error.Deserialize);
                        }
                        else
                        {
                            ContinueReceive(receiveEvent);
                        }
                    }
                    else
                    {
                        CloseSocketWhenReceive(receiveEvent, Error.NoData);
                    }
                    break;                    
                case SocketError.ConnectionReset:
                    CloseSocketWhenReceive(receiveEvent, Error.Reset);
                    break;
                case SocketError.ConnectionAborted:
                    CloseSocketWhenReceive(receiveEvent, Error.Abort);
                    break;
                case SocketError.OperationAborted:
                    CloseSocketWhenReceive(receiveEvent, Error.OpAbort);
                    break;
                default:
                    Log.Debug("[QConnBase] ProcessReceive Error:" + evt.SocketError);
                    CloseSocketWhenReceive(receiveEvent, Error.UnKonw);
                    break;
            }
        }

        /// <summary>
        /// 每一次成功收到消息之后都要调用一下这个函数开启下一次监听
        /// 这个机制保证了一个用户仅能同时接收到一个事件，保证可以复用一个监听事件的参数结构，
        /// </summary>
        /// <param name="token"></param>
        private void ContinueReceive(ReceiveEventArgs receiveEvent)
        {
            if (receiveEvent == null)
            {
                return;
            }

            bool willRaiseEvent = true;
            try
            {
                willRaiseEvent = receiveEvent.Socket.ReceiveAsync(receiveEvent);
            }
            catch(Exception)
            {
                CloseSocketWhenReceive(receiveEvent,Error.UnKonw);
            }

            if (!willRaiseEvent)
            {
                ProcessReceive(receiveEvent);
            }
        }

        protected void ProcessSend(SocketAsyncEventArgs evt)
        {
            var sendEvent = evt as SendEventArgs;
            if(sendEvent == null)
            {
                return;
            }

            switch (evt.SocketError)
            {
                case SocketError.Success:
                    //用完了这个事件要回收
                    sendEvent.Socket = null;
                    m_SendEventPool.Push(sendEvent);
                    break;                
                case SocketError.ConnectionReset:
                    CloseSocketWhenSend(sendEvent, Error.Reset);
                    break;
                case SocketError.ConnectionAborted:
                    CloseSocketWhenSend(sendEvent, Error.Abort);
                    break;
                case SocketError.OperationAborted:
                    CloseSocketWhenSend(sendEvent, Error.OpAbort);
                    break;
                default:
                    Log.Debug("[QConnBase] ProcessSend Error:" + evt.SocketError);
                    CloseSocketWhenSend(sendEvent, Error.UnKonw);
                    break;
            }
        }

        protected void CloseSocketWhenReceive(ReceiveEventArgs receiveEvent,Error resaon)
        {
            //ToDo:准备去掉
            Log.Debug("[QConnBase] CloseSocketWhenReceive:" + resaon);

            try
            {
                //向外部传播用户掉线
                OnClientDisconnect(receiveEvent);
            }
            catch (Exception ex)
            {
                Log.Error("[QConnBase] OnClientDisconnect Error: " + ex.Message);
            }

            try
            {
                //根据情况区分，不需要调用Close的地方需要屏蔽二次调用            
                if(receiveEvent.Socket != null)
                {
                    receiveEvent.Socket.Close();
                    receiveEvent.Socket = null;
                }
                //Log.Debug("[QConnBase] CloseSocketWhenReceive .");
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error("[QConnBase] CloseClientSocket Disposed: " + ex.Message);
            }
            // throws if client process has already closed
            catch (Exception ex)
            {
                Log.Error("[QConnBase] CloseClientSocket: " + ex.Message);
            }

            //先从用户列表中移除
            OnClientClosing(receiveEvent);
            //当一个用户意外结束的时候，回收这个重复使用的事件
            m_ReceiveEventPool.Push(receiveEvent);
            OnClientClosed(receiveEvent);

        }

        protected void CloseSocketWhenSend(SendEventArgs sendEvent, Error resaon)
        {
            //ToDo:准备去掉
            Log.Debug("[QConnBase] CloseSocketWhenSend Socket:" + resaon);

            try
            {
                //根据情况区分，不需要调用Close的地方需要屏蔽二次调用            
                if (sendEvent.Socket != null)
                {
                    sendEvent.Socket.Close();
                }
            }
            catch (ObjectDisposedException ex)
            {
                Log.Error("[QConnBase] CloseClientSocket Disposed: " + ex.Message);
            }
            // throws if client process has already closed
            catch (Exception ex)
            {
                Log.Error("[QConnBase] CloseClientSocket: " + ex.Message);
            }
            //当一个用户意外结束的时候，回收这个重复使用的事件
            m_SendEventPool.Push(sendEvent);
        }

        protected void OnGetProtocol(ReceiveEventArgs receiveEvent, Protocol protocol)
        {
            ExcuteProtocol(protocol.GetType(), receiveEvent, protocol);
        }

        protected void OnSendProtocol(Socket socket, Protocol protocol)
        {
            //发生的时间不确定也可能同时，需要重新分配事件
            //有可能主动发送跟回复协议一起在发送，这样同时占用同一个事件就会出错

            var sendEvent = m_SendEventPool.Pop();
            sendEvent.Socket = socket;

            try
            {
                var serializer = new DataContractJsonSerializer(protocol.GetType());
                var stream = new MemoryStream(sendEvent.Buffer, sendEvent.Offset, m_ReceiveBufferSize);
                stream.Position = 8;
                serializer.WriteObject(stream, protocol);

                //写入长度
                int len = (int)stream.Position;
                var lenBuffer = BitConverter.GetBytes((len - 8));

                stream.Position = 0;
                stream.Write(lenBuffer, 0, 4);

                //写入ID
                int id = ProtocolTable.GetID(protocol.GetType());
                if (id == 0)
                {
                    Log.Error("[OnSendProtocol] id == 0 -> " + protocol.GetType());
                    CloseSocketWhenSend(sendEvent, Error.Serialize);
                    return;
                }

                var idBuffer = BitConverter.GetBytes(id);
                stream.Position = 4;
                stream.Write(idBuffer, 0, 4);

                //重新设置buffer并发送
                sendEvent.SetBuffer(sendEvent.Buffer,sendEvent.Offset, len);


                bool willRaiseEvent = socket.SendAsync(sendEvent);
                if (!willRaiseEvent)
                {
                    ProcessSend(sendEvent);
                }

            }
            catch (Exception e)
            {
                Log.Error("[OnSendProtocol] " + e);
                CloseSocketWhenSend(sendEvent,Error.Serialize);
            }
        }
    }
}
