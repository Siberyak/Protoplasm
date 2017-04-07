using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraTab;

namespace Protoplasm.ViewsAndActions
{
    public partial class TabControlView : BaseView
    {
        private ViewInfo[] _viewInfos = new ViewInfo[0];
        private readonly List<TabPageInfo> _infos = new List<TabPageInfo>();

        public TabControlView()
        {
            InitializeComponent();
            if(DesignMode)
                return;

            _tabControl.SelectedPageChanged += SelectedPageChanged;
        }

        public override bool ShowCaption => false;


        private void SelectedPageChanged(object sender, TabPageChangedEventArgs e)
        {
            var info = ((TabPageInfo) e.Page.Tag);
            info.Activate();
        }

        protected override object GetDatasource()
        {
            return _viewInfos;
        }

        protected override void SetDatasource(object datasource)
        {
            var selected = (TabPageInfo)_tabControl.SelectedTabPage?.Tag;

            var viewInfos = (ViewInfo[]) datasource;

            var oldIds = _viewInfos.Select(x => x.ID).ToArray();
            var newIds = viewInfos.Select(x => x.ID).ToArray();

            var intersection = newIds.Intersect(oldIds).ToArray();

            var forUpdate = intersection.Select(x => viewInfos.First(y => y.ID == x)).ToArray();
            foreach (var viewInfo in forUpdate)
            {
                var tabPageInfo = _infos.First(x => x.ViewInfo.ID == viewInfo.ID);
                tabPageInfo.UpdateViewInfo(viewInfo);
                if(tabPageInfo == selected)
                    selected.Activate();
            }

            var forRemove = oldIds.Except(intersection).Select(x => _viewInfos.First(y => y.ID == x)).ToArray();
            foreach (var viewInfo in forRemove)
            {
                var tabPageInfo = _infos.First(x => x.ViewInfo.ID == viewInfo.ID);
                _tabControl.TabPages.Remove(tabPageInfo.TabPage);
            }

            var forAdd = newIds.Except(intersection).Select(x => viewInfos.First(y => y.ID == x)).ToArray();
            foreach (var viewInfo in forAdd)
            {
                AddTab(viewInfo);
            }

            _viewInfos = viewInfos;
        }

        class TabPageInfo
        {
            internal ViewHelper _viewHelper;

            public ViewInfo ViewInfo;
            public bool Activated;
            public XtraTabPage TabPage;

            public void UpdateViewInfo(ViewInfo viewInfo)
            {
                ViewInfo = viewInfo;
                Activated = false;
            }

            public void Activate()
            {
                if (Activated)
                    return;

                Activated = true;

                _viewHelper.Apply(TabPage, ViewInfo);
            }
        }

        void AddTab(ViewInfo viewInfo)
        {


            var tabPageInfo = new TabPageInfo {_viewHelper = ViewHelper, ViewInfo = viewInfo };
            var tabPage = new XtraTabPage {Tag = tabPageInfo };
            tabPageInfo.TabPage = tabPage;
            _infos.Add(tabPageInfo);

            tabPage.SuspendLayout();

            _tabControl.TabPages.Add(tabPage);


            //var gridControlView = new GridView
            //{
            //    Dock = DockStyle.Fill,
            //    Name = $"tabbedGridControlView{_tabControl.TabPages.Count}",
            //    TabIndex = 0
            //};
            //tabPage.Controls.Add(gridControlView);

            tabPage.Name = $"tabPage{_tabControl.TabPages.Count}";
            tabPage.Text = viewInfo.Caption;


            tabPage.ResumeLayout(false);

        }

    }
}
