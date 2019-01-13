using QConnection;
using QData;
using QGameCenterLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QXMLFileView
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private QServerConfig m_ServerConfig;
        private ClientData m_ClientData;
        private GameData m_GameData;

        private QServer m_Server;
        private IPBroadcaster m_Broadcaster;

        private AutoConfClientID m_AutoConfigureClientsIP;

        private XMLFileViewLogic m_XMLFileViewLogin;

        public MainWindow()
        {
            InitializeComponent();
            Log.SetLogToFile();

            m_ServerConfig = QServerConfig.LoadData("Configs/ServerConfig.xml");
            if (m_ServerConfig == null)
            {
                Log.Error("[QXMLFileView] MainWindow Can't Load ServerConfig.xml");
                MessageBox.Show("程序无法加载服务端的配置文件，无法启动");
                this.Close();
                Environment.Exit(0);
                return;
            }

            m_ClientData = ClientData.LoadData("Configs/ClientData.xml");
            if (m_ClientData == null)
            {
                Log.Error("[QXMLFileView] MainWindow Can't Load ClientData.xml");
                MessageBox.Show("程序无法加载客户端的配置文件，无法启动");
                this.Close();
                Environment.Exit(0);
                return;
            }

            m_GameData = GameData.LoadData("Configs/GameData.xml");
            if (m_GameData == null)
            {
                Log.Error("[QXMLFileView] MainWindow Can't Load GameData.xml");
                MessageBox.Show("程序无法加载游戏配置文件，无法启动");
                this.Close();
                Environment.Exit(0);
                return;
            }

            try
            {
                m_Server = new QServer(m_ServerConfig);
                var ip = Utils.GetLocalIP();
                var endpoint = new IPEndPoint(IPAddress.Parse(ip), m_ServerConfig.Port);
                //启动服务器
                m_Server.Start(endpoint);
                m_Broadcaster = new IPBroadcaster();
                //启动广播
                m_Broadcaster.Start(ip, m_ServerConfig.Port);
            }
            catch (Exception e) 
            {
                MessageBox.Show("无法启动网络.");
                Log.Error("[QXMLFileView] MainWindow Error : "+e.Message);
                this.Close();
                Environment.Exit(0);
                return;
            }

            var clientdatapath = "Configs/ClientData.xml";
            m_AutoConfigureClientsIP = new AutoConfClientID(m_Server, m_ClientData, clientdatapath);
            
            var textboxs = new TextBox[4];
            textboxs[0] = this.keyTextBox;
            textboxs[1] = this.ValueTextBox;
            textboxs[3] = this.xmlLog;

            var labels = new Label[4];
            labels[0] = this.labelOne;
            labels[1] = this.labelTwo;
            labels[2] = this.labelThree;
            labels[3] = this.labelFour;

            var radiobuttons = new RadioButton[4];
            radiobuttons[0] = this.radioButtonOne;
            radiobuttons[1] = this.radioButtonTwo;
            radiobuttons[2] = this.radioButtonThreee;
            radiobuttons[3] = this.radioButtonFour;

            var buttons = new Button[2];
            buttons[0] = this.buttonReadXml;
            buttons[1] = this.buttonMotifyXml;
            m_XMLFileViewLogin = new XMLFileViewLogic(m_Server, m_ClientData, m_GameData, this, this.dataGrid, textboxs,labels,radiobuttons,buttons,(string str)=> { MessageBox.Show(str); });

        }

    

        private void OnWindowClosed(object sender, EventArgs e)
        {
          
            if(m_XMLFileViewLogin != null)
            {
                m_XMLFileViewLogin.Stop();
                m_XMLFileViewLogin = null;
            }

            if (m_Broadcaster != null)
            {
                m_Broadcaster.Stop();
                m_Broadcaster = null;
            }

            if (m_Server != null)
            {
                m_Server.Stop();
                m_Server = null;
            }

            if (m_GameData != null)
            {
                m_GameData = null;
            }

            if (m_ClientData != null)
            {
                m_ClientData = null;
            }

            if (m_ServerConfig != null)
            {
                m_ServerConfig = null;
            }
        
        }
    }
}
