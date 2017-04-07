using System.Collections.Generic;

namespace Protoplasm.Utils.Graph
{
    public class DataEdge<TEdgeData> : IDataEdge<TEdgeData>
    {
        private readonly MutableDataGraph _dataGraph;

        public DataEdge(MutableDataGraph dataGraph, TEdgeData data, INode @from, INode to, bool isBackreference)
        {
            _dataGraph = dataGraph;
            From = @from;
            To = to;
            IsBackreference = isBackreference;
            Data = data;
        }

        public IMutableDataGraph Graph { get { return _dataGraph; } }
        IGraph IEdge.Graph
        {
            get { return Graph; }
        }

        public object EdgeData()
        {
            return Data;
        }

        public INode From { get; }

        public INode To { get; }

        public bool IsBackreference { get; }
        public Dictionary<object, object> Tags { get; } = new Dictionary<object, object>();

        public TEdgeData Data { get; }

        public override string ToString()
        {
            return string.Format("Edge, Data: [{0}], From: [{1}], To: [{2}]", Data, From, To);
        }
    }
}