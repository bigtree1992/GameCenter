using QData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;

namespace QGameCenterLogic
{
    public class CheckRouterLogic
    {
        private Window m_Window;
        private int m_ClientCount;
        private ClientData m_ClientData;
        private Action<String> m_PopMessageAction;
        private Thread m_RouterThread;

        public CheckRouterLogic(Window window, int length, ClientData clientdata, Action<string> popmessage)
        {
            m_Window = window;
            m_ClientCount = length;
            m_ClientData = clientdata;
            m_PopMessageAction = popmessage;
            m_RouterThread = new Thread(CheckRouterThread);
        }

        public void CheckRouter()
        {
            try
            {
                //获取所有网卡
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

                if (nics == null)
                {
                    m_Window.Dispatcher.Invoke(() =>
                    {
                        m_Window.Visibility = Visibility.Hidden;
                        m_PopMessageAction("未检测到网卡，请检查网卡状态，程序即将关闭！");
                        Log.Error("[CheckRouterLogic] CheckRouter Error : No network card detected. Please check the network card. The program is closing!");
                    });

                    Stop();
                }

                //遍历数组
                foreach (var netWork in nics)
                {
                    //单个网卡的IP对象
                    IPInterfaceProperties ip = netWork.GetIPProperties();
                    //获取该IP对象的网关
                    GatewayIPAddressInformationCollection gateways = ip.GatewayAddresses;
                    foreach (var gateWay in gateways)
                    {
                        if (gateWay == null)
                        {
                            return;
                        }

                        Ping p = new Ping();//创建Ping对象p
                        PingReply pr = p.Send(gateWay.Address);//向指定网关发送ICMP协议的ping数据包

                        if (pr.Status != IPStatus.Success)
                        {
                            m_Window.Dispatcher.Invoke(() =>
                            {
                                m_Window.Visibility = Visibility.Hidden;
                                m_PopMessageAction("路由器连接断开，请检查路由器连接，程序即将关闭！");
                                Log.Error("[CheckRouterLogic] CheckRouter Error : The router is offline. Please check the router. The program is closing!");
                            });

                            Stop();
                        }
                        else
                        {
                            p = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        public void Start()
        {
            m_RouterThread.Start();
        }

        public void Stop()
        {
            m_RouterThread.Abort();
        }

        private void CheckRouterThread()
        {
            while (true)
            {
                Random m_number = new Random();
                int random = m_number.Next(10, 11);
                Thread.Sleep(2000);
                CheckRouter();
                Thread.Sleep(random * 1000);
            }
        }
    }
}