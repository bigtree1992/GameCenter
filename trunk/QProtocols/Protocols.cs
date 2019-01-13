using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QProtocols
{
    [DataContract]
    public class KeyValuePair
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Value { get; set; }
    }

    [ProtoID(1001)]
    [DataContract]
    public class P_ClientState : Protocol
    {
        [DataMember]
        public long Time { get; set; }
    }

    [ProtoID(1003)]
    [DataContract]
    public class P_ComputerOp : Protocol
    {
        [DataMember]
        public ComputerOp Operation { get; set; }
    }

    [ProtoID(1004)]
    [DataContract]
    public class P_ComputerOpRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
    }

    [ProtoID(1005)]
    [DataContract]
    public class P_ProcessOp : Protocol
    {
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string GameName { get; set; }
        [DataMember]
        public ProcessOp Operation { get; set; }
        [DataMember]
        public byte NoBorder { get; set; }
        [DataMember]
        public byte IsKillGameGA { get; set; }
    }

    [ProtoID(1006)]
    [DataContract]
    public class P_ProcessOpRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
    }

    [ProtoID(1007)]
    [DataContract]
    public class P_XMLFileOp : Protocol
    {
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public List<KeyValuePair> pairs { get; set; }
        
    }
    
    [ProtoID(1008)]
    [DataContract]
    public class P_XMLFileOpRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
        [DataMember]
        public OpType Type { get; set; }
    }

    [ProtoID(1009)]
    [DataContract]
    public class P_XMLFileOpView : Protocol
    {
        [DataMember]
        public string Path { get; set; }
    }

    [ProtoID(1010)]
    [DataContract]
    public class P_XMLFileViewRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
        [DataMember]
        public string XmlContent { get; set; }
    }

    [ProtoID(1013)]
    [DataContract]
    public class P_GetClientMachineCode : Protocol
    {
        [DataMember]
        public bool Get { get; set; }
    }

    [ProtoID(1014)]
    [DataContract]
    public class P_GetClientMachineCodeRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
        [DataMember]
        public string MachineCode { get; set; }        
    }


    [ProtoID(1017)]
    [DataContract]
    public class P_SendFile : Protocol
    {
        [DataMember]
        public string SourcePath { get; set; }
        [DataMember]
        public string TargetPath { get; set; }

    }

    [ProtoID(1018)]
    [DataContract]
    public class P_SendFileRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }

        [DataMember]
        public OpState State { get; set; }

        [DataMember]
        public float Progress { get; set; }

        [DataMember]
        public string Info { get; set; }
    }

    [ProtoID(1019)]
    [DataContract]
    public class P_GetMachineInfo : Protocol
    {
        [DataMember]
        public bool GetMachineInfo { get; set; }
    }

    [ProtoID(1020)]
    [DataContract]
    public class P_GetMachineInfoRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
        [DataMember]
        public string Machine { get; set; }
        [DataMember]
        public string Info { get; set; }
        [DataMember]
        public string NetInfo { get; set; }
        [DataMember]
        public List<string> DiskInfo { get; set; }      
    }

    [ProtoID(1021)]
    [DataContract]
    public class P_StartReceiveInfo : Protocol
    {
        [DataMember]
        public bool Toggle { get; set; }
    }

    [ProtoID(1022)]
    [DataContract]
    public class P_StartReceiveInfoRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
        [DataMember]
        public bool IsStart { get; set; }

    }

    [ProtoID(1023)]
    [DataContract]
    public class P_GetClientVersion : Protocol
    {
        [DataMember]
        public List<string> CheckPaths;
    }

    [DataContract]
    public class FileInfo
    {
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string Info { get; set; }
    }

    [ProtoID(1024)]
    [DataContract]
    public class P_GetClientVersionRsp : Protocol
    {        
        [DataMember]
        public Code Code { get; set; }

        [DataMember]
        public string ClientVersion { get; set; }

        [DataMember]
        public List<FileInfo> ClientFileInfos { get; set; }
       
        [DataMember]
        public List<FileInfo> OtherFileInfos { get; set; }
    }

    [ProtoID(1025)]
    [DataContract]
    public class P_QClientOp : Protocol
    {
        [DataMember]
        public QClientOp Operation { get; set; }
        [DataMember]
        public string FileName { get; set; }
    }

    [ProtoID(1026)]
    [DataContract]
    public class P_QClientOpRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
    }

    [ProtoID(1027)]
    [DataContract]
    public class P_ZipOp : Protocol
    {
        [DataMember]
        public ZipOp Operation { get; set; }
        [DataMember]
        public string SourcePath { get; set; }
        [DataMember]
        public string TargetPath { get; set; }
    }

    [ProtoID(1028)]
    [DataContract]
    public class P_ZipOpRsp : Protocol
    {
        [DataMember]
        public Code Code { get; set; }
        [DataMember]
        public OpState State { get; set; }
        [DataMember]
        public float Progress { get; set; }
        [DataMember]
        public string Info { get; set; }
    }

}
