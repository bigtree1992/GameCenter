using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
//using AxRDPCOMAPILib;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

namespace QConnection
{
    public class QRDPScreenClient
    {
       
        //private AxRDPViewer m_AxRDPViewer;

        private WindowsFormsHost m_WindowsFormsHost;

        private double m_Width;
        private double m_Height;

        public QRDPScreenClient(WindowsFormsHost winform,double width, double height)
        {
            m_WindowsFormsHost = winform;
            m_Width = width;
            m_Height = height;
            //m_AxRDPViewer = new AxRDPViewer();

            Log.Debug("width : " + width + "   height : " + height);

           // m_AxRDPViewer.AccessibleRole = AccessibleRole.None;
           // m_AxRDPViewer.Dock = DockStyle.None;
           // m_AxRDPViewer.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
           // m_AxRDPViewer.Enabled = true;
           // m_AxRDPViewer.Location = new System.Drawing.Point(0, 0);
           // m_AxRDPViewer.Name = "ClientRDPViewer";
           // m_AxRDPViewer.OcxState = null;
           // //m_AxRDPViewer.Size = new System.Drawing.Size(1032, 447);
           //// m_AxRDPViewer.Size = new System.Drawing.Size((int)m_Width, (int)m_Height);
           // m_AxRDPViewer.TabIndex = 0;

           // m_AxRDPViewer.BeginInit();
           // m_WindowsFormsHost.Child = m_AxRDPViewer;
           // m_AxRDPViewer.EndInit();

           // m_AxRDPViewer.SmartSizing = true;


           // m_AxRDPViewer.OnConnectionEstablished += OnConnectionEstablished;
           // m_AxRDPViewer.OnConnectionFailed += OnConnectionFailed;
           // m_AxRDPViewer.OnConnectionTerminated += OnConnectionTerminated;
           // m_AxRDPViewer.OnError += OnError;

        }

        public void Start(string connectionStr,WindowsFormsHost winform)
        {
            //Stop();

            //m_WindowsFormsHost = winform;
            //m_AxRDPViewer.BeginInit();
            //m_WindowsFormsHost.Child = m_AxRDPViewer;
            //m_AxRDPViewer.EndInit();

            ////m_Width = width;
            ////m_Height = height;
            ////m_AxRDPViewer.SmartSizing = true;

            //m_AxRDPViewer.Connect(connectionStr, Environment.UserName, "");
        }
        
        public void Stop()
        {
            //if (m_AxRDPViewer != null)
            //{
            //    m_AxRDPViewer.Dispose();
            //    m_AxRDPViewer = null;
            //}
           
          

        }
        
       
        /// <summary>
        /// 建立连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionEstablished(object sender, EventArgs e)
        {
            //LogTextBox.Text += "Connection Established" + Environment.NewLine;
            Log.Error("[QRDPScreenClient] OnConnectionEstablished Debug : Connection Established " + Environment.NewLine);

        }
        /// <summary>
        /// 连接结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnConnectionTerminated(object sender, _IRDPSessionEvents_OnConnectionTerminatedEvent e)
        //{
        //    Log.Error("[QRDPScreenClient] Connection Terminated. Reason: " + e.discReason + Environment.NewLine);

        //}
        /// <summary>
        /// 建立连接失败
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionFailed(object sender, EventArgs e)
        {
            Log.Error("[QRDPScreenClient] OnConnectionFailed Error : Connection Failed." + Environment.NewLine);

        }
        /// <summary>
        /// 建立连接发生错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void OnError(object sender, _IRDPSessionEvents_OnErrorEvent e)
        //{
        //    int ErrorCode = (int)e.errorInfo;
        //    Log.Error("[QRDPScreenClient] OnError Error : 0x" + ErrorCode.ToString("X") + Environment.NewLine);
        //}

       


    }
}
