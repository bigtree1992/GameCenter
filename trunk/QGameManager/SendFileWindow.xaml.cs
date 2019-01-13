using System;
using System.Windows;
using System.Windows.Controls;

namespace QGameManager
{
    /// <summary>
    /// SendFileWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SendFileWindow : Window
    {
        public SendFileWindow(SendFileLogic sendfilelogic,string gamepath)
        {
            InitializeComponent();
           
            var buttons = new Button[2];
            buttons[0] = this.SendButton;
            buttons[1] = this.OpenGame;
            sendfilelogic.BindUI(gamepath,this, this.dataGrid,this.ExePath, buttons,this.PathTxt);
            this.Topmost = true;

        }

        private void Test(ComboBox exePath)
        {
            throw new NotImplementedException();
        }
    }
}
