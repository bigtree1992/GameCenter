using QConnection;
using QData;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace QGameCenterLogic
{
    public class CheckCoinLogic
    {
        private bool m_IsUserCoin = false;
        public bool IsUserCoin
        {
            get { return m_IsUserCoin; }
            set{ m_IsUserCoin = value; }
        }

        private CoinDetection m_CoinDetection;
        private int m_CurSinglePrice = 0;
        private int m_CoinCount = 0;
        private int m_CoinCharge = 0;

        private GameCenterConfig m_GameCenterConfig;
        private GameData m_GameData;

        private Window m_Window;

        /// <summary>
        /// 选中的CheckBox
        /// </summary>
        private CheckBox[] m_CheckBoxs;
        private Action<string> m_PopMessageAction;

        private Label[] m_Labels;
        private Button[] m_Button;
        private Image m_ImageBG;

        public Action OnAfterCoinSuccess;
        private Action m_OnReturnButtonAction;

        public CheckCoinLogic(GameCenterConfig gameCenterConfig,GameData data, CheckBox[] checkboxs,Window window, Action<string> popMessage)
        {
            m_CoinDetection = new CoinDetection();

            m_GameCenterConfig = gameCenterConfig;
            m_GameData = data;
            m_Window = window;
            m_CheckBoxs = checkboxs;
            m_PopMessageAction = popMessage;
        }
        /// <summary>
        /// 设置是否需要投币操作
        /// </summary>
        /// <param name="isUse"></param>
        public void SetIsUseCoin(bool isUse)
        {

            IsUserCoin = isUse;
        }
        
        ~CheckCoinLogic()
        {
            Clear();
        }
        /// <summary>
        /// 释放内存
        /// </summary>
        public void Clear()
        {
            if (m_CoinDetection != null)
            {
                m_CoinDetection.Stop();
                m_CoinDetection = null;
            }

            if (OnAfterCoinSuccess != null)
            {
                OnAfterCoinSuccess = null;
            }
        }

        public void SetCurIndex(int index)
        {
            if(index <0 || index > m_GameData.GameInfos.Count)
            {
                Log.Error("[CheckCoinLogic] SetCurIndex Error : Out Array Index" );
                return;
            }
            m_CurSinglePrice = m_GameData.GameInfos[index].SinglePrice;
        }

        /// <summary>
        /// 绑定UI组件
        /// </summary>
        /// <param name="labels"></param>
        /// <param name="button"></param>
        /// <param name="returnButtonAction"></param>
        public void BindUI(Label[] labels,Button[] button,Image bg,Action returnButtonAction)
        {
            m_Labels = labels;
            m_ImageBG = bg;
            //将倒数的Label进行隐藏
            m_Window.Dispatcher.Invoke(()=> {
                m_Labels[0].Visibility = Visibility.Visible;
                m_Labels[1].Visibility = Visibility.Hidden;
                m_Labels[2].Visibility = Visibility.Hidden;
                m_Labels[3].Visibility = Visibility.Hidden;
            });
           
            //根据当前选中的客户端的数量来计算刷卡金额
            var count = 0;
            foreach(var b in m_CheckBoxs)
            {
                if( b.IsChecked == true )
                {
                    count++;
                }
            }
            try
            {
                m_CoinCharge = m_CurSinglePrice * count;
                m_CoinCount = 0;

                m_Window.Dispatcher.Invoke(() => {
                    m_Labels[0].Content = m_CoinCharge.ToString();
                });

                m_Button = button;
                m_OnReturnButtonAction = returnButtonAction;

                m_Button[0].Click += (sender, e) => { OnCloseButton(); };

                if (m_GameCenterConfig.IsTest == 1)
                {
                    m_Button[1].Click += (sender, e) => { OnCoinDetected(1); };
                }
                else
                {
                    m_Window.Dispatcher.Invoke(() => {
                        m_Button[1].Visibility = Visibility.Hidden;
                    });
                }
                
                //不处于测试模式的话，则加载真正的刷卡检测回调函数
                if (m_GameCenterConfig.IsTest == 0)
                {
                    try
                    {
                        m_CoinDetection.Start(m_GameCenterConfig.CoinComPort);
                        m_CoinDetection.OnCoinDetected = OnCoinDetected;
                        m_CoinDetection.OnErrorReported = OnErrorReported;
                    }
                    catch(Exception e)
                    {
                        Log.Error("[CheckCoinLogic] BindUI Error : " + e.ToString());
                        m_Window.Dispatcher.Invoke(() => {
                            m_PopMessageAction(e.Message);
                        });
                        if(returnButtonAction != null)
                        {
                            OnCloseButton();
                        }
                        else
                        {
                            Application.Current.Shutdown();
                        }
                    }
                   
                }

                if (count <= 0)
                {
                    m_Window.Dispatcher.Invoke(() => {
                        m_PopMessageAction("您没有选择任何一个客户端，请选择您需要开启的客户端");
                        OnCloseButton();
                    });
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Error("[CheckCoinLogic] BindUI : Error : " + e.Message);
            }

        }

        /// <summary>
        /// 解绑UI组件
        /// </summary>
        public void UnBindUI()
        {
            try
            {
                m_Labels = null;

                m_Button = null;

                if (m_GameCenterConfig.IsTest == 0)
                {
                    m_CoinDetection.Stop();
                    m_CoinDetection.OnCoinDetected -= null;
                    m_CoinDetection.OnErrorReported -= null;
                }
            }
            catch(Exception e)
            {
                Log.Error("[CheckCoinLogic] UnBindUI  Error : " + e.Message);
            }
        }

        /// <summary>
        /// 关闭投币面板按钮事件
        /// </summary>
        private void OnCloseButton()
        {
            UnBindUI();
            if (m_OnReturnButtonAction != null)
            {
                m_OnReturnButtonAction();
            }
        }

        /// <summary>
        /// 投币出现错误回调
        /// </summary>
        /// <param name="message"></param>
        private void OnErrorReported(string message)
        {
            if(m_PopMessageAction != null)
            {
                m_Window.Dispatcher.Invoke(() => {
                    m_PopMessageAction("投币出现错误 : " + message);
                });
            }
            Log.Error("[CheckCoinLogic] OnErrorReported Error : " + message);
        }
        /// <summary>
        /// 投币成功一次回调函数
        /// </summary>
        /// <param name="obj"></param>
        private void OnCoinDetected(int obj)
        {
            m_CoinCount++;
            try
            {
                m_Window.Dispatcher.Invoke(() =>
                {
                    m_Labels[0].Content = m_CoinCharge - m_CoinCount;
                });
                if (m_CoinCount == m_CoinCharge)
                {
                    TimeShutdown();
                }
            }
            catch(Exception e)
            {
                Log.Error("[CheckCoinLogic] OnCoinDetected Error : " + e.Message);
            }
        }

        /// <summary>
        /// 进行游戏开始倒数10秒
        /// </summary>
        private void TimeShutdown()
        {
            try
            {
                m_Window.Dispatcher.Invoke(() => {
                    var imageURI = new BitmapImage();
                    imageURI.BeginInit();
                    imageURI.UriSource = new Uri("Resources/Game/CoinMessageBG.png", UriKind.RelativeOrAbsolute);
                    imageURI.EndInit();
                    m_ImageBG.Source = imageURI;
                    m_Labels[0].Visibility = Visibility.Hidden;
                    m_Labels[1].Visibility = Visibility.Visible;
                    m_Labels[2].Visibility = Visibility.Visible;
                    m_Labels[3].Visibility = Visibility.Visible;
                });
            }
            catch(Exception e)
            {
                Log.Error("[CheckCoinLogic] TimeShutdown Error : " + e.Message);
            }

            try
            {
                var timer = 10;
                new Thread(() => {
                    while(true)
                    {
                        Thread.Sleep(1000);
                        timer--;
                        m_Window.Dispatcher.Invoke(() => {
                            m_Labels[3].Content = timer.ToString();
                        });
                        if (timer == 0)
                        {
                            //可以开始游戏了
                            m_Window.Dispatcher.Invoke(()=> {
                                if (OnAfterCoinSuccess != null)
                                {
                                    OnAfterCoinSuccess();
                                }
                                OnCloseButton();
                            });
                            break; 
                        }
                    }
                    
                }).Start();
            }
            catch (Exception e)
            {
                Log.Error("[CheckCoinLogic] TimeShutdown Error : " + e.Message);
            }
            
        }

    }
}
