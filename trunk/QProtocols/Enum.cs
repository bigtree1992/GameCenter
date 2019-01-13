using System.Runtime.Serialization;

namespace QProtocols
{
    [DataContract]
    public enum Code
    {
        Success,
        Failed,
        NotExist,
        AlreadyExist,
        UnKnow
    }

    [DataContract]
    public enum ComputerOp
    {
        Shutdown,
        Restart
    }

    [DataContract]
    public enum ProcessOp
    {
        Start,
        Stop
    }


    [DataContract]
    public enum OpType
    {
        File,
        Key
    }

    [DataContract]
    public enum QClientOp
    {
        Shutdown,
        Restart,
        Newstart
    }

    [DataContract]
    public enum ZipOp
    {
        UnZip,
        Zip
    }

    [DataContract]
    public enum OpState
    {
        Start,
        Doing,
        Done
    }
}
