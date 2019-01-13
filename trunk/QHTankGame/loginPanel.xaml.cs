using System;
using System.Windows;
using System.Windows.Controls;

namespace QGameCenterLogic
{
    /// <summary>
    /// loginPanel.xaml 的交互逻辑
    /// </summary>
    public partial class loginPanel : UserControl
    {
        private Canvas m_Parent;
        private Action<bool> m_OnLoginSucess;

        public loginPanel()
        {
            InitializeComponent();
            this.UserNameInput.GotFocus += ClearUsername;
            this.PasswordNameInput.GotFocus += ClearPassword;
        }

        public static void ShowInputPanel(Canvas parent, Action<bool> onloginsucess)
        {
            var panel = new loginPanel();
            panel.m_Parent = parent;
            panel.m_OnLoginSucess = onloginsucess;
            panel.m_Parent.Children.Add(panel);
        }

        private void ClearPassword(object sender, RoutedEventArgs e)
        {
            this.PasswordNameInput.Password = "";
        }
        private void ClearUsername(object sender, RoutedEventArgs e)
        {
            this.UserNameInput.Text = "";
        }


        private void OnCancelButton(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            string user = this.UserNameInput.Text;
            string pwd = this.PasswordNameInput.Password;

            if (user == string.Empty || pwd == string.Empty)
            {
                Message.ShowMessage("请先输入用户名和密码");
                return;
            }

            if (LoginLogic.Check(user, pwd))
            {
                m_Parent.Children.Remove(this);
                var usercoin = (bool)this.IsCoin.IsChecked;
                if (m_OnLoginSucess != null)
                {
                    m_OnLoginSucess(usercoin);
                }
            }
            else
            {
                Message.ShowMessage("密码不正确，请重新输入");
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(-1);
            Environment.Exit(0);
        }
    }
}
