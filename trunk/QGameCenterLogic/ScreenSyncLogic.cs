using QConnection;
using System;
using System.Windows.Controls;
using QData;
using System.Windows.Threading;
using System.Windows;
using System.Net;
using System.Threading;

namespace QGameCenterLogic
{
    /// <summary>
    /// 定义了同屏幕相关的逻辑
    /// 使用Tab组件来切换图片显示同屏幕收到的数据
    /// </summary>
    public class ScreenSyncLogic
    {
        private Window m_Window;
        private QServer m_Server;
        private ClientData m_ClientData;
        private QServerConfig m_ServerConfig;

        /// <summary>
        /// 保存了用于显示图像的组件
        /// </summary>
        private Image[] m_Images;
        /// <summary>
        /// 用于控制显示同屏幕画面
        /// </summary>
        private QScreenClient m_ScreenClient;

        private TabControl m_TabControl;

        private int m_ClientID= -1;

        private string m_ScreenName;

        public ScreenSyncLogic(TabControl tabcontrol, Image[] images,ClientData clientdata,  QServer server, QServerConfig serverconfig, Window window)
        {          
            m_Images = images;
            m_ClientData = clientdata;

            m_Server = server;

            m_ServerConfig = serverconfig;
            m_Window = window;

            m_TabControl = tabcontrol;
            m_TabControl.SelectionChanged += TabControl_SelectionChanged;
            m_ScreenClient = new QScreenClient();

            m_ClientID = 1;
            m_ScreenClient.OnScreenChange += OnScreenChange;
        }
        
        public void Stop()
        {
            if (m_ScreenClient != null)
            {
                m_ScreenClient = null;
            }
            else
            {
                Log.Error("[ScreenSyncLogic] Stop Error : m_ScreenClient == null.");
            }
        }

        public void GetScreenName(string screenName)
        {
            m_ScreenName = screenName;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = m_TabControl.SelectedIndex;
            try
            {
                
                m_ClientID = id + 1;
                var ip = m_ClientData.GetClient(m_ClientID );
               
                if (ip == null)
                {
                    
                    if (m_ScreenClient != null)
                    {
                        m_ScreenClient.Stop();
                    }
                    return;
                }
                if (m_Server.IsConnect(ip))
                {
                    if (m_ScreenClient != null)
                    {
                        m_ScreenClient.Stop();
                    }
                    //Log.Debug("  id : " + (id + 1) + "    " + ip);
                    m_ScreenClient.Start(ip.ToString(), m_ServerConfig.ScreenPort);

                }
            }
            catch (Exception ex)
            {
                Log.Error("[ScreenSyncLogic] ScreenChange Error : " + ex);
            }
        }

        private void OnScreenChange(System.Drawing.Bitmap bit)
        {
            var index = m_ClientID - 1;
            if (index < 0 || index >= m_Images.Length)
            {
                Log.Error("[ScreenSyncLogic] OnScreenChange Error : Invalid index : " + index);
                return;
            }

            var image = m_Images[index];
            if (image == null)
            {
                Log.Error("[ScreenSyncLogic] OnScreenChange Error : Image Is Null, Index=" + index);
                return;
            }

            m_Window.Dispatcher.Invoke(() => {
                try
                {
                    image.Source = Utils.ChangeBitmapToImageSource(bit);
                }
                catch(Exception e)
                {
                    Log.Error("[ScreenSyncLogic] ChangeBitmapToImageSource Error : " + e.Message);
                }         
               
            });
        }

    }

}
