namespace QControlManagerNS
{
    partial class ClientListForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_ListView = new System.Windows.Forms.ListView();
            this.IP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.State = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_ConnectBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_ListView
            // 
            this.m_ListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_ListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.IP,
            this.State});
            this.m_ListView.FullRowSelect = true;
            this.m_ListView.GridLines = true;
            this.m_ListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_ListView.Location = new System.Drawing.Point(0, -2);
            this.m_ListView.MultiSelect = false;
            this.m_ListView.Name = "m_ListView";
            this.m_ListView.Size = new System.Drawing.Size(250, 339);
            this.m_ListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.m_ListView.TabIndex = 0;
            this.m_ListView.UseCompatibleStateImageBehavior = false;
            this.m_ListView.View = System.Windows.Forms.View.Details;
            // 
            // IP
            // 
            this.IP.Text = "IP";
            this.IP.Width = 140;
            // 
            // State
            // 
            this.State.Text = "状态";
            this.State.Width = 100;
            // 
            // m_ConnectBtn
            // 
            this.m_ConnectBtn.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.m_ConnectBtn.Location = new System.Drawing.Point(0, 333);
            this.m_ConnectBtn.Name = "m_ConnectBtn";
            this.m_ConnectBtn.Size = new System.Drawing.Size(250, 37);
            this.m_ConnectBtn.TabIndex = 1;
            this.m_ConnectBtn.Text = "连接";
            this.m_ConnectBtn.UseVisualStyleBackColor = true;
            // 
            // ClientListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 373);
            this.Controls.Add(this.m_ConnectBtn);
            this.Controls.Add(this.m_ListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ClientListForm";
            this.ShowInTaskbar = false;
            this.Text = "客户端列表";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView m_ListView;
        private System.Windows.Forms.Button m_ConnectBtn;
        private System.Windows.Forms.ColumnHeader IP;
        private System.Windows.Forms.ColumnHeader State;
    }
}