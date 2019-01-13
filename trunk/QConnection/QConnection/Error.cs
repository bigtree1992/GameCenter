using System;
using System.Collections.Generic;


namespace QConnection
{
    public enum Error
    {
        UnKonw,
        TimeOut,
        Reset,//对方直接关闭程序或者杀死进程
        Abort,
        NoData,
        NotReceiveOrSend,
        NoToken,
        Serialize,
        Deserialize,
        OpAbort
    }
}
