using QData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Net;

namespace QGameCenterLogic
{
    /// <summary>
    /// 使用按钮来进行游戏的路径选择
    /// </summary>
    public class UseButtonChoseGameLogic
    {
        private QServer m_Server;
        private GameData m_GameData;
        private Image m_Image;
        private Label[] m_Labels;
        private Window m_Window;
        private Action<string> m_PopMessagePanel;
        //private Action<string,string> m_SetGamePath;
        //private Action<int> m_SetGameSinglePrice;

        private Action<int> m_SetCurIndex;
        public Action<string> GetGameName;

        private int m_GameIndex = 0;
        public UseButtonChoseGameLogic(QServer server,GameData gamedata,Button[] buttons,Image image,Label[] labels, Window window,Action<string> popmessage,Action<int> setcurdex)
        {
            m_Server = server;
            m_GameData = gamedata;

            buttons[0].Click += ChoseLeftButton;
            buttons[1].Click += ChoseRightButton;

            m_Image = image;

            m_Labels = labels;

            m_Window = window;

            m_PopMessagePanel = popmessage;

            //m_SetGamePath = setgamepath;

            //if( m_SetGamePath != null)
            //{
            //    m_SetGamePath(m_GameData.GameInfos[m_GameIndex].Path, m_GameData.GameInfos[m_GameIndex].GameConfigPath);
            //}

            //m_SetGameSinglePrice = setsigleprice;
            //if(m_SetGameSinglePrice != null)
            //{
            //    m_SetGameSinglePrice(m_GameData.GameInfos[m_GameIndex].SinglePrice);
            //}
            m_SetCurIndex = setcurdex;
            if(m_SetCurIndex != null)
            {
                m_SetCurIndex(m_GameIndex);
            }

            m_Window.Dispatcher.Invoke(() => {
                if(m_Labels != null)
                {
                    m_Labels[0].Content = m_GameData.GameInfos[m_GameIndex].Name;
                    m_Labels[1].Content = m_GameData.GameInfos[m_GameIndex].Detail;
                }
            });
            ChangeGameImage();
        }

    
        private void ChoseRightButton(object sender, RoutedEventArgs e)
        {
            try
            {
                ++m_GameIndex;
                if (m_GameIndex > m_GameData.GameInfos.Count -1  )
                {
                    m_GameIndex = m_GameData.GameInfos.Count -1;
                }
                //if(m_SetGamePath != null)
                //{
                //    m_SetGamePath(m_GameData.GameInfos[m_GameIndex].Path, m_GameData.GameInfos[m_GameIndex].GameConfigPath);
                //}
                //if (m_SetGameSinglePrice != null)
                //{
                //    m_SetGameSinglePrice(m_GameData.GameInfos[m_GameIndex].SinglePrice);
                //}
                m_SetCurIndex(m_GameIndex);
                GetGameName(m_GameData.GameInfos[m_GameIndex].Name);
                m_Window.Dispatcher.Invoke(()=> {
                    ChangeGameImage();
                });
            }
            catch( Exception ex )
            {
                m_PopMessagePanel(ex.Message+"  index :"+m_GameIndex);
            }
        }

        private void ChoseLeftButton(object sender, RoutedEventArgs e)
        {
            m_GameIndex--;
            if( m_GameIndex < 0 )
            {
                m_GameIndex = 0;
            }
            //if(m_SetGamePath != null)
            //{
            //    m_SetGamePath(m_GameData.GameInfos[m_GameIndex].Path, m_GameData.GameInfos[m_GameIndex].GameConfigPath);
            //}
            //if(m_SetGameSinglePrice != null)
            //{
            //    m_SetGameSinglePrice(m_GameData.GameInfos[m_GameIndex].SinglePrice);
            //}
            m_SetCurIndex(m_GameIndex);
            GetGameName(m_GameData.GameInfos[m_GameIndex].Name);

            m_Window.Dispatcher.Invoke(() => {
                ChangeGameImage();
            });
        }
        private void ChangeGameImage()
        {
            if(m_Labels != null)
            {
                m_Labels[0].Content = m_GameData.GameInfos[m_GameIndex].Name;
                m_Labels[1].Content = m_GameData.GameInfos[m_GameIndex].Detail;
            }
           
            try
            {
                if(m_Image != null)
                {
                    var imageURI = new BitmapImage();
                    imageURI.BeginInit();
                    imageURI.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + m_GameData.GameInfos[m_GameIndex].Icon, UriKind.RelativeOrAbsolute);
                    imageURI.EndInit();
                    m_Image.Source = imageURI;
                }
              
            }
            catch(Exception e)
            {
                Log.Error("[ChoseGameLogic] ChangeGameImage Error : "+e.Message);
            }
           
        }
    }
}
