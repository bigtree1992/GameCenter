using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace QControlService
{
    public partial class MainForm : Form
    {
        private QControlService m_ControlService;
        public MainForm()
        {
            InitializeComponent();
                        
            m_ControlService = new QControlService();
            m_ControlService.Start();
            var icon = new MyNotifyIcon(m_NotifyIcon, this);
            icon.OnClosed += OnClosed;
            m_NotifyIcon.ShowBalloonTip(0, "", "准备连接中...", ToolTipIcon.Info);

            this.Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            this.Hide();
        }

        private void OnClosed()
        {
            m_ControlService.Stop();
        }
    }
}
