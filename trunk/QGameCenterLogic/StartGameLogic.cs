using QData;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using QProtocols;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using QConnection;
using System.Diagnostics;

namespace QGameCenterLogic
{
    /// <summary>
    /// 定义了开始结束游戏按钮，以及显示状态，还有通知客户端开始游戏的逻辑
    /// </summary>
    public class StartGameLogic
    {
        private QServer m_Server;
        private ClientData m_ClientData;
        private GameCenterConfig m_GameCenterData;
        private GameData m_GameData;

        private Button[] m_Buttons;
        private Label[] m_Labels;
        private Image[] m_Images;
        private CheckBox[] m_CheckBoxs;
        private Window m_Window;

        private Image[] m_StartedShowImages;

        private string m_GamePath;
        private string m_GameConfigPath;

        private Action<string> m_PopMessagePanel;
        private bool m_IsShowCoinPanel = false;
        private bool[] IsConnectDic = new bool[3];
        public bool IsShowCoinPanel
        {
            get
            {
                return m_IsShowCoinPanel;
            }

            set
            {
                m_IsShowCoinPanel = value;
            }
        }
        private Action m_ShowCoinPanel;


        private bool m_IsReadyToStartGame = true;
        private bool m_IsGameStart = false;

        private GameCenterDBEntities m_GameCenterDBEntities;

        /// <summary>
        /// 成功开始游戏以及成功关闭游戏逻辑
        /// </summary>
        public Action<bool> OnIsStartOrCloseGame;


        public int m_CurSelectedIndex = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="data">客户端配置文件</param>
        /// <param name="gamecenterdata">中控配置文件</param>
        /// <param name="buttons">主要包含开始游戏按钮和关闭游戏按钮</param>
        /// <param name="labels">显示客户端上线掉线的文本框</param>
        /// <param name="image">TabControl里面的图片 如果掉线的话 则显示为指定的掉线图片，避免出现卡在掉线那一帧的画面</param>
        /// <param name="checkboxs">客户端多选框</param>
        /// <param name="window"></param>
        /// <param name="popmessage">消息弹出框</param>
        public StartGameLogic(QServer server, ClientData data, GameCenterConfig gamecenterdata, GameData gamedata, Button[] buttons, Label[] labels, Image[] image, CheckBox[] checkboxs, Window window, Action<string> popmessage, Action showcoinPanel = null)
        {
            m_ClientData = data;
            m_GameData = gamedata;

            m_GameCenterData = gamecenterdata;

            //开始和关闭按钮
            m_Buttons = buttons;
            m_Buttons[0].Click += (sender, e) =>
            {
                OnStartGame();
            };
            if (m_Buttons.Length >= 2)
            {
                m_Buttons[1].Click += (sender, e) => { OnCloseGame(); };
            }

            m_Labels = labels;
            m_Images = image;
            //m_CheckboxImage = checkboximage;
            m_CheckBoxs = checkboxs;
            m_Window = window;
            m_PopMessagePanel = popmessage;
            m_ShowCoinPanel = showcoinPanel;
            m_Server = server;
            m_Server.OnClientConnected += OnClientConnect;
            m_Server.OnClientDisconnected += OnClientDisconnect;

            if (!File.Exists("Data/GameCenterDB.db"))
            {
                MessageBox.Show("无法加载GameCenterDB.db文件.");
                m_Window.Close();
                return;
            }

            try
            {
                m_GameCenterDBEntities = new GameCenterDBEntities();
            }
            catch (Exception e)
            {
                Log.Error("[StartGameLogic] StartGameLogic Error : " + e.Message);
            }

        }

        public void Clear()
        {

            m_IsReadyToStartGame = true;
            m_IsGameStart = false;


            if (m_GameCenterDBEntities != null)
            {
                m_GameCenterDBEntities = null;
            }
            if (OnIsStartOrCloseGame != null)
            {
                OnIsStartOrCloseGame = null;
            }
        }

        /// <summary>
        /// 在其他类里面注册调用该函数 例如在选择游戏的按钮中 动态改变m_GamePath的值
        /// </summary>
        /// <param name="path"></param>
        public void SetGamePath(string path, string gameConfigPath)
        {
            m_GamePath = path;
            m_GameConfigPath = gameConfigPath;
        }
        public void SetGamePath(string path)
        {
            m_GamePath = path;
        }

        public void SetCurGameIndex(int index)
        {
            if (index < 0 || index > m_GameData.GameInfos.Count)
            {
                Log.Error("[CheckCoinLogic] SetCurIndex Error : Out Array Index");
                return;
            }
            m_CurSelectedIndex = index;
            m_GamePath = m_GameData.GameInfos[index].Path;
            m_GameConfigPath = m_GameData.GameInfos[index].GameConfigPath;
        }

        public void SetShatedShowImages(Image[] images)
        {
            m_StartedShowImages = new Image[images.Length];
            m_StartedShowImages = images;
            for (int m = 0; m < m_ClientData.ClientInfos.Count; m++)
            {
                if (m_Server.IsConnect(IPAddress.Parse(m_ClientData.ClientInfos[m].IP)))
                {
                    OnClientConnect(IPAddress.Parse(m_ClientData.ClientInfos[m].IP));
                }
            }
        }

        /// <summary>
        /// 关闭游戏的逻辑
        /// </summary>
        private void OnCloseGame()
        {
            if (m_GamePath == string.Empty)
            {
                Log.Error(" [StartGameLogic]  Start Error : path is null.");
                return;
            }


            for (int id = 0; id < m_CheckBoxs.Length; id++)
            {
                if (m_CheckBoxs[id].IsChecked == false)
                {
                    continue;
                }

                //只修改有上线的client  将 m_CheckBos[id]设置为可以点击
                try
                {
                    var ipclose = m_ClientData.GetClient(id + 1);
                    if (string.IsNullOrEmpty(ipclose.ToString()))
                    {
                        continue;
                    }
                    if (m_Server.IsConnect(ipclose))
                    {
                        m_CheckBoxs[id].IsEnabled = true;
                    }
                }
                catch
                {

                }

                m_Window.Dispatcher.Invoke(() =>
                {
                    if (m_StartedShowImages != null)
                    {
                        if (m_Labels[id].Visibility != Visibility.Hidden)
                        {
                            m_StartedShowImages[id].Visibility = Visibility.Hidden;
                        }
                        m_StartedShowImages[id].Source = SetImageSource(@"\Resources\Game\ConnectBG.png");
                        m_Labels[id].Content = "客户端" + (id + 1).ToString() + "连接";
                        m_CheckBoxs[id].IsEnabled = true;
                    }
                });

                var info = m_GameData.GameInfos[m_CurSelectedIndex];

                var ip = m_ClientData.GetClient(id + 1);
                if (string.IsNullOrEmpty(ip.ToString()))
                {
                    return;
                }
                m_Server.ProcessOP(ip, ProcessOp.Stop, m_GamePath, info.Name,
                    info.IsDeleteBorder == 1 ? true : false, info.IsKillCloseGa == 1 ? true : false, (Code code) =>
                 {
                     if (code == Code.Success)
                     {
                         m_IsGameStart = false;
                     }
                     if (code == Code.Failed)
                     {
                         if (m_PopMessagePanel != null)
                         {
                             m_Window.Dispatcher.Invoke(() =>
                             {
                                 m_PopMessagePanel("关闭游戏客户端" + (id + 1).ToString() + "失败，原因是:" + code);
                             });

                         }
                         Log.Error(" [StartGameLogic]  OnCloseGameClick Error : start client[" + (id + 1) + "]  ip :  " + m_ClientData.GetClient(id) + "  Failed :" + code);
                     }
                 });
            }

            if (OnIsStartOrCloseGame != null)
            {
                OnIsStartOrCloseGame(false);
            }

            m_IsReadyToStartGame = false;
            new Thread(() =>
            {
                Thread.Sleep(5000);
                m_IsReadyToStartGame = true;
                m_IsGameStart = false;

            }).Start();

        }

        /// <summary>
        /// 设置是否使用投币操作
        /// </summary>
        /// <param name="isUse"></param>
        public void SetIsUseCoin(bool isUse)
        {
            IsShowCoinPanel = isUse;
        }

        /// <summary>
        /// 开始游戏的逻辑
        /// </summary>
        protected virtual void OnStartGame()
        {
            if (m_IsGameStart)
            {
                m_PopMessagePanel("游戏已开始!");
                return;
            }
            if (m_IsReadyToStartGame == false)
            {
                m_PopMessagePanel("游戏刚关闭，请稍候5秒启动");
                return;
            }

            if (m_CheckBoxs[0].Visibility == Visibility.Hidden)
            {
                for (int i = 0; i < m_ClientData.ClientInfos.Count; i++)
                {
                    if (!m_Server.IsConnect(IPAddress.Parse(m_ClientData.ClientInfos[i].IP)))
                    {
                        m_PopMessagePanel("请确保所有客户端已连接！  可参考界面下方小图标");
                        return;
                    }
                }
            }

            //根据是否需要投币来显示投币面板
            if (IsShowCoinPanel && m_ShowCoinPanel != null)
            {
                m_ShowCoinPanel();
            }
            else
            {
                OnStartGameAfterCoin();
            }
        }

        /// <summary>
        /// 投币结束后  再开启游戏
        /// </summary>
        public void OnStartGameAfterCoin()
        {
            if (m_GamePath == null)
            {
                Log.Error("[StartGamelogic] HorseStartLogic Error : m_GamePath == null .");
                return;
            }

            //防止第一次启动 没有数据进行连接
            if (m_ClientData.ClientInfos.Count <= 0)
            {
                m_PopMessagePanel("还没有客户端进行连接，请先启动一台客户端!");
                return;
            }
            //确保主机连接  主要是针对需要第一台游戏必须启动的项目
            if ((m_GameCenterData.MustHostStart == 1) && (m_Server.IsConnect(m_ClientData.GetClient(1)) == false))
            {
                m_PopMessagePanel("请确保主机已连接!");
                return;
            }

            //计算出当前局域网一共有多少Client在线
            var count = 0;
            foreach (var check in m_CheckBoxs)
            {
                if (check.IsChecked == true)
                {
                    count++;
                }
            }

            var pairs = m_GameData.GameInfos[m_CurSelectedIndex].KeyValuePairs;

            //拷贝文件
            var pairs_copy = new List<QData.KeyValuePair>();
            for (int i = 0; i < pairs.Count; i++)
            {
                var p = new QData.KeyValuePair();
                p.Key = pairs[i].Key;
                p.Value = pairs[i].Value;
                pairs_copy.Add(p);
            }
            //只修改有上线的client
            for (var id = 0; id < m_CheckBoxs.Length; id++)
            {
                try
                {
                    var ip = m_ClientData.GetClient(id + 1);
                    if (string.IsNullOrEmpty(ip.ToString()))
                    {
                        continue;
                    }
                    if (m_Server.IsConnect(ip))
                    {
                        m_CheckBoxs[id].IsEnabled = true;
                    }
                }
                catch
                {

                }

            }

            for (var id = 0; id < m_CheckBoxs.Length; id++)
            {
                if ((bool)m_CheckBoxs[id].IsChecked == false)
                {
                    continue;
                }
                ModifyXmlAndStartGame(count, id, pairs_copy);
            }
        }


        /// <summary>
        /// 修改游戏的配置文件以及开始游戏
        /// </summary>
        /// <param name="count"></param>
        /// <param name="id"></param>
        private void ModifyXmlAndStartGame(int count, int id, List<QData.KeyValuePair> pairs)
        {
            var ip = m_ClientData.GetClient(id + 1);
            var gameconfigpath = m_GameConfigPath;


            for (var index = 0; index < pairs.Count; index++)
            {

                var p = pairs[index];
                //处理特殊策略
                if (p.Value.StartsWith("(") && p.Value.EndsWith(")"))
                {
                    if (p.Value.Contains("PlayerNum"))
                    {
                        p.Value = count.ToString();
                    }
                    else if (p.Value.Contains("NetworkAddress"))
                    {
                        m_Window.Dispatcher.Invoke(() =>
                        {
                            if (m_CheckBoxs[0].IsChecked == true)
                            {
                                p.Value = m_ClientData.GetClient(1).ToString();
                            }
                            else if (m_CheckBoxs[1].IsChecked == true)
                            {
                                p.Value = m_ClientData.GetClient(2).ToString();
                            }
                            else
                            {
                                p.Value = m_ClientData.GetClient(3).ToString();
                            }
                        });

                    }
                    else if (p.Value.Contains("SeverPlayerId"))
                    {
                        m_Window.Dispatcher.Invoke(() =>
                        {
                            if (m_CheckBoxs[0].IsChecked == true)
                            {
                                p.Value = "100";
                            }
                            else if (m_CheckBoxs[1].IsChecked == true)
                            {
                                p.Value = "101";
                            }
                            else
                            {
                                p.Value = "102";
                            }
                        });
                    }

                }
                else
                {
                    //使用默认配置
                }
            }

            var are = new AutoResetEvent(false);
            //修改xml的值
            m_Server.XMLFileOp(ip, m_GameConfigPath, pairs, (code) =>
            {
                are.Set();
                if (code == Code.Failed)
                {
                    if (m_PopMessagePanel != null)
                    {
                        m_Window.Dispatcher.Invoke(() =>
                        {
                            m_PopMessagePanel("修改客户端" + (id + 1).ToString() + "失败，原因是:" + code);
                        });
                    }
                    return;
                }
                if (code == Code.Success)
                {
                    m_Window.Dispatcher.Invoke(() =>
                    {
                        if (m_StartedShowImages != null)
                        {
                            m_StartedShowImages[id].Visibility = Visibility.Visible;
                            m_StartedShowImages[id].Source = SetImageSource(@"\Resources\Game\yellow.png");
                            m_Labels[id].Content = "客户端" + (id + 1) + "游戏运行中";
                        }
                    });

                    var info = m_GameData.GameInfos[m_CurSelectedIndex];
                    m_Server.ProcessOP(ip, ProcessOp.Start, m_GamePath, info.Name, info.IsDeleteBorder == 1 ? true : false,
                        info.IsKillCloseGa == 1 ? true : false, (Code code1) =>
                        {
                            PrintProcessOPCode(code1, id);
                        });
                }
            });

            if (OnIsStartOrCloseGame != null)
            {
                OnIsStartOrCloseGame(true);
            }

            are.WaitOne(1000 * 3);
            are.Dispose();

            //插入数据
            StartInsetrDB(count);

        }
        private Thread m_InsertDBThread;



        private void StartInsetrDB(int count)
        {
            StopInsertDB();

            if (m_InsertDBThread == null)
            {
                m_InsertDBThread = new Thread(() =>
                {

                    Thread.Sleep(1000 * m_GameCenterData.InSertDBTime);
                    try
                    {
                        m_Window.Dispatcher.Invoke(() =>
                        {

                            if (m_IsGameStart)
                            {

                                m_GameCenterDBEntities.AddAGameReecordInfo(new GameRecord()
                                {
                                    Name = m_GameData.GameInfos[m_CurSelectedIndex].Name,
                                    Count = count,
                                    Amount = count * m_GameData.GameInfos[m_CurSelectedIndex].SinglePrice,
                                    RunTime = DateTime.Now
                                });
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Log.Error("[StartGameLogic] InsertDB Error : " + e.ToString());
                    }

                });
            }
            m_InsertDBThread.Start();

        }

        private void StopInsertDB()
        {
            if (m_InsertDBThread != null)
            {
                m_InsertDBThread.Abort();
                m_InsertDBThread = null;
            }
        }
        /// <summary>
        /// 打开游戏时候的Code情况
        /// </summary>
        /// <param name="code"></param>
        /// <param name="id"></param>
        private void PrintProcessOPCode(Code code, int id)
        {
            m_Window.Dispatcher.Invoke(() =>
            {
                switch (code)
                {
                    case Code.AlreadyExist:
                        if (m_PopMessagePanel != null)
                        {
                            m_PopMessagePanel("开启游戏客户端" + (id + 1).ToString() + "失败，原因是 : 游戏已经开启");
                        }
                        Log.Error("[StartGameLogic] PrintProcessOPCode Error : The Game already exist.");
                        break;

                    case Code.Failed:
                        if (m_PopMessagePanel != null)
                        {
                            m_PopMessagePanel("开启游戏客户端" + (id + 1).ToString() + "失败.");
                        }
                        Log.Error("[StartGameLogic] PrintProcessOPCode Error : Failed.");
                        break;

                    case Code.NotExist:
                        if (m_PopMessagePanel != null)
                        {
                            m_PopMessagePanel("开启游戏客户端" + (id + 1).ToString() + "失败，原因是 : 游戏不存在");
                        }
                        Log.Error("[StartGameLogic] PrintProcessOPCode Error : The Game not exist .");
                        break;

                    case Code.Success:
                        m_IsGameStart = true;
                        break;
                    case Code.UnKnow:
                        if (m_PopMessagePanel != null)
                        {
                            m_PopMessagePanel("开启游戏客户端" + (id + 1).ToString() + ",位置原因");
                        }
                        Log.Error("[StartGameLogic] PrintProcessOPCode Error : Failed.");
                        break;
                    default:
                        break;
                }
            });

        }

        /// <summary>
        /// 当有一个客户端上线
        /// </summary>
        /// <param name="address"></param>
        private void OnClientConnect(IPAddress address)
        {
            var id = m_ClientData.GetClient(address);

            if ((id - 1) < 0 || (id - 1) > m_Labels.Length)
            {
                Log.Error("[StartGameLogic] OnClientConnect Error : ip : " + address + " - id is not not exit. ");
                return;
            }
            m_Window.Dispatcher.Invoke(() =>
            {
                m_StartedShowImages[id - 1].Source = SetImageSource(@"Resources\Game\ConnectBG.png");
                if (m_Labels != null)
                {
                    m_Labels[id - 1].Content = "客户端" + (id).ToString() + "连接";
                    m_Labels[id - 1].Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                if (m_CheckBoxs != null)
                {
                    m_CheckBoxs[id - 1].IsEnabled = true;
                    m_CheckBoxs[id - 1].IsChecked = true;

                }
            });
        }

        /// <summary>
        /// 当有一个客户端掉线
        /// </summary>
        /// <param name="address"></param>
        private void OnClientDisconnect(IPAddress address)
        {
            try
            {
                var id = m_ClientData.GetClient(address);
                if ((id - 1) < 0 || (id - 1) > m_Labels.Length)
                {
                    Log.Error("[StartGameLogic] OnClientConnect Error : ip+" + address + " - id is not not exit. ");
                }
                m_Window.Dispatcher.Invoke(() =>
                {

                    if (m_Labels != null)
                    {
                        m_StartedShowImages[id - 1].Source = SetImageSource(@"Resources\Game\DisConnectBG.png");
                        m_Labels[id - 1].Content = "客户端" + (id).ToString() + "掉线";
                        m_Labels[id - 1].Foreground = new SolidColorBrush(Color.FromRgb(147, 156, 156));
                    }
                    if (m_CheckBoxs != null)
                    {
                        m_CheckBoxs[id - 1].IsEnabled = false;
                        m_CheckBoxs[id - 1].IsChecked = false;
                    }

                    if (m_Images != null)
                    {
                        var imageURI = new BitmapImage();
                        imageURI.BeginInit();
                        imageURI.UriSource = new Uri("Resources/Logo/newlogo.jpg", UriKind.RelativeOrAbsolute);
                        imageURI.EndInit();
                        m_Images[id - 1].Source = imageURI;
                    }

                });
            }
            catch (Exception e)
            {
                Log.Error("[StartGameLogic] OnClientDisconnect Erro ：" + e.Message);
            }
        }


        public void SetMachineButtonClick(Button[] buttons)
        {
            buttons[0].Click += (sender, e) => { OnResetMachine(); };
            buttons[1].Click += (sender, e) => { OnResetHeadset(); };
            buttons[2].Click += (sender, e) => { OnCheckMachineState(); };
        }
        /// <summary>
        /// 重置动感平台
        /// </summary>
        private void OnResetMachine()
        {
            try
            {
                for (var i = 0; i < m_CheckBoxs.Length; i++)
                {
                    if (m_CheckBoxs[i].IsChecked == false)
                    {
                        continue;
                    }
                    var ip = m_ClientData.GetClient(i + 1);
                    if (m_Server.IsConnect(ip))
                    {
                        //
                        m_Server.ProcessOP(ip, ProcessOp.Start, "MachineTools/ResetMachine/ResetMachine.exe", "ResetMachine", false, false, (code) =>
                        {
                            if (code != Code.Success)
                            {
                                Log.Error("[SettingLogic] ResetMachine Error : ResetMachine Lose .");
                            }
                        });
                    }
                    else
                    {
                        Log.Error("[SettingLogic] ResetMachine Error : current client id : " + (i + 1) + " ip :" + ip);
                        m_PopMessagePanel(" 客户端" + (i + 1) + "掉线，关闭客户端失败");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[SettingLogic] ResetHeadset Error :" + ex.Message);
            }
        }
        /// <summary>
        /// 重置头盔方向
        /// </summary>
        private void OnResetHeadset()
        {
            try
            {
                for (var i = 0; i < m_CheckBoxs.Length; i++)
                {
                    if (m_CheckBoxs[i].IsChecked == false)
                    {
                        continue;
                    }
                    var ip = m_ClientData.GetClient(i + 1);
                    if (m_Server.IsConnect(ip))
                    {
                        //ResetHeadset
                        m_Server.ProcessOP(ip, ProcessOp.Start, "MachineTools/ResetHeadset/ResetHeadset.exe", "ResetHeadset", false, false, (code) =>
                        {
                            if (code != Code.Success)
                            {
                                Log.Error("[SettingLogic] ResetMachine Error : ResetMachine Lose .");
                            }
                        });
                    }
                    else
                    {
                        Log.Error("[SettingLogic] ResetHeadset Error : current client id : " + (i + 1) + " ip :" + ip);
                        m_PopMessagePanel(" 客户端" + (i + 1) + "掉线，关闭客户端失败");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[SettingLogic] ResetHeadset Error :" + ex.Message);
            }
        }

        /// <summary>
        /// 检测机电状态
        /// </summary>
        private void OnCheckMachineState()
        {
            m_PopMessagePanel(Utils.MachineStatus("192.168.15.201"));
        }

        private ImageSource SetImageSource(string path)
        {
            try
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();  //必须对图像初始化
                img.UriSource = new Uri(path, UriKind.Relative);
                img.EndInit();//结束初始化
                return img;
            }
            catch (Exception e)
            {
                Log.Error("[SettingLogic] SetImageSource Error :" + e.Message);
                return null;
            }
        }
    }
}
