using QConnection;
using QData;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using QGameCenterLogic;
using System.ComponentModel;

namespace QUpdateGame
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private QServer m_Server;
        private IPBroadcaster m_Broadcaster;
        private QServerConfig m_ServerConfig;
        private CheckRouterLogic m_CheckRouterLogic;
        private UpdateClientLogic m_UpdateClientLogic;
        private QFileServer m_FileServer;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                m_ServerConfig = QServerConfig.LoadData("Configs/ServerConfig.xml");
                if (m_ServerConfig == null)
                {
                    Log.Error("[QGameCenter] MainWindow Can't Load ServerConfig.xml");
                    this.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            try
            {
                m_Server = new QServer(m_ServerConfig);
                
                m_UpdateClientLogic = new UpdateClientLogic(this, m_Server);
                var buttons = new Button[3];
                buttons[0] = this.UpdateClientBtn;
                buttons[1] = this.UpdateOtherFilesBtn;
                buttons[2] = this.ForceUpdateClientBtn;

                m_UpdateClientLogic.BindingUI(dataGrid, buttons);

                var ip = Utils.GetLocalIP();
                //获取不到本地连接以及无线网络连接  则使用配置ip
                if (string.IsNullOrEmpty(ip))
                {
                    this.Close();
                    Application.Current.Shutdown();
                    return;
                }

                var endpoint = new IPEndPoint(IPAddress.Parse(ip), m_ServerConfig.Port);                
                //启动服务器
                m_Server.Start(endpoint);

                m_Broadcaster = new IPBroadcaster();
                //启动广播
                m_Broadcaster.Start(ip, m_ServerConfig.Port);               
            }
            catch (Exception e)
            {
                Log.Error("[QGameCenter] MainWindow Error : " + e.Message);
            }

            if (!Directory.Exists("ClientFiles"))
            {
                Directory.CreateDirectory("ClientFiles");
            }
            m_FileServer = new QFileServer();
            m_FileServer.Start(Path.GetFullPath("ClientFiles"));
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show("是否退出系统", "退出", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }  
        }

        protected override void OnClosed(EventArgs e)
        {
            if (m_UpdateClientLogic != null)
            {
                m_UpdateClientLogic.UnBindUI();
                m_UpdateClientLogic = null;
            }

            if (m_CheckRouterLogic != null)
            {
                m_CheckRouterLogic.Stop();
                m_CheckRouterLogic = null;
            }

            if (m_Broadcaster != null)
            {
                m_Broadcaster.Stop();
                m_Broadcaster = null;
            }

            if (m_Server != null)
            {
                m_Server.Stop();
                m_Server = null;
            }

            if (m_ServerConfig != null)
            {
                m_ServerConfig = null;
            }

            Environment.Exit(0);
        }

    }
}
