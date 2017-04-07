using System;

namespace Protoplasm.ViewsAndActions
{
    public class ViewInfo
    {
        private readonly Func<object> _getDatasource;
        private Func<object,string> _getCaption;

        public Guid ID { get; private set; }

        public ViewInfo(Guid id)
        {
            ID = id;
        }

        public ViewInfo(Guid id, object datasource) : this(id, () => datasource, null)
        {
        }


        public ViewInfo(Guid id, Func<object> getDatasource, Func<object, string> getCaption) : this(id)
        {
            _getDatasource = getDatasource;
            _getCaption = getCaption;
        }

        public string Caption
        {
            get { return _getCaption?.Invoke(DataSource); }
            set { _getCaption = ds => value; }
        }

        public bool IsMasterDetail => MasterDetails != null;
        public MasterDetails MasterDetails { get; set; }

        public object DataSource => _getDatasource?.Invoke();
        public object ViewToken { get; set; }
        public object ViewContext { get; set; }
    }
}