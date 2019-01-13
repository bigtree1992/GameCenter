using System;
using System.Collections.Generic;
using QProtocols;

namespace QConnection
{
    public class ProtocolCenter
    {
        private Dictionary<Type, Action<ReceiveEventArgs, Protocol>> m_Protocols = new Dictionary<Type, Action<ReceiveEventArgs, Protocol>>();

        public void SetProtocol(Type evt, Action<ReceiveEventArgs, Protocol> callback)
        {
            if (evt == null || callback == null)
            {
                return;
            }

            m_Protocols[evt] = callback;

        }

        public void ClearProtocol(Type evt)
        {
            if (evt == null)
            {
                return;
            }

            if (m_Protocols.ContainsKey(evt))
            {
                m_Protocols.Remove(evt);
            }
        }

        internal void ExcuteProtocol(Type evt, ReceiveEventArgs token, Protocol protocol)
        {
            if (!m_Protocols.ContainsKey(evt))
            {
                return;
            }

            var callback = m_Protocols[evt];
            if (callback == null)
            {
                return;
            }

            try
            {
                //动态调用事件
                callback(token,protocol);
            }
            catch (Exception e)
            {
                Log.Error(String.Format("[ProtocolCenter] Excute Protocol {0} Error : {1}", evt, e.Message));
            }
        }
    }
}
