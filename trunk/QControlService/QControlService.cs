using System;
using System.Text;
using System.Threading.Tasks;
using RDPCOMAPILib;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace QControlService
{
    internal class QControlService
    {
        private const int Port = 4939;

        internal enum Event
        {
            Connect,
            Disconnect,
            Error
        }

        private RDPSession m_RDPSession;
        private bool m_Running = false;
        private Action<Event, string> OnAttendee;
        private UdpClient m_UdpClient;
        private CancellationTokenSource m_TokenSource;

        internal void Start()
        {
            try
            {
                m_RDPSession = new RDPSession();
                m_RDPSession.OnAttendeeConnected +=
                    OnAttendeeConnected;
                m_RDPSession.OnAttendeeDisconnected +=
                    OnAttendeeDisconnected;
                m_RDPSession.OnControlLevelChangeRequest +=
                    OnControlLevelChangeRequest;

                m_RDPSession.SetDesktopSharedRect(
                    0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                m_RDPSession.Open();

                var invitation = m_RDPSession.Invitations.CreateInvitation(
                    "FantasyForest", "FantasyForestGroup", "", 4);

                StartBrocast(invitation.ConnectionString);
                
                m_Running = true;
            }
            catch(Exception e)
            {
                MessageBox.Show("QControlService", e.Message);
            }
        }

        private void StartBrocast(string ConnectionString)
        {
            var broadCastEp = new IPEndPoint(IPAddress.Broadcast, QControlService.Port);
            m_UdpClient = new UdpClient();
            m_UdpClient.EnableBroadcast = true;
            m_TokenSource = new CancellationTokenSource();

            var task = new Task(() =>
            {
                while (!m_TokenSource.IsCancellationRequested)
                {
                    var dataStr = $"{Utils.GetLocalIP()}|{ConnectionString}";
                    var buf = Encoding.UTF8.GetBytes(dataStr);
                    m_UdpClient.Send(buf, buf.Length, broadCastEp);
                    Thread.Sleep(2000);
                }

            }, m_TokenSource.Token);

            task.ContinueWith(t =>
            {
                m_UdpClient.Close();
                m_UdpClient = null;
            });

            task.Start();
        }

        internal void Stop()
        {
            try
            {
                m_TokenSource.Cancel();
                m_RDPSession.Close();
            }
            finally
            {
                m_Running = true;
            }
        }

        private void OnControlLevelChangeRequest(object pAttendee, CTRL_LEVEL requestedLevel)
        {
            // Do nothing...
        }

        private void OnAttendeeConnected(object pObjAttendee)
        {
            var pAttendee = pObjAttendee as IRDPSRAPIAttendee;
            pAttendee.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE;
            try
            {
                OnAttendee?.Invoke(Event.Connect, pAttendee.RemoteName);
            }
            catch
            {

            }
        }

        private void OnAttendeeDisconnected(object pDisconnectInfo)
        {
            var pDiscInfo = pDisconnectInfo as IRDPSRAPIAttendeeDisconnectInfo;
            try
            {
                OnAttendee?.Invoke(Event.Disconnect, pDiscInfo.Attendee.RemoteName);
            }
            catch
            {

            }
              
        }

    }
}
