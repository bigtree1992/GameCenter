using System.Collections.Generic;

namespace QGameManager
{
    class ClientFileInfo
    {
        public bool Selected { get; set; }
        public int ID { get; set; }

        public string IP { get; set; }

        public string FilePath { get; set; }

        public double Progress { get; set; }

        public string State { get; set; }

    }

    public class FileInfo
    {
        public int FileID { get; set; }
        public string FileName { get; set; }
    }

    public class SportNoBorderInfo
    {
        public int SportID { get; set; }
        public string SportInfo { get; set; }
    }
}
