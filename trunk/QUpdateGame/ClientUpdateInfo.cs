using System.Collections.Generic;
using System.ComponentModel;

namespace QUpdateGame
{
    public enum StateInfo
    {
        未连接,
        已连接,
        更新文件丢失,
        无需更新,
        客户端需要更新,
        游戏需要更新,
        全部需要更新,
        等待操作,
        客户端更新中,
        SteamVR安装中,
        等待更新游戏文件中,
        文件上传中,
        文件解压中,
        错误
    }

    public class ClientUpdateInfo : INotifyPropertyChanged
    {
        public ClientUpdateInfo()
        {
            VersionInfo = new ClientVersionInfo();
        }

        public bool Selected { get; set; }

        private string m_ClientIP;
        public string ClientIP
        {
            get { return m_ClientIP; }
            set
            {
                m_ClientIP = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ClientIP"));
            }
        }

        private float m_Progress;
        public float Progress
        {
            get
            {
                return m_Progress;
            }
            set
            {
                m_Progress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Progress"));
            }
        }

        private StateInfo m_State;
        public StateInfo State
        {
            get
            {
                return m_State;
            }
            set
            {
                m_State = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
            }
        }

        private string m_Detail;
        public string Detail
        {
            get
            {
                return m_Detail;
            }
            set
            {
                m_Detail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Detail"));
            }
        }

        public ClientVersionInfo VersionInfo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
