using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace QData
{
    public class ClientData 
    {
        private  Dictionary<int, IPAddress> m_IPDict = new Dictionary<int, IPAddress>();
        private  Dictionary<IPAddress,int > m_IPDict2 = new Dictionary<IPAddress, int>();

        private static List<ClientInfo> m_ClientInfos;

        [XmlElement("ClientInfo")]
        public List<ClientInfo> ClientInfos
        {
            get { return m_ClientInfos; }
            set { m_ClientInfos = value; }
        }

        public bool Contains(IPAddress ip)
        {
            return m_IPDict2.ContainsKey(ip);
        }
        public void CreateAClient(string path,IPAddress ip)
        {
            lock (this)
            {
                int id = 1;
                for (int i = 0; i < m_ClientInfos.Count; i++)
                {
                    if (m_ClientInfos[i].ID >= id)
                    {
                        id = m_ClientInfos[i].ID + 1;
                    }
                }       
                AddClientInfo(id, ip);
                //
                try
                {
                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "    ";
                    settings.NewLineChars = "\r\n";
                    settings.Encoding = Encoding.UTF8;

                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    var file = File.Open(path, FileMode.OpenOrCreate);
                    var xmlSerializer = new XmlSerializer(typeof(ClientData));
                
                    xmlSerializer.Serialize(file, this, namespaces);
                    file.Close();
                }
                catch (Exception e)
                {
                    Log.Error("[ClientData] CreateAClient Error : " + e.Message);
                }
            }        
        }

        public static ClientData LoadData(string path)
        {
            try
            {               
                var file = File.Open(path, FileMode.Open);
                var xmlSerializer = new XmlSerializer(typeof(ClientData));
                var clientData = xmlSerializer.Deserialize(file) as ClientData;
                file.Close();
                if (clientData != null)
                {
                    foreach (var info in m_ClientInfos)
                    {
                        if(clientData.m_IPDict.ContainsKey(info.ID) == false)
                        {
                            clientData.m_IPDict.Add(info.ID, IPAddress.Parse(info.IP));
                            clientData.m_IPDict2.Add(IPAddress.Parse(info.IP), info.ID);
                        }
                    }
                }
                else
                {
                    Log.Error("[ClientData] LoadData Error : clientData==null.");
                }

                return clientData;
            }
            catch (Exception exception)
            {
                Log.Error("[ClientData] LoadData Error : " + exception.Message);
                return null;
            }
        }

        public void Clear()
        {
            m_IPDict.Clear();
            m_IPDict2.Clear();
            ClientInfos.Clear();
        }


        public IPAddress GetClient(int id)
        {
            IPAddress ip = null;
            if (m_IPDict.ContainsKey(id))
            {
                m_IPDict.TryGetValue(id, out ip);
            }
            return ip;
        }

        public int GetClient(IPAddress ip)
        {
            int ID = -1;
            if (m_IPDict2.ContainsKey(ip))
            {
                m_IPDict2.TryGetValue(ip, out ID);
            }
            return ID;
        }

        public void AddClientInfo(int id, IPAddress ip)
        {
            if (m_IPDict.ContainsValue(ip))
            {
                return;
            }
            m_ClientInfos.Add(new ClientInfo { ID = id,IP = ip.ToString()});
            m_IPDict.Add(id,ip);
            
            m_IPDict2.Add(ip, id);
            m_ClientInfos.Clear();
            foreach (KeyValuePair<IPAddress, int> info in m_IPDict2)
            {
                m_ClientInfos.Add(new ClientInfo { ID = info.Value, IP = info.Key.ToString() });
            }
            //students.Sort(delegate(Student a, Student b) { return a.Age.CompareTo(b.Age); });
            m_ClientInfos.Sort(delegate(ClientInfo a, ClientInfo b) { return a.IP.CompareTo(b.IP); });

            for(var i = 0; i < m_ClientInfos.Count; i++ )
            {
                m_ClientInfos[i].ID = i + 1;
            }

        }

        public void Print()
        {
            foreach(var v in m_ClientInfos)
            {
                Log.Debug("Id : " + v.ID + "  IP : " + v.IP);
            }
        }

    }
}
