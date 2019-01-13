using System;
using System.Windows;


namespace QGameCenterLogic
{
    /// <summary>
    /// MessagePanel.xaml 的交互逻辑
    /// </summary>
    public partial class MessagePanel : Window
    {
        public MessagePanel()
        {
            InitializeComponent();
        }


        public static void ShowMessage(string content)
        {
            MessagePanel message = new MessagePanel();
            message.textContent.Text = content;
            message.Topmost = true;
            message.ShowDialog();
        }

        private void MessageOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
