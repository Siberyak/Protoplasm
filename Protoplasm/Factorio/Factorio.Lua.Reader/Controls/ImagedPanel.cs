using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.XtraEditors;

namespace Factorio.Lua.Reader.Controls
{
    public partial class ImagedPanel : DevExpress.XtraEditors.XtraUserControl
    {
        private bool _compactView;

        public ImagedPanel()
        {
            InitializeComponent();
        }

        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Image Image
        {
            get { return _pictureEdit.Image; }
            set
            {
                _pictureEdit.SuspendLayout();
                _pictureEdit.MinimumSize = value?.Size ?? new Size(32,32);
                _pictureEdit.Image = value;
                _pictureEdit.ResumeLayout();
                Application.DoEvents();
            }
        }

        public bool CompactView
        {
            get { return _compactView; }
            set
            {
                if(_compactView == value)
                    return;
                _compactView = value;

                _labelControl.Visible = !value;
            }
        }

        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string LabelText
        {
            get { return _labelControl.Text; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _pictureEdit.SuperTip = null;
                else
                {
                    _pictureEdit.SuperTip = new SuperToolTip() {AllowHtmlText = DefaultBoolean.True};
                    var item = _pictureEdit.SuperTip.Items.Add(value);
                    item.AllowHtmlText = DefaultBoolean.True;
                }
                _labelControl.SuspendLayout();
                _labelControl.Text = value;
                _labelControl.ResumeLayout(true);
                _labelControl.Refresh();
            }
        }

        public void Reset()
        {
            LabelText = null;
            Image = null;
        }
    }
}
