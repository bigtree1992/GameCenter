using Lemony.SystemInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QConnection
{
    public class QNetInfoServer
    {

        private Thread m_ThreadAccped = null;
        private Thread m_ThreadSend = null;

        private Socket m_SocketAccept = null;
        private Socket m_ClientSocket = null;

        private bool m_Running;
        private int m_Frequency;

        private bool m_Sending;

        private SystemInfo m_SystemInfo;
        private int m_SendProcessTimer;

        public QNetInfoServer()
        {

        }


        public void Start(int port, int frequency)
        {
            try
            {
                m_Frequency = frequency;
                m_SocketAccept = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Any, port);
                m_SocketAccept.Bind(endPoint);
                m_SocketAccept.Listen(10);

                m_ThreadAccped = new Thread(new ThreadStart(OnAcceptSocket));
                m_ThreadAccped.Start();
            }
            catch(Exception e)
            {
                Log.Error("[QNetInfoServer] Start Error : " + e.ToString());
            } 


        }

        private void OnAcceptSocket()
        {
            m_Running = true;
           

            while (m_Running)
            {
                Socket socket = null;
                try
                {
                    socket = m_SocketAccept.Accept();
                }
                catch (Exception e)
                {
                    if (m_Running)
                    {
                        Log.Error("[QNetInfoServer] AcceptSocket Error:" + e.Message);
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                StopSending();
                m_SystemInfo = new SystemInfo();
                m_SendProcessTimer = 100;
                m_ClientSocket = socket;
                m_ClientSocket.SendBufferSize = 1024 * 1024;
                m_ClientSocket.ReceiveBufferSize = 1024 * 1024;
                m_Frequency = 1000;
                m_ThreadSend = new Thread(OnSendInfoData);
                m_ThreadSend.Start();
            }
            
        }
        
        private void OnSendInfoData()
        {
            m_Sending = true;
            while(m_Sending)
            {
                //发送网络信息
                OnSendNetInfo();
                //发送系统信息
                OnSendSystemInfo();

                //发送进程信息
                m_SendProcessTimer++;
                if(m_SendProcessTimer > 30)
                {
                    m_SendProcessTimer = 0;
                    OnSendSystemProcessInfo();
                }

                //发送当前运行的应用信息
                OnSendSystemAppInfo();

                Thread.Sleep(m_Frequency);
            }
        }
       
        private void OnSendNetInfo()
        {
            //刷新网络信息
            var nInfo = SystemInfo.GetNetInfoByIp(Utils.GetLocalIP());
            var data = "名称 ： " + nInfo.Name + "\n类型 ： " + nInfo.Type.ToString() + "\n状态 ： " + nInfo.Status.ToString() + "\n速度 ：" + nInfo.Speed.ToString() +
                            "\n总接收 ： " + nInfo.InOctets.ToString() + "\n总发送 : " + nInfo.OutOctets.ToString() + "\n总错收 : " + nInfo.InErrors.ToString() +
                            "\n总错发 ： " + nInfo.OutErrors.ToString() + "\n未知协议 : " + nInfo.InUnknownProtos.ToString() + "\n物理地址 ： " + nInfo.PhysAddr;
            OnSendData(data);
        }

        private void OnSendSystemInfo()
        {
            var data = "";
            var processCount = m_SystemInfo.GetProcessInfo().Count;
            var cpuLoad = string.Format("{0:f}%", m_SystemInfo.CpuLoad);

            var pMemory = m_SystemInfo.PhysicalMemory;
            var aMemory = m_SystemInfo.MemoryAvailable;
            var lMemory = pMemory - aMemory;
            var physicalMemory = string.Format("{0:f}%", (lMemory/ (float)pMemory) * 100);
            data = processCount + "|" + cpuLoad + "|" + physicalMemory;
            OnSendData(data);

        }
        private void OnSendSystemProcessInfo()
        {

            var pInfoList = new List<string>();

            var htProcess = new Hashtable();  //进程哈希表
            var pInfo = m_SystemInfo.GetProcessInfo();

            for (var i = 0; i < pInfo.Count; i++)
            {
                htProcess.Add(pInfo[i].ProcessID.ToString(), pInfo[i].ProcessID);

                var processId = pInfo[i].ProcessID.ToString();
                var processName = pInfo[i].ProcessName;
                var cpu = string.Format("{0:00}", 0);
                var workingSet = pInfo[i].WorkingSet.ToString();
                var processPath = pInfo[i].ProcessPath;
                var processorTime = pInfo[i].ProcessorTime;
                pInfoList.Add(processId + "|" + processName + "|" + cpu + "|" + workingSet + "|" + processPath + "|" + processorTime);

            }
            OnSendData(pInfoList);
        }

        private IntPtr m_CurrentHandle;

        public IntPtr CurrentHandle
        {
            get
            {
                return m_CurrentHandle;
            }

            set
            {
                m_CurrentHandle = value;
            }
        }

        private void OnSendSystemAppInfo()
        {
            //var apps = SystemInfo.FindAllApps(CurrentHandle.ToInt32());
            //OnSendData(apps);

            var apps = new List<string>();

            var ps = Process.GetProcesses();
            for(var i = 0; i < ps.Length;i++)
            {
                if(ps[i].MainWindowHandle == null)
                {
                    continue;
                }
                apps.Add(ps[i].MainWindowTitle);
            }

            OnSendData(apps);
        }

        
        private void OnSendData(string data)
        {
            if (m_ClientSocket != null && !m_ClientSocket.Connected)
            {
                return;
            }

            var dataSize = data.Length;
            var dataSizeBuffer = BitConverter.GetBytes(dataSize);
            var sendedSize = 0;
            //首先发送NetInfo头部4字节表示缓冲区长度
            while (4 - sendedSize > 0)
            {
                try
                {
                    sendedSize += m_ClientSocket.Send(dataSizeBuffer, sendedSize, 4 - sendedSize, SocketFlags.None);
                }
                catch (SocketException)
                {
                    break;
                }
            }
            sendedSize = 0;
            var bodyBuffer = Encoding.UTF8.GetBytes(data);
            while (dataSize - sendedSize != 0)
            {
                try
                {
                    sendedSize += m_ClientSocket.Send(bodyBuffer, sendedSize, dataSize - sendedSize, SocketFlags.None); 
                }
                catch (SocketException)
                {
                    //Log.Error("[QScreenServer] SendRawData2 Error : " + e);
                    break;
                }
            }

         
        }
        private void OnSendData(List<string> datas)
        {
            if (m_ClientSocket != null && !m_ClientSocket.Connected)
            {
                return;
            }
            //首先发送list的长度
            var dataSize = datas.Count;
            var dataSizeBuffer = BitConverter.GetBytes(dataSize);
            var sendedSize = 0;
            //首先发送NetInfo头部4字节表示缓冲区长度
            while (4 - sendedSize > 0)
            {
                try
                {
                    sendedSize += m_ClientSocket.Send(dataSizeBuffer, sendedSize, 4 - sendedSize, SocketFlags.None);
                }
                catch (SocketException)
                {
                    break;
                }
            }


            for (var i = 0; i < datas.Count;i++)
            {
                OnSendData(datas[i]);
            }
            
        }
       

        private void StopSending()
        {
            m_Sending = false;

            if (m_ThreadSend != null && m_ThreadSend.IsAlive)
            {
                try
                {
                    m_ThreadSend.Abort();
                }
                catch { }
            }

            if (m_ClientSocket != null && m_ClientSocket.Connected)
            {
                try
                {
                    m_ClientSocket.Close();
                }
                catch { }
            }
            if (m_SystemInfo != null)
            {
                m_SystemInfo = null;
            }
            m_SendProcessTimer = 0;
        }

        public void Stop()
        {
            StopSending();
            try
            {
                m_Running = false;

                if (m_SocketAccept != null)
                {
                    m_SocketAccept.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error("[QNetInfoServer] Stop Error " + e.Message);
            }
            finally
            {
                m_ClientSocket = null;
                m_ThreadSend = null;
                m_ThreadAccped = null;
                m_SocketAccept = null;
            }



        }




    }
}
