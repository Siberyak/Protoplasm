namespace Factorio.Lua.Reader.Controls
{
    partial class ImagedPanel
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
            this._labelControl = new DevExpress.XtraEditors.LabelControl();
            this._pictureEdit = new DevExpress.XtraEditors.PictureEdit();
            ((System.ComponentModel.ISupportInitialize)(this._pictureEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // _labelControl
            // 
            this._labelControl.AllowHtmlString = true;
            this._labelControl.AllowHtmlTextInToolTip = DevExpress.Utils.DefaultBoolean.True;
            this._labelControl.Appearance.Options.UseTextOptions = true;
            this._labelControl.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this._labelControl.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this._labelControl.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._labelControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelControl.LineLocation = DevExpress.XtraEditors.LineLocation.Bottom;
            this._labelControl.Location = new System.Drawing.Point(32, 0);
            this._labelControl.Name = "_labelControl";
            this._labelControl.Padding = new System.Windows.Forms.Padding(3);
            this._labelControl.Size = new System.Drawing.Size(239, 32);
            this._labelControl.TabIndex = 1;
            // 
            // _pictureEdit
            // 
            this._pictureEdit.AllowHtmlTextInToolTip = DevExpress.Utils.DefaultBoolean.True;
            this._pictureEdit.Cursor = System.Windows.Forms.Cursors.Default;
            this._pictureEdit.Dock = System.Windows.Forms.DockStyle.Left;
            this._pictureEdit.Location = new System.Drawing.Point(0, 0);
            this._pictureEdit.MinimumSize = new System.Drawing.Size(32, 32);
            this._pictureEdit.Name = "_pictureEdit";
            this._pictureEdit.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this._pictureEdit.Properties.Appearance.Options.UseBackColor = true;
            this._pictureEdit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._pictureEdit.Properties.NullText = " ";
            this._pictureEdit.Properties.ShowCameraMenuItem = DevExpress.XtraEditors.Controls.CameraMenuItemVisibility.Auto;
            this._pictureEdit.Properties.ZoomAccelerationFactor = 1D;
            this._pictureEdit.Size = new System.Drawing.Size(32, 32);
            this._pictureEdit.TabIndex = 2;
            // 
            // ImagedPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this._labelControl);
            this.Controls.Add(this._pictureEdit);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(100, 32);
            this.Name = "ImagedPanel";
            this.Size = new System.Drawing.Size(271, 32);
            ((System.ComponentModel.ISupportInitialize)(this._pictureEdit.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraEditors.LabelControl _labelControl;
        private DevExpress.XtraEditors.PictureEdit _pictureEdit;
    }
}
