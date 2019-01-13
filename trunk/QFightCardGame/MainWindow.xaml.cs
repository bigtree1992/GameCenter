using QConnection;
using QData;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QGameCenterLogic
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //四个配置文件
        private QServerConfig m_ServerConfig;
        private ClientData m_ClientData;
        private GameData m_GameData;
        private GameCenterConfig m_GameCenterConfig;

        private CheckDogLogic m_CheckLogic;

        private QServer m_Server;
        private IPBroadcaster m_Broadcaster;

        private StartGameLogic m_StartGameLogic;
        private SettingLogic m_SettingLogic;
        private AutoConfClientID m_AutoConfigureClientsIP;

        private CheckCoinLogic m_CheckCoinLogic;

        private GetClientMachineCodeLogic m_GetClientMachineCodeLogic;

        public MainWindow()
        {
            InitializeComponent();

            //m_CheckLogic = new CheckDogLogic(this, (string message) => { Message.ShowMessage(message); });
            //m_CheckLogic.Start();

            LoadAllConfig();

            //try
            //{
            //    this.Cursor = Utils.CreateBmpCursor(m_GameCenterConfig.CursorPath);
            //}
            //catch (Exception e)
            //{
            //    Log.Error(e.Message);
            //}

            if (m_GameCenterConfig.SetFullScreen == 1)
            {
                SetFullScreen();
            }

            try
            {
                m_Server = new QServer(m_ServerConfig);
                var ip = Utils.GetLocalIP();

                //获取不到本地连接以及无线网络连接  则使用配置ip
                if (string.IsNullOrEmpty(ip))
                {
                    Message.ShowMessage("请先配置网络连接问题 .");
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

            var clientdatapath = "Configs/ClientData.xml";
            m_AutoConfigureClientsIP = new AutoConfClientID(m_Server, m_ClientData, clientdatapath);

            //开始游戏逻辑
            var startbuttons = new Button[2];
            startbuttons[0] = this.StartGameBtn;
            startbuttons[1] = this.CloseGameBtn;

            var startlabes = new Label[3];
            startlabes[0] = this.CheckLabel01;
            startlabes[1] = this.CheckLabel02;
            startlabes[2] = this.CheckLabel03;

            var checkboxs = new CheckBox[3];
            checkboxs[0] = this.checkBox1;
            checkboxs[1] = this.checkBox2;
            checkboxs[2] = this.checkBox3;

            var startedShowImages = new System.Windows.Controls.Image[3];
            startedShowImages[0] = this.StartShow01;
            startedShowImages[1] = this.StartShow02;
            startedShowImages[2] = this.StartShow03;

            m_CheckCoinLogic = new CheckCoinLogic(m_GameCenterConfig, m_GameData, checkboxs, this, (string message) => { Message.ShowMessage(message); });

            m_StartGameLogic = new StartGameLogic(m_Server, m_ClientData, m_GameCenterConfig, m_GameData, startbuttons, startlabes, null, checkboxs, this,
                (string message) => { Message.ShowMessage(message); },
                () => { CoinOperation.ShowPanel(this.ParentCanvas, m_CheckCoinLogic); });
            m_StartGameLogic.SetShatedShowImages(startedShowImages);
            m_CheckCoinLogic.OnAfterCoinSuccess += m_StartGameLogic.OnStartGameAfterCoin;

            m_StartGameLogic.SetIsUseCoin(false);
            m_CheckCoinLogic.SetIsUseCoin(false);
            //成功开始游戏的回调
            m_StartGameLogic.OnIsStartOrCloseGame += (isStartGame) => {
                this.Dispatcher.Invoke(() => {
                    StartGameBtn.IsEnabled = !isStartGame;
                });
            };
            
            //开始设置逻辑
            m_SettingLogic = new SettingLogic(m_Server, m_ClientData, m_GameData, m_GameCenterConfig, this, (string message) => { Message.ShowMessage(message); });
            m_GetClientMachineCodeLogic = new GetClientMachineCodeLogic(m_Server, m_ClientData);
            m_SettingLogic.GetClientMachineCodeLogic = m_GetClientMachineCodeLogic;
            m_SettingLogic.OnSportCoin += (isUseCoin) => {
                m_CheckCoinLogic.SetIsUseCoin(isUseCoin);
                m_StartGameLogic.SetIsUseCoin(isUseCoin);
            };

            m_CheckCoinLogic.SetCurIndex(0);
            m_StartGameLogic.SetCurGameIndex(0);
        }


        /// <summary>
        /// 加载所有的配置文件
        /// </summary>
        private void LoadAllConfig()
        {
            m_ServerConfig = QServerConfig.LoadData("Configs/ServerConfig.xml");
            if (m_ServerConfig == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load ServerConfig.xml");
                Message.ShowMessage("程序无法加载服务端的配置文件，无法启动");
                this.Close();
                return;
            }

            m_ClientData = ClientData.LoadData("Configs/ClientData.xml");
            if (m_ClientData == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load ClientData.xml");
                Message.ShowMessage("程序无法加载客户端的配置文件，无法启动");
                this.Close();
                return;
            }

            m_GameData = GameData.LoadData("Configs/GameData.xml");
            if (m_GameData == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load GameData.xml");
                Message.ShowMessage("程序无法加载游戏配置文件，无法启动");
                this.Close();
                return;
            }

            m_GameCenterConfig = GameCenterConfig.LoadData("Configs/GameCenterConfig.xml");
            if (m_GameCenterConfig == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load GameCenterData.xml");
                Message.ShowMessage("程序无法加载中控文件，无法启动");
                this.Close();
                return;
            }
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

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
        }

        private void OnWindowClose(object sender, EventArgs e)
        {
            if (m_CheckCoinLogic != null)
            {
                m_CheckCoinLogic.Clear();
                m_CheckCoinLogic = null;
            }
            if (m_GetClientMachineCodeLogic != null)
            {
                m_GetClientMachineCodeLogic = null;
            }
            if (m_AutoConfigureClientsIP != null)
            {
                m_AutoConfigureClientsIP = null;
            }
            if (m_SettingLogic != null)
            {
                m_SettingLogic = null;
            }
            if (m_StartGameLogic != null)
            {
                m_StartGameLogic.Clear();
                m_StartGameLogic = null;
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

            if (m_GameCenterConfig != null)
            {
                m_GameCenterConfig = null;
            }

            if (m_GameData != null)
            {
                m_GameData = null;
            }

            if (m_ClientData != null)
            {
                m_ClientData = null;
            }

            if (m_ServerConfig != null)
            {
                m_ServerConfig = null;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            SettingPanel.ShowSettingPanel(this.ParentCanvas, m_SettingLogic, m_StartGameLogic);
        }
    }
}
