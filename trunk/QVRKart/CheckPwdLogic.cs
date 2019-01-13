using System;
using System.Collections.Generic;

namespace QGameCenterLogic
{
    /// <summary>
    /// 进入设置界面之前的检查密码的逻辑
    /// </summary>
    public class CheckPwdLogic
    {
        private string m_Password;
        public string Password
        {
            get
            {
                return m_Password;
            }
        }
        public CheckPwdLogic()
        {
            m_Password = "";
        }

        public void AddChar(string c)
        {
            m_Password += c;
        }

        public void BackAChar()
        {
            if(!String.IsNullOrEmpty(m_Password))
            {
                m_Password = m_Password.Substring(0,m_Password.Length - 1);
            }
        }

        public void Clear()
        {
            m_Password = "";
        }

        public bool CheckPassword()
        {
            if( m_Password == "666666" )
            {
                m_Password = null;
                return true;
            }
            m_Password = "";
            return false;
        }

    }
}
