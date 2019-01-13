using System;
using System.Threading;
using System.Windows;

namespace QGameCenterLogic
{
    /// <summary>
    /// 检查加密狗的逻辑
    /// </summary>
    public class CheckDogLogic
    {
        private Window m_Window;
        private Action<String> m_PopMessageAction;
        private Thread m_DogThread;
        
        private bool IsSoftDog = true;
        
        public CheckDogLogic(Window window,Action<string> popmessage)
        {
            m_Window = window;
            m_PopMessageAction = popmessage;
            m_DogThread = new Thread(CheckDogThread);

            CheckDog();
        }

        private void CheckDog()
        {
            if (this.IsSoftDog)
            {
                if (!CheckDogUtils.Verify())
                {
                    m_Window.Dispatcher.Invoke(() => {
                        m_Window.Visibility = Visibility.Hidden;
                        m_PopMessageAction("该系统没有激活，请先联系管理员进行激活系统.");
                        Log.Error("[CheckDogLogic] CheckDog Error : The system is not active. Please contact the administrator to activate the system");
                    });
                    System.Environment.Exit(0);
                }
            }
        }

        public void Start()
        {
            m_DogThread.Start();
        }

        public void Stop()
        {
            m_DogThread.Abort();
        }
        

        //检测加密狗的线程
        private void CheckDogThread()
        {
            while (true)
            {
                Random m_number = new Random();
                int random = m_number.Next(8, 16);
                Thread.Sleep(random * 1000);
                CheckDog();
            }
        }
        

    }
}
