using QConnection;
using System.Collections.Generic;

namespace QUpdateGame
{
    public class ClientVersionInfo
    {
        public ClientVersionInfo()
        {
            ClientFileInfos = new List<QProtocols.FileInfo>();
            OtherFileInfos = new List<QProtocols.FileInfo>();
        }

        public string ClientVersion { get; set; }

        public List<QProtocols.FileInfo> ClientFileInfos { get; set; }

        public List<QProtocols.FileInfo> OtherFileInfos { get; set; }
        
    }
}
