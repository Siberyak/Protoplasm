using System;
using System.Collections;
using System.Linq;
using DevExpress.XtraExport;
using Protoplasm.Utils.Graph;

namespace Factorio.Lua.Reader
{
    class N : StoragedNode
    {
        public N(Storage storage, string caption) : base(storage, caption, null)
        {
        }

        protected override IList GetChildren()
        {
            var types = Storage.Nodes.Select(x => x.GetType()).Distinct();

            return types.Select(t => new TN(Storage, t)).ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return "Nodes";
        }
    }

    class TN : StoragedNode
    {
        private readonly Type _type;

        public TN(Storage storage, Type type) : base(storage, type.Name, null)
        {
            _type = type;
        }

        protected override IList GetChildren()
        {
            return Storage.Nodes.Where(x => x.GetType() == _type).Select(x => new DataNode(x)).ToArray();
        }
    }

    class DataNode : VirtualTreeListData<INode>
    {

        public DataNode(INode data) : base((Storage) data.Graph, data)
        {
        }

        protected override IList GetChildren()
        {
            return Data.References.GroupBy(x => x.GetType()).Select(x => new ColNode(Storage, x.Key, x.Select(y => y.To).ToArray())).ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return Data.ToString();
        }
    }

    class ColNode : VirtualTreeListData
    {
        private readonly Type _type;
        private readonly INode[] _nodes;

        public ColNode(Storage storage, Type type, INode[] nodes) : base(storage)
        {
            _type = type;
            _nodes = nodes;
        }

        protected override IList GetChildren()
        {
            return _nodes.Select(x => new DataNode(x)).ToArray();
        }

        protected override object GetValue(string fieldName)
        {
            return _type.Name;
        }

        public override object NodeData => null;


    }
}