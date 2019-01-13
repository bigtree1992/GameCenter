using QControlManagerNS.Win32;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace QControlManagerNS
{
    public partial class ChromeTabControl : TabControl
    {
        public Action<int> OnAddTabPage;
        public Action<string> OnCloseTabPage;

        private Rectangle m_AddRectangle;
        private Color m_ColorMouseOn = Color.Silver;

        private Image m_CloseImage;
        private Image m_CloseNormalImage;
        private Image m_TabIcon;

        private Rectangle m_RectClose;
        private Rectangle m_RectIcon;
        private Rectangle m_RectFont;
        private Color m_OnSelectedColor1 = Color.White;
        private Color m_OnSelectedColor2 = Color.Pink;
        private Color m_OffSelectedColor1 = Color.FromArgb(192, 255, 255);
        private Color m_OffSelectedColor2 = Color.FromArgb(200, 66, 204, 255);
        private Color m_MoveSelectedColor1 = Color.FromArgb(200, 66, 204, 255);
        private Color m_MoveSelectedColor2 = Color.FromArgb(192, 255, 255);
        private Color m_BottomLineColor = Color.FromArgb(188, 188, 188);
        private SolidBrush m_BrushFont = new SolidBrush(Color.Black);
        private Color m_BackColor = System.Drawing.SystemColors.Control;
        private Image m_BackgroundImage = null;
        private Rectangle m_RectTabHeaderColor;
        private Rectangle m_RectTabHeaderImage;
        private int m_TabIndex = 0;
        private int m_OverIndex = -1;
        private int m_TabId = 0;
        private bool m_AllSelected = false;
        private Point m_TabPoint;
        private TabPage m_SourceTabPage = null;
        private int m_TabHOffset = 0;

        /// <summary>
        /// 设置选项卡处于选中状态时第一背景色.
        /// </summary>
        [Description("设置选项卡处于选中状态时第一背景色。")]
        [DefaultValue(typeof(Color), "White")]
        [Browsable(true)]
        public Color TabOnColorState
        {
            get { return m_OnSelectedColor1; }
            set
            {
                if (!value.Equals(m_OnSelectedColor1))
                {
                    m_OnSelectedColor1 = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 设置选项卡处于选中状态时第二背景色.
        /// </summary>
        [Description("设置选项卡处于选中状态时第二背景色。")]
        [DefaultValue(typeof(Color), "Pink")]
        [Browsable(true)]
        public Color TabOnColorEnd
        {
            get { return m_OnSelectedColor2; }
            set
            {
                if (!value.Equals(m_OnSelectedColor2))
                {
                    m_OnSelectedColor2 = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 设置选项卡处于非选中状态时第一背景色.
        /// </summary>
        [Description("设置选项卡处于非选中状态时第一背景色。")]
        [DefaultValue(typeof(Color), "192, 255, 255")]
        [Browsable(true)]
        public Color TabOffColorState
        {
            get { return m_OffSelectedColor1; }
            set
            {
                if (!value.Equals(m_OffSelectedColor1))
                {
                    m_OffSelectedColor1 = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 设置选项卡处于非选中状态时第二背景色.
        /// </summary>
        [Description("设置选项卡处于非选中状态时第二背景色。")]
        [DefaultValue(typeof(Color), "200, 66, 204, 255")]
        [Browsable(true)]
        public Color TabOffColorEnd
        {
            get { return m_OffSelectedColor2; }
            set
            {
                if (!value.Equals(m_OffSelectedColor2))
                {
                    m_OffSelectedColor2 = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 设置鼠标移动到非选中状态选项卡时第一背景色.
        /// </summary>
        [Description("设置鼠标移动到非选中状态选项卡时第一背景色。")]
        [DefaultValue(typeof(Color), "200, 66, 204, 255")]
        [Browsable(true)]
        public Color TabMoveColorState
        {
            get { return m_MoveSelectedColor1; }
            set
            {
                if (!value.Equals(m_MoveSelectedColor1))
                {
                    m_MoveSelectedColor1 = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 设置鼠标移动到非选中状态选项卡时第二背景色.
        /// </summary>
        [Description("设置鼠标移动到非选中状态选项卡时第二背景色。")]
        [DefaultValue(typeof(Color), "192, 255, 255")]
        [Browsable(true)]
        public Color TabMoveColorEnd
        {
            get { return m_MoveSelectedColor2; }
            set
            {
                if (!value.Equals(m_MoveSelectedColor2))
                {
                    m_MoveSelectedColor2 = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 设置选项卡工作区背景色.
        /// </summary>
        [Description("设置选项卡工作区背景色。")]
        [DefaultValue(typeof(Color), "Control")]
        [Browsable(true)]
        public Color BackTabPageColor
        {
            get { return m_BackColor; }
            set
            {
                if (!value.Equals(m_BackColor))
                {
                    m_BackColor = value;
                    Invalidate();
                    Update();
                }
            }
        }

        /// <summary>
        /// 设置选项卡工作区背景图.
        /// </summary>
        [Description("设置选项卡工作区背景图。")]
        [Browsable(true)]
        public Image BackTabPageImage
        {
            get
            {
                return m_BackgroundImage;
            }
            set
            {
                if (value != null)
                {
                    if (!value.Equals(m_BackgroundImage))
                    {
                        m_BackgroundImage = value;
                        Invalidate();
                        Update();
                    }
                }
                else
                {
                    m_BackgroundImage = null;
                    Invalidate();
                    Update();
                }
            }
        }

        public ChromeTabControl()
        { 
            this.SetStyle(  ControlStyles.AllPaintingInWmPaint | 
                            ControlStyles.OptimizedDoubleBuffer | 
                            ControlStyles.UserPaint |
                            ControlStyles.SupportsTransparentBackColor | 
                            ControlStyles.ResizeRedraw |
                            ControlStyles.UserMouse, true);

            this.ItemSize = new Size(200, 25);
            this.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            //this.AllowDrop = true; 


            //从资源文件（嵌入到程序集）里读取图片
            m_CloseImage = new Bitmap(Properties.Resources.close);
            m_CloseNormalImage = new Bitmap(Properties.Resources.close_normal);
        }

        ~ChromeTabControl()
        {
            GC.SuppressFinalize(this);
        }

        protected virtual void Draw(Graphics g)
        {
            DrawBorder(g);

            Rectangle rct = this.ClientRectangle;
            rct.Inflate(-1, -1);
            Rectangle rctTabArea = this.DisplayRectangle;

            if (this.TabCount > 0)
            {
                m_RectTabHeaderColor = new Rectangle(rct.Left, rct.Top, rct.Width, rctTabArea.Top);
                m_RectTabHeaderImage = new Rectangle(rct.Left - 1, rct.Top - 1, rct.Width + 3, rctTabArea.Top);
            }
            else
            {
                m_RectTabHeaderColor = new Rectangle(rct.Left, rct.Top, rct.Width, rctTabArea.Top + 24);
                m_RectTabHeaderImage = new Rectangle(rct.Left - 1, rct.Top - 1, rct.Width + 3, rctTabArea.Top + 25);
            }

            using (Bitmap overlay = new Bitmap(m_RectTabHeaderImage.Width, m_RectTabHeaderImage.Height))
            {
                using (Graphics gr = Graphics.FromImage(overlay))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;

                    if (m_BackgroundImage != null)
                    {
                        using (Brush brush = new TextureBrush(m_BackgroundImage, WrapMode.TileFlipXY))
                            gr.FillRectangle(brush, 0, 0, overlay.Width, overlay.Height);
                    }
                    else
                    {
                        g.FillRectangle(new SolidBrush(m_BackColor), 0, 0, m_RectTabHeaderColor.Width + 2, m_RectTabHeaderColor.Height);
                    }
                }

                g.DrawImage(overlay, m_RectTabHeaderImage, 1, 1, overlay.Width, overlay.Height, GraphicsUnit.Pixel);
                g.DrawLine(new Pen(new SolidBrush(m_BottomLineColor), 1), 0, 28, this.ClientSize.Width, 28);
            }
        }

        protected virtual void DrawBorder(Graphics g)
        {
            Rectangle rct = this.ClientRectangle;
            Rectangle rctTabArea = this.DisplayRectangle;

            using (Pen pen = new Pen(Color.Silver))
            {
                pen.DashStyle = DashStyle.Solid;

                g.DrawLine(pen, rct.X, rctTabArea.Y, rct.X, rct.Bottom - 1);
                g.DrawLine(pen, rct.X, rct.Bottom - 1, rct.Width - 1, rct.Bottom - 1);
                g.DrawLine(pen, rct.Width - 1, rct.Bottom - 1, rct.Width - 1, rctTabArea.Y);
            }
        }

        protected virtual void DrawAll(Graphics g, Rectangle rect, string title, bool selected, bool mouseOver)
        {
            try
            {
                m_RectFont = new Rectangle(rect.X + 35, rect.Y + 9, rect.Width - 60, rect.Height - 10);
                m_RectClose = new Rectangle(rect.X + rect.Width - 25, 11, 12, 12);

                drawRect(g, rect, selected, mouseOver);
                drawString(g, m_RectFont, title, Font);
                drawClose(g, m_RectClose, CloseHitTest(this.PointToClient(Cursor.Position)));

                for (int i = 0; i < this.TabCount; i++)
                {
                    rect = this.GetTabRect(i);

                    if (this.ImageList != null && !this.TabPages[i].ImageIndex.Equals(-1))
                    {
                        if (this.TabPages[i].ImageIndex <= this.ImageList.Images.Count - 1)
                        {
                            m_TabIcon = this.ImageList.Images[this.TabPages[i].ImageIndex];
                            m_RectIcon = new Rectangle(rect.X + 16, 8, 16, 16);
                            drawTabIcon(g, m_RectIcon);
                            m_TabIcon.Dispose();
                        }
                    }
                }
            }
            catch { }
        }

        protected virtual void drawRect(Graphics g, Rectangle rect, bool selected, bool mouseOver)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddBezier(
                    new Point(rect.X, rect.Bottom + 2),
                    new Point(rect.X + 3, rect.Bottom - 2),
                    new Point(rect.X + 3, rect.Bottom - 2),
                    new Point(rect.X, rect.Bottom + 2));
                //path.AddLine(rect.X + 4, rect.Bottom - 4, rect.Left + 15 - 4, rect.Y + 4);
                path.AddBezier(
                    new Point(rect.Left + 15 - 4, rect.Y + 4),
                    new Point(rect.Left + 15 - 3, rect.Y + 2),
                    new Point(rect.Left + 15 - 3, rect.Y + 2),
                    new Point(rect.Left + 15, rect.Y));
                //path.AddLine(rect.Left + 15, rect.Y, rect.Right - 15, rect.Y);
                path.AddBezier(
                    new Point(rect.Right - 15, rect.Y),
                    new Point(rect.Right - 15 + 3, rect.Y + 2),
                    new Point(rect.Right - 15 + 3, rect.Y + 2),
                    new Point(rect.Right - 15 + 4, rect.Y + 4));
                //path.AddLine(rect.Right - 15 + 4, rect.Y + 4, rect.Right - 4, rect.Bottom - 4);
                path.AddBezier(
                    new Point(rect.Right, rect.Bottom),
                    new Point(rect.Right - 3, rect.Bottom - 3),
                    new Point(rect.Right - 3, rect.Bottom - 3),
                    new Point(rect.Right + 1, rect.Bottom + 1));

                Region region = new System.Drawing.Region(path);

                g.DrawPath(new Pen(Color.Black), path);

                if (mouseOver)
                {
                    using (LinearGradientBrush brush = new LinearGradientBrush(rect, m_MoveSelectedColor1, m_MoveSelectedColor2, LinearGradientMode.Vertical))
                    {
                        g.FillPath(brush, path);
                    }
                    //g.FillPath(new SolidBrush(MoveSelectedColor), path);
                }
                else
                {
                    using (LinearGradientBrush brush = selected ? new LinearGradientBrush(rect, m_OnSelectedColor1, m_OnSelectedColor2, LinearGradientMode.Vertical) : new LinearGradientBrush(rect, m_OffSelectedColor1, m_OffSelectedColor2, LinearGradientMode.Vertical))
                    {
                        g.FillPath(brush, path);
                    }
                    //g.FillPath(new SolidBrush(selected ? onSelectedColor : offSelectedColor), path);
                }
                g.DrawLine(new Pen(selected ? m_OnSelectedColor2 : m_BottomLineColor, 1), rect.X + 1, rect.Bottom + 1, rect.Right, rect.Bottom + 1);
            }
        }

        protected virtual void drawString(Graphics g, Rectangle rect, string title, Font font)
        {
            g.DrawString(title, font, m_BrushFont, rect);
        }

        protected virtual void drawTabIcon(Graphics g, Rectangle rect)
        {
            g.DrawImageUnscaled(m_TabIcon, rect);
        }

        protected virtual void drawClose(Graphics g, Rectangle rect, bool mouseOn)
        {
            if (mouseOn)
                g.DrawImage(m_CloseImage, rect);
            else
                g.DrawImage(m_CloseNormalImage, rect);
        }

        private bool CloseHitTest(Point cltPosition)
        {
            return m_RectClose.Contains(cltPosition);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw(e.Graphics);
            Graphics g = e.Graphics;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            if (this.TabCount > 0)
            {
                if ((this.ItemSize.Width * this.TabCount) + 30 > this.ClientSize.Width || (this.ClientSize.Width - 30) / (this.TabCount) > this.ItemSize.Width)
                {
                    if (this.ClientSize.Width < (this.TabCount * this.ItemSize.Width) + 30 || this.ItemSize.Width < 200)
                    {
                        if (this.TabCount > 0)
                        {
                            this.ItemSize = new Size((this.ClientSize.Width - 30) / (this.TabCount), 25);
                        }
                    }

                    if (this.ItemSize.Width > 200)
                    {
                        this.ItemSize = new Size(200, 25);
                    }
                }

                m_AddRectangle = new Rectangle((this.ItemSize.Width * this.TabCount) + 5, 8, 16, 16); //指定显示区域的位置的大小
                e.Graphics.FillEllipse(new SolidBrush(m_ColorMouseOn), m_AddRectangle);

                e.Graphics.DrawLine(new Pen(Color.White, 1.55f), (this.ItemSize.Width * this.TabCount) + 8, 16, (this.ItemSize.Width * this.TabCount) + 18, 16);
                e.Graphics.DrawLine(new Pen(Color.White, 1.55f), (this.ItemSize.Width * this.TabCount) + 13, 11, (this.ItemSize.Width * this.TabCount) + 13, 21);

                for (int i = 0; i < this.TabCount; i++)
                {
                    if (m_TabIndex != 0)
                    {
                        if (m_TabIndex < this.TabCount)
                        {
                            if (m_TabIndex == i)
                            {
                                this.SelectedIndex = i;
                                DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, true, false);
                            }
                            else
                            {
                                if (m_OverIndex == i)
                                {
                                    DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, false, true);
                                }
                                else
                                {
                                    DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, false, false);
                                }
                            }
                        }
                        else
                        {
                            if ((m_TabIndex - 1) == i)
                            {
                                this.SelectedIndex = i;
                                DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, true, false);
                            }
                            else
                            {
                                if (m_OverIndex == i)
                                {
                                    DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, false, true);
                                }
                                else
                                {
                                    DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, false, false);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (this.SelectedIndex == i)
                        {
                            DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, true, false);
                        }
                        else
                        {
                            if (m_OverIndex == i)
                            {
                                DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, false, true);
                            }
                            else
                            {
                                DrawAll(g, this.GetTabRect(i), this.TabPages[i].Text, false, false);
                            }
                        }
                    }
                }
                m_TabIndex = 0;
            }
            else
            {
                m_TabIndex = 0;
                m_AddRectangle = new Rectangle(10, 8, 16, 16); //指定显示区域的位置的大小
                e.Graphics.FillEllipse(new SolidBrush(m_ColorMouseOn), m_AddRectangle);

                e.Graphics.DrawLine(new Pen(Color.White, 1.55f), 10 + 3, 16, 10 + 13, 16);
                e.Graphics.DrawLine(new Pen(Color.White, 1.55f), 10 + 8, 11, 10 + 8, 21);
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == (int)User32.Msgs.WM_NCHITTEST)
            {
                if (m.Result.ToInt32() == User32._HT_TRANSPARENT)
                    m.Result = (IntPtr)User32._HT_CLIENT;
            }

            if (m.Msg == (int)User32.Msgs.WM_LBUTTONDOWN)
            {
                if (this.TabCount > 1)
                {
                    TabPage selectingTabPage = OverTab();
                    if (selectingTabPage != null)
                    {
                        int index = TabPages.IndexOf(selectingTabPage);
                        if (index != this.SelectedIndex)
                        {
                            if (!selectingTabPage.Enabled)
                            {
                                m.Result = new IntPtr(1);
                            }
                            else
                            {
                                this.SelectedTab = selectingTabPage;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                this.m_SourceTabPage = OverTab();

                m_TabPoint = new Point(e.X, e.Y);

                if (m_AddRectangle.Contains(e.Location))
                {
                    m_ColorMouseOn = Color.Orange;
                }
            }

            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                bool AscendingMove = false;

                if (m_AddRectangle.Contains(e.Location))
                {
                    m_ColorMouseOn = Color.FromArgb(229, 233, 227);
                    //添加按钮被点击
                    OnAddTabPage?.Invoke(this.TabCount + 1);
                }

                if (this.TabCount > 0)
                {
                    if (!m_AllSelected)
                    {
                        var tabRect = new Rectangle(this.GetTabRect(this.SelectedIndex).X + this.GetTabRect(this.SelectedIndex).Width - 25, 11, 12, 12);

                        if (tabRect.Contains(e.Location))
                        {
                            AscendingMove = true;
                            m_TabIndex = this.SelectedIndex;
                            OnCloseTabPage?.Invoke(this.SelectedTab.Text);
                            this.TabPages.Remove(this.SelectedTab);
                        }
                        else
                        {
                            AscendingMove = false;
                        }
                    }
                    else
                    {
                        Rectangle tabRect = new Rectangle(this.GetTabRect(m_TabId).X + this.GetTabRect(m_TabId).Width - 25, 11, 12, 12);

                        if (tabRect.Contains(e.Location))
                        {
                            AscendingMove = true;
                            this.TabPages.RemoveAt(m_TabId);
                            m_AllSelected = false;
                        }
                        else
                        {
                            AscendingMove = false;
                        }
                    }

                    if (this.m_SourceTabPage != null)
                    {
                        TabPage currTabPage = GetTabPageFromXY(e.X, e.Y);

                        if ((currTabPage != null) && (!currTabPage.Equals(this.m_SourceTabPage)))
                        {
                            Rectangle currRect = base.GetTabRect(base.TabPages.IndexOf(currTabPage));
                            if ((base.TabPages.IndexOf(currTabPage) < base.TabPages.IndexOf(this.m_SourceTabPage)))
                            {
                                base.TabPages.Remove(this.m_SourceTabPage);
                                base.TabPages.Insert(base.TabPages.IndexOf(currTabPage), this.m_SourceTabPage);
                                base.SelectedTab = this.m_SourceTabPage;
                            }
                            else if ((base.TabPages.IndexOf(currTabPage) > base.TabPages.IndexOf(this.m_SourceTabPage)))
                            {
                                if (!AscendingMove)
                                {
                                    base.TabPages.Remove(this.m_SourceTabPage);
                                    base.TabPages.Insert(base.TabPages.IndexOf(currTabPage) + 1, this.m_SourceTabPage);
                                    base.SelectedTab = this.m_SourceTabPage;
                                }
                            }
                        }
                    }
                }
            }
            this.m_SourceTabPage = null;
            base.Cursor = Cursors.Default;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            m_OverIndex = -1;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            for (int i = 0; i < this.TabCount; i++)
            {
                if (this.SelectedIndex != i && this.GetTabRect(i).Contains(e.Location))
                {
                    m_OverIndex = i;
                    break;
                }
                else
                {
                    m_OverIndex = -1;
                }
            }

            if (m_AddRectangle.Contains(e.Location))
            {
                m_ColorMouseOn = Color.OrangeRed;
            }
            else
            {
                m_ColorMouseOn = Color.Silver;
            }

            if ((e.Button == MouseButtons.Left) && (this.m_SourceTabPage != null))
            {
                TabPage currTabPage = GetTabPageFromXY(e.X, e.Y);
                if ((currTabPage != null))
                {
                    Rectangle currRect = base.GetTabRect(base.TabPages.IndexOf(currTabPage));
                    if ((base.TabPages.IndexOf(currTabPage) < base.TabPages.IndexOf(this.m_SourceTabPage)))
                    {
                        base.Cursor = Cursors.PanWest;
                    }
                    else if ((base.TabPages.IndexOf(currTabPage) > base.TabPages.IndexOf(this.m_SourceTabPage)))
                    {
                        base.Cursor = Cursors.PanEast;
                    }
                    else
                    {
                        base.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    this.Cursor = Cursors.No;
                }
            }
            else
            {
                this.Cursor = Cursors.Default;
            }

            this.Invalidate();
        }

        private TabPage GetTabPageFromXY(int x, int y)
        {
            for (int i = 0; i <= base.TabPages.Count - 1; i++)
            {
                if (base.GetTabRect(i).Contains(x, y))
                {
                    return base.TabPages[i];
                }
            }
            return null;
        }

        protected override void OnSelecting(TabControlCancelEventArgs e)
        {
            base.OnSelecting(e);

            if (m_OverIndex > -1)
            {
                Rectangle tabRect = new Rectangle(this.GetTabRect(e.TabPageIndex).X + this.GetTabRect(e.TabPageIndex).Width - 25, 11, 12, 12);

                if (tabRect.Contains(m_TabPoint))
                {
                    e.Cancel = true;
                    m_AllSelected = true;
                    m_TabId = e.TabPageIndex;
                }
                else
                {
                    m_AllSelected = false;
                }
            }
            else
            {
                m_AllSelected = false;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        public override Rectangle DisplayRectangle
        {
            get
            {
                Rectangle rect = base.DisplayRectangle;
                return new Rectangle(rect.Left - 3, rect.Top, rect.Width + 6, rect.Height + 3);
            }
        }

        public void AddTabPage(string tabName)
        {
            TabPages.Add(tabName);
            SelectTab(TabPages.Count - 1);
            this.SelectedTab.AutoScroll = true;
        }

        protected virtual TabPage OverTab()
        {
            TabPage over = null;

            Point pt = this.PointToClient(Cursor.Position);
            User32.TCHITTESTINFO mouseInfo = new User32.TCHITTESTINFO(pt, User32.TabControlHitTest.TCHT_ONITEM);
            int currentTabIndex = User32.SendMessage(this.Handle, User32._TCM_HITTEST, IntPtr.Zero, ref mouseInfo);

            if (currentTabIndex > -1)
            {
                Rectangle currentTabRct = this.GetTabRect(currentTabIndex);

                if (currentTabIndex == 0)
                    currentTabRct.X += m_TabHOffset;

                if (currentTabRct.Contains(pt))
                    over = this.TabPages[currentTabIndex] as TabPage;
            }

            return over;
        }
    }

}
