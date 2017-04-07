using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Protoplasm.ViewsAndActions
{
    public class ViewHelper
    {
        private readonly ModelViewController _mvc;

        Dictionary<object, Func<ViewInfo, BaseView>> _byToken
            = new Dictionary<object, Func<ViewInfo, BaseView>>();

        private static Type _masterDetailsToken = typeof(MasterDetails);

        private static Type _multiviewToken = typeof(ViewInfo[]);
        private static Guid  _multiviewId = Guid.NewGuid();

        public ViewHelper(ModelViewController mvc)
        {
            _mvc = mvc;

            RegisterToken<Type, MasterDetailsView>(_masterDetailsToken);
            RegisterToken<Type, TabControlView>(_multiviewToken);
        }

        public void RegisterToken<TTocken, TView>(TTocken tocken)
            where TView : BaseView, new()
        {
            _byToken.Add(tocken, CreateView<TView>);
        }

        private BaseView CreateView<TView>(ViewInfo viewInfo)
            where TView : BaseView, new()
        {
            var view = new TView();
            view.Dock = DockStyle.Fill;
            view.ViewHelper = this;
            view.ViewInfo = viewInfo;

            return view;
        }

        private BaseView GetView(ViewInfo viewInfo)
        {
            BaseView view = null;
            if (viewInfo.IsMasterDetail)
            {
                var masterDetailView = new MasterDetailsView();
                view = masterDetailView;
            }

            view.ViewInfo = viewInfo;
            return view;
        }

        public BaseView Apply(Control holder, object datasource, object viewContext)
        {
            return Apply(holder, datasource, viewContext, null);
        }

        private BaseView Apply(Control holder, object datasource, object viewContext, string viewCaption)
        {
            var vis = _mvc.GetViewInfos(datasource, viewContext).ToArray();

            if (vis.Length == 0)
            {
                holder.Controls.Clear();
                return null;
            }

            ViewInfo vi;

            if (vis.Length == 1)
            {
                vi = vis[0];
                vi.Caption = vi.Caption ?? viewCaption;
            }
            else // TabControlView
            {
                vi = new ViewInfo(_multiviewId, vis) {ViewContext = ModelViewController.DefaultViewContext, ViewToken = _multiviewToken};
            }

            return Apply(holder, vi);
        }

        public BaseView Apply(Control holder, ViewInfo viewInfo)
        {
            var viewToken = viewInfo.IsMasterDetail ? _masterDetailsToken : viewInfo.ViewToken;
            if (viewToken == null)
            {
                return Apply(holder, viewInfo.DataSource, viewInfo.ViewContext, viewInfo.Caption);
            }

            var view = holder.Controls.OfType<BaseView>().FirstOrDefault();
            if (view?.ViewInfo.ID == viewInfo.ID)
            {
                view.ViewInfo = viewInfo;
                return view;
            }

            holder.Controls.Clear();

            view = _byToken[viewToken](viewInfo);
            holder.Controls.Add(view);
            return view;
        }
    }


}