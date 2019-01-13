using QData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

namespace QGameManager
{
    /// <summary>
    /// InsertAndUpdateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InsertAndUpdateWindow : Window
    {
        private GameData m_GameData;
        private GameInfo m_GameInfo;

        private string m_ImagePath;

        private int m_Typr = -1;
        public InsertAndUpdateWindow(GameData data, int type)
        {
            InitializeComponent();
            this.Topmost = true;
            m_Typr = type;
            if (type == 0)
            {
                this.Title = "增加游戏窗口";
            }

            var sportList = new List<SportNoBorderInfo>() {

                new SportNoBorderInfo() { SportID = 1,SportInfo = "支持"},
                new SportNoBorderInfo() { SportID = 2,SportInfo = "不支持"},
            };
            sportNoBorder.ItemsSource = sportList;
            sportNoBorder.SelectedIndex = 0;

            closeGA.ItemsSource = sportList;
            closeGA.SelectedIndex = 0;

            m_GameData = data;
            m_GameInfo = new GameInfo();
            m_Typr = 0;
        }

        public InsertAndUpdateWindow(GameData data, GameInfo info,int type)
        {
            InitializeComponent();

            m_Typr = type;
            
            this.Title = "修改游戏窗口";
            
            this.Topmost = true;
            m_GameData = data;
            m_GameInfo = info;

            this.NameTxt.Text = info.Name;
            this.PriceTxt.Text = info.SinglePrice.ToString();
            this.DetailTxt.Text = info.Detail;
            this.PathTxt.Text = info.Path;
            this.ConfigTxt.Text = info.GameConfigPath;

            var count = info.KeyValuePairs.Count;
            for (var i = 0;i < count;i++)
            {
                this.KeyValueXml.Text += "<KeyValuePair>\n";
                this.KeyValueXml.Text += "    <Key>" + info.KeyValuePairs[i].Key + "</Key>\n";
                this.KeyValueXml.Text += "    <Value>" + info.KeyValuePairs[i].Value + "</Value>\n";
                this.KeyValueXml.Text += "</KeyValuePair>\n";

            }

            var sportList = new List<SportNoBorderInfo>() {

                new SportNoBorderInfo() { SportID = 1,SportInfo = "支持"},
                new SportNoBorderInfo() { SportID = 2,SportInfo = "不支持"},
            };
            sportNoBorder.ItemsSource = sportList;
            
            if(info.IsDeleteBorder == 1)
            {
                sportNoBorder.SelectedIndex = 0;
            }
            else
            {
                sportNoBorder.SelectedIndex = 1;
            }

            closeGA.ItemsSource = sportList;

            if(info.IsKillCloseGa == 1)
            {
                closeGA.SelectedIndex = 0;
            }
            else
            {
                closeGA.SelectedIndex = 1;
            }
        

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if(m_Typr == 0)
            {
                InsertAGameInfo();
            }
            else if(m_Typr == 1){
                ModifyAGameInfo();
            }
            this.Close();
        }

        private void InsertAGameInfo()
        {

            if(!IsXmlVaild())
            {
                return;
            }
            Regex regeNum = new Regex(@"^[-]?[1-9]{1}\d*$|^[0]{1}$");
            if (!regeNum.IsMatch(this.PriceTxt.Text))
            {
                MessageBox.Show("请输入正确的单次消费价格，输入格式为数字.");
                return;
            }

            if (string.IsNullOrEmpty(this.NameTxt.Text))
            {
                MessageBox.Show("游戏名称不能为空.");
                return;
            }
            if (string.IsNullOrEmpty(this.PriceTxt.Text))
            {
                MessageBox.Show("单次消费价格不能为空");
                return;
            }
            if (string.IsNullOrEmpty(this.DetailTxt.Text))
            {
                MessageBox.Show("游戏详细信息还是写点吧");
                return;
            }
            if (string.IsNullOrEmpty(this.PathTxt.Text))
            {
                MessageBox.Show("游戏上传路径不能为空.");
                return;
            }
            if (string.IsNullOrEmpty(this.ConfigTxt.Text))
            {
                MessageBox.Show("游戏配置文件路径不能为空.");
                return;
            }
            m_GameInfo.Name = this.NameTxt.Text;
            m_GameInfo.SinglePrice = int.Parse(this.PriceTxt.Text);
            m_GameInfo.Detail = this.DetailTxt.Text;
            m_GameInfo.Path = this.PathTxt.Text;
            m_GameInfo.GameConfigPath = this.ConfigTxt.Text;
            m_GameInfo.IsDeleteBorder = sportNoBorder.SelectedIndex == 0 ? 1 : 0;
            m_GameInfo.IsKillCloseGa = closeGA.SelectedIndex == 0 ? 1 : 0;

            try
            {
                Log.Debug("  " + AppDomain.CurrentDomain.BaseDirectory + "Resources");
                File.Copy(m_ImagePath, AppDomain.CurrentDomain.BaseDirectory + "Resources\\" + m_GameInfo.Icon);
            }
            catch (Exception e)
            {
                Log.Error("[InsertAndUpdateWindow] InsertAGameInfo Error : " + e.Message);
            }
           
            m_GameData.CreateAGameInfo(m_GameInfo, "Configs/GameData.xml");
        }

        private void ModifyAGameInfo()
        {
            if (!IsXmlVaild())
            {
                return;
            }
            m_GameInfo.Name = this.NameTxt.Text;
            m_GameInfo.SinglePrice = int.Parse(this.PriceTxt.Text);
            m_GameInfo.Detail = this.DetailTxt.Text;
            m_GameInfo.Path = this.PathTxt.Text;
            m_GameInfo.GameConfigPath = this.ConfigTxt.Text;
            m_GameInfo.IsDeleteBorder = sportNoBorder.SelectedIndex == 0 ? 1 : 0;
            m_GameInfo.IsKillCloseGa = closeGA.SelectedIndex == 0 ? 1 : 0;

            m_GameData.ModifyAGameInfo(m_GameInfo, "Configs/GameData.xml");
        }


        private bool IsXmlVaild()
        {
            if(m_GameInfo.KeyValuePairs == null)
            {
                m_GameInfo.KeyValuePairs = new List<KeyValuePair>();
            }
            m_GameInfo.KeyValuePairs.Clear();
            var xml = new XmlDocument();
            var xmlContent = "<Root>\n" + this.KeyValueXml.Text + " \n</Root>";

            //Log.Debug("name : " + root.Name);
            try
            {
                xml.InnerXml = xmlContent;

                foreach (var n1 in xml.FirstChild.ChildNodes)
                {
                    var pair = new KeyValuePair();
                    var ele1 = n1 as XmlElement;
                    if (ele1.Name != "KeyValuePair")
                    {
                        MessageBox.Show("配置的Xml内容出现错误,主要是 : KeyValuePair .");
                        return false;
                    }
                    foreach (var n2 in ele1.ChildNodes)
                    {
                        var ele2 = n2 as XmlElement;
                        if (ele2.Name != "Key" && ele2.Name != "Value")
                        {
                            MessageBox.Show("配置的Xml内容出现错误,主要是 : Key和Value .");
                            return false;
                        }
                    }
                    pair.Key = ele1["Key"].InnerText;
                    pair.Value = ele1["Value"].InnerText;
                    m_GameInfo.KeyValuePairs.Add(pair);
                }
            }
            catch
            {
                MessageBox.Show("配置文件某处的<XXX></XXX>未匹配");
                return false;
            }
            return true;
        }

        private void Browse(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".jpg";
            ofd.Filter = "JPG|*.jpg|BMP|*.bmp|All File|*.*";
            if (ofd.ShowDialog() == true)
            {
                //此处做你想做的事 ...=ofd.FileName; 
                var array = ofd.FileName.Split('\\');
                m_GameInfo.Icon = array[array.Length - 1];
                //m_GameInfo.Icon = ofd.FileName;
                m_ImagePath = ofd.FileName;
                BitmapImage imageURI = new BitmapImage();
                imageURI.BeginInit();
                imageURI.UriSource = new Uri(ofd.FileName, UriKind.Absolute);
                imageURI.EndInit();
                Icon.Source = imageURI;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
