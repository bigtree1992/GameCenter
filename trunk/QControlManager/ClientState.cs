using System;

namespace QControlManagerNS
{
    internal class ClientState
    {
        internal string ConnectionString { get; set; }

        internal bool IsOpen{get;set;}

        internal DateTime UpdateTime { get; set; }
    }
}
