using QData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace QEventStatistics
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameCenterDBEntities m_GameCenterDBEntities;
        private List<GameRecord> m_CurrentManagerList;
        private Pager m_Pager;

        public MainWindow()
        {
            InitializeComponent();
            Log.SetLogToFile();
            
            this.Hide();
            if (!LoginPanel.Show(this))
            {
                Environment.Exit(0);
            }

            try
            {
                GameTypeInfosCombo.ItemsSource = GameData.LoadData("Configs/GameData.xml").GameInfos;
                GameTypeInfosCombo.SelectedIndex = 0;
            }
            catch
            {
                MessageBox.Show("无法加载配置文件 .");
                this.Close();
                return;
            }

            if (!File.Exists("Data/GameCenterDB.db"))
            {
                MessageBox.Show("无法加载GameCenterDB.db文件.");
                this.Close();
                return;
            }

            m_GameCenterDBEntities = new GameCenterDBEntities();
            m_CurrentManagerList = m_GameCenterDBEntities.GameRecords.ToList();


            TotalCount.Text = (m_CurrentManagerList.Count() - 1) > 0 ? (m_CurrentManagerList.Count() - 1).ToString() : "0";


            m_Pager = new Pager(PreviousButton, NextButton, GOButton, InputPage, CurrentPage, TotalPage, (beforeCurentPageCount, curentPageCount) => {

                var list = m_CurrentManagerList.Select(p => new { p.Id, p.Name, p.RunTime, p.Amount, p.Count })
                                                            .OrderBy(p => p.Id)
                                                            .Skip(beforeCurentPageCount)
                                                            .Take(curentPageCount);
                this.dataGrid.ItemsSource = list.ToList();
                this.dataGrid.Items.Refresh();
            });

            m_Pager.InitMaxPage(m_GameCenterDBEntities.GameRecords.Count());


            m_GameCenterDBEntities.OnOperationScuess += () => {
                this.dataGrid.ItemsSource = m_CurrentManagerList;
                this.dataGrid.Items.Refresh();
            };

        }

      
        private void OnAddAInfo(object sender, RoutedEventArgs e)
        {
            var info = new GameRecord();
            info.RunTime = DateTime.Now;
            info.Name = this.gameName.Text;
            m_GameCenterDBEntities.AddAGameReecordInfo(info);

            m_CurrentManagerList = m_GameCenterDBEntities.GameRecords.ToList();

            m_Pager.InitMaxPage(m_CurrentManagerList.Count());
            TotalCount.Text = (m_CurrentManagerList.Count() - 1) > 0 ? (m_CurrentManagerList.Count() - 1).ToString() : "0";

        }

        private void OnWindowClosed(object sender, System.EventArgs e)
        {
            if(m_GameCenterDBEntities != null)
            {
                m_GameCenterDBEntities.Dispose();
            }
            if (m_CurrentManagerList != null)
            {
                m_CurrentManagerList = null;
            }
            if (m_Pager != null)
            {
                m_Pager = null;
            }
            Environment.Exit(0);
        }

        private void OnDeleteInfo(object sender, RoutedEventArgs e)
        {
            m_GameCenterDBEntities.DeleteAllGameRecord();
            m_CurrentManagerList.Clear();
        }

        private void OnCkeckClick(object sender, RoutedEventArgs e)
        {
            var info = this.GameTypeInfosCombo.SelectedItem as GameInfo;
            m_CurrentManagerList = m_GameCenterDBEntities.CheckGameRecordInfo(DateTime.Parse(t1.SelectedDate.ToString()), DateTime.Parse(t2.SelectedDate.ToString()), info.Name);
            m_Pager.InitMaxPage(m_CurrentManagerList.Count());
            TotalCount.Text = (m_CurrentManagerList.Count() - 1) > 0 ? (m_CurrentManagerList.Count() - 1).ToString() : "0";          
        }

        private void OnModifyClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
