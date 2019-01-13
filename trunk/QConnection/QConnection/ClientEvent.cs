using System;
using System.Collections.Generic;
using System.Net.Sockets;
using QProtocols;
using System.Net;

namespace QConnection
{
    public class ClientEvent : SocketAsyncEventArgs
    {
        internal Socket Socket { get; set; }
        
        public DateTime ActiveTime { get; set; } 

        private Action<Socket, Protocol> OnSendProtocol = null;

        public ClientEvent(Action<Socket, Protocol> onSendProtocol)
        {
            OnSendProtocol = onSendProtocol;
        }

        public IPAddress Address
        {
            get;set;
        }        

        public void SendProtocol(Protocol protocol)
        {
            OnSendProtocol(Socket, protocol);
        }

        public void SendData(byte[] data)
        {
            Socket.Send(data);
        }

        internal void KickOut()
        {
            try
            {
                if(Socket != null)
                {
                    Socket.Close();
                }
            }
            catch(Exception e)
            {
                Log.Error("[ClientEvent] KickOut Error:" + e.Message);
            }
        }

        internal virtual void Reset()
        {
            ActiveTime = DateTime.Now;
        }
    }
}
