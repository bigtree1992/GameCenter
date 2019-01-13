using System.Windows;

namespace QGameCenterLogic
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {

        private Window m_MainWindow;

        public LoginWindow()
        {
            InitializeComponent();
            this.User.GotFocus += (sender, e) => {
                this.User.Text = "";
            };
            this.Password.GotFocus += (sender, e) => {
                this.Password.Password = "";
            };

        }


        public static bool Show(Window window)
        {
            var panel = new LoginWindow();

            panel.m_MainWindow = window;

            return panel.ShowDialog().Value;
        }

        private void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Password.Password))
            {
                MessageBox.Show("请先输入密码");
                return;
            }
            if (string.IsNullOrEmpty(this.User.Text))
            {
                MessageBox.Show("请先输入账号");
                return;
            }

            if (LoginLogic.Check(this.User.Text, this.Password.Password))
            {
                m_MainWindow.Show();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("密码错误");
            }

        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            m_MainWindow.Close();
            this.Close();
        }

    }
}
