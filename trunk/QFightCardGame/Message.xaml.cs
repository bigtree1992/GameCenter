using System.Windows;

namespace QGameCenterLogic
{
    /// <summary>
    /// Message.xaml 的交互逻辑
    /// </summary>
    public partial class Message : Window
    {
        public Message()
        {
            InitializeComponent();
        }
        public static void ShowMessage(string content)
        {
            Message message = new Message();
            message.textContent.Text = content;
            message.Topmost = true;
            message.Show();
        }


        private void MessageOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
