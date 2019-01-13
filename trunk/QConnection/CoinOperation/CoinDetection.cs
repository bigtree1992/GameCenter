using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace QConnection
{
    /// <summary>
    /// 用于投币检测
    /// </summary>
    public class CoinDetection
    {
        /// <summary>
        /// 通知检测到了有一个硬币投进去了
        /// </summary>
        public Action<int> OnCoinDetected;

        /// <summary>
        /// 异常返回
        /// </summary>
        public Action<string> OnErrorReported;

        /// <summary>
        /// usb端口号名称
        /// TODO :  需要使用配置文件进行动态设置
        /// </summary>
        //const string Pattern = "Prolific USB-to-Serial Comm Port";

        private static string GetCoinPort(string Pattern)
        {
            try
            {
                using (var searcher =
                       new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        string port = hardInfo.Properties["Name"].Value as string;

                        if (port.Contains(Pattern))
                        {
                            port = port.Replace(Pattern, "");
                            port = port.Replace('(', ' ');
                            port = port.Replace(')', ' ');
                            port = port.Trim();
                            return port;
                        }

                    }
                }
            }
            catch (Exception)
            {

            }
            finally
            {
            }
            return "";
        }



        /// <summary>
        /// 开始跟投币器的通讯,检测是否有投入了硬币
        /// </summary>
        public void Start(string Pattern)
        {
            m_SerialPort = new SerialPort();

            if (m_SerialPort.IsOpen)
            {
                return;
            }

            string portname = GetCoinPort(Pattern);
            if (string.IsNullOrEmpty(portname))
            {
                Log.Error("[CoinDetection] : portname为空");
                throw new Exception("无法找到刷卡器的端口，请查询是否连接刷卡器以及配置正确的刷卡器端口。");
                //Environment.Exit(0);
                //return;
            }
            m_SerialPort.PortName = portname;
            m_SerialPort.BaudRate = 9600;
            m_SerialPort.StopBits = StopBits.One;
            m_SerialPort.Parity = Parity.None;
            //m_SerialPort.RtsEnable = false;
            m_SerialPort.DataBits = 8;

            try
            {
                m_SerialPort.Open();
            }
            catch (Exception)
            {
                m_SerialPort = new SerialPort();
                ReportError("[CoinDetection] Open SerialPort Failed:" + portname);
            }

            //ReportError("[CoinDetection] COMX Connected.");

            m_IsClosing = false;
            m_IsDataReciving = false;

            m_Thread = new Thread(ThreadFunction);
            m_Thread.Start();
        }

        /// <summary>
        /// 结束跟投币器的通讯。
        /// </summary>
        public void Stop()
        {
            try
            {
                if (m_SerialPort.IsOpen)
                {
                    if (!m_IsDataReciving)
                    {
                        m_IsClosing = true;
                        m_SerialPort.Close();
                    }
                    else
                    {
                        ReportError("[CoinDetection] Close Port While DataRecivind.");
                    }
                }
                else
                {
                    //ReportError("[CoinDetection] Port is already Closed.");
                }
            }
            catch(Exception e)
            {
                Log.Error("[CoinDeetection] Stop Error : " + e.Message);
            }
           
        }

        private SerialPort m_SerialPort;

        private Thread m_Thread;

        private byte[] data = { 0x02, 0x08, 0x10, 0x7F, 0x10, 0x00, 0x03, 0x77 };

        private void ThreadFunction()
        {
            try
            {
                m_SerialPort.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                ReportError("[CoinDetection] Write Data Failed:" + e.Message);
            }

            while (!m_IsClosing)
            {
                SerialPortOnDataReceived();
                Thread.Sleep(50);
            }
        }

        private enum State
        {
            FindHead = 0,
            CheckEnd = 1
        }

        private State m_State = State.FindHead;

        private byte[] m_tmp_buf = new byte[1024];

        private List<byte> m_Buffer = new List<byte>();

        private int m_HeadStart = 0;

        private bool m_IsDataReciving = false;

        private bool m_IsClosing = false;

        private void ProcessFindHead()
        {
            if (m_Buffer.Count < 2)
            {
                return;
            }

            for (int i = 0; i < (m_Buffer.Count - 1); i++)
            {
                if (m_Buffer[i] == 0x02 && m_Buffer[i + 1] == 0x0b)
                {
                    m_HeadStart = i;
                    m_State = State.CheckEnd;
                    break;
                }
            }
        }

        private void ProcessCheckEnd()
        {
            if (m_Buffer.Count - m_HeadStart < 11)
            {
                return;
            }

            //说明找到了合法的数据进行处理
            //if (m_Buffer[m_HeadStart + 3] == 0x10)
            if (m_Buffer[m_HeadStart + 3] == 0x10 && m_Buffer[m_HeadStart + 10] == 0x1A)
            {
                SendValue();
            }

            //重置Buff状态
            m_State = State.FindHead;
            m_HeadStart = 0;
            m_Buffer.RemoveRange(0, 11);

            try
            {
                m_SerialPort.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                ReportError("[CoinDetection] Write Data Failed:" + e.Message);
            }
        }

        private void SerialPortOnDataReceived(/*object sender1, SerialDataReceivedEventArgs eventa*/)
        {
            if (!m_SerialPort.IsOpen)
            {
                ReportError("[CoinDetection] Port Not Opened!");
                return;
            }

            try
            {
                int datatoread = 1;
                m_SerialPort.Read(m_tmp_buf, 0, datatoread);
                m_Buffer.Add(m_tmp_buf[0]);

                switch (m_State)
                {
                    case State.FindHead:
                        ProcessFindHead();
                        break;
                    case State.CheckEnd:
                        ProcessCheckEnd();
                        break;
                }
            }
            catch (Exception e)
            {
                ReportError("[CoinDetection] Read Data Failed! " + e.ToString());
            }

        }

        private void SendValue()
        {
            //ReportError("[CoinDetection] SendValue " + m_Buffer[m_HeadStart + 2] * 1.41f);

            if (OnCoinDetected == null)
            {
                return;
            }

            if (m_IsClosing)
            {
                return;
            }

            try
            {
                m_IsDataReciving = true;
                OnCoinDetected(1);
            }
            catch (Exception)
            {

            }
            finally
            {
                m_IsDataReciving = false;
            }

        }

        private void ReportError(string info)
        {
            if (OnErrorReported != null)
                OnErrorReported(info + "\n");
        }

        private void ReportDebug(string info)
        {
            if (OnErrorReported != null)
                OnErrorReported(info);
        }

    }
}
