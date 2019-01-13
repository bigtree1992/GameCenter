using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace QClientNS
{
    public class MyNotifyIcon
    {
        private NotifyIcon m_NotifyIcon;
        private Window m_Window;

        public MyNotifyIcon(Window win)
        {
            m_Window = win;

            this.m_NotifyIcon = new NotifyIcon();
            this.m_NotifyIcon.BalloonTipText = "系统监控中... ...";
            this.m_NotifyIcon.ShowBalloonTip(2000);
            this.m_NotifyIcon.Text = "系统监控中... ...";
            this.m_NotifyIcon.Visible = true;

            if (File.Exists(@"AppIcon.ico"))
            {
                this.m_NotifyIcon.Icon = new Icon(@"AppIcon.ico");
            }
            else
            {
                var path = Process.GetCurrentProcess().MainModule.FileName;
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon != null)
                {
                    this.m_NotifyIcon.Icon = icon;
                }
            }

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

            this.m_NotifyIcon.MouseDoubleClick += new MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) this.Show(o, e);
            });
        }

        private void About(object sender, EventArgs e)
        {
            System.Windows.MessageBox.Show(
                @"奇境森林客户端控制程序", "深圳奇境森林科技有限公司");
        }

        private void Show(object sender, EventArgs e)
        {
            m_Window.Visibility = Visibility.Visible;
            m_Window.WindowState = WindowState.Normal;
            m_Window.ShowInTaskbar = true;
            m_Window.Activate();
        }

        private void Hide(object sender, EventArgs e)
        {
            m_Window.Visibility = Visibility.Hidden;
        }

        private void Close(object sender, EventArgs e)
        {
            Dispose();
            System.Windows.Application.Current.Shutdown();
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
