namespace Factorio.Lua.Reader.Controls
{
    partial class CraftInfoCard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._crafterPanel = new Factorio.Lua.Reader.Controls.ImagedPanel();
            this._recipePanel = new Factorio.Lua.Reader.Controls.ImagedPanel();
            this._topPanel = new DevExpress.XtraEditors.PanelControl();
            this._leftPanel = new DevExpress.XtraEditors.PanelControl();
            this._rightPanel = new DevExpress.XtraEditors.PanelControl();
            this.panelControl1 = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this._topPanel)).BeginInit();
            this._topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._leftPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._rightPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _crafterPanel
            // 
            this._crafterPanel.AutoSize = true;
            this._crafterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._crafterPanel.Image = null;
            this._crafterPanel.LabelText = "";
            this._crafterPanel.Location = new System.Drawing.Point(2, 34);
            this._crafterPanel.Margin = new System.Windows.Forms.Padding(0);
            this._crafterPanel.MinimumSize = new System.Drawing.Size(100, 32);
            this._crafterPanel.Name = "_crafterPanel";
            this._crafterPanel.Size = new System.Drawing.Size(359, 32);
            this._crafterPanel.TabIndex = 1;
            // 
            // _recipePanel
            // 
            this._recipePanel.AutoSize = true;
            this._recipePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._recipePanel.Image = null;
            this._recipePanel.LabelText = "";
            this._recipePanel.Location = new System.Drawing.Point(2, 2);
            this._recipePanel.Margin = new System.Windows.Forms.Padding(0);
            this._recipePanel.MinimumSize = new System.Drawing.Size(100, 32);
            this._recipePanel.Name = "_recipePanel";
            this._recipePanel.Size = new System.Drawing.Size(359, 32);
            this._recipePanel.TabIndex = 0;
            // 
            // _topPanel
            // 
            this._topPanel.AutoSize = true;
            this._topPanel.Controls.Add(this._crafterPanel);
            this._topPanel.Controls.Add(this._recipePanel);
            this._topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._topPanel.Location = new System.Drawing.Point(0, 0);
            this._topPanel.Margin = new System.Windows.Forms.Padding(0);
            this._topPanel.MinimumSize = new System.Drawing.Size(0, 65);
            this._topPanel.Name = "_topPanel";
            this._topPanel.Size = new System.Drawing.Size(363, 68);
            this._topPanel.TabIndex = 0;
            // 
            // _leftPanel
            // 
            this._leftPanel.AutoSize = true;
            this._leftPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._leftPanel.Location = new System.Drawing.Point(1, 1);
            this._leftPanel.Margin = new System.Windows.Forms.Padding(0);
            this._leftPanel.MinimumSize = new System.Drawing.Size(0, 32);
            this._leftPanel.Name = "_leftPanel";
            this._leftPanel.Padding = new System.Windows.Forms.Padding(5);
            this._leftPanel.Size = new System.Drawing.Size(14, 32);
            this._leftPanel.TabIndex = 1;
            // 
            // _rightPanel
            // 
            this._rightPanel.AutoSize = true;
            this._rightPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._rightPanel.Location = new System.Drawing.Point(64, 3);
            this._rightPanel.Margin = new System.Windows.Forms.Padding(0);
            this._rightPanel.MinimumSize = new System.Drawing.Size(0, 32);
            this._rightPanel.Name = "_rightPanel";
            this._rightPanel.Padding = new System.Windows.Forms.Padding(5);
            this._rightPanel.Size = new System.Drawing.Size(14, 32);
            this._rightPanel.TabIndex = 2;
            // 
            // panelControl1
            // 
            this.panelControl1.AutoSize = true;
            this.panelControl1.Controls.Add(this._leftPanel);
            this.panelControl1.Controls.Add(this._rightPanel);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 68);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Padding = new System.Windows.Forms.Padding(1);
            this.panelControl1.Size = new System.Drawing.Size(363, 39);
            this.panelControl1.TabIndex = 3;
            // 
            // CraftInfoCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.panelControl1);
            this.Controls.Add(this._topPanel);
            this.Name = "CraftInfoCard";
            this.Size = new System.Drawing.Size(363, 124);
            ((System.ComponentModel.ISupportInitialize)(this._topPanel)).EndInit();
            this._topPanel.ResumeLayout(false);
            this._topPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._leftPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._rightPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.panelControl1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ImagedPanel _crafterPanel;
        private ImagedPanel _recipePanel;
        private DevExpress.XtraEditors.PanelControl _topPanel;
        private DevExpress.XtraEditors.PanelControl _leftPanel;
        private DevExpress.XtraEditors.PanelControl _rightPanel;
        private DevExpress.XtraEditors.PanelControl panelControl1;
    }
}
