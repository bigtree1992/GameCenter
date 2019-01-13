using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace QGameCenterLogic
{
    public class TopWinLogic
    {
        private bool m_ThreadRunning = false;
        private Thread m_SetWindowThread;
        
        private Window m_Window;
        private string m_GamePath;

        public Action<bool> StopGame;

        public TopWinLogic()
        {
        }

        public void Clear()
        {
            m_ThreadRunning = false;

            if (m_SetWindowThread != null)
            {
                m_SetWindowThread.Abort();
                m_SetWindowThread = null;
            }
            if(StopGame != null)
            {
                StopGame = null;
            }
        }

        public void Start(string gamePath)
        {
            Stop();
            m_ThreadRunning = true;
            m_GamePath = gamePath;
            m_SetWindowThread = new Thread(SetWindow);
            m_SetWindowThread.Start();

        }

        public void Stop()
        {
            m_ThreadRunning = false;

            if (m_SetWindowThread != null)
            {
                m_SetWindowThread.Abort();
                m_SetWindowThread = null;
            }
        }

        public void BindUI(Window window,Button button)
        {
            m_Window = window;
            button.Click += (sender, e) => { OnWindow_GotFocus(); };
        }

        public void UnBindUI()
        {
            m_Window = null;
        }

        /// <summary>
        /// 成功开启游戏后，设置顶部窗体
        /// </summary>
        private void SetWindow()
        {
            while (m_ThreadRunning)
            {
                m_Window.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var wndHelper = new WindowInteropHelper(m_Window);

                        IntPtr hwnd = wndHelper.Handle;
                        if (hwnd == null)
                        {
                            m_ThreadRunning = false;
                            return;
                        }
                        SetWindowPos(hwnd, -1, 900, 0, 0, 0, SWP_NOACTIVATE | SWP_NOSIZE);
                    }
                    catch
                    {
                        m_ThreadRunning = false;
                    }
                });
                Thread.Sleep(2000);
            }

            try
            {
                //m_Window.Close();
            }
            catch
            {

            }
        }

        private void OnWindow_GotFocus()
        {
            if (string.IsNullOrEmpty(m_GamePath))
            {
                Log.Error("[VRKartTopWinLogic] OnWindow_GotFocus Error : m_GamePath = null.");
                return;
            }
            m_Window.Visibility = Visibility.Hidden;
            if (MessageBox.Show("是否确定关闭游戏", "关闭", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                //获取到当前连接的客户端
                var processName = Path.GetFileNameWithoutExtension(m_GamePath);
                var ps = Process.GetProcessesByName(processName);
                if (ps.Length < 1)
                {
                    return;
                }
                ps[0].Kill();
                if(StopGame != null)
                {
                    StopGame(false);
                }
            }
            else
            {
                m_Window.Visibility = Visibility.Visible;
            }
        }


        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_NOREDRAW = 0x0008;
        const int SWP_NOACTIVATE = 0x0010;
        const int SWP_FRAMECHANGED = 0x0020;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SWP_HIDEWINDOW = 0x0080;
        const int SWP_NOCOPYBITS = 0x0100;
        const int SWP_NOOWNERZORDER = 0x0200;
        const int SWP_NOSENDCHANGING = 0x0400;
        const int TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);


    }
}
