using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraLayout;

namespace Factorio.Lua.Reader
{
    public partial class LayoutedView : DevExpress.XtraEditors.XtraUserControl, ILayoutedView, ISelectorView
    {
        public LayoutedView()
        {
            InitializeComponent();
        }

        public LayoutControl LayoutControl => _layoutControl;

        public event EventHandler Selected;
        protected virtual void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        void ISelectorView.AfterLoad()
        {
            AfterLoad();
        }

        protected virtual void AfterLoad()
        {}
    }


}
