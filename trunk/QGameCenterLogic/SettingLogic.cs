using System.Net;
using QProtocols;
using System;
using System.Windows;
using QData;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using QConnection;

namespace QGameCenterLogic
{
    /// <summary>
    /// 通过密码框进入之后的设置面板
    /// </summary>
    public class SettingLogic
    {
        private QServer m_Server;
        private ClientData m_ClientData;
        private GameData m_GameData;
        private Window m_Window;
        private Action<string> m_PopMessagePanel;

        private Label[] m_Labels;
        private Button[] m_Buttons;
        private Action m_ReturnButtonAction;
        private CheckBox[] m_CheckBox;

        private GameCenterConfig m_GameCenterData;

        private bool m_IsBinded = false;

        private List<string> m_LabelContents_Copy = new List<string>();
        private List<int> m_Index_Cpoy = new List<int>();
        public GetClientMachineCodeLogic GetClientMachineCodeLogic;

        public Action<bool> OnSportCoin;
        private CheckBox m_SportCheckBox;
        public SettingLogic(QServer server,ClientData clientdata,GameData gamedata, GameCenterConfig data,Window window,Action<string> popmessage)
        {
            m_Server = server;
            m_Server.OnClientConnected += OnClientConnected;
            m_Server.OnClientDisconnected += OnClientDisconnected;

            m_ClientData = clientdata;

            m_GameCenterData = data;

            m_GameData = gamedata;

            m_Window = window;

            m_PopMessagePanel = popmessage;

            for(var i = 0;i < 4;i++)
            {
                m_LabelContents_Copy.Add("客户端0" + (i +1) + "掉线");
            }
        }

        private void OnClientDisconnected(IPAddress ip)
        {
            if( m_IsBinded )
            {
                var id = m_ClientData.GetClient(ip);
                m_Window.Dispatcher.Invoke(()=> {
                    m_Labels[id - 1 ].Content = "客户端0" + (id) + "掉线";
                    m_LabelContents_Copy[id - 1] = "客户端0" + (id) + "掉线";
                    m_Labels[id - 1 ].Foreground = new SolidColorBrush(Color.FromRgb(147, 156, 156));
                    m_CheckBox[id - 1 ].IsChecked = false;
                    m_CheckBox[id - 1].IsEnabled = false;
                });
            }
        }

        private void OnClientConnected(IPAddress ip)
        {
            var id = m_ClientData.GetClient(ip);

            m_LabelContents_Copy[id - 1] = "客户端0" + (id) + "连接";
            if (m_IsBinded)
            {

                m_Window.Dispatcher.Invoke(() => {
                    m_Labels[id - 1 ].Content = "客户端0" + (id) + "连接";
                    
                    m_Labels[id - 1 ].Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                    m_CheckBox[id - 1].IsEnabled = true;
                    m_CheckBox[id - 1].IsChecked = true;

                });
            }
        }
        
        public void BindingUI(Label[] labels,Button[] buttons,CheckBox[] checkboxs,Image iconimage,Action returnbutton,CheckBox sportCoin = null)
        {
            m_Labels = labels;
           
            m_Labels[0].MouseEnter += (sender, e) =>
            {
                if(GetClientMachineCodeLogic != null)
                {
                    var str = string.Empty;
                    GetClientMachineCodeLogic.GetMachineString(0, out str);
                    m_Labels[0].Content = str;
                }
                
            };
            m_Labels[0].MouseLeave += (sender, e) =>
            {
                if (GetClientMachineCodeLogic != null)
                {
                    m_Labels[0].Content = m_LabelContents_Copy[0];

                }
            };

           
            m_Labels[1].MouseEnter += (sender, e) =>
            {
                if (GetClientMachineCodeLogic != null)
                {
                    var str = string.Empty;
                    GetClientMachineCodeLogic.GetMachineString(1, out str);
                    m_Labels[1].Content = str;
                }
               
            };
            m_Labels[1].MouseLeave += (sender, e) =>
            {
                if (GetClientMachineCodeLogic != null)
                {
                    m_Labels[1].Content = m_LabelContents_Copy[1];

                }
            };

            

            m_Labels[2].MouseEnter += (sender, e) =>
            {
                if (GetClientMachineCodeLogic != null)
                {
                    var str = string.Empty;
                    GetClientMachineCodeLogic.GetMachineString(2, out str);
                    m_Labels[2].Content = str;
                }
               
            };
            m_Labels[2].MouseLeave += (sender, e) =>
            {
                if (GetClientMachineCodeLogic != null)
                {
                    m_Labels[2].Content = m_LabelContents_Copy[2];
                }
            };

           if(m_Labels[3] != null)
            {
                m_Labels[3].MouseEnter += (sender, e) =>
                {
                    if (GetClientMachineCodeLogic != null)
                    {
                        var str = string.Empty;
                        GetClientMachineCodeLogic.GetMachineString(3, out str);
                        m_Labels[3].Content = str;
                    }
                   
                };
                m_Labels[3].MouseLeave += (sender, e) =>
                {
                    if (GetClientMachineCodeLogic != null)
                    {
                        m_Labels[3].Content = m_LabelContents_Copy[3];
                    }
                };
            }
           

            m_Buttons = buttons;

            m_Buttons = buttons;
            m_Buttons[0].Click += CloseGameCenterButton;
            m_Buttons[1].Click += CloseClientsButton;
            m_Buttons[2].Click += ResetGameCenterButton;
            m_Buttons[3].Click += ResetClientsButton;
            m_Buttons[4].Click += ReturnButton;
            m_Buttons[5].Click += ResetSteamVR;

            m_CheckBox = checkboxs;

            if(sportCoin != null)
            {
                m_SportCheckBox = sportCoin;
                if (sportCoin != null)
                {
                    m_SportCheckBox.Click += (sender, e) => {
                        OnSportCoinClick();
                    };
                }
            }
           

            m_ReturnButtonAction = returnbutton;

            if(iconimage != null)
            {
                if(!File.Exists(Path.GetFullPath(m_GameCenterData.IconPath)))
                {
                    m_PopMessagePanel("不存在" + m_GameCenterData.IconPath + "的图标图片");
                    return;
                }
                try
                {
                    m_Window.Dispatcher.Invoke(() => {
                        var imageURI = new BitmapImage();
                        imageURI.BeginInit();
                        imageURI.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + m_GameCenterData.IconPath, UriKind.RelativeOrAbsolute);
                        imageURI.EndInit();
                        iconimage.Source = imageURI;
                    });

                }
                catch (Exception e)
                {
                    Log.Error("[SettingLogic] BindingUI Error : " + e.Message);
                }
            }

            m_IsBinded = true;

            //遍历上线的Client
            foreach(ClientInfo info in m_ClientData.ClientInfos)
            {
                var id = info.ID;
                if(string.IsNullOrEmpty(info.IP))
                {
                    continue;
                }
                if(!m_Server.IsConnect(IPAddress.Parse(info.IP)))
                {
                    continue;
                }
                m_Window.Dispatcher.Invoke(() => {
                    m_Labels[id -1].Content = "客户端0" + (id ) + "连接";
                    m_Labels[id -1].Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));
                    m_CheckBox[id -1].IsEnabled = true;
                    m_CheckBox[id -1].IsChecked = true;
                });
            }

        }
        public void UninstallUI()
        {
            m_Labels = null;
            //卸载Button的点击事件
            m_Buttons[0].Click -= CloseGameCenterButton;
            m_Buttons[1].Click -= CloseClientsButton;
            m_Buttons[2].Click -= ResetGameCenterButton;
            m_Buttons[3].Click -= ResetClientsButton;
            m_Buttons[4].Click -= ReturnButton;
            m_Buttons[5].Click -= ResetSteamVR;
            m_ReturnButtonAction = null;
            m_CheckBox = null;
            m_IsBinded = false;
        }

        private void OnSportCoinClick()
        {
            try
            {
                Log.Debug(" (bool)m_SportCheckBox.IsChecked ； " + (bool)m_SportCheckBox.IsChecked);
                if (OnSportCoin != null)
                {
                    OnSportCoin((bool)m_SportCheckBox.IsChecked);
                }
            }
            catch(Exception e)
            {
                Log.Error("[SettingLogic] OnSportCoinClick Error : " + e.ToString());
            }
        }


        private void ResetSteamVR(object sender, RoutedEventArgs e)
        {

        }
       
        private void ReturnButton(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_ReturnButtonAction != null)
                {
                    m_ReturnButtonAction();
                }
                UninstallUI();
            }
            catch (Exception ex)
            {
                Log.Error("[SettingLogic] ReturnButton : " + ex.Message);
            }

           
        }
        
        private void ResetClientsButton(object sender, RoutedEventArgs e)
        {
            try
            {
                int count = 0;
                for (int i = 0; i < m_CheckBox.Length; i++)
                {
                    if (m_CheckBox[i].IsChecked == false)
                    {
                        count++;
                        continue;
                    }
                    IPAddress ip = m_ClientData.GetClient(i + 1);
                    if (m_Server.IsConnect(ip))
                    {
                        m_Server.ComputerOP(ip,ComputerOp.Restart,(Code code)=> {
                            if (code == Code.Failed)
                            {
                                m_PopMessagePanel("重启客户端电脑错误，错误原因是:" + code);
                                Log.Error("[SettingLogic] CloseClientsButton Error : current ip :" + ip + " , client id:" + (i + 1));
                            }
                        });
                    }
                    else
                    {
                        Log.Error("[SettingLogic] CloseClientsButton Error : current client id : " + (i + 1) + " ip :" + ip);
                        m_PopMessagePanel(" 客户端" + (i + 1) + "掉线，关闭客户端失败");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[SettingLogic] CloseClientsButton Error :" + ex.Message);
            }
        }
       
        private void ResetGameCenterButton(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = "Destory";
            Thread.Sleep(10);
            m_Window.Hide();
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Thread.Sleep(10);
            Application.Current.Shutdown();
        }

        private void CloseClientsButton(object sender, RoutedEventArgs e)
        {
            try
            {
                int count = 0;
                for (int i = 0; i <m_CheckBox.Length; i++)
                {
                    if (m_CheckBox[i].IsChecked == false)
                    {
                        count++;
                        continue;
                    }
                    IPAddress ip = m_ClientData.GetClient(i + 1);
                    if( m_Server.IsConnect(ip))
                    {
                        m_Server.ComputerOP(ip, ComputerOp.Shutdown, (Code code) =>
                        {
                            if (code == Code.Failed)
                            {
                                m_PopMessagePanel("关闭客户端电脑错误，错误原因是:" + code);
                                Log.Error("[SettingLogic] CloseClientsButton Error : current ip :" + ip + " , client id:" + (i + 1));
                            }
                        });
                    }
                    else
                    {
                        Log.Error("[SettingLogic] CloseClientsButton Error : current client id : "+(i+1)+" ip :"+ip);
                        m_PopMessagePanel(" 客户端"+(i+1)+"掉线，关闭客户端失败");
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Error("[SettingLogic] CloseClientsButton Error :"+ex.Message);
            }
           
        }
      
        private void CloseGameCenterButton(object sender, RoutedEventArgs e)
        {
            if( m_Window != null )
            {
                // TODO 输入密码进行验证关闭
                m_Window.Dispatcher.Invoke(() => {
                    Application.Current.Shutdown();
                });
            }
        }
        
        
    }
}
