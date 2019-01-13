using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QEventStatistics
{
    public class Pager
    {
        private const int PageCount = 36;
        private int m_PageIndex = 1;
        private int m_MaxPage;

        public int PageIndex { get { return m_PageIndex; } }

        private Button ui_PreButton;
        private Button ui_NextButton;
        private Button ui_GoButton;

        private TextBox ui_InputBox;
        private TextBlock ui_PageIndex;
        private Label ui_MaxPage;

        private Action<int,int> m_RealQuery;

        public Pager(Button pre_button,Button next_button,Button go_button,TextBox inputText, TextBlock page_index,Label max_page,Action<int,int> real_query)
        {
            ui_PreButton = pre_button;
            ui_NextButton = next_button;
            ui_GoButton = go_button;
            ui_PageIndex = page_index;
            ui_MaxPage = max_page;
            ui_InputBox = inputText;

            ui_PageIndex.Text = m_PageIndex.ToString();
            ui_MaxPage.Content =m_MaxPage.ToString();

            ui_PreButton.Click += OnPrePageClick;
            ui_NextButton.Click += OnNextPageClick;
            ui_GoButton.Click += OnGoClick;
            m_RealQuery = real_query;
        }

        public void InitMaxPage(int itemcount)
        {
            m_MaxPage = itemcount / PageCount + 1;
            ui_MaxPage.Content = m_MaxPage.ToString();
            m_PageIndex = 1;
            ui_PageIndex.Text = m_PageIndex.ToString();

            m_RealQuery((m_PageIndex - 1) * PageCount, PageCount);
        }

        public void SetToPage(int itemcount,int page)
        {
            m_MaxPage = itemcount / PageCount + 1;
            ui_MaxPage.Content = m_MaxPage.ToString();
            m_PageIndex = m_MaxPage;
            ui_PageIndex.Text = m_PageIndex.ToString();

            m_RealQuery((m_PageIndex - 1) * PageCount, PageCount);
        }

        public void Refresh()
        {
            m_RealQuery((m_PageIndex - 1) * PageCount, PageCount);
        }

        private void OnPrePageClick(object sender, RoutedEventArgs e)
        {
            if (m_PageIndex == 1)
            {
                return;
            }

            m_PageIndex--;
            ui_PageIndex.Text = m_PageIndex.ToString();
            m_RealQuery((m_PageIndex - 1) * PageCount, PageCount);
        }

        private void OnNextPageClick(object sender, RoutedEventArgs e)
        {
            if (m_PageIndex >= m_MaxPage)
            {
                return;
            }

            m_PageIndex++;
            ui_PageIndex.Text = m_PageIndex.ToString();

            m_RealQuery((m_PageIndex - 1) * PageCount, PageCount);
        }

        private void OnGoClick(object sender, RoutedEventArgs e)
        {
          
            var page = m_PageIndex;
            int.TryParse(ui_InputBox.Text, out page);
            if (page < 1 || page == m_PageIndex || page > m_MaxPage)
            {
                ui_PageIndex.Text = m_PageIndex.ToString();
                return;
            }
            
            m_PageIndex = page;
            ui_PageIndex.Text = m_PageIndex.ToString();

            m_RealQuery((m_PageIndex - 1) * PageCount, PageCount);
        }
    }
}
