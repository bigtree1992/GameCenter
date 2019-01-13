using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class QServerConfig
{
    [XmlElement("MaxConnections")]
    public int MaxConnections { get; set; }
    [XmlElement("ReceiveBufferSize")]
    public int ReceiveBufferSize { get; set; }
    [XmlElement("DecodeBufferSize")]
    public int DecodeBufferSize { get; set; }
    [XmlElement("ClientTimeout")]
    public int ClientTimeout { get; set; }
    [XmlElement("Port")]
    public int Port { get; set; }
    [XmlElement("ScreenPort")]
    public int ScreenPort { get; set; }
    [XmlElement("FilePort")]
    public int FilePort { get; set; }

    public static QServerConfig LoadData(string path)
    {
        try
        {
            var file = File.Open(path, FileMode.Open);
            var xmlSerializer = new XmlSerializer(typeof(QServerConfig));
            var config = xmlSerializer.Deserialize(file) as QServerConfig;
            file.Close();
            return config;
        }
        catch (Exception exception)
        {
            Log.Error("[QServerConfig] QServerConfig Error:" + exception.Message);
            return null;
        }
    }




}