namespace QControlManagerNS
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.m_TabControl = new QControlManagerNS.ChromeTabControl();
            this.SuspendLayout();
            // 
            // m_TabControl
            // 
            this.m_TabControl.BackTabPageImage = null;
            this.m_TabControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.m_TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_TabControl.ItemSize = new System.Drawing.Size(200, 25);
            this.m_TabControl.Location = new System.Drawing.Point(0, 0);
            this.m_TabControl.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.m_TabControl.Name = "m_TabControl";
            this.m_TabControl.SelectedIndex = 0;
            this.m_TabControl.Size = new System.Drawing.Size(784, 562);
            this.m_TabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.m_TabControl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.m_TabControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "奇境森林远程管理端";
            this.ResumeLayout(false);

        }

        #endregion

        private ChromeTabControl m_TabControl;
        
    }
}

