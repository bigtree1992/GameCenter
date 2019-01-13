using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QControlManagerNS
{
    internal class QControlManager
    {
        internal Action<string, string> OnAddClient;
        internal Action<string> OnRemoveClient;

        private const int Port = 4939;

        private UdpClient m_UdpClient;
        private CancellationTokenSource m_UDPTokenSource;
        private CancellationTokenSource m_CheckTokenSource;

        private Dictionary<string, ClientState> m_ClientStates 
            = new Dictionary<string, ClientState>();
        private Dictionary<string, AxRDPCOMAPILib.AxRDPViewer> m_AxRDPViewers
            = new Dictionary<string, AxRDPCOMAPILib.AxRDPViewer>();

        internal Dictionary<string, ClientState> AllClient
        {
            get { return m_ClientStates; }
        }

        internal string GetConnectionString(string ip)
        {
            if (m_ClientStates.ContainsKey(ip))
            {
                return m_ClientStates[ip].ConnectionString;
            }
            return "";
        }

        internal void StartFind()
        {
            var remotEndPoint = new IPEndPoint(IPAddress.Broadcast, QControlManager.Port);
            m_UdpClient = new UdpClient(QControlManager.Port);
            m_UDPTokenSource = new CancellationTokenSource();

            var task = new Task(() =>
            {
                while (!m_UDPTokenSource.IsCancellationRequested)
                {
                    var data = m_UdpClient.Receive(ref remotEndPoint);
                    var str = Encoding.UTF8.GetString(data);

                    string ip = "";
                    string conn_str = "";

                    if (!string.IsNullOrEmpty(str))
                    {
                        var vals = str.Split('|');
                        if (vals.Length >= 2)
                        {
                            ip = vals[0];
                            conn_str = vals[1];
                        }
                        
                    }

                    if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(conn_str))
                    {
                        OnClientInfo(ip, conn_str);
                    }
                }
                m_UdpClient.Close();
                m_UdpClient = null;
                m_UDPTokenSource = null;
            }, m_UDPTokenSource.Token);

            task.Start();

            m_CheckTokenSource = new CancellationTokenSource();
            var task1 = new Task(() =>
            {
                while (!m_CheckTokenSource.IsCancellationRequested)
                {
                    var toRemove = new List<string>();
                    foreach(var pair in m_ClientStates)
                    {
                        if ((pair.Value.UpdateTime.AddSeconds(3) < DateTime.Now))
                        {
                            toRemove.Add(pair.Key);
                        }
                    }

                    for(int i = 0; i< toRemove.Count; i++)
                    {
                        m_ClientStates.Remove(toRemove[i]);
                        OnRemoveClient?.Invoke(toRemove[i]);
                    }
                    Thread.Sleep(2000);
                }
                m_CheckTokenSource = null;
            }, m_CheckTokenSource.Token);
            task1.Start();
        }

        internal void StopFind()
        {
            if(m_UDPTokenSource != null)
            {
                m_UDPTokenSource.Cancel();
            }

            if (m_CheckTokenSource != null)
            {
                m_CheckTokenSource.Cancel();
            }            
        }

        private void OnClientInfo(string ip,string connection_string)
        {
            if (m_ClientStates.ContainsKey(ip))
            {
                m_ClientStates[ip].ConnectionString = connection_string;
                m_ClientStates[ip].UpdateTime = DateTime.Now;
            }
            else
            {
                m_ClientStates[ip] = new ClientState();
                m_ClientStates[ip].ConnectionString = connection_string;
                m_ClientStates[ip].UpdateTime = DateTime.Now;
                OnAddClient?.Invoke(ip, connection_string);
            }
        }

        internal void StartConnection(AxRDPCOMAPILib.AxRDPViewer axRDPViewer, string ip)
        {
            var conn_str = GetConnectionString(ip);
            axRDPViewer.OnConnectionEstablished += (sender, e) => {
                m_AxRDPViewers[ip] = axRDPViewer;

                if (m_ClientStates.ContainsKey(ip))
                {
                    m_ClientStates[ip].IsOpen = true;
                }
                
            };
            axRDPViewer.OnConnectionFailed += (sender, e) => { };
            axRDPViewer.OnConnectionTerminated += (sender, e) => { };
            axRDPViewer.OnError += (sender, e) => { };
            axRDPViewer.Connect(conn_str, Environment.UserName, "");
        }

        internal void CloseConnection(string ip)
        {
            if (m_AxRDPViewers.ContainsKey(ip))
            {
                var viewer = m_AxRDPViewers[ip];
                try
                {
                    viewer.Disconnect();
                }
                catch
                {

                }

                m_AxRDPViewers.Remove(ip);
                if (m_ClientStates.ContainsKey(ip))
                {
                    m_ClientStates[ip].IsOpen = false;
                }
            }
        }

    }
}
