using QGameCenterLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Windows.Controls;
using QProtocols;
using QData;
using System.Windows.Media;
using System.Xml;
using System.Collections.ObjectModel;
using System.Data;

namespace QXMLFileView
{
    public class XMLFileViewLogic
    {

        private Window m_Window;
        private QServer m_Server;
        private ClientData m_ClientData;
        private GameData m_GameData;

        private Label[] m_Labels;
        private TextBox[] m_TextBlocks;

        private DataGrid m_DataGrid;

        private RadioButton[] m_RadioButton;

        private Action<string> m_PopMessageAction;


        private IPAddress m_CurrentIP;
        private string m_CurrentGamePath;

        public XMLFileViewLogic(QServer server,ClientData clientdata,GameData gamedata, Window window,DataGrid datagrid,  TextBox[] textblock, Label[] labels,RadioButton[] radiobuttons,Button[] buttons,Action<string> popmessage)
        {
            m_Window = window;

            m_Server = server;
            m_Server.OnClientConnected += OnClientConnected;
            m_Server.OnClientDisconnected += OnClientDisconnected;

            m_ClientData = clientdata;
            m_GameData = gamedata;

            m_DataGrid = datagrid;
            m_DataGrid.SelectedCellsChanged += OnDataGridSelectedCellsChanged;
            //0 : key
            //1 : value
            //2 : intruction
            //3 : logxml
            m_TextBlocks = textblock;
            //m_TextBlocks[3].Text = "1.请先选中连接的客户端后，点击读取xml内容按钮;\n2.输入Key值和Value值";
            m_Labels = labels;
            m_RadioButton = radiobuttons;
            buttons[0].Click += (send,e) => { OnReadXmlClick(); };
            buttons[1].Click += (send, e) => { OnOnModifyXmlButtonClick(); };

            m_PopMessageAction = popmessage;

        }

        /// <summary>
        /// 单击DataGrid的一行获取其中的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDataGridSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                m_TextBlocks[0].Text = (m_DataGrid.SelectedItem as XMLData).Key;
                m_TextBlocks[1].Text = (m_DataGrid.SelectedItem as XMLData).Value;
            }
            catch (Exception ex)
            {
                Log.Error("[XMLFileViewLogin] M_DataGrid_SelectedCellsChanged Error : "+ex.Message);
            }
        }
        
        public void Stop()
        {
            m_CurrentIP = null;
            m_CurrentGamePath = null;
        }
        
        /// <summary>
        /// 从客户端发过来的字符串里面读取出数据来
        /// </summary>
        /// <param name="p"></param>
        private void ReadXml(P_XMLFileViewRsp p)
        {
            var datas = new ObservableCollection<XMLData>();
            var xml = new XmlDocument();
            var dataList = new List<string>();
            lock(this)
            {
                xml.InnerXml = p.XmlContent;
                foreach (var n1 in xml.ChildNodes)
                {
                    XmlElement xe1 = n1 as XmlElement;
                    if (xe1 == null)
                    {
                        continue;
                    }
                    foreach (XmlNode n2 in xe1.ChildNodes)
                    {
                        var data = new XMLData();
                        if (n2 is XmlComment)
                        {
                            var x2 = n2 as XmlElement;
                            var next = n2.NextSibling;
                            data.Instruction = n2.InnerText;
                            data.Key = next.Name;
                            data.Value = next.InnerText;
                            dataList.Add(data.Key);
                        }
                        else
                        {
                            if(dataList.Contains(n2.Name))
                            {
                                continue;
                            }
                            data.Instruction = "";
                            data.Key = n2.Name;
                            data.Value = n2.InnerText;
                        }
                        datas.Add(data);
                    }
                }
            }
           
            m_Window.Dispatcher.Invoke(() => {
                m_TextBlocks[3].Text = "你现在修改的是 : " + m_ClientData.GetClient(m_CurrentIP).ToString() + "的" + m_CurrentGamePath;
                try
                {
                    m_DataGrid.DataContext = datas;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                }
            });
        }

        /// <summary>
        /// 设置当前选择的IP以及游戏配置的路径
        /// </summary>
        /// <returns></returns>
        private bool SetGamePathAndIP()
        {
            for (int i = 0; i < m_RadioButton.Length; i++)
            {
                if (m_RadioButton[i].IsChecked == true)
                {
                    m_CurrentGamePath = m_GameData.GameInfos[i].GameConfigPath;
                    m_CurrentIP = m_ClientData.GetClient(i+1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 点击读取按钮的事件
        /// </summary>
        private void OnReadXmlClick()
        {
            if (SetGamePathAndIP() == false)
            {
                Log.Error("[XMLFileViewLogin] SetGamePathAndIP Error : None radiobutton is checked.");
                return;
            }

            try
            {
                m_Server.XMLFileViewOp(m_CurrentIP, m_CurrentGamePath, (p) => {
                    if (p.Code == Code.Failed)
                    {
                        Log.Error("[XMLFileViewLogin] OnReadXmlButtonClick Error : Code == Failed");
                    }
                    if (p.Code == Code.Success)
                    {
                        ReadXml(p);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error("[XMLFileViewLogin] OnReadXmlButtonClick Error : " + ex.Message);
            }

        }

        /// <summary>
        /// 修改xml的内容
        /// </summary>
        private void OnOnModifyXmlButtonClick()
        {
            if (SetGamePathAndIP() == false)
            {
                Log.Error("[XMLFileViewLogin] SetGamePathAndIP Error : None radiobutton is checked.");
                return;
            }

            if (m_TextBlocks == null)
            {
                Log.Error("[XMLFileViewLogin] OnModifyXmlButtonClick Error : m_TextBlock == null");
                return;
            }
            if(string.IsNullOrEmpty(m_TextBlocks[0].Text) || string.IsNullOrEmpty(m_TextBlocks[1].Text))
            {
                m_PopMessageAction("请先输入Key和Value的值 .");
                Log.Error("[XMLFileViewLogin] OnModifyXmlButtonClick Error : key == null || value == null.");

                return;
            }

            List<QData.KeyValuePair> pairs = new List<QData.KeyValuePair>();

            pairs.Add(new QData.KeyValuePair { Key = m_TextBlocks[0].Text ,Value = m_TextBlocks[1].Text });

            m_Server.XMLFileOp(m_CurrentIP, m_CurrentGamePath, pairs, (code) =>
            {
                if (code == Code.Failed)
                {
                    Log.Error("[XMLFileViewLogin] OnModifyXmlButtonClick Error : code == Failed");
                }
                if (code == Code.Success)
                {
                    m_PopMessageAction("修改成功.");
                    m_TextBlocks[0].Text = "";
                    m_TextBlocks[1].Text = "";
                }
            });
            OnReadXmlClick();
        }

        private void OnClientDisconnected(IPAddress ip)
        {
            try
            {
                var id = m_ClientData.GetClient(ip);
                m_Window.Dispatcher.Invoke(() => {
                    m_Labels[id - 1].Content = "客户端0" + (id).ToString() + "掉线";
                    m_Labels[id - 1].Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(147, 156, 156));
                    m_RadioButton[id - 1].IsEnabled = false;
                });
            }
            catch(Exception e)
            {
                Log.Error("[XmlFileViewLogic] OnClientDisconnected Error : "+e.Message);
            }
          
        }

        private void OnClientConnected(IPAddress ip)
        {
            try
            {
                var id = m_ClientData.GetClient(ip);
                m_Window.Dispatcher.Invoke(() => {
                    m_Labels[id - 1].Content = "客户端0" + (id).ToString() + "连接";
                    m_Labels[id - 1].Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                    m_RadioButton[id - 1].IsEnabled = true;
                });
              
            }
            catch (Exception e)
            {
                Log.Error("[XmlFileViewLogic] OnClientConnected Error : " + e.Message);
            }

        }

    }
}
