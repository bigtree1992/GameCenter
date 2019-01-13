using QConnection;
using QData;
using QProtocols;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net;

namespace QGameCenterLogic
{
    /// <summary>
    /// 用于管理跟客户端通讯的逻辑
    /// </summary>
    public class QServer : QServerBase
    {
        public Action<IPAddress> OnClientConnected;
        public Action<IPAddress> OnClientDisconnected;

        private Dictionary<IPAddress, ClientEvent> m_Clients;

        /// <summary>
        /// 启动Server
        /// </summary>
        /// <param name="config"></param>
        public QServer(QServerConfig config) : base(config)
        {
            m_Clients = new Dictionary<IPAddress, ClientEvent>();
        }

        /// <summary>
        /// 停止Server
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            m_Clients.Clear();
        }

        public bool IsConnect(IPAddress address)
        {
            if (m_Clients.ContainsKey(address))
            {
                var client = m_Clients[address];
                if (client != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool SendProtocol(IPAddress address, Protocol protocol)
        {
            if (m_Clients.ContainsKey(address))
            {
                var client = m_Clients[address];
                if (client != null)
                {
                    client.SendProtocol(protocol);
                    return true;
                }
                else
                {
                    Log.Error("[QServer] SendProtocol Error: Client Null : " + address);
                }
            }
            else
            {
                Log.Error("[QServer] SendProtocol Error: Client NotExist : " + address);
            }
            return false;
        }


        protected override void OnClientConnect(ClientEvent client)
        {
            Log.Debug("[OnClientConnect] " + client.Address);

            if (m_Clients.ContainsKey(client.Address))
            {
                Log.Error("[OnClientConnect] Fatal Error : Dup client:" + client.Address);
            }
            else
            {
                m_Clients.Add(client.Address, client);
            }
            
            if (OnClientConnected != null)
            {
                OnClientConnected(client.Address);
            }
        }

        protected override void OnClientDisconnect(ClientEvent client)
        {
            //Log.Debug("[OnClientDisconnect] " + client.Address);

            if (OnClientConnected != null)
            {
                OnClientDisconnected(client.Address);
            }
            m_Clients.Remove(client.Address);
        }

        protected override void OnClientTimeout(ClientEvent client)
        {
            Log.Debug("[OnClientTimeout] " + client.Address);
        }

        protected override void OnRegisterProtocols()
        {
            SetProtocol(typeof(P_ClientState), OnClientState);
            SetProtocol(typeof(P_ComputerOpRsp), OnComputerOpRsp);
            SetProtocol(typeof(P_QClientOpRsp), OnQClientOpRsp);
            SetProtocol(typeof(P_ZipOpRsp), OnZipOpRsp);
            SetProtocol(typeof(P_ProcessOpRsp), OnProcessOpRsp);
            SetProtocol(typeof(P_XMLFileOpRsp), OnXMLFileOpRsp);
            SetProtocol(typeof(P_XMLFileViewRsp), OnXMLFileViewRsp);
            SetProtocol(typeof(P_GetClientMachineCodeRsp), OnGetClientMachineCodeRsp);
            SetProtocol(typeof(P_GetClientVersionRsp), OnGetClientVesionRsp);
            SetProtocol(typeof(P_SendFileRsp), OnSendFileRsp);
            SetProtocol(typeof(P_GetMachineInfoRsp), OnGetMachineInfoRsp);
            SetProtocol(typeof(P_StartReceiveInfoRsp), OnToggleReveiveMachineInfoRsp);
        }

        public void SendFile(IPAddress address,string sourcePath, string targetPath,Action<IPAddress, P_SendFileRsp> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(null,new P_SendFileRsp { Code = Code.Failed});
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(null,new P_SendFileRsp { Code = Code.Failed });
                return;
            }

            var p = new P_SendFile();
            p.SourcePath = sourcePath;
            p.TargetPath = targetPath;
            client.SendProtocol(p);
            OnSendFileInfo = callback;
        }

        private Action<IPAddress,P_SendFileRsp> OnSendFileInfo;
        private void OnSendFileRsp(ReceiveEventArgs client, Protocol protocol)
        {
            var p = protocol as P_SendFileRsp;
            if (OnSendFileInfo != null)
            {
                OnSendFileInfo(client.Address, p);
            }
        }

        private void OnClientState(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_ClientState;
                p.Time = Utils.DateTimeToUnixTimestamp(DateTime.Now);
                client.ActiveTime = DateTime.Now;
                client.SendProtocol(p);
            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnClientState Error : " + e.Message);
            }
        }

        private Action<Code> m_ComputerOPCallback;
        public void ComputerOP(IPAddress address, ComputerOp op, Action<Code> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(Code.Failed);
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(Code.Failed);
                return;
            }

            var p = new P_ComputerOp();
            p.Operation = op;
            client.SendProtocol(p);
            m_ComputerOPCallback = callback;
        }

        private void OnComputerOpRsp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_ComputerOpRsp;
                m_ComputerOPCallback(p.Code);
            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnComputerOpRsp Error:" + e.Message);
            }

        }

        private Action<IPAddress,Code> m_QClientOPCallback;
        public void QClientOP(IPAddress address, string fileName, QClientOp op, Action<IPAddress, Code> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(null,Code.Failed);
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(null,Code.Failed);
                return;
            }

            var p = new P_QClientOp();
            p.Operation = op;
            p.FileName = fileName;
            client.SendProtocol(p);
            m_QClientOPCallback = callback;
        }

        private void OnQClientOpRsp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_QClientOpRsp;
                m_QClientOPCallback(client.Address, p.Code);
            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnQClientOpRsp Error:" + e.Message);
            }
        }

        private Action<IPAddress,P_ZipOpRsp> m_ZipOPCallback;
        public void ZipOP(IPAddress address, string fileName, string targetPath, ZipOp op, Action<IPAddress, P_ZipOpRsp> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(null,new P_ZipOpRsp {Code = Code.Failed });
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(null,new P_ZipOpRsp { Code = Code.Failed });
                return;
            }

            var p = new P_ZipOp();
            p.Operation = op;
            p.SourcePath = fileName;
            p.TargetPath = targetPath;
            client.SendProtocol(p);
            m_ZipOPCallback = callback;
        }

        private void OnZipOpRsp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_ZipOpRsp;
                m_ZipOPCallback(client.Address,p);
            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnZipOpRsp Error:" + e.Message);
            }
        }

        private Action<Code> m_ProcessOpCallback;
        public void ProcessOP(IPAddress address, ProcessOp op, string path, string gameName, bool isDeleteBorder, bool isKillGameGA, Action<Code> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(Code.Failed);
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null || string.IsNullOrEmpty(path))
            {
                callback(Code.Failed);
                return;
            }

            var p = new P_ProcessOp();
            p.Operation = op;
            p.Path = path;

            if (isDeleteBorder)
            {
                p.NoBorder = 1;
            }
            else
            {
                p.NoBorder = 0;
            }

            if (isKillGameGA)
            {
                p.IsKillGameGA = 1;
            }
            else
            {
                p.IsKillGameGA = 0;
            }

            p.GameName = gameName;
            client.SendProtocol(p);
            m_ProcessOpCallback = callback;
        }

        private void OnProcessOpRsp(ClientEvent client, Protocol protocol)
        {
            try
            {
                lock (this)
                {
                    var p = protocol as P_ProcessOpRsp;
                    m_ProcessOpCallback(p.Code);
                }

            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnProcessOpRsp Error:" + e.Message);
            }
        }

        private Action<Code> m_XMLFileOpCallback;
        public void XMLFileOp(IPAddress address, string path, List<QData.KeyValuePair> pairs, Action<Code> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(Code.Failed);
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(Code.Failed);
                return;
            }

            //if(string.IsNullOrEmpty(path) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            //{
            //    callback(Code.Failed);
            //    return;
            //}
            if (string.IsNullOrEmpty(path))
            {
                callback(Code.Failed);
                return;
            }
            var p = new P_XMLFileOp();
            p.Path = path;

            var sendPairs = new List<QProtocols.KeyValuePair>();
            for (var i = 0; i < pairs.Count; i++)
            {
                var pair = new QProtocols.KeyValuePair();
                pair.Key = pairs[i].Key;
                pair.Value = pairs[i].Value;
                sendPairs.Add(pair);
            }

            p.pairs = sendPairs;
            client.SendProtocol(p);
            m_XMLFileOpCallback = callback;
        }

        private void OnXMLFileOpRsp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_XMLFileOpRsp;
                m_XMLFileOpCallback(p.Code);

            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnXMLFileOpRsp Error:" + e);
            }
        }

        private Action<P_XMLFileViewRsp> m_XMLFileViewOpCallback;
        public void XMLFileViewOp(IPAddress address, string path, Action<P_XMLFileViewRsp> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(new P_XMLFileViewRsp { Code = Code.Failed });
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(new P_XMLFileViewRsp { Code = Code.Failed });
                return;
            }
            var p = new P_XMLFileOpView();
            p.Path = path;
            client.SendProtocol(p);
            m_XMLFileViewOpCallback = callback;
        }

        private void OnXMLFileViewRsp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_XMLFileViewRsp;
                m_XMLFileViewOpCallback(p);
            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnXMLFileViewRsp Error:" + e.Message);
            }
        }

        private Action<P_GetClientMachineCodeRsp> m_GetMachineCodeCallback;
        public void GetMachineCode(IPAddress address, bool isGetcode, Action<P_GetClientMachineCodeRsp> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(new P_GetClientMachineCodeRsp { Code = Code.Failed });
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(new P_GetClientMachineCodeRsp { Code = Code.Failed });
                return;
            }

            var p = new P_GetClientMachineCode();
            p.Get = isGetcode;

            m_GetMachineCodeCallback = callback;
            client.SendProtocol(p);
        }

        private void OnGetClientMachineCodeRsp(ReceiveEventArgs client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_GetClientMachineCodeRsp;
                m_GetMachineCodeCallback(p);
            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnGetClientMachineCodeRsp Error:" + e.Message);
            }
        }

        public void GetClientMachineInfo(IPAddress address, Action<string, string, string, List<string>> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(null,null,null,null);
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(null, null, null, null);
                return;
            }

            var p = new P_GetMachineInfo();

            p.GetMachineInfo = true;

            m_GetMachineInfoback = callback;

            client.SendProtocol(p);
        }

        private Action<string, string, string, List<string>> m_GetMachineInfoback;

        private void OnGetMachineInfoRsp(ReceiveEventArgs client, Protocol protocol)
        {
            var p = protocol as P_GetMachineInfoRsp;
            m_GetMachineInfoback(client.Address.ToString(), p.Machine, p.Info, p.DiskInfo);
        }

        private Action<IPAddress,P_GetClientVersionRsp> m_GetClientVersionCallback;
        public void GetClientVersion(IPAddress address,List<string> checkPaths, Action<IPAddress, P_GetClientVersionRsp> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(null,new P_GetClientVersionRsp { Code = Code.Failed});
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(null,new P_GetClientVersionRsp { Code = Code.Failed });
                return;
            }

            if(checkPaths == null)
            {
                checkPaths = new List<string>();
            }

            var p = new P_GetClientVersion();
            p.CheckPaths = checkPaths;

            m_GetClientVersionCallback = callback;
            client.SendProtocol(p);
        }

        private void OnGetClientVesionRsp(ReceiveEventArgs client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_GetClientVersionRsp;
                m_GetClientVersionCallback(client.Address, p);
            }
            catch (Exception e)
            {
                Log.Error("[QServer] OnGetClientVesionRsp Error:" + e.Message);
            }
        }

        private Action<bool> m_ToggleReceiveMachineInfoCallback;
        public void ToggleReceiveMachineInfo(IPAddress address, bool toggle, Action<bool> callback)
        {
            if (!m_Clients.ContainsKey(address))
            {
                callback(false);
                return;
            }

            var client = m_Clients[address];
            if (client == null || address == null)
            {
                callback(false);
                return;
            }

            var p = new P_StartReceiveInfo();
            p.Toggle = toggle;

            m_ToggleReceiveMachineInfoCallback = callback;
            client.SendProtocol(p);

        }

        private void OnToggleReveiveMachineInfoRsp(ReceiveEventArgs client, Protocol protocol)
        {            
            var p = protocol as P_StartReceiveInfoRsp;
            m_ToggleReceiveMachineInfoCallback(p.IsStart);
        }
    }
}
