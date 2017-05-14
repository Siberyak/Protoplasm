using System;
using System.Collections;
using System.Linq;

namespace Factorio.Lua.Reader
{
    class StoragedNode : VirtualTreeListData
    {
        readonly string _caption;
        private readonly Func<Storage, IEnumerable> _getChildren;

        public StoragedNode(Storage storage, string caption, Func<Storage, IEnumerable> getChildren) : base(storage)
        {
            _caption = caption;
            _getChildren = getChildren;
        }

        protected override IList GetChildren()
        {
            return _getChildren(Storage).Cast<object>().ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return _caption;
        }

        public override object NodeData => null;
    }
}