using System;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows;

namespace QControlManagerNS
{
    internal class Win32API
    {
        //internal const int WM_KEYDOWN = 0x100;

        //internal const int WM_KEYUP = 0x101;

        internal const int WM_SYSKEYDOWN = 0x104;

        internal const int WM_SYSKEYUP = 0x105;

        internal const int WH_KEYBOARD_LL = 13;



        [StructLayout(LayoutKind.Sequential)] //声明键盘钩子的封送结构类型
        internal class KeyboardHookStruct
        {

            internal int vkCode; //表示一个在1到254间的虚似键盘码 

            internal int scanCode; //表示硬件扫描码 

            internal int flags;

            internal int time;

            internal int dwExtraInfo;

        }

        internal delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);

        //安装钩子的函数 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);


        //卸下钩子的函数 

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool UnhookWindowsHookEx(int idHook);


        //下一个钩挂的函数 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        static extern bool LockSetForegroundWindow(uint uLockCode);

        [DllImport("user32.dll")]
        internal static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        internal static extern int PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        internal const int WM_LBUTTONDOWN = 0x201; //Left mousebutton down
        internal const int WM_LBUTTONUP = 0x202;  //Left mousebutton up
        internal const int WM_LBUTTONDBLCLK = 0x203; //Left mousebutton doubleclick
        internal const int WM_RBUTTONDOWN = 0x204; //Right mousebutton down
        internal const int WM_RBUTTONUP = 0x205;   //Right mousebutton up
        internal const int WM_RBUTTONDBLCLK = 0x206; //Right mousebutton doubleclick
        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_KEYUP = 0x101;

        //移动鼠标 
        internal const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        internal const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        internal const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        internal const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        internal const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        internal const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        internal const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        internal const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        [DllImport("user32")]
        internal static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        internal static extern int SetCursorPos(int x, int y);

        [DllImport("kernel32.dll")]
        internal static extern uint GetLastError();

        
        // 映射 DEVMODE 结构
        // 可以参照 DEVMODE结构的指针定义：
        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd183565(v=vs.85).aspx
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        internal struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string dmDeviceName;

            internal short dmSpecVersion;
            internal short dmDriverVersion;
            internal short dmSize;
            internal short dmDriverExtra;
            internal int dmFields;
            internal int dmPositionX;
            internal int dmPositionY;
            internal int dmDisplayOrientation;
            internal int dmDisplayFixedOutput;
            internal short dmColor;
            internal short dmDuplex;
            internal short dmYResolution;
            internal short dmTTOption;
            internal short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            internal string dmFormName;

            internal short dmLogPixels;
            internal short dmBitsPerPel;
            internal int dmPelsWidth;
            internal int dmPelsHeight;
            internal int dmDisplayFlags;
            internal int dmDisplayFrequency;
            internal int dmICMMethod;
            internal int dmICMIntent;
            internal int dmMediaType;
            internal int dmDitherType;
            internal int dmReserved1;
            internal int dmReserved2;
            internal int dmPanningWidth;
            internal int dmPanningHeight;
        };
        // 平台调用的申明
        [DllImport("user32.dll")]
        internal static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        [DllImport("user32.dll")]
        internal static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        // 控制改变屏幕分辨率的常量
        internal const int ENUM_CURRENT_SETTINGS = -1;
        internal const int CDS_UPDATEREGISTRY = 0x01;
        internal const int CDS_TEST = 0x02;
        internal const int DISP_CHANGE_SUCCESSFUL = 0;
        internal const int DISP_CHANGE_RESTART = 1;
        internal const int DISP_CHANGE_FAILED = -1;

        // 控制改变方向的常量定义
        internal const int DMDO_DEFAULT = 0;
        internal const int DMDO_90 = 1;
        internal const int DMDO_180 = 2;
        internal const int DMDO_270 = 3;

        internal static void ChangeResolution(int width, int height)
        {
            // 初始化 DEVMODE结构
            var devmode = new DEVMODE();
            devmode.dmDeviceName = new String(new char[32]);
            devmode.dmFormName = new String(new char[32]);
            devmode.dmSize = (short)Marshal.SizeOf(devmode);

            if (0 != EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devmode))
            {
                return;   
            }

            devmode.dmPelsWidth = width;
            devmode.dmPelsHeight = height;

            // 改变屏幕分辨率
            int iRet = ChangeDisplaySettings(ref devmode, CDS_TEST);

            if (iRet == DISP_CHANGE_FAILED)
            {
                //MessageBox.Show("不能执行你的请求", "信息");
                return;
            }
            iRet = ChangeDisplaySettings(ref devmode, CDS_UPDATEREGISTRY);

            switch (iRet)
            {
                // 成功改变
                case DISP_CHANGE_SUCCESSFUL:
                    {
                        break;
                    }
                case DISP_CHANGE_RESTART:
                    {

                        break;
                    }
                default:
                    {

                        break;
                    }
            }
        }


        public static void SetWinPos(IntPtr handle, int x, int y, int width, int height)
        {
            if (handle != System.IntPtr.Zero)
            {
                uint num = GetWindowLong(handle, -16);
                num &= 4281597951u;
                SetWindowLong(handle, -16, num);
                SetWindowPos(handle, 0, x, y, width, height, 548u);
            }
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nlndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, uint flags);
    }

}
