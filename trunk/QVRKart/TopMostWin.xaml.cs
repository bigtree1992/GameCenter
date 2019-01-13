using System.Windows;


namespace QGameCenterLogic
{
    /// <summary>
    /// TopMostWin.xaml 的交互逻辑
    /// </summary>
    public partial class TopMostWin : Window
    {
        
        public TopMostWin(TopWinLogic topwinlogic)
        {
            InitializeComponent();
            InitializeComponent();
            topwinlogic.BindUI(this, this.CloseGameButton);
            this.Visibility = Visibility.Hidden;
        }


        public void SetTopWinLogic(TopWinLogic topwinlogic)
        {
            InitializeComponent();
            topwinlogic.BindUI(this, this.CloseGameButton);
            this.Visibility = Visibility.Hidden;
        }

    }

}
