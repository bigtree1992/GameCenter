using System;
using System.Windows;
using System.Windows.Controls;

namespace QGameCenterLogic
{
    /// <summary>
    /// InputPasswordPanel.xaml 的交互逻辑
    /// </summary>
    public partial class InputPasswordPanel : UserControl
    {

        private Canvas m_Parent;
        private CheckPwdLogic m_CheckPwdLogic;
        private Action m_ShowSettingPanel;


        public InputPasswordPanel(Canvas parent)
        {
            InitializeComponent();
            m_Parent = parent;
            m_CheckPwdLogic = new CheckPwdLogic();
        }

        public static void ShowKeyBorde(Canvas parent, Action showsettingpanel)
        {
            InputPasswordPanel panel = new InputPasswordPanel(parent);
            panel.m_Parent.Children.Add(panel);
            panel.m_ShowSettingPanel = showsettingpanel;
        }

        private void NumberButtonClcik(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            m_CheckPwdLogic.AddChar(button.Tag.ToString());
            this.passwordBox.Password = m_CheckPwdLogic.Password;
        }

        private void BackButtonClcik(object sender, RoutedEventArgs e)
        {
            m_CheckPwdLogic.BackAChar();
            this.passwordBox.Password = m_CheckPwdLogic.Password;
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            m_CheckPwdLogic.Clear();
            this.passwordBox.Password = m_CheckPwdLogic.Password;
        }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.passwordBox.Password == string.Empty)
            {
                Message.ShowMessage("请先输入密码再确认.");
                return;
            }
            if (m_CheckPwdLogic.CheckPassword())
            {
                m_Parent.Children.Remove(this);
                //RemoveCanvas();
                m_ShowSettingPanel();
            }
            else
            {
                Message.ShowMessage("密码输入错误，请从重新输入");
                this.passwordBox.Password = "";
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            m_Parent.Children.Remove(this);
            m_Parent = null;
            m_CheckPwdLogic = null;
            m_ShowSettingPanel = null;
        }

        private void RemoveCanvas()
        {
            m_Parent.Children.Remove(this);
            m_Parent = null;
            m_CheckPwdLogic = null;
            m_ShowSettingPanel = null;
        }
    }
}
