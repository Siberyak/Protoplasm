namespace Protoplasm.ViewsAndActions
{
    partial class MasterDetailsView
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
            this._splitContainerControl = new DevExpress.XtraEditors.SplitContainerControl();
            this._groupControl = new DevExpress.XtraEditors.GroupControl();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainerControl)).BeginInit();
            this._splitContainerControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._groupControl)).BeginInit();
            this.SuspendLayout();
            // 
            // _splitContainerControl
            // 
            this._splitContainerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainerControl.Location = new System.Drawing.Point(0, 0);
            this._splitContainerControl.Name = "_splitContainerControl";
            this._splitContainerControl.Panel1.Text = "Panel1";
            this._splitContainerControl.Panel2.Controls.Add(this._groupControl);
            this._splitContainerControl.Panel2.Text = "Panel2";
            this._splitContainerControl.Size = new System.Drawing.Size(432, 309);
            this._splitContainerControl.SplitterPosition = 214;
            this._splitContainerControl.TabIndex = 0;
            this._splitContainerControl.Text = "_splitContainerControl";
            // 
            // groupControl1
            // 
            this._groupControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._groupControl.Location = new System.Drawing.Point(0, 0);
            this._groupControl.Margin = new System.Windows.Forms.Padding(0);
            this._groupControl.Name = "_groupControl";
            this._groupControl.Size = new System.Drawing.Size(213, 309);
            this._groupControl.TabIndex = 0;
            this._groupControl.Text = "<нет данных>";
            // 
            // MasterDetailsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._splitContainerControl);
            this.Name = "MasterDetailsView";
            this.Size = new System.Drawing.Size(432, 309);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainerControl)).EndInit();
            this._splitContainerControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._groupControl)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SplitContainerControl _splitContainerControl;
        private DevExpress.XtraEditors.GroupControl _groupControl;
    }
}
