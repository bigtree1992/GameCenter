using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QGameCenterLogic
{
    public class KeyboardSetting
    {
        private PasswordBox m_PasswordBox;
        private Button[] m_Buttons;
        private Canvas m_ParentCanvas;
        private Canvas m_SettingPanel;
        private Window m_Window;
        private Action<string> m_PopMessagePanel;

        private CheckPwdLogic m_CheckPwdLogic;

        public KeyboardSetting( PasswordBox passwordbox,Button[] buttons,Canvas parent,Canvas settingpanel, Window window,Action<string> popmessagepanel)
        {
            m_CheckPwdLogic = new CheckPwdLogic();

            m_PasswordBox = passwordbox;
            m_Buttons = buttons;
            m_ParentCanvas = parent;
            m_SettingPanel = settingpanel;
            m_Window = window;
            m_PopMessagePanel = popmessagepanel;

            buttons[0].Click += NumberButtonClick;
            buttons[1].Click += NumberButtonClick;
            buttons[2].Click += NumberButtonClick;
            buttons[3].Click += NumberButtonClick;
            buttons[4].Click += NumberButtonClick;
            buttons[5].Click += NumberButtonClick;
            buttons[6].Click += NumberButtonClick;
            buttons[7].Click += NumberButtonClick;
            buttons[8].Click += NumberButtonClick;
            buttons[9].Click += NumberButtonClick;
            buttons[10].Click += BackButtonClick;
            buttons[11].Click += ClearButtonClick;
            buttons[12].Click += OKButtonClick;
            buttons[13].Click += CancelButtonClick;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                m_Window.Dispatcher.Invoke(() =>
                {
                    var parent = m_Window.FindName("ParentCanvas") as Canvas;
                    if (parent == null)
                    {
                        m_PopMessagePanel("关闭时，parent == null");
                        return;
                    }
                    var keyborardpanel = m_PasswordBox.Parent as Canvas;
                    if (keyborardpanel == null)
                    {
                        m_PopMessagePanel("关闭时，keyborardpanel == null");
                        return;
                    }
                    m_PopMessagePanel(keyborardpanel.Name);
                    parent.Children.Remove(keyborardpanel);
                });
            }
            catch(Exception ex)
            {
                Log.Error("[KeyboardSetting] CancelButtonClick Error : "+ex.Message);
            }
        }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            m_CheckPwdLogic.Clear();
            m_Window.Dispatcher.Invoke(() => {
                m_PasswordBox.Password = m_CheckPwdLogic.Password;
            });
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            m_CheckPwdLogic.BackAChar();
            m_Window.Dispatcher.Invoke(() => {
                m_PasswordBox.Password = m_CheckPwdLogic.Password;
            });
        }

        private void NumberButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Button button = (Button)sender;
            m_CheckPwdLogic.AddChar(button.Tag.ToString());
            m_Window.Dispatcher.Invoke(()=> {
                m_PasswordBox.Password = m_CheckPwdLogic.Password;
            });
        }
    }
}
