using QConnection;
using QData;
using QGameCenterLogic;
using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace QGameManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientData m_ClientData;
        private GameData m_GameData;
        private QServerConfig m_ServerConfig;

        private QServer m_Server;
        private IPBroadcaster m_Broadcaster;

        private AutoConfClientID m_AutoConfigureClientsIP;
        private GameManagerLogic m_GameManagerLogic;
        private SendFileLogic m_SendFileLogic;

        public MainWindow()
        {
            InitializeComponent();
            Log.SetLogToFile();

            m_ServerConfig = QServerConfig.LoadData("Configs/ServerConfig.xml");
            if (m_ServerConfig == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load ServerConfig.xml");
                MessageBox.Show("程序无法加载服务端的配置文件，无法启动");
                this.Close();
                return;
            }

            m_ClientData = ClientData.LoadData("Configs/ClientData.xml");
            if (m_ClientData == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load ClientData.xml");
                MessageBox.Show("程序无法加载客户端的配置文件，无法启动");
                this.Close();
                return;
            }

            m_GameData = GameData.LoadData("Configs/GameData.xml");
            if (m_GameData == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load GameData.xml");
                MessageBox.Show("程序无法加载游戏配置文件，无法启动");
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

            if (!File.Exists("Configs/ClientData.xml"))
            {
                Log.Error("[QGameCenter] MainWindow Can't Load ClientData.xml");
                MessageBox.Show("程序无法加载客户端的配置文件，无法启动");
                this.Close();
                return;
            }


            m_AutoConfigureClientsIP = new AutoConfClientID(m_Server, m_ClientData, "Configs/ClientData.xml");

            m_SendFileLogic = new SendFileLogic(m_Server, m_ServerConfig, m_ClientData, m_GameData);

            var buttons = new Button[4];
            buttons[0] = this.AddButton;
            buttons[1] = this.ModifyButton;
            buttons[2] = this.DeleteButton;
            buttons[3] = this.SendButton;
            m_GameManagerLogic = new GameManagerLogic(null, m_GameData, dataGrid, buttons, m_SendFileLogic);

            m_SendFileLogic.OnSendScuess += () =>
            {
                m_GameManagerLogic.OnRefresh();
            };

        }

        private void OnClosed(object sender, System.EventArgs e)
        {
            if (m_GameManagerLogic != null)
            {
                m_GameManagerLogic.Stop();
                m_GameManagerLogic = null;
            }
            if (m_SendFileLogic != null)
            {
                m_SendFileLogic.Stop();
                m_SendFileLogic = null;
            }
            if (m_ClientData != null)
            {
                m_ClientData = null;
            }
            if (m_GameData != null)
            {
                m_GameData = null;
            }
            if (m_ServerConfig != null)
            {
                m_ServerConfig = null;
            }
            if (m_Server != null)
            {
                m_Server.Stop();
                m_Server = null;
            }
            if (m_Broadcaster != null)
            {
                m_Broadcaster.Stop();
                m_Broadcaster = null;
            }
        }
    }
}
