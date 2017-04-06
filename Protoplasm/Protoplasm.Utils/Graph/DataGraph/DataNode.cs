using System.Collections.Generic;
using KG.SE2.Utils.Collections;

namespace KG.SE2.Utils.Graph
{
    public class DataNode<TNodeData> : IDataNode<TNodeData>
    {
        protected readonly MutableDataGraph _dataGraph;
        private PredicatedList<IEdge> _backReferences;

        private PredicatedList<IEdge> _references;

        public DataNode(MutableDataGraph dataGraph, TNodeData data)
        {
            _dataGraph = dataGraph;
            Data = data;
        }

        public IMutableDataGraph Graph { get { return _dataGraph; } }
        IGraph INode.Graph
        {
            get { return Graph; }
        }

        public object NodeData()
        {
            return Data;
        }

        public string Caption
        {
            get { return "" + Data; }
        }

        public IEnumerable<IEdge> References
        {
            get { return _references ?? (_references = new PredicatedList<IEdge>(_dataGraph.EdgesDataList, x => x.IsBackreference ? x.To == this : x.From == this)); }
        }

        public IEnumerable<IEdge> BackReferences
        {
            get { return _backReferences ?? (_backReferences = new PredicatedList<IEdge>(_dataGraph.EdgesDataList, x => x.IsBackreference ? x.From == this : x.To == this)); }
        }

        public TNodeData Data { get; }

        public override string ToString()
        {
            return string.Format("Node, Data: [{0}]", Data);
        }

        public Dictionary<object, object> Tags { get; } = new Dictionary<object, object>();

    }
}