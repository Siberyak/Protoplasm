using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Protoplasm.ViewsAndActions
{
    public partial class MasterDetailsView : BaseView
    {
        public MasterDetailsView()
        {
            InitializeComponent();
        }

        private BaseView _master;

        protected override object GetDatasource()
        {
            return _master?.DataSource;
        }

        protected override void SetDatasource(object datasource)
        {
            _splitContainerControl.Horizontal = _viewInfo.MasterDetails?.SplitterMoveHorizontal == true;
            _splitContainerControl.IsSplitterFixed = _viewInfo.MasterDetails?.IsSplitterFixed == true;

            _master = ViewHelper.Apply(_splitContainerControl.Panel1, datasource, ViewInfo.ViewContext);
            if(_master != null)
            {
                _master.SelectionChanged += MasterSelectionChanged;
                UpdateDetails();
            }

        }

        private void MasterSelectionChanged(object sender, EventArgs e)
        {
            UpdateDetails();
        }

        private void UpdateDetails()
        {

            var selection = _master.Selection;
            if (selection == null)
            {
                _groupControl.Controls.Clear();
                return;
            }


            ViewHelper.Apply(_groupControl, selection, ViewInfo.MasterDetails.DetailsViewContext ?? ViewInfo.ViewContext);
            var view = _groupControl.Controls.OfType<BaseView>().FirstOrDefault();
            if(view == null)
                return;

            _groupControl.ShowCaption = view.ShowCaption;
            _groupControl.Text = view.ViewInfo.Caption;
        }
    }
}
