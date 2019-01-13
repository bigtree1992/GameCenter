using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace QConnection
{
    internal class SocketEventPool<T> where T : SocketAsyncEventArgs 
    {
        private Stack<T> m_Pool;

        internal SocketEventPool(int capacity)
        {
            m_Pool = new Stack<T>(capacity);
        }

        internal void Push(T item)
        {
            if (item == null)
            {
                return;
            }

            lock (m_Pool)
            {
                m_Pool.Push(item);
            }
        }

        internal T Pop()
        {
            while (m_Pool.Count == 0)
            {
                Thread.Sleep(1);
            }
            lock (m_Pool)
            {
                return m_Pool.Pop();
            }
        }

        internal int Count
        {
            get { return m_Pool.Count; }
        }
    }
}
