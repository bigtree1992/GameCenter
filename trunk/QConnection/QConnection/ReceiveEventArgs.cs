using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

using QProtocols;

namespace QConnection
{
    public class ReceiveEventArgs : ClientEvent
    {
        private Action<ReceiveEventArgs, Protocol> OnGetProtocol = null;

        private enum DecodeState
        {
            Length,
            ID,
            Content
        }

        private DecodeState State;
        private byte[] m_DecodeBuffer;
        private int m_Index;
        private int m_ProtocolLen;
        private int m_ProtocolID;

        public ReceiveEventArgs(int decodeBufferSize,
            Action<ReceiveEventArgs, Protocol> onGetProtocol,
            Action<Socket, Protocol> onSendProtocol) : base(onSendProtocol) 
        {
            m_DecodeBuffer = new byte[decodeBufferSize];
            OnGetProtocol = onGetProtocol;
        }

        internal override void Reset()
        {
            base.Reset();
            State = DecodeState.Length;
            m_Index = 0;
            m_ProtocolLen = 0;
            m_ProtocolID = 0;
        }

        //协议的组装与解析。
        internal bool DecodeBuffer(byte[] buffer, int start, int count)
        {
            if (count <= 0)
            {
                Log.Debug("[DecodeBuffer] : count <= 0");
                return false;
            }

            if (count > (m_DecodeBuffer.Length - m_Index))
            {
                Log.Debug($"[DecodeBuffer] -> BufferOverflow Client May Send Protocol Too Fast.");
                return false;
            }

            Array.Copy(buffer, start, m_DecodeBuffer, m_Index, count);
            m_Index += count;

            DecodeLength:
            if (State == DecodeState.Length)
            {
                if (m_Index >= 4)
                {
                    m_ProtocolLen = BitConverter.ToInt32(m_DecodeBuffer, 0);
                    
                    if (m_ProtocolLen > (m_DecodeBuffer.Length - 8))
                    {
                        Log.Debug("[DecodeBuffer] : " + m_ProtocolLen + "|" + m_DecodeBuffer.Length + "|" + m_Index);
                        return false;
                    }

                    if (m_ProtocolLen <= 0)
                    {
                        Log.Debug("[DecodeBuffer] : m_ProtocolLen <= 0");
                        return false;
                    }

                    State = DecodeState.ID;
                }
                else
                {
                    return true;
                }
            }

            if (State == DecodeState.ID)
            {
                if (m_Index >= 8)
                {
                    m_ProtocolID = BitConverter.ToInt32(m_DecodeBuffer, 4);

                    if (m_ProtocolID <= 0 || !ProtocolTable.Contains(m_ProtocolID))
                    {
                        Log.Debug("[DecodeBuffer] 3 -> " + m_ProtocolID);
                        return false;
                    }

                    State = DecodeState.Content;
                }
                else
                {
                    return true;
                }
            }

            int left = m_Index - 8 - m_ProtocolLen;
            if (State == DecodeState.Content)
            {
                if (left >= 0)
                {
                    //找到了一个完整的协议，开始解析
                    var type = ProtocolTable.GetType(m_ProtocolID);

                    Protocol protocol = null;
                    try
                    {
                        //ToDo:缓存优化
                        var serializer = new DataContractJsonSerializer(type);
                        var ms = new MemoryStream(m_DecodeBuffer, 8, m_ProtocolLen);
                        protocol = serializer.ReadObject(ms) as Protocol;
                    }
                    catch (Exception e)
                    {
                        Log.Debug("[DecodeBuffer] 4 -> " + e);
                        protocol = null;
                    }

                    if (protocol != null)
                    {
                        try
                        {
                            OnGetProtocol(this, protocol);
                        }
                        catch (Exception e)
                        {
                            Log.Debug("[DecodeBuffer] 5 -> " + e);
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    if (left > 0)
                    {
                        Array.Copy(m_DecodeBuffer, 8 + m_ProtocolLen, m_DecodeBuffer, 0, left);
                        m_Index = left;
                        m_ProtocolID = -1;
                        m_ProtocolLen = 0;

                        State = DecodeState.Length;

                        goto DecodeLength;
                    }
                    else
                    {
                        m_Index = 0;
                        m_ProtocolID = -1;
                        m_ProtocolLen = 0;

                        State = DecodeState.Length;
                    }
                }               
            }

            return true;
        }
    }
}
