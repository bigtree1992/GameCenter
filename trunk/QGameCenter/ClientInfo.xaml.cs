using Microsoft.Research.DynamicDataDisplay.DataSources;
using QConnection;
using QGameCenter.Data;
using QGameCenterLogic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using Microsoft.Research.DynamicDataDisplay;

namespace QGameCenter
{
    /// <summary>
    /// ClientInfo.xaml 的交互逻辑
    /// </summary>
    public partial class ClientInfo : Window
    {
        private ClientMachineInfo m_ClientMachineInfo;

        private QServer m_QServer;

        private QNetInfoClient m_QNetInfoClient;
        
        public ClientInfo()
        {
            InitializeComponent();
        }
        public ClientInfo(QServer server,ClientMachineInfo info)
        {
            InitializeComponent();

            m_QServer = server;

            m_QNetInfoClient = new QNetInfoClient();
            m_ClientMachineInfo = info;
            //this.Visibility = Visibility.Hidden;

            m_QServer.ToggleReceiveMachineInfo(IPAddress.Parse(info.IP), true, (isStart) => {
                
                if(isStart)
                {
                    Dispatcher.Invoke(() =>{
                        this.Visibility = Visibility.Visible;
                        Init();
                       
                    });
                }
                else
                {
                    Dispatcher.Invoke(() => {
                        this.Close();
                    });
                }
            });

            m_QNetInfoClient.NetInfo = (netInfo) => {
                Dispatcher.Invoke(() => {
                    netInfoLabel.Content = netInfo;
                });

            };

           
            m_QNetInfoClient.SystemInfo = (systemInfo) => {
                var array = systemInfo.Split('|');
                Dispatcher.Invoke(() => {
                    ProcessorCount.Text = "进程数 ： " + array[0];
                    CpuLoad.Text = "CPU使用率 ： " + array[1];
                    PhysicalMemory.Text = "物理内存 ： " + array[2];
                    
                });
            };

            m_QNetInfoClient.ProcessInfo = (processInfo) => {

                var list =new List<ProcessInfo>();
                 
               //Log.Debug("count : " + processInfo.Count);

                for(var i = 0; i < 10; i++)
                {
                    var array = processInfo[i].Split('|');
                    list.Add(new ProcessInfo() {
                        ProcessID = array[0],
                        ProcessName = array[1],
                        ProcessCPU = array[2],
                        WorkingSet = array[3],
                        ProcessPath = array[4],
                        ProcessorTime = array[5]
                    });
                   //Log.Debug("ProcessID : " + array[0] + "  ProcessName : " + array[1] + "  i = " + i);
                }
                Dispatcher.Invoke(() =>{
                   // Log.Debug("jieshu");
                    processDataGrid.ItemsSource = list;
                    processDataGrid.Items.Refresh();
                });
            };

            m_QNetInfoClient.AppsInfo = (appInfos) => {
                //Log.Debug("count : " + appInfos.Count);
                for(var i = 0; i < appInfos.Count;i++)
                {
                   
                }
            };

        }

        private void Tets(ChartPlotter plotter)
        {
            throw new NotImplementedException();
        }

        private void Init()
        {
            m_QNetInfoClient.Start(m_ClientMachineInfo.IP, 9019);

            var diskList = new List<DiskInfoType>();
            for(var i = 0; i < m_ClientMachineInfo.DiskInfo.Count; i++)
            {
                var info = m_ClientMachineInfo.DiskInfo[i];
                var array = info.Split('|');
                diskList.Add(new DiskInfoType() {
                    Name = array[0],
                    Description = array[1],
                    Size = array[2],
                    FreeSpace = array[3]
                });
            }
            diskDataGrid.ItemsSource = diskList;
            diskDataGrid.Items.Refresh();

            UserText.Text = m_ClientMachineInfo.Info.Split('|')[0];
            MachineText.Text = m_ClientMachineInfo.Machine;
            IPText.Text = m_ClientMachineInfo.IP;
        }


        private void OnWindowCloed(object sender, System.EventArgs e)
        {
            if(m_QNetInfoClient != null)
            {
                m_QNetInfoClient.Stop();
                m_QNetInfoClient = null;
            }


            Environment.Exit(0);

        }

        private void OnKillProcessClick(object sender, RoutedEventArgs e)
        {
            var path = (processDataGrid.SelectedItem as ProcessInfo).ProcessPath;
            if(string.IsNullOrEmpty(path))
            {
                return;
            }
            m_QServer.ProcessOP(IPAddress.Parse(m_ClientMachineInfo.IP), QProtocols.ProcessOp.Stop, path, "", false, false, (code) =>
            {
                if(code != QProtocols.Code.Success)
                {
                    Log.Error("[Client] OnKillProcessClick : Unknow error .");
                }
            });
        }

        private void OnStartGameClick(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(startgamePath.Text))
            {
                return;
            }
            m_QServer.ProcessOP(IPAddress.Parse(m_ClientMachineInfo.IP), QProtocols.ProcessOp.Stop, startgamePath.Text.Trim(), "", false, false, (code) =>
            {
                if (code != QProtocols.Code.Success)
                {
                    Log.Error("[Client] OnStartGameClick : Unknow error .");
                }
            });
        }

        private void OnReSetClick(object sender, RoutedEventArgs e)
        {
            m_QServer.ComputerOP(IPAddress.Parse(m_ClientMachineInfo.IP), QProtocols.ComputerOp.Restart, (code) => {
                if (code != QProtocols.Code.Success)
                {
                    Log.Error("[Client] OnStartGameClick : Unknow error .");
                }
                else
                {
                    this.Close();
                }
            });
        }

        private void OnShutdownClick(object sender, RoutedEventArgs e)
        {
            m_QServer.ComputerOP(IPAddress.Parse(m_ClientMachineInfo.IP), QProtocols.ComputerOp.Shutdown, (code) => {
                if (code != QProtocols.Code.Success)
                {
                    Log.Error("[Client] OnStartGameClick : Unknow error .");
                }
                else
                {
                    this.Close();
                }
            });
        }
    }
}
