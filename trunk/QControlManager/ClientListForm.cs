using System;
using System.Windows.Forms;

namespace QControlManagerNS
{
    public partial class ClientListForm : Form
    {
        public Action<string> OnConnectionResult;
        private QControlManager m_ControlManager;

        internal ClientListForm(QControlManager manager)
        {
            InitializeComponent();
            m_ControlManager = manager;

            m_ConnectBtn.Click += OnConnectBtn_Click;

            m_ListView.ColumnWidthChanging += OnColumnWidthChanging;

            var lv = new ListViewItem();
            foreach (var pair in m_ControlManager.AllClient)
            {
                lv = new ListViewItem();
                lv.Text = pair.Key;
                lv.SubItems.Add(pair.Value.IsOpen ? "连接中" : "已就绪");
                m_ListView.Items.Add(lv);
            }

            m_ControlManager.OnAddClient += OnAddClient;
            m_ControlManager.OnRemoveClient += OnRemoveClient;
        }

        private void OnColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            var header = m_ListView.Columns[e.ColumnIndex];
            e.Cancel = true;
            e.NewWidth = m_ListView.Columns[e.ColumnIndex].Width;
        }

        private void OnAddClient(string ip, string connectionString)
        {
            this.Invoke(new AddClientDelegate(OnRealAddClient), new object[] { ip,connectionString});
        }

        private delegate void AddClientDelegate(string ip, string connectionString); 
        private void OnRealAddClient(string ip, string connectionString)
        {
            var lv = new ListViewItem();
            lv.Text = ip;
            lv.SubItems.Add("已就绪");
            m_ListView.Items.Add(lv);
        }

        private void OnRemoveClient(string ip)
        {
            this.Invoke(new RemoveClientDelegate(OnRealRemoveClient), new object[] { ip });
        }

        private delegate void RemoveClientDelegate(string ip);

        private void OnRealRemoveClient(string ip)
        {
            var item = m_ListView.FindItemWithText(ip);
            if (item != null)
            {
                m_ListView.Items.Remove(item);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            m_ControlManager.OnAddClient -= OnAddClient;
            m_ControlManager.OnRemoveClient -= OnRemoveClient;
        }

        private void OnConnectBtn_Click(object sender, EventArgs e)
        {
            if(m_ListView.SelectedItems.Count >= 1)
            {
                var item = m_ListView.SelectedItems[0];
                var sub = item.SubItems[1];
                if (sub.Text == "已就绪")
                {
                    OnConnectionResult?.Invoke(item.Text);
                    this.Close();
                }
            }
        }
    }
}
