using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Protoplasm.Utils;

namespace Protoplasm.ViewsAndActions
{
    public class ModelViewController
    {
        private readonly Dictionary<Type, RegistratorBase> _registrators = new Dictionary<Type, RegistratorBase>();
        public IEnumerable<ViewInfo> GetViewInfos(object dataSource, object viewContext)
        {
            var type = dataSource?.GetType() ?? typeof(Type);

            RegistratorBase registrator;
            if(!_registrators.TryGetValue(type, out registrator))
                return new ViewInfo[0];


            var viewsRegistrator = registrator.GetViewsRegistrator(viewContext);

            var viewInfos = viewsRegistrator.GetViewInfos(dataSource).ToArray();

            return viewInfos;

        }

        public IRegistrator<T> Registrator<T>()
        {
            var type = typeof(T);

            if (!_registrators.ContainsKey(type))
                _registrators.Add(type, new Registrator<T>());

            var registratorBase = _registrators[type];
            return (IRegistrator<T>)registratorBase;
        }
        
        public static readonly object DefaultViewContext = new object();
    }

    public abstract class RegistratorBase
    {
        public abstract ViewsRegistratorBase GetViewsRegistrator(object viewContext = null);
    }

    public class Registrator<T> : RegistratorBase, IRegistrator<T>
    {
        readonly Dictionary<object, ViewsRegistrator<T>> _viewsRegistrators 
            = new Dictionary<object, ViewsRegistrator<T>>();

        protected TViewsRegistrator Get<TViewsRegistrator>(object viewContext)
            where TViewsRegistrator : ViewsRegistrator<T>, new()
        {
            viewContext = viewContext ?? ModelViewController.DefaultViewContext;

            if (!_viewsRegistrators.ContainsKey(viewContext))
                _viewsRegistrators.Add(viewContext, new TViewsRegistrator {_viewContext = viewContext});

            return (TViewsRegistrator)_viewsRegistrators[viewContext];
        }

        public override ViewsRegistratorBase GetViewsRegistrator(object viewContext = null)
        {
            viewContext = viewContext ?? ModelViewController.DefaultViewContext;

            return _viewsRegistrators.ContainsKey(viewContext)
                ? _viewsRegistrators[viewContext]
                : null;
        }

        public IViewsRegistrator<T> ViewsRegistrator(object viewContext = null)
        {
            return Get<ViewsRegistrator<T>>(viewContext);
        }
    }

    public interface IRegistrator<T>
    {
        IViewsRegistrator<T> ViewsRegistrator(object viewContext = null);
    }

    public interface IListViewsRegistrator<T>
    {
        IListViewsRegistrator<T> ViewGrid(string defaultCaption = null);
        IListViewsRegistrator<T> ViewGrid(Func<T,string> getCaption);
    }

    public interface IEntityViewsRegistrator<T>
    {
        IEntityViewsRegistrator<T> Properties(string defaultCaption = null);
        IEntityViewsRegistrator<T> Properties(Func<T,string> getCaption);

        IEntityViewsRegistrator<T> Detail<TDatasource>(Expression<Func<T, TDatasource>> getDatasource, MasterDetails masterDetails = null, object viewContext = null);
        IEntityViewsRegistrator<T> Detail<TDatasource>(Func<T, TDatasource> getDatasource, string caption, MasterDetails masterDetails = null, object viewContext = null);

        IEntityViewsRegistrator<T> Detail<TDatasource>(Func<T, TDatasource> getDatasource, Func<TDatasource, string> getCaption, MasterDetails masterDetails = null, object viewContext = null);

    }


    public interface IViewsRegistrator<T>
    {
        IListViewsRegistrator<T> AsCollection();
        IEntityViewsRegistrator<T> AsEntity();
    }

    public abstract class ViewsRegistratorBase
    {
        protected internal object _viewContext;
        protected abstract class ViewInfoGetter
        {
            public abstract ViewInfo Get(object datasource);
        }

        protected class ViewInfoGetter<TDatasource> : ViewInfoGetter
        {
            readonly Func<TDatasource, ViewInfo> _getViewInfo;

            public ViewInfoGetter(Func<TDatasource, ViewInfo> getViewInfo)
            {
                _getViewInfo = getViewInfo;
            }

            public override ViewInfo Get(object datasource)
            {
                return _getViewInfo((TDatasource)datasource);
            }
        }

        protected readonly List<ViewInfoGetter> _getters = new List<ViewInfoGetter>();

        public IEnumerable<ViewInfo> GetViewInfos(object dataSource)
        {
            return _getters.Select(x => x.Get(dataSource));
        }
    }

    public class ViewsRegistrator<T> : ViewsRegistratorBase, IViewsRegistrator<T>, IListViewsRegistrator<T>, IEntityViewsRegistrator<T>
    {
        private ViewInfoGetter CreateGetter<TDatasource>(Func<TDatasource,string> getCaption, Func<T, TDatasource> getDataSource, object viewContext, object viewToken,  MasterDetails masterDetails)
        {
            var id = Guid.NewGuid();

            Func<object, string> getCaptionByDatasource = null;
            if (getCaption != null)
                getCaptionByDatasource = ds => getCaption((TDatasource)ds) ;

            return new ViewInfoGetter<T>(x => new ViewInfo(id, ()=>getDataSource(x), getCaptionByDatasource) {MasterDetails = masterDetails, ViewContext = viewContext, ViewToken = viewToken});
        }

        protected void Add<TDatasource>(Func<TDatasource, string> getCaption, Func<T, TDatasource> getDataSource, object viewContext, object viewToken, MasterDetails masterDetails)
        {
            _getters.Add(CreateGetter(getCaption, getDataSource, viewContext, viewToken, masterDetails));
        }

        public IListViewsRegistrator<T> ViewGrid(string defaultCaption = null)
        {
            return ViewGrid(GetCaption<T>(defaultCaption));
        }

        public IListViewsRegistrator<T> ViewGrid(Func<T, string> getCaption)
        {
            Add(getCaption, x => x, _viewContext, GeneralViewToken.Grid, null);
            return this;
        }

        public IEntityViewsRegistrator<T> Properties(string defaultCaption = null)
        {
            return Properties(GetCaption<T>(defaultCaption));
        }

        public IEntityViewsRegistrator<T> Properties(Func<T, string> getCaption)
        {
            Add(getCaption, x => x, _viewContext, GeneralViewToken.PropertiesGrid, null);
            return this;
        }

        public IEntityViewsRegistrator<T> Detail<TDatasource>(Expression<Func<T, TDatasource>> getDatasource, MasterDetails masterDetails = null, object viewContext = null)
        {
            var memberExpression = getDatasource.Body as MemberExpression;
            var member = memberExpression?.Member;
            var caption = member?.Attribute<DisplayNameAttribute>()?.DisplayName ?? member?.Name;

            return Detail(getDatasource.Compile(), caption, masterDetails, viewContext);
        }

        public IEntityViewsRegistrator<T> Detail<TDatasource>(Func<T, TDatasource> getDatasource, string caption, MasterDetails masterDetails = null, object viewContext = null)
        {
            Add(GetCaption<TDatasource>(caption), getDatasource, viewContext ?? _viewContext, null, masterDetails);
            return this;
        }

        public IEntityViewsRegistrator<T> Detail<TDatasource>(Func<T, TDatasource> getDatasource, Func<TDatasource, string> getCaption, MasterDetails masterDetails = null, object viewContext = null)
        {
            Add(getCaption, getDatasource, viewContext ?? _viewContext, null, masterDetails);
            return this;
        }

        private static Func<TDatasource, string> GetCaption<TDatasource>(string caption)
        {

            Func<TDatasource, string> getCaption = null;
            if (caption != null)
                getCaption = x => caption;

            return getCaption;
        } 

        public IListViewsRegistrator<T> AsCollection()
        {
            return this;
        }

        public IEntityViewsRegistrator<T> AsEntity()
        {
            return this;
        }
    }
}