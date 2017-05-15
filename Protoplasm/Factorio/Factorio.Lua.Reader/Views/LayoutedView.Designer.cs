namespace Factorio.Lua.Reader
{
    partial class LayoutedView
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
            this._layoutControl = new DevExpress.XtraLayout.LayoutControl();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            ((System.ComponentModel.ISupportInitialize)(this._layoutControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            this.SuspendLayout();
            // 
            // _layoutControl
            // 
            this._layoutControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._layoutControl.Location = new System.Drawing.Point(0, 0);
            this._layoutControl.Name = "_layoutControl";
            this._layoutControl.Root = this.layoutControlGroup1;
            this._layoutControl.Size = new System.Drawing.Size(409, 444);
            this._layoutControl.TabIndex = 0;
            this._layoutControl.Text = "_layoutControl";
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(180, 120);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // LayoutedView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._layoutControl);
            this.Name = "LayoutedView";
            this.Size = new System.Drawing.Size(409, 444);
            ((System.ComponentModel.ISupportInitialize)(this._layoutControl)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        protected DevExpress.XtraLayout.LayoutControl _layoutControl;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
    }
}
