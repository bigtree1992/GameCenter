using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using QProtocols;
using QConnection;
using QGameCenterLogic;

namespace QUpdateGame
{
    public class UpdateClientLogic
    {
        private string ClientZipFilePath = "ClientFiles\\QClient\\";
        private readonly List<string> CheckPaths = new List<string> { "Games", "MachineTools" };
        private const string ClientExeName = "奇境森林中控客户端.exe";
        private const string ClientSteamVRPath = "ClientFiles\\StreamVR\\SteamVR.zip";
        private Dictionary<IPAddress, ClientUpdateInfo> m_UpdateInfoDict;
        private ObservableCollection<ClientUpdateInfo> m_UpdateInfoList;        
        private LocalVersionInfo m_LocalVersionInfo;

        private Window m_Window;
        private QServer m_Server;
        private Button[] m_Buttons;
        private DataGrid m_DataGrid;

        private bool m_IsBindUI;

        public UpdateClientLogic(Window window, QServer server)
        {
            try
            {
                m_Window = window;
                m_Server = server;

                m_Server.OnClientConnected += OnClientConnected;
                m_Server.OnClientDisconnected += OnClientDisconnected;

                m_UpdateInfoDict = new Dictionary<IPAddress, ClientUpdateInfo>();
                m_UpdateInfoList = new ObservableCollection<ClientUpdateInfo>();
                m_LocalVersionInfo = new LocalVersionInfo();
                m_LocalVersionInfo.Load(ClientZipFilePath, CheckPaths);
                ClientZipFilePath = GetLastestClientPath();
                
            }
            catch (Exception e)
            {
                Log.Error("[UpdateClientLogic] UpdateClientLogic Error : " + e.Message);
            }
        }

        private string GetLastestClientPath()
        {
            var dirInfo = new DirectoryInfo(ClientZipFilePath);
            var files = dirInfo.GetFiles();
            System.IO.FileInfo target = null;
            foreach (var file in files)
            {
                if (file.Name.StartsWith("QClient-") &&
                    file.Name.EndsWith(".zip"))
                {
                    if(target == null ||
                       m_LocalVersionInfo.
                       CheckClientVersion(file.Name,target.Name))
                    {
                        target = file;
                    }
                }
            }

            return target == null ? "" : $"ClientFiles\\QClient\\{target.Name}";
        }

        public void BindingUI(DataGrid dataGrid, Button[] buttons)
        {
            try
            {
                m_DataGrid = dataGrid;

                m_Buttons = buttons;
                m_Buttons[0].Click += OnUpdateClientButton;
                m_Buttons[1].Click += OnUpdateOtherButton;
                m_Buttons[2].Click += OnForceUpdateOtherButton;

                m_IsBindUI = true;

                m_DataGrid.ItemsSource = m_UpdateInfoList;
            }
            catch (Exception e)
            {
                Log.Error("[UpdateClientLogic] BindingUI : " + e.Message);
            }
        }

        public void UnBindUI()
        {
            try
            {
                m_UpdateInfoList = null;
                m_Server.OnClientConnected -= OnClientConnected;
                m_Server.OnClientDisconnected -= OnClientDisconnected;

                m_Buttons[0].Click -= OnUpdateClientButton;
                m_Buttons[1].Click -= OnUpdateOtherButton;
                m_Buttons[2].Click -= OnForceUpdateOtherButton;
                m_IsBindUI = false;
            }
            catch (Exception ex)
            {
                Log.Error("[UpdateClientLogic] UnBindUI : " + ex.Message);
            }
        }

        /// <summary>
        /// 升级客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateClientButton(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateAllClient(true, false);
            }
            catch (Exception ex)
            {
                Log.Error("[UpdateClientLogic] OnUpdateClientButton : " + ex.Message);
            }
        }

        /// <summary>
        /// 升级其他文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateOtherButton(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateAllClient(false, false);
            }
            catch (Exception ex)
            {
                Log.Error("[UpdateClientLogic] OnUpdateOtherButton : " + ex.Message);
            }
        }

        /// <summary>
        /// 强制替换其他文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnForceUpdateOtherButton(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("是否进行替换游戏文件", "强制替换", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    UpdateAllClient(false, true);
                }
                catch (Exception ex)
                {
                    Log.Error("[UpdateClientLogic] OnForceUpdateOtherButton : " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 安装或者重新安装SteamVR
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInstallOrRestoreSteamVRButton(object sender, RoutedEventArgs e)
        {

            if (!File.Exists(ClientSteamVRPath))
            {
                return;
            }

            for (int i = 0; i < m_UpdateInfoList.Count; i++)
            {
                var ip = IPAddress.Parse(m_UpdateInfoList[i].ClientIP);
                if (!m_UpdateInfoDict.ContainsKey(ip))
                {
                    continue;
                }

                if (!m_UpdateInfoList[i].Selected)
                {
                    m_UpdateInfoList[i].Detail = "未选择该客户端进行操作";
                    continue;
                }

                UpdateAClientSteamVR(ip);

            }
        }

        private void UpdateAClientSteamVR(IPAddress ip)
        {
            var info = m_UpdateInfoDict[ip];
            info.State = StateInfo.SteamVR安装中;
            info.Detail = "SteamVR文件上传中";
            //
            string targetPath = $"..\\StreamVR";

            UpdateAClientFile(ip, ClientSteamVRPath, targetPath, (response_ip) => {
                string target = $"..\\StreamVR\\Installer.exe";
                //调用SteamVR安装程序进行重新安装
                info = m_UpdateInfoDict[response_ip];
                info.Detail = "正在安装SteamVR";

                m_Server.ProcessOP(response_ip,ProcessOp.Start, target,"", false, false, (code) =>
                {
                    if (code == Code.Success)
                    {
                        info = m_UpdateInfoDict[response_ip_1];
                        info.State = StateInfo.等待操作;
                        info.Detail = "客户端已经更新";
                    }
                    else
                    {
                        
                    }
                });
            });
        }

        /// <summary>
        /// 当客户端上线时
        /// </summary>
        /// <param name="ip"></param>
        private void OnClientConnected(IPAddress ip)
        {
            if (m_IsBindUI == false)
            {
                return;
            }

            try
            {
                if (!m_UpdateInfoDict.ContainsKey(ip))
                {
                    var info = new ClientUpdateInfo();
                    info.ClientIP = ip.ToString();
                    info.Progress = 0;
                    info.State = StateInfo.已连接;
                    info.Detail = "";
                    info.Selected = false;

                    m_UpdateInfoDict.Add(ip, info);
                    m_Window.Dispatcher.Invoke(() => {
                        m_UpdateInfoList.Add(info);
                    });                   
                }
                else
                {
                    var info = m_UpdateInfoDict[ip];
                    info.Progress = 0;
                    info.State = StateInfo.已连接;
                    info.Selected = false;                
                }
                CheckVersionAndUpdate(ip, false, false, false);
            }
            catch (Exception e)
            {
                Log.Error("[UpdateClientLogic] OnClientConnected Error : " + e.Message);
            }
        }

        /// <summary>
        /// 当客户端掉线时执行操作
        /// </summary>
        /// <param name="ip"></param>
        private void OnClientDisconnected(IPAddress ip)
        {
            try
            {
                var info = m_UpdateInfoDict[ip];
                info.Progress = 0;
                info.State = StateInfo.未连接;
                info.Detail = "";
                info.Selected = false;               
            }
            catch (Exception e)
            {
                Log.Error("[UpdateClientLogic] OnClientDisconnected Error : " + e.Message);
            }
        }

        /// <summary>
        /// 对所有的客户端执行更新操作
        /// </summary>
        /// <param name="clientOnly">是否只更新客户端</param>
        private void UpdateAllClient(bool clientOnly, bool forceUpdate)
        {
            m_LocalVersionInfo.Load(ClientZipFilePath, CheckPaths);

            for (int i = 0; i < m_UpdateInfoList.Count; i++)
            {
                var ip = IPAddress.Parse(m_UpdateInfoList[i].ClientIP);
                if (!m_UpdateInfoDict.ContainsKey(ip))
                {
                    continue;
                }

                if (!m_UpdateInfoList[i].Selected)
                {
                    m_UpdateInfoList[i].Detail = "未选择该客户端进行操作";
                    continue;
                }

                CheckVersionAndUpdate(ip, true, clientOnly, forceUpdate);
            }
        }

        /// <summary>
        /// 检查客户端跟文件是否需要更新，并显示信息
        /// </summary>
        /// <param name="needUpdate"></param>
        private void CheckVersionAndUpdate(IPAddress ip, bool needUpdate, bool clientOnly, bool forceUpdate)
        {
            m_Server.GetClientVersion(ip, CheckPaths, (response_ip, rsp) => {

                if (!m_UpdateInfoDict.ContainsKey(response_ip))
                {
                    return;
                }

                var info = m_UpdateInfoDict[response_ip];
                info.VersionInfo.ClientVersion = rsp.ClientVersion;
                info.VersionInfo.ClientFileInfos = rsp.ClientFileInfos;
                info.VersionInfo.OtherFileInfos = rsp.OtherFileInfos;

                bool exist = File.Exists(ClientZipFilePath);
                // 所有客户端并行执行
                // 1 先进行更新客户端
                bool needUpdateClient = m_LocalVersionInfo.
                    CheckClientFileChange(info.VersionInfo.ClientFileInfos);

                var paths = m_LocalVersionInfo.
                GetNeedUpdatePath(info.VersionInfo.OtherFileInfos, forceUpdate);

                if (!exist)
                {
                    info.State = StateInfo.更新文件丢失;
                }
                else if (needUpdateClient)
                {
                    if (paths.Count == 0)
                    {
                        info.State = StateInfo.客户端需要更新;
                    }
                    else
                    {
                        info.State = StateInfo.全部需要更新;
                    }                                        
                }
                else
                {
                    if (paths.Count == 0)
                    {
                        info.State = StateInfo.无需更新;
                    }
                    else
                    {
                        info.State = StateInfo.游戏需要更新;
                    }                   
                }

                needUpdateClient = (needUpdateClient) && exist;

                if (!needUpdate)
                {
                    return;
                }

                if (clientOnly && needUpdateClient)
                {
                    UpdateClientFile(response_ip);
                }
                else if ( !clientOnly && paths.Count != 0)
                {
                    // 仅仅更新其他文件
                    UpdateOtherFiles(response_ip, forceUpdate, paths);                    
                }
            });
        }

        private void UpdateClientFile(IPAddress ip)
        {
            var info = m_UpdateInfoDict[ip];
            info.State = StateInfo.客户端更新中;
            info.Detail = "文件上传中";
            //客户端压缩包名需要自带版本号的信息
            string targetPath = $"..\\{Path.GetFileNameWithoutExtension(ClientZipFilePath)}";

            UpdateAClientFile(ip, ClientZipFilePath, targetPath, (response_ip) => {
                string target = 
                $"..\\{Path.GetFileNameWithoutExtension(ClientZipFilePath)}\\{ClientExeName}";
                //调用进程启动命令进行重新启动客户端
                info = m_UpdateInfoDict[response_ip];
                info.Detail = "正在重启客户端";

                m_Server.QClientOP(response_ip, target, QClientOp.Newstart, (response_ip_1, code) =>
                {
                    if (code == Code.Success)
                    {
                        info = m_UpdateInfoDict[response_ip_1];
                        info.State = StateInfo.等待操作;
                        info.Detail = "客户端已经更新";
                    }
                    else
                    {
                        Log.Error("[CheckVersionAndUpdate] QClientOP Newstart Failed.");
                    }
                });
            });
        }

        /// <summary>
        /// 更新除了客户端之外的其他文件
        /// </summary>
        /// <param name="ip"></param>
        private void UpdateOtherFiles(IPAddress ip, bool forceUpdate, List<string> paths)
        {
            if (!m_UpdateInfoDict.ContainsKey(ip))
            {
                return;
            }

            UpdateAClientFileByIndex(ip, paths, 0);
        }

        /// <summary>
        /// 根据索引更新path中的文件
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="paths"></param>
        /// <param name="index"></param>
        private void UpdateAClientFileByIndex(IPAddress ip, List<string> paths, int index)
        {
            if (index >= paths.Count)
            {
                //全部文件更新完毕
                var info = m_UpdateInfoDict[ip];
                info.State = StateInfo.等待操作;
                info.Detail = "文件更新完成";
                return;
            }

            string path = paths[index];
            string target = m_LocalVersionInfo.ToClientPath(path,false);
                            
            UpdateAClientFile(ip, path , target ,(response_ip) => {
                UpdateAClientFileByIndex(response_ip, paths, ++index);
            });
        }

        /// <summary>
        /// 更新一个压缩文件
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="path"></param>
        /// <param name="complete"></param>
        private void UpdateAClientFile(IPAddress ip, string path, string target, Action<IPAddress> complete)
        {
            SendFileToClient(ip, path, (right_ip) => {
                
                UnZipFile(right_ip, path, target, complete);
            });
        }

        /// <summary>
        /// 发送一个压缩文件到客户端
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="path"></param>
        /// <param name="complete"></param>
        private void SendFileToClient(IPAddress ip, string path, Action<IPAddress> complete)
        {
            var info = m_UpdateInfoDict[ip];

            if (!m_Server.IsConnect(ip))
            {
                info.State = StateInfo.未连接;
                return;
            }

            if (!File.Exists(path))
            {
                info.State = StateInfo.更新文件丢失;
                info.Detail = $"{path} 丢失";
                return;
            }

            info.State = StateInfo.文件上传中;
            info.Detail = $"{path} 正在传输";

            string filename = Path.GetFileName(path);
            path = m_LocalVersionInfo.ToClientPath(path, true);
          
            m_Server.SendFile(ip, path, $"..\\Temp\\{filename}", (response_ip, rsp) =>
            {
                info = m_UpdateInfoDict[response_ip];
                if (rsp.Code == Code.Success)
                {
                    if (rsp.State == OpState.Done)
                    {
                        complete?.Invoke(response_ip);
                        info.Progress = rsp.Progress;
                    }
                    else if (rsp.State == OpState.Doing)
                    {
                        m_Window.Dispatcher.Invoke(() => {
                            info.Progress = rsp.Progress;
                        });                        
                    }
                }
                else
                {
                    //发送文件出错，需要及时显示到UI信息
                    info.State = StateInfo.错误;
                    info.Detail = $"传输<{path}>出错: " + rsp.Info;
                }
            });
        }

        /// <summary>
        /// 在客户端进行解压一个文件
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="source"></param>
        /// <param name="complete"></param>
        private void UnZipFile(IPAddress ip, string source, string target, Action<IPAddress> complete)
        {
            var info = m_UpdateInfoDict[ip];
            info.State = StateInfo.文件解压中;
            info.Detail = $"正在操作: {source} -> {target}";

            string filename = Path.GetFileName(source);

            m_Server.ZipOP(ip, $"..\\Temp\\{filename}", target, ZipOp.UnZip, (response_ip, rsp) =>
            {
                info = m_UpdateInfoDict[response_ip];
                if (rsp.Code == Code.Success)
                {
                    if (rsp.State == OpState.Done)
                    {
                        complete?.Invoke(response_ip);
                        info.Progress = 1;                        
                    }
                    else if (rsp.State == OpState.Doing)
                    {
                        info.Progress = rsp.Progress;                        
                    }
                }
                else
                {
                    //解压出错，需要及时显示到UI信息
                    info.State = StateInfo.错误;
                    info.Detail = $"解压<{filename}>出错: {rsp.Info}";
                }
            });
        }

    }
}
