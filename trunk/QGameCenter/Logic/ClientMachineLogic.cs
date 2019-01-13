using System;
using System.Net;
using QGameCenterLogic;
using System.Collections.Generic;
using QGameCenter.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows;

namespace QGameCenter
{
    public class ClientMachineLogic
    {
        private Dictionary<IPAddress, ClientMachineInfo> m_ClientMachineDict;

        private QServer m_Server;

        private Window m_Window;
        private DataGrid m_DataGrid;

        private List<string> m_DiskInfos;

        public ClientMachineLogic(QServer server,Window window, DataGrid dateGrid)
        {
            m_ClientMachineDict = new Dictionary<IPAddress, ClientMachineInfo>();
            m_DiskInfos = new List<string>(); 
            m_Server = server;

            m_Server.OnClientConnected += OnClientConnected;

            m_Window = window;
            m_DataGrid = dateGrid;
            m_DataGrid.SelectedCellsChanged += (sender, e) => {
                SelectedCellsChanged();
            };
        }
        /// <summary>
        /// 当有一个client连接后，向连接的client索取电脑信息
        /// </summary>
        /// <param name="ip"></param>
        private void OnClientConnected(IPAddress ip)
        {
           
            if (m_ClientMachineDict.ContainsKey(ip))
            {
                return;
            }
            m_Server.GetClientMachineInfo(ip, (ip1, machineCode, info, diskInfo) =>
            {

                if (m_ClientMachineDict.ContainsKey(IPAddress.Parse(ip1)))
                {
                    return;
                }

                var id = m_ClientMachineDict.Count + 1;
                m_ClientMachineDict.Add(IPAddress.Parse(ip1), new ClientMachineInfo()
                {
                    ID = id,
                    IP = ip1,
                    Machine = machineCode,
                    Info = info,
                    DiskInfo = diskInfo
                });

                //m_ClientMachineDict.Add(IPAddress.Parse("192.168.1.101"), new ClientMachineInfo()
                //{
                //    ID = 123,
                //    IP = "123435",
                //    Machine = "asdasd",
                //    Info = "asdasdas",
                //    DiskInfo = diskInfo
                //});

                m_Window.Dispatcher.Invoke(() =>
                {

                    var list = m_ClientMachineDict.Values.ToList();
                    m_DataGrid.ItemsSource = list;
                    m_DataGrid.Items.Refresh();
                });
                
            });
        }

        private void SelectedCellsChanged()
        {
            var ip = (m_DataGrid.SelectedItem as ClientMachineInfo).IP;

            var info = m_ClientMachineDict[IPAddress.Parse(ip)];
            if(info== null)
            {
                Log.Error("[ClientMachineLogic] SelectedCellsChanged Error : info == null , ip = " + ip);
                return;
            }
            
            var win = new ClientInfo(m_Server,info);
            win.Topmost = true;
            win.Show();


        }


        public void Clear()
        {
            m_ClientMachineDict.Clear();
        }


    }
}
