using System;
using System.IO;
using System.Xml.Serialization;
using QConnection;

public class QClientConfig
{
    [XmlElement("ReceiveBufferSize")]
    public int ReceiveBufferSize { get; set; }

    [XmlElement("DecodeBufferSize")]
    public int DecodeBufferSize { get; set; }

    [XmlElement("ClientTimeout")]
    public int Timeout { get; set; }

    [XmlElement("Port")]
    public int Port { get; set; }

    [XmlElement("ScreenPort")]
    public int ScreenPort { get; set; }
    [XmlElement("FilePort")]
    public int FilePort { get; set; }

    [XmlElement("Frequency")]
    public int Frequency { get; set; }

    [XmlElement("WindowLocationLeft")]
    public int WindowLocationLeft;

    [XmlElement("WindowLocationTop")]
    public int WindowLocationTop;

    [XmlElement("IsMoveGame")]
    public int IsMoveGame;
    [XmlElement("Top")]
    public int Top;
    [XmlElement("Left")]
    public int Left;
    [XmlElement("Height")]
    public int Height;
    [XmlElement("Width")]
    public int Width;

    [XmlElement("Version")]
    public string Version;

    public static QClientConfig LoadData(string path)
    {
        try
        {
            var file = File.Open(path, FileMode.Open);
            var xmlSerializer = new XmlSerializer(typeof(QClientConfig));
            var config = xmlSerializer.Deserialize(file) as QClientConfig;
            file.Close();
            return config;
        }
        catch (Exception e)
        {
            Log.Error("[QClientConfig] LoadData Error : " + e.Message);
            return null;
        }
    }

    



}
