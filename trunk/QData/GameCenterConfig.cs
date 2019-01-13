using System;
using System.IO;
using System.Xml.Serialization;

namespace QData
{
    public class GameCenterConfig
    {
        [XmlElement("UserName")]
        public string UserName { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }

        [XmlElement("ChangeScreenTime")]
        public int ChangeScreenTime { get; set; }

        [XmlElement("SetFullScreen")]
        public int SetFullScreen { get; set; }

        [XmlElement("CursorPath")]
        public string CursorPath { get; set; }

        [XmlElement("UseCoin")]
        public int UseCoin { get; set; }

        [XmlElement("IsTest")]

        public int IsTest { get; set; }

        [XmlElement("IconPath")]
        public string IconPath { get; set; }

        [XmlElement("WechatIcon")]
        public string WechatIcon { get; set; }

        [XmlElement("MustHostStart")]
        public int MustHostStart;
        
        [XmlElement("InSertDBTime")]
        public int InSertDBTime;

        [XmlElement("CoinComPort")]
        public string CoinComPort;

        public static GameCenterConfig LoadData(string path)
        {
            try
            {
                var file = File.Open(path, FileMode.Open);
                var xmlSerializer = new XmlSerializer(typeof(GameCenterConfig));
                var config = xmlSerializer.Deserialize(file) as GameCenterConfig;
                file.Close();
                return config;
            }
            catch (Exception exception)
            {
                Log.Error("[QGameCenterData] QGameCenterData Error:" + exception.Message);
                return null;
            }
        }

    }
}
