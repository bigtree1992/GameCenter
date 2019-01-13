using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace QControlManagerNS
{
    internal class KeyboardHook
    {
        internal Action<Keys> OnKeyDown = null;
        internal Action<Keys> OnKeyUp = null;

        private int hHook;

        private Win32API.HookProc KeyboardHookDelegate;

        /// <summary>
        /// 安装键盘钩子
        /// </summary>
        internal void InstallHook()
        {
            try
            {
                KeyboardHookDelegate = new Win32API.HookProc(KeyboardHookProc);

                var cModule = Process.GetCurrentProcess().MainModule;

                var mh = Win32API.GetModuleHandle(cModule.ModuleName);

                hHook = Win32API.SetWindowsHookEx(Win32API.WH_KEYBOARD_LL, KeyboardHookDelegate, mh, 0);
            }
            catch(Exception)
            {
                
            }
        }

        /// <summary>
        /// 卸载键盘钩子
        /// </summary>
        internal void UnInstallHook()
        {
            Win32API.UnhookWindowsHookEx(hHook);
        }

        /// <summary>
        /// 获取键盘消息
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            // 如果该消息被丢弃（nCode<0
            if (nCode >= 0)
            {

                Win32API.KeyboardHookStruct KeyDataFromHook = (Win32API.KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32API.KeyboardHookStruct));

                int keyData = KeyDataFromHook.vkCode;

                //WM_KEYDOWN和WM_SYSKEYDOWN消息，将会引发OnKeyDownEvent事件
                if (OnKeyDown != null && (wParam == Win32API.WM_KEYDOWN || wParam == Win32API.WM_SYSKEYDOWN))
                {
                    // 此处触发键盘按下事件
                    // keyData为按下键盘的值,对应 虚拟码
                    if(keyData == 122)
                    {
                        OnKeyDown(Keys.F11);
                    }
                    
                }

                //WM_KEYUP和WM_SYSKEYUP消息，将引发OnKeyUpEvent事件 

                if (OnKeyUp != null && (wParam == Win32API.WM_KEYUP || wParam == Win32API.WM_SYSKEYUP))
                {
                    // 此处触发键盘抬起事件
                    if (keyData == 122)
                    {
                        OnKeyUp(Keys.F11);
                    }
                }

            }

            return Win32API.CallNextHookEx(hHook, nCode, wParam, lParam);

        }
    }
}
