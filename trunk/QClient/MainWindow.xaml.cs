using QConnection;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace QClientNS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MyNotifyIcon m_Notify;

        private QClient m_QClient;

        private QClientConfig m_Config;

        private IPBroadcastReceiver m_BroadRece;

        private QScreenServer m_ScreenServer;

        private QNetInfoServer m_QNetInfoServer;

        public MainWindow()
        {
            InitializeComponent();

            Log.SetLogToFile();

            EnvConfig.SetEnv();

            if (Directory.Exists(Path.GetFullPath(@"..\Temp\")))
            {
                Directory.Delete(Path.GetFullPath(@"..\Temp\"), true);
            }
         
            m_Notify = new MyNotifyIcon(this);

            m_Config = QClientConfig.LoadData("Configs/ClientConfig.xml");
            if (m_Config == null)
            {
                Log.Error("[QClient] MainWindow Error : config == null");
                return; ;
            }
            try
            {
                m_BroadRece = new IPBroadcastReceiver();
                m_BroadRece.StartGetServerIP(OnGetServerIP, m_Config.Port);

                m_QClient = new QClient(m_Config);
                m_QClient.OnClientConnected += OnClientConnected;
                m_QClient.OnClientDisconnected += OnClientDisconnected;                
                m_QClient.OnShutDownApp = OnShutDownApp;
                
                m_ScreenServer = new QScreenServer();
                m_ScreenServer.Start(m_Config.ScreenPort, m_Config.Frequency);

                m_QClient.OnGetStartGameName += (startgamename) =>
                {
                    m_ScreenServer.ScreenGame = startgamename;
                };

                m_QNetInfoServer = new QNetInfoServer();
                m_QNetInfoServer.CurrentHandle = (new WindowInteropHelper(this)).Handle;
                m_QClient.OnToggleSendInfo = (isStart) =>
                {
                    m_QNetInfoServer.Start(9019, 40);
                };

                this.Dispatcher.Invoke(() =>
                {
                    this.LocalIP.Text = Utils.GetLocalIP();
                    this.MachineCodeTxt.Text = m_QClient.MachineCode;
                });
            }
            catch (Exception e)
            {
                Log.Error("[QClient] MainWindow Error : " + e.Message);
            }

            this.Closing += MainWindow_Closing;

            MainIcon.MouseDown += OnIconMouseDown;

            this.Left = m_Config.WindowLocationLeft;
            this.Top = m_Config.WindowLocationTop;
            this.ShowInTaskbar = false;
        }

        private void OnShutDownApp()
        {
            if(this.m_Notify != null)
            {
                m_Notify.Dispose();
                m_Notify = null;
            }
            this.Dispatcher.Invoke((ThreadStart)delegate ()
             {
                 Application.Current.Shutdown();
             });
        }

        private int m_Count = 0;
        private void OnIconMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (m_Count >= 2)
            {
                m_Count = 0;
                this.Visibility = Visibility.Hidden;
            }
            m_Count++;

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                e.Cancel = true;
                this.Hide();
                this.WindowState = WindowState.Minimized;
            });
        }

        private void OnGetServerIP(string serverIP)
        {
            try
            {
                m_BroadRece.Stop();

                m_QClient.Start(new IPEndPoint(IPAddress.Parse(serverIP), m_Config.Port));

                this.Dispatcher.Invoke(() =>
                {
                    this.RemoteIP.Text = serverIP;
                    this.ConnectionStat.Text = "连接中";
                });

                //ToDo:显示文件上传还是解压的状态

                //m_QClient.OnUnZiped += () =>
                //{                   
                //    this.Dispatcher.Invoke(() =>
                //    {
                //        this.RemoteIP.Text = serverIP;
                //        this.ConnectionStat.Text = "连接状态";
                //    });
                //};
            }
            catch (Exception e)
            {
                Log.Error("[Client] GetServerIP Error : " + e.Message);
            }
        }

        private void OnClientConnected()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.ConnectionStat.Text = "已连接";
            });
        }
        /// <summary>
        /// 失去连接 尝试重连
        /// </summary>
        private void OnClientDisconnected()
        {            
            Thread.Sleep(1000);
            m_BroadRece.StartGetServerIP(OnGetServerIP, m_Config.Port);
            this.Dispatcher.Invoke(() =>
            {
                this.ConnectionStat.Text = "搜索中控状态";
                this.RemoteIP.Text = "";
            });
            m_ScreenServer.ScreenGame = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (m_Notify != null)
            {
                m_Notify.Dispose();
                m_Notify = null;
            }
            
            if (m_QClient != null)
            {
                m_QClient.Stop();
                m_QClient = null;
            }
            else
            {
                Log.Error("[QClient] OnClose Error : m_QClient == null.");
            }

            if (m_BroadRece != null)
            {
                m_BroadRece.Stop();
                m_BroadRece = null;
            }
            else
            {
                Log.Error("[QClient] OnClose Error : m_BroadRece == null.");
            }

            if (m_ScreenServer != null)
            {
                m_ScreenServer.Stop();
                m_ScreenServer = null;
            }
            else
            {
                Log.Error("[QClient] OnClose Error : m_ScreenServer == null.");
            }
        }
    }
}
