using QData;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QGameCenterLogic
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameData m_GameData;
        private VRKartLogic m_VRKartLogic;
        private CheckProgramLogic m_CheckProgramLogic;
        private TopWinLogic m_TopWinLogic;
        private GameCenterConfig m_GameCenterConfig;

        private Window m_TopWindow;
        

        private void OnWindowClosed(object sender, EventArgs e)
        {
           
            if(m_GameCenterConfig != null)
            {
                m_GameCenterConfig = null;
            }
            if (m_GameData != null)
            {
                m_GameData = null;
            }
            if (m_VRKartLogic != null)
            {
                m_VRKartLogic.Clear();
                m_VRKartLogic = null;
            }
            if (m_CheckProgramLogic != null)
            {
                m_CheckProgramLogic.Stop();
                m_CheckProgramLogic.Clear();
            }
            if (m_TopWinLogic != null)
            {
                m_TopWinLogic.Stop();
                m_TopWinLogic.Clear();
            }
            if (m_TopWindow != null)
            {
                m_TopWindow.Close();
            }
            this.Close();
            Environment.Exit(0);
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Hide();

            this.Hide();
            if (!LoginWindow.Show(this))
            {
                Environment.Exit(0);
            }

            m_GameCenterConfig = GameCenterConfig.LoadData("Configs/GameCenterConfig.xml");
            if (m_GameCenterConfig == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load GameCenterData.xml");
                MessagePanel.ShowMessage("程序无法加载中控文件，无法启动");
                this.Close();
                return;
            }

            m_GameData = GameData.LoadData("Configs/GameData.xml");
            if (m_GameData == null)
            {
                Log.Error("[QGameCenter] MainWindow Can't Load GameData.xml");
                // Message.ShowMessage("程序无法加载游戏配置文件，无法启动");
                MessagePanel.ShowMessage("程序无法加载游戏配置文件，无法启动");
                this.Close();
                return;
            }

          



            var buttons = new Button[4];
            buttons[0] = this.Previous;
            buttons[1] = this.Next;
            buttons[2] = this.JoinRoom;
            buttons[3] = this.CreateRoom;

            var buttonSelecteds = new Button[4];
            buttonSelecteds[0] = this.CarButton1;
            buttonSelecteds[1] = this.CarButton2;
            buttonSelecteds[2] = this.CarButton3;
            buttonSelecteds[3] = this.CarButton4;

            var images = new Image[4];
            images[0] = this.Car1;
            images[1] = this.Car2;
            images[2] = this.Car3;
            images[3] = this.Car4;

            m_TopWinLogic = new TopWinLogic();

            m_TopWindow = new TopMostWin(m_TopWinLogic);
            m_TopWindow.Visibility = Visibility.Hidden;
            

            m_VRKartLogic = new VRKartLogic(m_GameData, m_GameCenterConfig, buttons, buttonSelecteds,adverts, images);


            m_CheckProgramLogic = new CheckProgramLogic();
            m_CheckProgramLogic.OnExistProgram += () => {
                Dispatcher.Invoke(() => {
                    m_TopWindow.Visibility = Visibility.Visible;
                    this.Visibility = Visibility.Hidden;
                });
            };
            m_CheckProgramLogic.OnNOExistProgram += () => {
                Dispatcher.Invoke(()=> {
                    m_TopWindow.Visibility = Visibility.Hidden;
                    this.Visibility = Visibility.Visible;
                });
              
                m_TopWinLogic.Stop();
                m_CheckProgramLogic.Stop();
            };

            m_VRKartLogic.OnStartGameScuess += (programName,gamePath) => {
                m_CheckProgramLogic.Start(programName);

                m_TopWinLogic.Start(gamePath);
            };

            m_TopWinLogic.StopGame += (isStartgame) => {
                m_VRKartLogic.IsStartGame = isStartgame;
            };

            if (m_GameCenterConfig.SetFullScreen == 1)
            {
                SetFullScreen();
            }
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

        private void OnLoadWindow(object sender, RoutedEventArgs e)
        {
        }

        private void OnExitButtonClick(object sender, RoutedEventArgs e)
        {
            InputPassworldPanel.ShowKeyBorde(this.ParentCanvas, () => {
                this.Close();
            });
                
        }
    }
}
