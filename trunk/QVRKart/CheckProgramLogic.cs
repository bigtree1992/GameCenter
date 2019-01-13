using System;
using System.Linq;
using System.Threading;

namespace QGameCenterLogic
{
    class CheckProgramLogic
    {
        private string m_ProgramName;

        private Thread m_CheckThread;
        private bool m_IsStratCheck = false;

        public Action OnExistProgram;
        public Action OnNOExistProgram;



        public CheckProgramLogic()
        {
           

        }

        public void Clear()
        {
            Stop();
            if(OnExistProgram != null)
            {
                OnExistProgram = null;
            }
            if(OnNOExistProgram != null)
            {
                OnNOExistProgram = null;
            }
            
        }

        public void Start(string programName)
        {
            Stop();
            m_ProgramName = programName;
            m_IsStratCheck = true;
            m_CheckThread = new Thread(CheckProgram);
            m_CheckThread.Start();
            m_CheckThread.IsBackground = true;
        }


        public void Stop()
        {
            m_IsStratCheck = false;
            if(m_CheckThread != null)
            {
                m_CheckThread.Abort();
                m_CheckThread = null;
            }
        }
        private void CheckProgram()
        {
            while(m_IsStratCheck)
            {
                if (System.Diagnostics.Process.GetProcessesByName(m_ProgramName).ToList().Count > 0)
                {
                    //存在  表明游戏开始
                    if(OnExistProgram != null)
                    {
                        OnExistProgram();
                    }
                }
                else
                {
                    //不存在
                    if( OnNOExistProgram != null )
                    {
                        OnNOExistProgram();
                    }
                }

            }
        }



    }
}
