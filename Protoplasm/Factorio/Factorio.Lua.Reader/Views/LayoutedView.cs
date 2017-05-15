using DevExpress.XtraLayout;

namespace Factorio.Lua.Reader
{
    public partial class LayoutedView : DevExpress.XtraEditors.XtraUserControl, ILayoutedView
    {
        public LayoutedView()
        {
            InitializeComponent();
        }

        public LayoutControl LayoutControl => _layoutControl;
    }
}
