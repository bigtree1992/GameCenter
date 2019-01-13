using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace QConnection
{
    /// <summary>
    /// 缓存管理器，提供一块大的内存，供所有Socket异步事件使用
    /// </summary>
    internal class BufferManager
    {
        private int m_TotalBytes;
        private byte[] m_Buffer;
        private Stack<int> m_FreeIndexPool;
        private int m_CurrentIndex;
        private int m_BufferSize;
        internal int TotalBytes{ get { return m_TotalBytes; } }

        internal BufferManager(int totalBytes, int bufferSize)
        {
            m_TotalBytes = totalBytes;
            m_CurrentIndex = 0;
            m_BufferSize = bufferSize;
            m_FreeIndexPool = new Stack<int>();
            m_Buffer = new byte[m_TotalBytes];
        }

        internal bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (m_FreeIndexPool.Count > 0)
            {
                args.SetBuffer(m_Buffer, m_FreeIndexPool.Pop(), m_BufferSize);
            }
            else
            {
                if (m_CurrentIndex > (m_TotalBytes - m_BufferSize))
                {
                    return false;
                }
                args.SetBuffer(m_Buffer, m_CurrentIndex, m_BufferSize);
                m_CurrentIndex += m_BufferSize;
            }

            return true;
        }

        internal void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_FreeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
