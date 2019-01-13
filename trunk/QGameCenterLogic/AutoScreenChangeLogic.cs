using System;
using System.Windows;
using System.Windows.Controls;
using QConnection;
using System.Threading;
using QData;

namespace QGameCenterLogic
{
    /// <summary>
    /// 定义了自动切换同屏的逻辑
    /// </summary>
    public class AutoScreenChangeLogic 
    {

        private Window m_Window;
        private QServer m_Server;
        private ClientData m_ClientData;
        private QServerConfig m_ServerConfig;
        private GameCenterConfig m_GameCenterConfig;

        private Image m_Image;

        private QScreenClient m_ScreenClient;

        private int m_ClientCount = 0;

        private bool m_IsRunning = false;
        private Thread m_AutoUpdateThread;

        private int m_CurClientIndex = -1;

        public AutoScreenChangeLogic(Image image,int length, ClientData clientdata, QServer server, QServerConfig serverconfig,GameCenterConfig config, Window window) 
        {
            m_Image = image;
            m_ClientCount = length;
            m_ClientData = clientdata;
            m_Server = server;
            m_ServerConfig = serverconfig;
            m_Window = window;
            m_ScreenClient = new QScreenClient();
            m_ScreenClient.OnScreenChange += OnScreenChange;
            
            m_GameCenterConfig = config;
        }

        private bool m_IsStartGame = false;
        private Image[] m_ShowIsStartGameInfos;
      
        public void SetCheckIsStartGameIamge(Image[] images)
        {
            m_ShowIsStartGameInfos = images;
        }
        public void SetStartGame(bool IsStartGame)
        {
            m_IsStartGame = IsStartGame;
        }
        private void AutoUpdate()
        {
            while (m_IsRunning)
            {
                m_CurClientIndex++;
                m_CurClientIndex = (m_CurClientIndex) % m_ClientCount;

                var are = new AutoResetEvent(false);
                if (m_IsStartGame && m_ShowIsStartGameInfos != null)
                {
                    m_Window.Dispatcher.Invoke(() =>
                    {
                        are.Set();
                        if (m_ShowIsStartGameInfos[m_CurClientIndex].Visibility == Visibility.Visible)
                        {
                        }
                        else 
                        {
                            m_CurClientIndex++;
                            m_CurClientIndex = (m_CurClientIndex) % m_ClientCount;
                            if(m_ShowIsStartGameInfos[m_CurClientIndex].Visibility == Visibility.Visible)
                            {
                            }
                            else
                            {
                                m_CurClientIndex++;
                                m_CurClientIndex = (m_CurClientIndex) % m_ClientCount;
                            }
                        }
                    });
                }

                are.WaitOne(10);
                are.Close();
                are.Dispose();
                are = null;

                try
                {
                    //获取IP 从配置文件获取
                    var id = m_CurClientIndex + 1;
                    if(id > m_ClientCount)
                    {
                        id = m_ClientCount;
                    }
                    var ip = m_ClientData.GetClient(id);

                    if (ip == null)
                    {
                        continue;
                    }

                    if (m_Server.IsConnect(ip))
                    {
                        m_ScreenClient.Start(ip.ToString(), m_ServerConfig.ScreenPort);
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("[ScreenLogic] ScreenChange Error : " + ex.ToString());
                    continue;
                }

                Thread.Sleep(1000 * m_GameCenterConfig.ChangeScreenTime);

            }

        }
        private void StartScreenClient()
        {
            try
            {
                //获取IP 从配置文件获取
                var id = m_CurClientIndex + 1;
                var ip = m_ClientData.GetClient(id);

                if (ip == null)
                {
                    return;
                }

                if (m_Server.IsConnect(ip))
                {
                    m_ScreenClient.Start(ip.ToString(), m_ServerConfig.ScreenPort);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error("[ScreenLogic] ScreenChange Error : " + ex.ToString());
                return;
            }
        }

        //System.Drawing.Bitmap bitmap
        private void OnScreenChange(System.Drawing.Bitmap bit)
        {
            m_Window.Dispatcher.Invoke(() =>
            {
                try
                {
                    m_Image.Source = Utils.ChangeBitmapToImageSource(bit);
                }
                catch (Exception e)
                {
                    Log.Error("[ScreenSyncLogic] ChangeBitmapToImageSource Error : " + e.Message);
                }

            });
        }

        public void Start()
        {
            Stop();
            m_IsRunning = true;
            m_AutoUpdateThread = new Thread(AutoUpdate);
            m_AutoUpdateThread.Start();
        }

        public void Stop()
        {
            m_IsRunning = false;
            if (m_AutoUpdateThread != null)
            {
                m_AutoUpdateThread = null;
            }

        }


    }
}
