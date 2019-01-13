using QData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QGameCenterLogic
{
    public class GetClientMachineCodeLogic
    {
        private QServer m_Server;
        private ClientData m_ClientData;

        private Dictionary<IPAddress, string> m_ClientMachineCodesDict_IP = new Dictionary<IPAddress, string>();
        private Dictionary<int, string> m_ClientMachineCodesDict_ID = new Dictionary<int, string>();
        public GetClientMachineCodeLogic(QServer server,ClientData clientdata)
        {
            m_Server = server;
            m_ClientData = clientdata;
            m_Server.OnClientConnected += OnClientConnected;

        }
        private int Index = 0;
        private void OnClientConnected(IPAddress ip)
        {
            var are = new AutoResetEvent(false);
            m_Server.GetMachineCode(ip, true, (p) => {
                //are.Set();
                if (p.Code == QProtocols.Code.Success)
                {
                    if (!m_ClientMachineCodesDict_IP.ContainsValue(p.MachineCode))
                    {
                        //var id = m_Server.ClientManager.Count - Index;
                        var ip_copy = m_ClientData.GetClient(Index + 1);
                        //Log.Debug("Index : " + Index);
                        m_ClientMachineCodesDict_IP.Add(ip_copy, p.MachineCode);
                        Index++;
                    }
                }
            });
            are.WaitOne(300 * 3);
            are.Dispose();
        }

        public void GetMachineString(IPAddress ip,out string machinecode)
        {
            if(m_ClientMachineCodesDict_IP.ContainsKey(ip))
            {
                machinecode = m_ClientMachineCodesDict_IP[ip];
            }
            else
            {
                machinecode = "None";
            }
        }

        public void GetMachineString(int id, out string machinecode)
        {
            var ip = m_ClientData.GetClient(id + 1);
            if(ip != null)
            {
                GetMachineString(ip, out machinecode);
            }
            else
            {
                machinecode = "None";
            }
        }

    }
}
