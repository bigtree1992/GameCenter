using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NetFwTypeLib;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace QClientNS
{
    public class EnvConfig
    {
        public static void SetEnv()
        {
            SetAutoStart();
            SetQControlServiceAutoStart();
            SleepCtr(true);
            SetNetEnv();
            NetFwAddApp("QClient", Process.GetCurrentProcess().MainModule.FileName);
            SetTheme();
            AutoCloseSetThemeWindow();
        }

        private static void SetAutoStart()
        {
            var key = Registry.LocalMachine.CreateSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            key.DeleteValue("奇境森林中控客户端", false);
            key.SetValue("奇境森林中控客户端", 
                Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).FullName 
                    + "\\奇境森林中控客户端.exe");
        }

        private static void SetQControlServiceAutoStart()
        {
            var path = Directory.GetParent(
                    AppDomain.CurrentDomain.BaseDirectory).FullName
                    + "\\远程操作系统.exe";
            if (!File.Exists(path))
            {
                Log.Error("[EnvConfig] SetQControlServiceAutoStart Error : 未找到远程操作系统");
                return;
            }

            var key = Registry.LocalMachine.CreateSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            key.DeleteValue("QControlService", false);
            key.SetValue("QControlService", path);
        }

        #region 控制关闭系统休眠时间 
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);

        [Flags]
        enum ExecutionFlag : uint
        {
            System = 0x00000001,
            Display = 0x00000002,
            Continus = 0x80000000,
        }

        private static void SleepCtr(bool closeSleep)
        {
            try
            {
                if (closeSleep)
                {
                    //阻止休眠时调用
                    SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display | ExecutionFlag.Continus);
                }
                else
                {
                    //恢复休眠时调用
                    SetThreadExecutionState(ExecutionFlag.Continus);
                }
            }
            catch (Exception e)
            {
                Log.Error("[EnvConfig] SleepCtr Error : " + e.Message);
            }
        }
        #endregion

        #region 设置网络为工作网络
        private static void SetNetEnv()
        {
            try
            {
                var localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey RKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles");
                var d1 = RKey.GetSubKeyNames();
                RKey.Close();
                for (int i = 0; i < d1.Length; i++)
                {
                    RegistryKey key = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles\" + d1[i], true);
                    key.SetValue("Category", 1, RegistryValueKind.DWord);
                    key.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error("[EnvConfig] SetNetEnv Error : " + e.Message);
            }
        }
        #endregion

        #region 设置应用程序到防火墙例外

        /// <summary>
        /// 将应用程序添加到防火墙例外
        /// </summary>
        /// <param name="name">应用程序名称</param>
        /// <param name="executablePath">应用程序可执行文件全路径</param>
        public static void NetFwAddApp(string name, string executablePath)
        {
            try
            {
                Type TfwMgr = Type.GetTypeFromProgID("HNetCfg.FwMgr");
                //创建firewall管理类的实例
                var netFwMgr = (INetFwMgr)Activator.CreateInstance(TfwMgr);

                if (!netFwMgr.LocalPolicy.CurrentProfile.FirewallEnabled)
                {
                    return;
                }

                Type tapp = Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication");
                var app = (INetFwAuthorizedApplication)Activator.CreateInstance(tapp);
                //在例外列表里，程序显示的名称
                app.Name = name;
                //程序的路径及文件名
                app.ProcessImageFileName = executablePath;
                //是否启用该规则
                app.Enabled = true;
                bool exist = false;

                //加入到防火墙的管理策略
                foreach (INetFwAuthorizedApplication mApp in netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications)
                {
                    if (app.Name == mApp.Name)
                    {
                        exist = true;
                        break;
                    }
                }

                if (!exist)
                {
                    netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(app);
                }
            }
            catch (Exception e)
            {
                Log.Error("[EnvConfig] NetFwAddApp Error : " + e.Message);
            }
        }

        #endregion

        #region 设置系统主题

        /// <summary>
        /// 窗口与要获得句柄的窗口之间的关系。
        /// </summary>
        enum GetWindowCmd : uint
        {
            /// <summary>
            /// 返回的句柄标识了在Z序最高端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在Z序最高端的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最高端的顶层窗口：
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最高端的同属窗口。
            /// </summary>
            GW_HWNDFIRST = 0,
            /// <summary>
            /// 返回的句柄标识了在z序最低端的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该柄标识了在z序最低端的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在z序最低端的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在Z序最低端的同属窗口。
            /// </summary>
            GW_HWNDLAST = 1,
            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口下的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口下的最高端窗口：
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口下的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口下的同属窗口。
            /// </summary>
            GW_HWNDNEXT = 2,
            /// <summary>
            /// 返回的句柄标识了在Z序中指定窗口上的相同类型的窗口。
            /// 如果指定窗口是最高端窗口，则该句柄标识了在指定窗口上的最高端窗口；
            /// 如果指定窗口是顶层窗口，则该句柄标识了在指定窗口上的顶层窗口；
            /// 如果指定窗口是子窗口，则句柄标识了在指定窗口上的同属窗口。
            /// </summary>
            GW_HWNDPREV = 3,
            /// <summary>
            /// 返回的句柄标识了指定窗口的所有者窗口（如果存在）。
            /// GW_OWNER与GW_CHILD不是相对的参数，没有父窗口的含义，如果想得到父窗口请使用GetParent()。
            /// 例如：例如有时对话框的控件的GW_OWNER，是不存在的。
            /// </summary>
            GW_OWNER = 4,
            /// <summary>
            /// 如果指定窗口是父窗口，则获得的是在Tab序顶端的子窗口的句柄，否则为NULL。
            /// 函数仅检查指定父窗口的子窗口，不检查继承窗口。
            /// </summary>
            GW_CHILD = 5,
            /// <summary>
            /// （WindowsNT 5.0）返回的句柄标识了属于指定窗口的处于使能状态弹出式窗口（检索使用第一个由GW_HWNDNEXT 查找到的满足前述条件的窗口）；
            /// 如果无使能窗口，则获得的句柄与指定窗口相同。
            /// </summary>
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        private static extern int GetWindowText(
            IntPtr hWnd,
            StringBuilder lpString,
            int nMaxCount
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private static void AutoCloseSetThemeWindow()
        {
            new Thread(() =>
            {
                int count = 0;
                bool processed = false;
                while (count++ < 20 && !processed)
                {
                    Thread.Sleep(1000);

                    //1、获取桌面窗口的句柄
                    IntPtr desktop_handle = GetDesktopWindow();
                    //2、获得一个子窗口（这通常是一个顶层窗口，当前活动的窗口）
                    IntPtr handle = GetWindow(desktop_handle, GetWindowCmd.GW_CHILD);

                    //3、循环取得桌面下的所有子窗口
                    while (handle != IntPtr.Zero)
                    {
                        //4、继续获取下一个子窗口
                        handle = GetWindow(handle, GetWindowCmd.GW_HWNDNEXT);

                        var builder = new StringBuilder(32);
                        GetWindowText(handle, builder, 32);
                        string s = builder.ToString();
                        if (s.Contains("个性化"))
                        {
                            PostMessage(handle, 0x0010, 0, 0);
                            processed = true;
                        }
                    }
                }
            }).Start();
        }

        /// <summary>
        /// 将系统的主题设置为经典，不需要管理员权限，需要主题文件在当前文件夹
        /// </summary>
        private static void SetTheme()
        {
            //先把背景图片保存到系统临时文件夹，主题里会引用到临时文件夹
            string temp = Environment.GetEnvironmentVariable("TEMP");
            if (File.Exists(".\\background.jpg") &&
                !File.Exists(temp + "\\background.jpg"))
            {
                File.Copy(".\\background.jpg", temp + "\\background.jpg");
            }

            try
            {
                if (GetThemeName() != "ForestFantasy")
                {
                    var cmd = new Process();
                    cmd.StartInfo.FileName = "cmd";
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;
                    cmd.Start();
                    cmd.StandardInput.WriteLine("ForestFantasy.theme");
                    cmd.StandardInput.Flush();
                    cmd.StandardInput.Close();

                    cmd.WaitForExit();

                    cmd.Close();
                    cmd.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Error("[EnvConfig] SetTheme Error : " + e.Message);
            }
        }

        /// <summary>
        /// 获取当前主题的名字
        /// </summary>
        /// <returns></returns>
        private static string GetThemeName()
        {
            string RegistryKey = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes";
            string theme;
            theme = (string)Registry.GetValue(RegistryKey, "CurrentTheme", string.Empty);
            theme = theme.Split('\\').Last().Split('.').First().ToString();
            return theme;
        }
        #endregion
    }
}