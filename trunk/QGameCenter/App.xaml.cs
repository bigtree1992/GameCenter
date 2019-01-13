using QConnection;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace QGameCenter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.SetLogToFile();

            Utils.SingleProgramTest(() => { MessageBox.Show("软件已经运行。"); });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            System.Environment.Exit(0);
        }

        public App()
        {
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += App_UnhandledException;
            
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string content = "我们很抱歉，当前应用程序遇到一些问题:" + e.Exception.Message + " 该操作已经终止.";
            MessageBox.Show(content, "意外的错误", MessageBoxButton.OK, MessageBoxImage.Error);

            Log.Error("[QServer] DispatcherUnhandledException error:" + e.Exception.Message);

            System.Environment.Exit(0);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string content = "我们很抱歉，当前应用程序遇到一些问题:" + e.ExceptionObject.ToString() + " 该操作已经终止.";
            MessageBox.Show(content, "意外的错误", MessageBoxButton.OK, MessageBoxImage.Error);

            Log.Error("[QServer] UnhandledException:" + e.ExceptionObject.ToString());

            System.Environment.Exit(0);
        }
    }
}
