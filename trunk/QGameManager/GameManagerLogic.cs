using QData;
using QGameCenterLogic;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QGameManager
{

    class GameManagerLogic
    {
        private QServer m_Server;
        private GameData m_GameData;
        private GameInfo m_GameInfo;

        private DataGrid m_DataGrid;

        private SendFileLogic m_SendFileLogic;
        
        public GameManagerLogic(QServer server,GameData gamedata,DataGrid dataGrid,Button[] buttons,SendFileLogic sendfilelogic)
        {
            m_Server = server;
            m_GameData = gamedata;
            m_DataGrid = dataGrid;

            m_DataGrid.DataContext = m_GameData.GameInfos;
            m_DataGrid.SelectedCellsChanged += OnSelectedCellsChanged;
            //if (m_GameData.GameInfos.Count > 0)
            //{
            //    m_GameInfo = m_GameData.GameInfos[0];
            //}

            buttons[0].Click += (senderm, e) => { OnCreateAGameInfo(); };
            buttons[1].Click += (sender, e) => { OnModifyAGameInfo(); };
            buttons[2].Click += (sender, e) => { OnDeleteAGameInfo(); };
            buttons[3].Click += (sender, e) => { OnSendAGame(); };

            m_SendFileLogic = sendfilelogic;
        }

        private void OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                m_GameInfo = m_DataGrid.SelectedItem as GameInfo;
            }
            catch (Exception ex)
            {
                Log.Error("[GameManagerLogic] OnSelectedCellsChanged Error : " + ex.Message);
            }
        }

        private void OnCreateAGameInfo()
        {
            var win = new InsertAndUpdateWindow(m_GameData,0);
            win.ShowDialog();
            m_DataGrid.Items.Refresh();
        }

        private void OnModifyAGameInfo()
        {
            if(m_GameInfo == null)
            {
                MessageBox.Show("请先选择一列数据","提示框");
                return;
            }
            var win = new InsertAndUpdateWindow(m_GameData,m_GameInfo,1);
            win.ShowDialog();
            m_DataGrid.Items.Refresh();
        }
        private void OnDeleteAGameInfo()
        {
            if (m_GameInfo == null)
            {
                MessageBox.Show("请先选择一列数据","提示框");
                return;
            }
            MessageBoxResult result = MessageBox.Show("确定要删除游戏配置吗？", "删除窗口", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var pairs = new System.Collections.Generic.List<KeyValuePair>();

                foreach (var kvp in m_GameInfo.KeyValuePairs)
                {
                    var pair = new KeyValuePair();
                    pair.Key = kvp.Key;
                    pair.Value = kvp.Value;
                    pairs.Add(pair);
                }

                m_GameInfo.KeyValuePairs = new System.Collections.Generic.List<KeyValuePair>();

                foreach (var kvp in pairs)
                {
                    var pair = new KeyValuePair();
                    pair.Key = kvp.Key;
                    pair.Value = kvp.Value;
                    m_GameInfo.KeyValuePairs.Add(pair);
                }
                m_GameData.DeleteAGameInfo(m_GameInfo, "Configs/GameData.xml");
                m_DataGrid.Items.Refresh();
            }
        }

        private void OnSendAGame()
        {
            if(string.IsNullOrEmpty(m_GameInfo.Path))
            {
                MessageBox.Show("请先选中一个游戏 .");
                return;
            }
            var win = new SendFileWindow(m_SendFileLogic,m_GameInfo.Path);
            win.ShowDialog();
        }

        public void OnRefresh()
        {
            m_DataGrid.Items.Refresh();
        }
        public void Stop()
        {

        }

    }
}
