using System;
using System.Drawing;
using System.Windows.Forms;

namespace QControlManagerNS
{
    public partial class MainForm : Form
    {
        private QControlManager m_ControlManager;
        private KeyboardHook m_keyboardHook;

        public MainForm()
        {
            InitializeComponent();

            m_ControlManager = new QControlManager();
            m_ControlManager.StartFind();

            m_TabControl.OnAddTabPage += OnAddTabPage;
            m_TabControl.OnCloseTabPage += OnCloseTabPage;

            m_keyboardHook = new KeyboardHook();
            m_keyboardHook.InstallHook();
            m_keyboardHook.OnKeyUp += OnGlobalKeyUp;
        }

        private AxRDPCOMAPILib.AxRDPViewer m_AxRDPViewer;
        private Size m_PreSize;

        private void OnGlobalKeyUp(Keys key)
        {
            if(key == Keys.F11)
            {
                if(m_TabControl.SelectedIndex < 0)
                {
                    return;
                }
                var page = m_TabControl.TabPages[m_TabControl.SelectedIndex];

                if (m_AxRDPViewer == null)
                {
                    this.WindowState = FormWindowState.Normal;

                    this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                   
                    int SH = Screen.PrimaryScreen.Bounds.Height;
                    int SW = Screen.PrimaryScreen.Bounds.Width;
                    m_AxRDPViewer = page.Controls[0] as AxRDPCOMAPILib.AxRDPViewer;
                    m_PreSize = m_AxRDPViewer.ClientSize;

                    page.Controls.RemoveAt(0);
                    this.Controls.Add(m_AxRDPViewer);

                    m_AxRDPViewer.ClientSize = new Size(SW, SH);
                    m_AxRDPViewer.BringToFront();
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                    this.WindowState = FormWindowState.Normal;
                    this.Controls.Remove(m_AxRDPViewer);
                    page.Controls.Add(m_AxRDPViewer);
                    
                    m_AxRDPViewer = null;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            m_ControlManager.StopFind();
            m_keyboardHook.UnInstallHook();
        }

        private void OnAddTabPage(int index)
        {
            var clientList = new ClientListForm(m_ControlManager);
            clientList.StartPosition = FormStartPosition.CenterParent;
            clientList.OnConnectionResult += OnConnectionResult;
            clientList.ShowDialog();
        }

        private void OnConnectionResult(string ip)
        {                  
            var axRDPViewer = new AxRDPCOMAPILib.AxRDPViewer();
            axRDPViewer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            axRDPViewer.Dock = DockStyle.Fill;
            axRDPViewer.Enabled = true;
            axRDPViewer.Location = new Point(0, 0);
            axRDPViewer.Name = "RDPViewer";           
            axRDPViewer.TabIndex = 0;
            
            var tabPage1 = new TabPage();
            tabPage1.Controls.Add(axRDPViewer);
            tabPage1.Location = new Point(1, 29);
            tabPage1.Name = ip;
            tabPage1.Padding = new Padding(3);
            tabPage1.TabIndex = 0;
            tabPage1.Text = ip;
            tabPage1.UseVisualStyleBackColor = true;

            m_TabControl.Controls.Add(tabPage1);

            m_ControlManager.StartConnection(axRDPViewer, ip);
        }

        private void OnCloseTabPage(string ip)
        {
            m_ControlManager.CloseConnection(ip);
        }

    }
}
