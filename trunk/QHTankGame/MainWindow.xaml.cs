using QConnection;
using QData;
using QGameCenter;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace QGameCenterLogic
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientData m_ClientData;
        private GameData m_GameData;
        private QServerConfig m_ServerConfig;
        private GameCenterConfig m_GameCenterConfig;


        private QServer m_Server;
        private IPBroadcaster m_Broadcaster;

        private StartGameLogic m_StartGameLogic;
        private UseButtonChoseGameLogic m_ChoseGameLogic;
        private ScreenSyncLogic m_ScreenSync;
        private SettingLogic m_SettingLogic;
        private AutoConfClientID m_AutoConfigureClientsIP;
        private CheckCoinLogic m_CheckCoinLogic;


        private ClientMachineLogic m_ClientMachineLogic;

        public MainWindow()
        {
            InitializeComponent();

            Log.SetLogToFile();

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

            var clientdatapath = "Configs/ClientData.xml";
            m_AutoConfigureClientsIP = new AutoConfClientID(m_Server, m_ClientData, clientdatapath);

            if (m_GameCenterConfig.SetFullScreen == 1)
            {
                SetFullScreen();
            }

            //开始游戏逻辑
            var startbuttons = new Button[2];
            startbuttons[0] = this.StartGame;
            startbuttons[1] = this.CloseGame;
            var startlabes = new Label[4];
            startlabes[0] = this.CheckLabel01;
            startlabes[1] = this.CheckLabel02;
            startlabes[2] = this.CheckLabel03;
            startlabes[3] = this.CheckLabel04;
            var images = new Image[4];
            images[0] = this.GameImage01;
            images[1] = this.GameImage02;
            images[2] = this.GameImage03;
            images[3] = this.GameImage04;
            var checkboxs = new CheckBox[4];
            checkboxs[0] = this.checkBox1;
            checkboxs[1] = this.checkBox2;
            checkboxs[2] = this.checkBox3;
            checkboxs[3] = this.checkBox4;

            var showStartInfoImages = new Image[4];
            showStartInfoImages[0] = showStartInfo1;
            showStartInfoImages[1] = showStartInfo2;
            showStartInfoImages[2] = showStartInfo3;
            showStartInfoImages[3] = showStartInfo4;
            //检测是否需要投币操作
            m_CheckCoinLogic = new CheckCoinLogic(m_GameCenterConfig,m_GameData, checkboxs, this, (string message) => { Message.ShowMessage(message); });

            //m_StartGameLogic = new StartGameLogic(m_Server, m_ClientData, startbuttons, startlabes, images, checkboxs, this, (string message) => { Message.ShowMessage(message); });
            m_StartGameLogic = new StartGameLogic(m_Server, m_ClientData, m_GameCenterConfig, m_GameData, startbuttons, startlabes, images, checkboxs, this, (string message) => { Message.ShowMessage(message); }, () => { CoinOperation.ShowPanel(this.ParentCanvas, m_CheckCoinLogic); });

            //成功开始游戏的回调
            m_StartGameLogic.OnIsStartOrCloseGame += (isStartGame) => {
                Dispatcher.Invoke(() => {
                    try
                    {
                        ChosePreviousGame.IsEnabled = !isStartGame;
                        ChoseNextGame.IsEnabled = !isStartGame;
                        StartGame.IsEnabled = !isStartGame;
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error : " + e.ToString());
                    }

                });
            };

            m_StartGameLogic.SetShatedShowImages(showStartInfoImages);


            m_CheckCoinLogic.OnAfterCoinSuccess += m_StartGameLogic.OnStartGameAfterCoin;


            m_SettingLogic = new SettingLogic(m_Server, m_ClientData, m_GameData,m_GameCenterConfig, this, (string message) => { Message.ShowMessage(message); });

            var machineButtons = new Button[3];
            machineButtons[0] = ResetMachine;
            machineButtons[1] = ResetHeadset;
            machineButtons[2] = CheckMachineState;
            m_StartGameLogic.SetMachineButtonClick(machineButtons);

            //开始同屏逻辑
            m_ScreenSync = new ScreenSyncLogic(this.GameTabControl, images, m_ClientData, m_Server, m_ServerConfig, this);
            //开始设置逻辑

            //开始选择游戏逻辑
            var chosebuttons = new Button[2];
            chosebuttons[0] = this.ChosePreviousGame;
            chosebuttons[1] = this.ChoseNextGame;
            var choselabels = new Label[2];
            choselabels[0] = this.ChoseGameNameLabel;
            choselabels[1] = this.ChoseGameIntruLabel;
            m_ChoseGameLogic = new UseButtonChoseGameLogic(m_Server, m_GameData, chosebuttons, this.ChoseGameBG, choselabels, this, (string message) => { Message.ShowMessage(message); },(index) => {
                m_CheckCoinLogic.SetCurIndex(index);
                m_StartGameLogic.SetCurGameIndex(index);
            } );

            //loginPanel.ShowInputPanel(this.ParentCanvas);
            loginPanel.ShowInputPanel(this.ParentCanvas, (usecoin) => {
                m_CheckCoinLogic.SetIsUseCoin(usecoin);
                m_StartGameLogic.SetIsUseCoin(usecoin);
            });

            for (int index = 0; index < checkboxs.Length; index++)
            {
                var check = checkboxs[index];
                check.IsChecked = false;
                check.IsEnabled = false;
            }

            m_Server.OnClientConnected += (ip) =>
            {
                var id = m_ClientData.GetClient(ip);
                var index = id - 1;
                if (index >= 0 && index < checkboxs.Length)
                {
                    var check = checkboxs[id];
                    check.IsEnabled = true;
                }
                else
                {
                    //ToDo:报错
                }

            };

            m_Server.OnClientDisconnected += (ip) =>
            {
                var id = m_ClientData.GetClient(ip);
                var index = id - 1;
                if (index >= 0 && index < checkboxs.Length)
                {
                    var check = checkboxs[index];
                    check.IsChecked = false;
                    check.IsEnabled = false;
                }
                else
                {
                    //ToDo:报错
                }
            };
        }

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

            if ( m_AutoConfigureClientsIP != null )
            {
                m_AutoConfigureClientsIP = null;
            }
            if (m_ScreenSync != null)
            {
                m_ScreenSync.Stop();
                m_ScreenSync = null;
            }
            if (m_StartGameLogic != null)
            {
                m_StartGameLogic.Clear();
                m_StartGameLogic = null;
            }

            if (m_ChoseGameLogic != null)
            {
                m_ChoseGameLogic = null;
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

        private void OnSettingButtonClick(object sender, RoutedEventArgs e)
        {
            InputPasswordPanel.ShowKeyBorde(this.ParentCanvas, () =>
            {
                SettingPanel.ShowSettingPanel(this.ParentCanvas, m_SettingLogic);
            });
        }
    }
}
