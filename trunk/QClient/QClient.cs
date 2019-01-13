using System;
using QProtocols;
using QConnection;
using System.Net;
using System.Threading;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Management;
using Lemony.SystemInfo;
using System.Collections.Generic;

namespace QClientNS
{
    internal class QClient : QClientBase
    {
        public Action<string> OnGetStartGameName;
        public Action<bool> OnToggleSendInfo;
        public Action OnClientConnected;
        public Action OnClientDisconnected;
        public Action OnShutDownApp;

        private QClientConfig m_QClientConfig;
        private IPEndPoint m_EndPoint; 
        private UnZipTask m_UnZipTask;
        private QFileClient m_FileClient;

        public string MachineCode
        {
            get;set;
        }

        public string Version
        {
            get; set;
        }

        internal QClient(QClientConfig config) : base(config)
        {
            m_QClientConfig = config;
            Version = config.Version;
            MachineCode = QClientNS.MachineCode.GetMachineCode();
            m_UnZipTask = new UnZipTask();
            m_FileClient = new QFileClient();
        }

        /// <summary>
        /// 开始启动Client
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        public override void Start(IPEndPoint remoteEndPoint)
        {
            base.Start(remoteEndPoint);
            m_EndPoint = remoteEndPoint;
        }

        public override void Stop()
        {
            base.Stop();
        }

        protected override void OnClientConnect(ClientEvent client)
        {
            Log.Debug("[QClient] OnClientConnect.");
            OnClientConnected?.Invoke();
        }

        protected override void OnClientDisconnect(ClientEvent client)
        {
            Log.Debug("[QClient] OnClientDisconnect.");

            m_UnZipTask.Stop();

            OnClientDisconnected?.Invoke();

            Thread.Sleep(1000);
            //Start(m_EndPoint);
            return;
        }

        protected override void OnClientTimeout(ClientEvent client)
        {
            //说明没有连接到主机
            if (client == null)
            {
                Thread.Sleep(1000);
                Start(m_EndPoint);
            }
            Log.Debug("[QClient] OnClientTimeout.");
        }

        protected override void OnRegisterProtocols()
        {
            base.OnRegisterProtocols();
            SetProtocol(typeof(P_ClientState), OnClientState);
            SetProtocol(typeof(P_ComputerOp), OnComputerOp);
            SetProtocol(typeof(P_QClientOp), OnQClientOp);
            SetProtocol(typeof(P_ProcessOp), OnProcessOp);
            SetProtocol(typeof(P_XMLFileOp), OnXMLFileOp);
            SetProtocol(typeof(P_XMLFileOpView), OnXmlFileOpView);
            SetProtocol(typeof(P_GetClientMachineCode), OnGetMachineCode);
            SetProtocol(typeof(P_GetMachineInfo), OnGetMachineInfo);
            SetProtocol(typeof(P_GetClientVersion), OnGetClientVersion);
            SetProtocol(typeof(P_StartReceiveInfo), OnStartReceiveInfo);
            SetProtocol(typeof(P_ZipOp), OnZipOp);
            SetProtocol(typeof(P_SendFile), OnSendFile);
        }

        private void OnGetMachineInfo(ClientEvent client, Protocol protocol)
        {
            var p = protocol as P_GetMachineInfo;

            if (p.GetMachineInfo == false)
            {
                return;
            }

            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            var info = machineName + "|" + userName;

            var rsp = new P_GetMachineInfoRsp();

            try
            {
                rsp.Info = info;
                rsp.Machine = MachineCode;

                //刷新网络信息
                var nInfo = SystemInfo.GetNetInfoByIp(Utils.GetLocalIP());
                rsp.NetInfo = "名称 - " + nInfo.Name + "类型 - " + nInfo.Type.ToString() + "状态 - " + nInfo.Status.ToString() + "速度 - " + nInfo.Speed.ToString() +
                                "总接收 - " + nInfo.InOctets.ToString() + "总发送 - " + nInfo.OutOctets.ToString() + "总错收 - " + nInfo.InErrors.ToString() +
                                "总错发 - " + nInfo.OutErrors.ToString() + "未知协议 - " + nInfo.InUnknownProtos.ToString() + "物理地址 - " + nInfo.PhysAddr;


                rsp.DiskInfo = new List<string>();
                //获取磁盘使用情况
                var diskClass = new ManagementClass("Win32_LogicalDisk");
                var disks = diskClass.GetInstances();
                foreach (var disk in disks)
                {
                    var diskName = disk["Name"].ToString();
                    var diskDescription = disk["Description"].ToString();
                    var diskSize = disk["Size"].ToString();
                    var diskFreeSpace = disk["FreeSpace"].ToString();
                    rsp.DiskInfo.Add(diskName + "|" + diskDescription + "|" + diskSize + "|" + diskFreeSpace);
                }
                SendProtocol(rsp);
            }
            catch (Exception e)
            {
                Log.Error("[QClient] OnGetMachineInfo Error : " + e.ToString());
            }
        }

        private void OnGetClientVersion(ClientEvent client, Protocol protocol)
        {
            var rsp = new P_GetClientVersionRsp();
            rsp.ClientFileInfos = null;
            rsp.OtherFileInfos = null;

            try
            {                
                var p = protocol as P_GetClientVersion;
                var ignoreList = new List<string> { "Log" };
                ignoreList.AddRange(p.CheckPaths);

                rsp.Code = Code.Success;
                rsp.ClientVersion = this.Version;
                rsp.ClientFileInfos = Utils.GetFileInfoFromPath(".", ignoreList);
                rsp.OtherFileInfos = new List<QProtocols.FileInfo>();

                foreach (var path in p.CheckPaths)
                {
                    if (Directory.Exists(path))
                    {
                        var infos = Utils.GetVersionInfo(path);
                        rsp.OtherFileInfos.AddRange(infos);
                    }                
                }               
            }
            catch (Exception e)
            {
                rsp.Code = Code.Failed;
                rsp.ClientVersion = this.Version;
                Log.Error("[QClient] OnGetClientVersion Error : " + e);
            }
            SendProtocol(rsp);
        }

        private void OnGetMachineCode(ClientEvent client, Protocol protocol)
        {
            var rsp = new P_GetClientMachineCodeRsp();

            try
            {
                var p = protocol as P_GetClientMachineCode;
                if (p.Get)
                {
                    rsp.Code = Code.Success;
                    rsp.MachineCode = MachineCode;
                }
                else
                {
                    rsp.Code = Code.Failed;
                    rsp.MachineCode = string.Empty;
                }
            }
            catch (Exception e)
            {
                rsp.Code = Code.Failed;
                rsp.MachineCode = string.Empty;
                Log.Error("[QClient] OnGetMachineCode Error : " + e.Message);
            }
            SendProtocol(rsp);
        }

        private void OnClientState(ClientEvent client, Protocol protocol)
        {
            try
            {
                client.ActiveTime = DateTime.Now;
            }
            catch (Exception e)
            {
                Log.Error("[QClient] OnClientState Error : " + e.Message);
            }
        }

        private void OnComputerOp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_ComputerOp;

                if (p.Operation == ComputerOp.Shutdown)
                {
                    Utils.ExecuteCmd("shutdown -s -t 3");
                }
                else if (p.Operation == ComputerOp.Restart)
                {
                    Utils.ExecuteCmd("shutdown -r -t 3");
                }

                var rsp = new P_ComputerOpRsp();
                rsp.Code = Code.Success;
                SendProtocol(rsp);
            }
            catch (Exception e)
            {
                Log.Error("[QClient] ComputerOp Error : " + e.Message);
                var rsp = new P_ComputerOpRsp();
                rsp.Code = Code.Failed;
                SendProtocol(rsp);
            }
        }

        private void OnQClientOp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_QClientOp;

                if (p.Operation == QClientOp.Shutdown)
                {
                    OnShutDownApp();
                }
                else if (p.Operation == QClientOp.Restart)
                {
                    var rsp = new P_QClientOpRsp();
                    rsp.Code = Code.Success;
                    SendProtocol(rsp);
                    OnShutDownApp();
                    Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
                else if (p.Operation == QClientOp.Newstart)
                {
                    var rsp = new P_QClientOpRsp();
                    rsp.Code = Code.Success;
                    SendProtocol(rsp);

                    Thread.Sleep(1000);
                    Process.Start(p.FileName);
                    OnShutDownApp();
                }
            }
            catch (Exception e)
            {
                Log.Error("[QClient] OnQClientOp Error : " + e.Message);
                var rsp = new P_QClientOpRsp();
                rsp.Code = Code.Failed;
                SendProtocol(rsp);
            }
        }
     
        private void OnProcessOp(ClientEvent client, Protocol protocol)
        {

            try
            {
                var p = protocol as P_ProcessOp;
                var name = Path.GetFullPath(p.Path);
                var path = Path.GetDirectoryName(name);


                if (p.Operation == ProcessOp.Start)
                {
                    if (!File.Exists(name))
                    {
                        var rsp = new P_ProcessOpRsp();
                        rsp.Code = Code.NotExist;
                        client.SendProtocol(rsp);
                        return;
                    }

                    var processName = Path.GetFileNameWithoutExtension(name);
                    if (Process.GetProcessesByName(processName).Length > 0)
                    {
                        var rsp = new P_ProcessOpRsp();
                        rsp.Code = Code.AlreadyExist;
                        client.SendProtocol(rsp);
                        return;
                    }

                    try
                    {
                        var start = new ProcessStartInfo();
                        start.FileName = name;
                        start.WorkingDirectory = path;
                        start.UseShellExecute = true;
                        if (m_QClientConfig.IsMoveGame != 1)
                        {
                            start.WindowStyle = ProcessWindowStyle.Maximized;
                        }

                        var process = Process.Start(start);

                        if (p.NoBorder == 1)
                        {
                            Utils.DeleteBorder(process);
                        }

                        if (p.GameName != "ResetMachine" && p.GameName != "ResetHeadset")
                        {
                            OnGetStartGameName(p.GameName);
                        }

                        if (m_QClientConfig.IsMoveGame == 1)
                        {
                            Win32API.MoveWindow(process.MainWindowHandle, m_QClientConfig.Left, m_QClientConfig.Top, m_QClientConfig.Width, m_QClientConfig.Height, true);
                        }

                        var rsp = new P_ProcessOpRsp();
                        rsp.Code = Code.Success;
                        SendProtocol(rsp);
                    }
                    catch (Exception e)
                    {
                        Log.Error("[QClient] StartProcess Error : " + e.Message);
                        var rsp = new P_ProcessOpRsp();
                        rsp.Code = Code.Failed;
                        SendProtocol(rsp);
                    }

                }
                else if (p.Operation == ProcessOp.Stop)
                {
                    //关闭马机进程
                    if (p.IsKillGameGA == 1)
                    {
                        var fileName = Path.GetFullPath("MachineTools/CloseGa/CloseGa.exe");
                        try
                        {
                            var start = new ProcessStartInfo();                            
                            start.FileName = fileName;
                            start.WorkingDirectory = Path.GetDirectoryName(fileName);
                            start.UseShellExecute = true;
                            Process.Start(start);                            
                        }
                        catch (Exception e)
                        {
                            Log.Error("[QClient] StopProcess Error : " + e.Message + "   path : " + fileName);
                        }
                    }

                    OnGetStartGameName("");
                    var processName = Path.GetFileNameWithoutExtension(name);
                    var ps = Process.GetProcessesByName(processName);
                    if (ps.Length < 1)
                    {
                        var rsp = new P_ProcessOpRsp();
                        rsp.Code = Code.AlreadyExist;
                        SendProtocol(rsp);
                        return;
                    }

                    try
                    {
                        ps[0].Kill();

                        var rsp = new P_ProcessOpRsp();
                        rsp.Code = Code.Success;
                        SendProtocol(rsp);
                    }
                    catch (Exception e)
                    {
                        Log.Error("[QClient] StopProcess Error : " + e.Message);
                        var rsp = new P_ProcessOpRsp();
                        rsp.Code = Code.Failed;
                        SendProtocol(rsp);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[QClient] ProcessOp Error : " + e.Message);
                var rsp = new P_ProcessOpRsp();
                rsp.Code = Code.Failed;
                SendProtocol(rsp);
            }
        }

        private void OnXMLFileOp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_XMLFileOp;
                var doc = new XmlDocument();

                var path = Path.GetFullPath(p.Path);

                //foreach (var pa in p.pairs)
                //{
                //    Log.Debug(" key : " + pa.Key + " value : " + pa.Value);
                //}

                if (string.IsNullOrEmpty(path))
                {
                    Log.Error("[QClient] OnXMLFileOp Error : path == null.");
                    return;
                }

                doc.Load(path);
                foreach (var n1 in doc.ChildNodes)
                {
                    XmlElement xe1;
                    xe1 = n1 as XmlElement;
                    if (xe1 == null)
                    {
                        continue;
                    }
                    foreach (var n2 in xe1.ChildNodes)
                    {
                        var xe2 = n2 as XmlElement;
                        if (xe2 == null)
                        {
                            continue;
                        }

                        for (var i = 0; i < p.pairs.Count; i++)
                        {
                            if (xe2.Name == p.pairs[i].Key)
                            {
                                xe2.InnerText = p.pairs[i].Value;
                            }
                        }
                    }
                }

                doc.Save(path);

                var rsp = new P_XMLFileOpRsp();
                rsp.Code = Code.Success;
                SendProtocol(rsp);
            }
            catch (Exception e)
            {
                Log.Error("[QClient] XMLFileOp Error : " + e.Message);
                var rsp = new P_XMLFileOpRsp();
                rsp.Code = Code.Failed;
                SendProtocol(rsp);
            }
        }

        private void OnXmlFileOpView(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_XMLFileOpView;
                var path = Path.GetFullPath(p.Path);

                if (File.Exists(p.Path) == false)
                {
                    Log.Error("[QClient] OnXmlFileOpView Error : path is not Exists. path : " + path);
                    return;
                }

                var stream = new StreamReader(path, Encoding.Default);
                string content = stream.ReadToEnd();
                stream.Close();

                var rsp = new P_XMLFileViewRsp();
                rsp.Code = Code.Success;
                rsp.XmlContent = content;

                SendProtocol(rsp);
            }
            catch (Exception e)
            {
                var rsp = new P_XMLFileViewRsp();
                rsp.Code = Code.Failed;
                rsp.XmlContent = "";
                Log.Error("[QClient] OnXMLFileOp Error : " + e.Message);
                SendProtocol(rsp);
            }
        }

        private void OnZipOp(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_ZipOp;
                var rsp = new P_ZipOpRsp();

                if (p.Operation != ZipOp.UnZip)
                {                    
                    rsp.Code = Code.Success;
                    rsp.Info = "ZipOp = Zip";
                    SendProtocol(rsp);                    
                    return;
                }

                string fullPath = Path.GetFullPath(p.SourcePath);
                if (!File.Exists(fullPath))
                {
                    Log.Error("[QClient] OnZipOp Error : Not Exist : " + p.SourcePath);
                    rsp = new P_ZipOpRsp();
                    rsp.Code = Code.NotExist;
                    rsp.Info = "文件不存在";
                    SendProtocol(rsp);
                    return;
                }
                
                m_UnZipTask.Stop();

                m_UnZipTask.OnProgress = (code, info,state, progress) =>
                {                 
                    rsp = new P_ZipOpRsp();
                    rsp.Code = code;                    
                    rsp.State = state;
                    rsp.Progress = progress;
                    rsp.Info = info;
                    SendProtocol(rsp);
                };

                m_UnZipTask.Start(fullPath, p.TargetPath);

            }
            catch (Exception e)
            {
                Log.Error("[QClient] OnZipOp Error : " + e.Message);
                var rsp = new P_ZipOpRsp();
                rsp.Code = Code.Failed;
                rsp.Info = e.Message;
                SendProtocol(rsp);
            }
        }

        private void OnStartReceiveInfo(ClientEvent client, Protocol protocol)
        {
            var P = protocol as P_StartReceiveInfo;

            if (OnToggleSendInfo != null)
            {
                OnToggleSendInfo(P.Toggle);
            }

            var rsp = new P_StartReceiveInfoRsp();
            rsp.IsStart = true;

            SendProtocol(rsp);
        }

        private void OnSendFile(ClientEvent client, Protocol protocol)
        {
            try
            {
                var p = protocol as P_SendFile;

                m_FileClient.Stop();

                m_FileClient.OnProgress = (progress) => 
                {
                    var rsp = new P_SendFileRsp();
                    rsp.Code = Code.Success;
                    rsp.State = OpState.Doing;
                    rsp.Progress = progress;
                    SendProtocol(rsp);
                };

                m_FileClient.OnError = (error) =>
                {
                    Log.Error("[QClient] FileClient Error : " + error);
                    var rsp = new P_SendFileRsp();
                    rsp.Code = Code.Failed;
                    rsp.State = OpState.Doing;
                    rsp.Progress = 0;
                    rsp.Info = error;
                    SendProtocol(rsp);
                };

                m_FileClient.OnFinshed = () =>
                {
                    var rsp = new P_SendFileRsp();
                    rsp.Code = Code.Success;
                    rsp.State = OpState.Done;
                    rsp.Progress = 1;
                    SendProtocol(rsp);
                };
                
                m_FileClient.Start(client.Address.ToString(), p.SourcePath, p.TargetPath);

            }
            catch(Exception e)
            {
                Log.Error("[QClient] OnSendFile Error : " + e);
                var rsp = new P_SendFileRsp();
                rsp.Code = Code.Failed;
                rsp.State = OpState.Start;
                rsp.Info = e.Message;
                SendProtocol(rsp);
            }
        }
    }
}
