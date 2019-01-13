using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace QConnection
{
    public class SendEventArgs : SocketAsyncEventArgs
    {
        internal Socket Socket { get; set; }
    }
}
