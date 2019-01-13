using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace QSetIP
{
    public class IPInfo
    {
        public int ID { get; set; }
        public string IPAddress { get; set; }
       
        public  string SubnetMask { get; set; }

        public string DefaultIPGateway { get; set; }
    }

    public class IPInfoConfig
    {
        private static List<IPInfo> m_IPInfos;

        public List<IPInfo> IPInfos
        {
            get { return m_IPInfos; }
            set { m_IPInfos = value; }
        }

        public static IPInfoConfig LoadData(string path)
        {
            try
            {
                var file = File.Open(path, FileMode.Open);
                var xmlSerializer = new XmlSerializer(typeof(IPInfoConfig));
                var config = xmlSerializer.Deserialize(file) as IPInfoConfig;
                file.Close();
                if (config != null)
                {
                    return config;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
