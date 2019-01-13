using QData;
using System;
using System.Windows.Controls;

namespace QGameCenterLogic
{
    /// <summary>
    /// 使用RadioButton来进行游戏路径的选择
    /// </summary>
    public class UseRadioChoseGameLogic
    {
        private GameData m_GameData;
        private Action<int> m_SetCurIndex;
        private RadioButton[] m_CheckBoxs;
        private Action<string> m_PopMessage;


        public UseRadioChoseGameLogic(GameData gamedata, RadioButton[] checkboxs, Action<int> setcurindex,Action<string> pop)
        {
            m_GameData = gamedata;
            m_CheckBoxs = checkboxs;
            m_SetCurIndex = setcurindex;
            m_PopMessage = pop;

            checkboxs[0].Click += OnChoseCondensed;
            checkboxs[1].Click += OnChoseFull;

            m_SetCurIndex(0);
        }

        private void OnChoseCondensed(object sender, System.Windows.RoutedEventArgs e)
        {
            m_SetCurIndex(1);
        }

        private void OnChoseFull(object sender, System.Windows.RoutedEventArgs e)
        {
            m_SetCurIndex(0);

        }
    }
}
