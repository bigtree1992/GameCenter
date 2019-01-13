using QData;
using QGameCenterLogic;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Net;
using System.Windows;
using QConnection;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace QGameManager
{

    public class SendFileLogic
    {
        private List<ClientFileInfo> m_ClientFileInfos;
        private List<FileInfo> m_FileInfos;

        private ClientData m_ClientData;
        private QServerConfig m_ServerConfig;
        private QServer m_Server;
        private GameData m_GameData;

        private DataGrid m_DataGrid;
        private ComboBox m_ComboBox;

        private string m_GamePath;
        private Window m_Window;

        private bool m_IsBindUI = false;

        private TextBox m_FilePathTextBox;
        private string m_FilePath = string.Empty;

        public Action OnSendScuess;

        public SendFileLogic(QServer server,QServerConfig serverConfig, ClientData clientdata,GameData gamedata)
        {
            m_ClientFileInfos = new List<ClientFileInfo>();
            m_ClientData = clientdata;
            m_ServerConfig = serverConfig;

            m_Server = server;
            server.OnClientConnected += OnClientConnected;
            server.OnClientDisconnected += OnClientDisconnected;
            for (var i = 0; i < clientdata.ClientInfos.Count; i++)
            {
                var info = new ClientFileInfo();
                info.ID = clientdata.ClientInfos[i].ID;
                info.IP = clientdata.ClientInfos[i].IP;
                info.Progress = 0;
                info.FilePath = "";
                info.State = "未连接";
                m_ClientFileInfos.Add(info);
            }
            
            m_GameData = gamedata;
        }

        private void OnSendFileState(string state,string ip)
        {
            var id = m_ClientData.GetClient(IPAddress.Parse(ip));
            m_ClientFileInfos[id - 1].State = state;
            m_Window.Dispatcher.Invoke(() =>
            {
                m_DataGrid.Items.Refresh();
            });
            if(state.Equals("解压完成"))
            {
                m_Window.Dispatcher.Invoke(()=> {
                    SendScuess();
                });
                
            }
        }
        private void OnClientDisconnected(IPAddress ip)
        {
             var id = m_ClientData.GetClient(ip);
            m_ClientFileInfos[id - 1].State = "未连接";
            if (m_IsBindUI == false)
            {
                return;
            }

            m_Window.Dispatcher.Invoke(() =>
            {
                m_DataGrid.Items.Refresh();
            });
        }

        private void OnClientConnected(IPAddress ip)
        {
            var id = m_ClientData.GetClient(ip);
            m_ClientFileInfos[id - 1].State = "连接";

            if (m_IsBindUI == false)
            {
                return;
            }
            m_Window.Dispatcher.Invoke(() =>
            {
                m_DataGrid.Items.Refresh();

            });
        }

        public void BindUI(string gamePath,Window window,DataGrid dataGrid, ComboBox exePath,Button[] button, TextBox filePath)
        {
          
            m_GamePath = gamePath;
            m_Window = window;
            m_DataGrid = dataGrid;

            for (var i = 0; i < m_ClientFileInfos.Count; i++)
            {
                m_ClientFileInfos[i].Progress = 0;
                if(m_Server.IsConnect(m_ClientData.GetClient(i + 1)))
                {
                    m_ClientFileInfos[i].State = "连接";

                }
                else
                {
                    m_ClientFileInfos[i].State = "未连接";
                }
            }

            m_DataGrid.ItemsSource = m_ClientFileInfos;

            m_FileInfos = new List<FileInfo>();

            exePath.ItemsSource = m_FileInfos;
            m_ComboBox = exePath;

            
            m_FilePathTextBox = filePath;

            button[0].Click += (sender, e) => { OnSendFile(); };
            button[1].Click += (sender, e) => {
                OnSelectedGame();
            };

            m_IsBindUI = true;

            //m_Window.Dispatcher.Invoke(() =>
            //{
            //    m_DataGrid.Items.Refresh();
            //});
        }
        
        public void UnBindUI()
        {
            m_Window = null;
         
            //m_DataGrid = null;

            m_FileInfos.Clear();
            m_FileInfos = null;

            m_IsBindUI = false;

        }

        public void Stop()
        {

        }

        private void OnCancel()
        {

        }

        private void OnSendFile()
        {
            if (string.IsNullOrEmpty(m_FilePathTextBox.Text))
            {
                for (var i = 0; i < m_ClientFileInfos.Count; i++)
                {
                    m_ClientFileInfos[i].State = "请先选择游戏 .";
                }
                m_Window.Dispatcher.Invoke(() =>
                {
                    m_DataGrid.Items.Refresh();
                });
                return;
            }

            for (var i = 0; i < m_ClientFileInfos.Count; i++)
            {
                if (!m_ClientFileInfos[i].Selected)
                {
                    continue;
                }
                var ip = m_ClientData.ClientInfos[i].IP;
                if (!m_Server.IsConnect(IPAddress.Parse(ip)))
                {
                    m_ClientFileInfos[i].State = "客户端没有连接，无法上传文件.";
                    m_Window.Dispatcher.Invoke(() =>
                    {
                        m_DataGrid.Items.Refresh();
                    });
                    continue;
                }
                SendFileToClient(ip, i);
            }

        }

        private void SendFileToClient(string ip,int i)
        {
            try
            {
                var path = m_FilePathTextBox.Text;
                if (!File.Exists(path))
                {
                    m_ClientFileInfos[i].State = "选中上传文件不存在.";
                    m_Window.Dispatcher.Invoke(() =>
                    {
                        m_DataGrid.Items.Refresh();
                    });
                    return;
                }
                //Log.Debug("path : " + path);
                m_Total++;
                

            }
            catch(Exception e)
            {
                Log.Error("[SendFileLogic] SendFileToClient Error : " + e.ToString());
            }
        }

        private int m_SendScuessFlag = 0;
        private int m_Total = 0;
        private void SendScuess()
        {
            m_SendScuessFlag++;
            if (m_SendScuessFlag == m_Total)
            {
                //1.检测上传的是什么游戏
                //2.判断配置文件中是否已经有了这个游戏的配置 如果有了 则更新启动项
                //               
                var array1 = m_FilePathTextBox.Text.Split('\\');
                var array2 = array1[array1.Length - 1].Split('.');
                for (var i = 0; i < m_GameData.GameInfos.Count; i++)
                {
                    if (m_GameData.GameInfos[i].Path.Contains(array2[0]))
                    {
                        var info = m_ComboBox.SelectedItem as FileInfo;
                        var array3 = m_GameData.GameInfos[i].Path.Split('\\');
                        //var newPath = array3[0] +"\\" + array2[0] + "\\" + info.FileName;
                        //Log.Debug("newPath : " + newPath);
                        var gameInfo = new GameInfo();
                        gameInfo = m_GameData.GameInfos[i];
                        gameInfo.Path = array3[0] + "\\" + array2[0] + "\\" + info.FileName;
                        m_GameData.ModifyAGameInfo(gameInfo, "Configs/GameData.xml");
                        if(OnSendScuess != null)
                        {
                            OnSendScuess();
                        }
                    }
                }
                m_Total = 0;
                m_SendScuessFlag = 0;
            }
        }

        private void OnSelectedGame()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            //ofd.Multiselect = true; // 多选
            ofd.DefaultExt = ".zip";
            ofd.Filter = "Zip|*.zip|All Files|*.*";
            if (ofd.ShowDialog() == true)
            {
                var zip = new ZipFile(ofd.FileName);
                var packages = zip.GetEnumerator();
                var id = 0;
                m_FileInfos.Clear();
                while (packages.MoveNext())
                {
                    var extension = Path.GetExtension(packages.Current.ToString());
                    var packageName = packages.Current.ToString();
                    if (extension.Equals(".exe"))
                    {
                        var array = packageName.Split('/');
                        var info = new FileInfo();
                        info.FileID = id;
                        //有多个/
                        if (array.Length > 1)
                        {
                            info.FileName = array[array.Length - 1];
                        }
                        else
                        {
                            info.FileName = packageName;
                        }
                        m_FileInfos.Add(info);
                        id++;
                    }
                }
                
                zip.Close();
                if (m_FileInfos.Count > 0)
                {
                    m_ComboBox.SelectedIndex = 0;
                }
                m_Window.Dispatcher.Invoke(()=> {
                    m_ComboBox.Items.Refresh();
                    m_FilePathTextBox.Text = ofd.FileName;
                });
            }
        }

    }
}
