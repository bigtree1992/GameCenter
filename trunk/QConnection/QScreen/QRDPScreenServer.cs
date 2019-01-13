using RDPCOMAPILib;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace QConnection
{
    public class QRDPScreenServer
    {
        private RDPSession m_RDPSession;
        private Action m_OnConnectedScuess;

        private Socket m_SocketAccept = null;
        //private Socket m_SocketClient = null;
        public string ContectionStr = string.Empty;


        public QRDPScreenServer()
        {
            m_RDPSession = new RDPSession();

            m_RDPSession.OnAttendeeConnected += OnAttendeeConnected;
            m_RDPSession.OnAttendeeDisconnected += OnAttendeeDisconnected;
            m_RDPSession.OnControlLevelChangeRequest += OnControlLevelChangeRequest;

            m_RDPSession.Open();

            //m_RDPSession.SetDesktopSharedRect(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            IRDPSRAPIInvitation invitation = m_RDPSession.Invitations.CreateInvitation("WinPresenter", "PresentationGroup", "", 64);  // 创建申请
            ContectionStr = invitation.ConnectionString;
            Log.Debug(ContectionStr);
        }

        public void Start(int port,Action<string> GetConnectionString , Action OnConnectedScuess)
        {
            Stop();

            m_OnConnectedScuess = OnConnectedScuess;

            try
            {
                //GetConnectionString(invitation.ConnectionString);

                var ip = Utils.GetLocalIP();
                m_SocketAccept = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                m_SocketAccept.Bind(endPoint);

                m_SocketAccept.Listen(1);
                var client = m_SocketAccept.Accept();
                var contectBytes = Encoding.Unicode.GetBytes(ContectionStr);
                var totalSize = contectBytes.Length;
                var sendSzie = 0;

                //first send header size
                var contextSizebytes = BitConverter.GetBytes(totalSize);
                while (4 - sendSzie > 0)
                {
                    sendSzie += client.Send(contextSizebytes, sendSzie, 4 - sendSzie, SocketFlags.None);
                }

                sendSzie = 0;
                // send content
                while (totalSize - sendSzie > 0)
                {
                    sendSzie += client.Send(contectBytes, sendSzie, totalSize - sendSzie, SocketFlags.None);
                }
                Log.Debug("ConnectionString : " + ContectionStr);
            }
            catch (Exception e)
            {
                Log.Error("[QRDPScreenServer] Start  Error : " + e);
            }
        }

        public void Stop()
        {
            if(m_RDPSession != null)
            {
                m_RDPSession.Close();
                m_RDPSession = null;
            }
            if( m_SocketAccept != null )
            {
                m_SocketAccept.Dispose();
                m_SocketAccept = null;
            }
        }

        private void OnAttendeeConnected(object pObjAttendee)
        {
            IRDPSRAPIAttendee pAttendee = pObjAttendee as IRDPSRAPIAttendee;
            pAttendee.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_VIEW;
            Log.Debug("[QRDPScreenServer] OnAttendeeConnected Debug : OnAttendeeConnected." + pAttendee.RemoteName + Environment.NewLine);

        }

        private void OnAttendeeDisconnected(object pDisconnectInfo)
        {
            IRDPSRAPIAttendeeDisconnectInfo pDiscInfo = pDisconnectInfo as IRDPSRAPIAttendeeDisconnectInfo;
            //LogTextBox.Text += ("Attendee Disconnected: " + pDiscInfo.Attendee.RemoteName + Environment.NewLine);
            Log.Debug("[QRDPScreenServer] OnAttendeeDisconnected Error : " + pDiscInfo.Attendee.RemoteName + Environment.NewLine);

        }

        private void OnControlLevelChangeRequest(object pObjAttendee, CTRL_LEVEL RequestedLevel)
        {
            IRDPSRAPIAttendee pAttendee = pObjAttendee as IRDPSRAPIAttendee;
            pAttendee.ControlLevel = RequestedLevel;
            Log.Debug("[QRDPScreenServer] OnControlLevelChangeRequest Debug : OnControlLevelChangeRequest.");

        }
    }
}
