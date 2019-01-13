using System;
using System.Windows.Forms;

namespace QControlService
{
    internal class MyNotifyIcon
    {
        internal Action OnClosed;
        private NotifyIcon m_NotifyIcon;
        private Form m_Form;

        internal MyNotifyIcon(NotifyIcon icon, Form form)
        {
            m_NotifyIcon = icon;
            m_Form = form;

            var open = new MenuItem("打开");
            open.Click += new EventHandler(Show);

            var hide = new MenuItem("隐藏");
            hide.Click += new EventHandler(Hide);

            var about = new MenuItem("关于");
            about.Click += new EventHandler(About);

            var exit = new MenuItem("关闭");
            exit.Click += new EventHandler(Close);

            //关联托盘控件
            var childen = new MenuItem[] { open, hide, about, exit };
            m_NotifyIcon.ContextMenu = new ContextMenu(childen);
            m_NotifyIcon.MouseDoubleClick += new MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Show(o, e);
            });
        }

        private void About(object sender, EventArgs e)
        {
            MessageBox.Show(
                @"奇境森林远程服务程序", "深圳奇境森林科技有限公司");
        }

        private void Show(object sender, EventArgs e)
        {
            m_Form.Show();
        }

        private void Hide(object sender, EventArgs e)
        {
            m_Form.Hide();
        }

        private void Close(object sender, EventArgs e)
        {
            Dispose();
            if(OnClosed != null) { OnClosed(); }
            Application.Exit();
        }

        public void Dispose()
        {
            if (m_NotifyIcon != null)
            {
                try
                {
                    m_NotifyIcon.Dispose();
                }
                catch
                {

                }
            }
        }
    }
}
