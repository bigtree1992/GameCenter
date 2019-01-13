using System.Windows.Controls;


namespace QGameCenterLogic
{
    /// <summary>
    /// CoinOperation.xaml 的交互逻辑
    /// </summary>
    public partial class CoinOperation : UserControl
    {
        private Canvas m_Parent;
        private CheckCoinLogic m_CheckCoinLogic;

        public CoinOperation()
        {
            InitializeComponent();
        }

        public static void ShowPanel(Canvas parent, CheckCoinLogic checkcoinlogic)
        {
            if (checkcoinlogic.IsUserCoin)
            {
                var panel = new CoinOperation();
                panel.m_Parent = parent;
                var buttons = new Button[2];
                buttons[0] = panel.CloseButton;
                buttons[1] = panel.Testbutton;

                var labels = new Label[4];
                labels[0] = panel.label0;
                labels[1] = panel.label1;
                labels[2] = panel.label2;
                labels[3] = panel.label3;
                panel.m_Parent.Children.Add(panel);
                checkcoinlogic.BindUI(labels, buttons, panel.ImageBG, () => { panel.m_Parent.Children.Remove(panel); });
            }
        }
    }
}
