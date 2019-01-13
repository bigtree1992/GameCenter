using QConnection;
using QGameCenter.Data;
using QGameCenterLogic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;


namespace QGameCenter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

      

        private QServerConfig m_ServerConfig;
        private QServer m_Server;
        private IPBroadcaster m_Broadcaster;

        private ClientMachineLogic m_ClientMachineLogic;

        public MainWindow()
        {
            InitializeComponent();

            m_ServerConfig = QServerConfig.LoadData("Configs/ServerConfig.xml");
            if (m_ServerConfig == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load ServerConfig.xml");
                MessageBox.Show("程序无法加载服务端的配置文件，无法启动");
                this.Close();
                return;
            }

            try
            {
                m_Server = new QServer(m_ServerConfig);
                var ip = Utils.GetLocalIP();
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

            m_ClientMachineLogic = new ClientMachineLogic(m_Server, this, dataGrid);

        }

        /// <summary>
        /// 设置全屏
        /// </summary>
        private void SetFullScreen()
        {
            this.WindowState = WindowState.Normal;
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
        }

        private void OnMainTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show("CLick");
        }

  

        private void OnWindowCloed(object sender, EventArgs e)
        {
            if(m_ClientMachineLogic != null)
            {
                m_ClientMachineLogic.Clear();
                m_ClientMachineLogic = null;
            }

            if(m_Server != null)
            {
                m_Server.Stop();
                m_Server = null;
            }

            if(m_ServerConfig != null)
            {
                m_ServerConfig = null;
            }

            if(m_Broadcaster != null)
            {
                m_Broadcaster.Stop();
                m_Broadcaster = null;
            }

            Environment.Exit(0);
        }
    }
}
