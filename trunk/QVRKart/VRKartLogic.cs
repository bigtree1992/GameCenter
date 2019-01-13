using QData;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

namespace QGameCenterLogic
{
    class VRKartLogic
    {
        private Image m_AdvertsImg;
        private GameData m_GameData;
        private Image[] m_SelectedImages;


        public Action<string,string> OnStartGameScuess;

        private int m_CurrentIndex = -1;

        private int m_SlelectedCount = 1;

        private GameCenterDBEntities m_GameCenterDBEntities;
        private bool m_IsStartGame = false;
        private Thread m_InsertDBThread = null;

        private GameCenterConfig m_GameCenterConfig;

        public bool IsStartGame
        {
            get
            {
                return m_IsStartGame;
            }

            set
            {
                m_IsStartGame = value;
            }
        }

        public VRKartLogic(GameData gameData, GameCenterConfig gamecenterConfig, Button[] buttons,Button[] buttonsSelected, Image advertsImg, Image[] images)
        {
            m_GameData = gameData;
            m_GameCenterConfig = gamecenterConfig;

            buttons[0].Click += (sender, e) => { OnPreviousButtonClick(); };
            buttons[1].Click += (sender, e) => { OnNextButtonClick(); };
            buttons[2].Click += (sender, e) => { OnXmlOpAndStartGame(); };
            buttons[3].Click += (sender, e) => { OnXmlOpAndStartGame(); };

            buttonsSelected[0].Click += (sender, e) => { OnSelectedCount1(); };
            buttonsSelected[1].Click += (sender, e) => { OnSelectedCount2(); };
            buttonsSelected[2].Click += (sender, e) => { OnSelectedCount3(); };
            buttonsSelected[3].Click += (sender, e) => { OnSelectedCount4(); };

            m_AdvertsImg = advertsImg;


            m_CurrentIndex = 0;

            m_SelectedImages = images;


            if (!File.Exists(Path.GetFullPath("Data/GameCenterDB.db")))
            {
                MessagePanel.ShowMessage("程序无法加载游戏事件统计源，无法启动");

               
                return;
            }
            try
            {
                m_GameCenterDBEntities = new GameCenterDBEntities();
            }
            catch (Exception e)
            {
                MessagePanel.ShowMessage("加载事件统计数据源出现错误 : " + e.Message);
            }
            
        }


        private void OnPreviousButtonClick()
        {
            m_CurrentIndex--;

            if (m_CurrentIndex < 0)
            {
                m_CurrentIndex = 0;
            }
            ChangeImageSource();
        }
        private void OnNextButtonClick()
        {
            m_CurrentIndex++;
            if (m_CurrentIndex > m_GameData.GameInfos.Count - 1)
            {
                m_CurrentIndex = m_GameData.GameInfos.Count - 1;
            }
            ChangeImageSource();
        }

        private void OnSelectedCount1()
        {
            m_SlelectedCount = 1;
            for(var i = 0; i < m_SelectedImages.Length; i++)
            {
                if(i == m_SlelectedCount - 1)
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Yellow.png",i);
                }
                else
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Violet.png",i);

                }
            }
        }

        private void ChangeSelectedCountImag(string url,int id)
        {
            try
            {
                var imageURI = new BitmapImage();
                imageURI.BeginInit();
                imageURI.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                imageURI.EndInit();
                m_SelectedImages[id].Source = imageURI;
            }
            catch(Exception e)
            {
                Log.Error("[VRKart] ChangeSelectedCountImag Error : " + e.ToString());
            }
           
        }

        private void OnSelectedCount2()
        {
            m_SlelectedCount = 2;
            for (var i = 0; i < m_SelectedImages.Length; i++)
            {
                if (i == m_SlelectedCount - 1)
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Yellow.png",i);
                }
                else
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Violet.png",i);
                }
            }

        }
        private void OnSelectedCount3()
        {
            m_SlelectedCount = 3;
            for (var i = 0; i < m_SelectedImages.Length; i++)
            {
                if (i == m_SlelectedCount - 1)
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Yellow.png",i);
                }
                else
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Violet.png",i);
                }
            }

        }
        private void OnSelectedCount4()
        {
            m_SlelectedCount = 4;
            for (var i = 0; i < m_SelectedImages.Length; i++)
            {
                if (i == m_SlelectedCount - 1)
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Yellow.png",i);
                }
                else
                {
                    ChangeSelectedCountImag("pack://application:,,,/Resources/Image/Car" + (i + 1).ToString() + "_Violet.png",i);
                }
            }

        }
        private void ChangeImageSource()
        {
            var imageURI = new BitmapImage();
            imageURI.BeginInit();
            imageURI.UriSource = new Uri(m_GameData.GameInfos[m_CurrentIndex].Icon, UriKind.RelativeOrAbsolute);
            imageURI.EndInit();
            m_AdvertsImg.Source = imageURI;
        }

     
        private void OnXmlOpAndStartGame()
        {
            if(m_SlelectedCount < 1)
            {
                MessagePanel.ShowMessage("请先选择一个人数后再创建房间");
            }

            var gameConfigPath = Path.GetFullPath(m_GameData.GameInfos[m_CurrentIndex].GameConfigPath);
            if (!File.Exists(gameConfigPath))
            {
                Log.Error("[VRKartLogic] OnJoinRoomButtonClick Error : Not Exist Game 。");
                MessagePanel.ShowMessage("不存在游戏配置文件 .");
                return;
            }
            if (XMLFileOp(gameConfigPath, "MaxPlayerNumber", m_SlelectedCount.ToString()))
            {
                var gamePath = Path.GetFullPath(m_GameData.GameInfos[m_CurrentIndex].Path);
                if (!File.Exists(gamePath))
                {
                    Log.Error("[VRKartLogic] OnJoinRoomButtonClick Error : Not Exist Game 。");
                    MessagePanel.ShowMessage("不存在游戏文件 .");
                    return;
                }
                Process.Start(gamePath);

                if(OnStartGameScuess != null)
                {
                    OnStartGameScuess(m_GameData.GameInfos[m_CurrentIndex].Name, m_GameData.GameInfos[m_CurrentIndex].Path);
                }

                IsStartGame = true;

            }
        }

        private void StartInsertDB()
        {
            if (m_InsertDBThread == null)
            {
                m_InsertDBThread = new Thread(() => {

                    Thread.Sleep(1000 * m_GameCenterConfig.InSertDBTime);

                    if(IsStartGame)
                    {
                        try
                        {
                            m_GameCenterDBEntities.AddAGameReecordInfo(new GameRecord()
                            {
                                Name = m_GameData.GameInfos[m_CurrentIndex].Name,
                                Count = 1,
                                Amount = m_GameData.GameInfos[m_CurrentIndex].SinglePrice * 1,
                                RunTime = DateTime.Now
                            });
                        }
                        catch(Exception e)
                        {
                            Log.Error("[VRKartLogic] StartInsertDB Error : " + e.ToString());
                        }
                    }
                });
            }

            m_InsertDBThread.Start();

        }
        private void StopInsertDB()
        {
            if(m_InsertDBThread != null)
            {
                m_InsertDBThread.Abort();
                m_InsertDBThread = null;
            }
        }

        private bool XMLFileOp(string path,string key,string value)
        {
            try
            {
                var doc = new XmlDocument();
                if (string.IsNullOrEmpty(path))
                {
                    Log.Error("[VRKartLogic] OnXMLFileOp Error : path == null.");
                    return false;
                }
                doc.Load(path);
                foreach (var n1 in doc.ChildNodes)
                {
                    XmlElement xe1;
                    xe1 = n1 as XmlElement;
                    if (xe1 == null)
                    {
                        continue;
                    }
                    foreach (var n2 in xe1.ChildNodes)
                    {
                        var xe2 = n2 as XmlElement;
                        if (xe2 == null)
                        {
                            continue;
                        }

                        if (xe2.Name == key)
                        {
                            xe2.InnerText = value;
                        }
                        
                    }
                }
                doc.Save(path);
            }
            catch (Exception e)
            {
                Log.Error("[VRKartLogic] XMLFileOp Error : " + e.Message);
                return false;
            }
            return true;
        }

        public void Clear()
        {
            
            m_CurrentIndex = -1;
            if(OnStartGameScuess != null)
            {
                OnStartGameScuess = null;
            }
            if (m_GameCenterDBEntities != null)
            {
                m_GameCenterDBEntities.Dispose();
                m_GameCenterDBEntities = null;
            }
            m_IsStartGame = false;
            if (m_InsertDBThread != null)
            {
                m_InsertDBThread.Abort();
                m_InsertDBThread = null;
            }
        }

    }
}
