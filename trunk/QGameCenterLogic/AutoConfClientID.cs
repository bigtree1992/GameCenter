using QData;
using System.Net;


namespace QGameCenterLogic
{
    public class AutoConfClientID 
    {
        private QServer m_Server;
        private string m_ClientDataPath;
        private ClientData m_ClientData ;

        public AutoConfClientID( QServer server , ClientData clientdata, string path)
        {
            m_Server = server;
            m_Server.OnClientConnected += OnClientConnected;
            m_ClientData = clientdata;
            m_ClientDataPath = path;
        }
       
        private void OnClientConnected(IPAddress ip)
        {
            if (!m_ClientData.Contains(ip))
            {
                m_ClientData.CreateAClient(m_ClientDataPath, ip);
            }
        }
    }
}
