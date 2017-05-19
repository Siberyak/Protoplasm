using System.Collections.Generic;
using Factorio.Lua.Reader.Graph;
using Protoplasm.Graph;

namespace Factorio.Lua.Reader
{
    public class EdgeBase : IEdge
    {
        protected readonly INode _from;
        protected readonly INode _to;
        public IGraph Graph { get; }
        INode IEdge.From => _from;
        INode IEdge.To => _to;
        public bool IsBackreference { get; }
        public Dictionary<object, object> Tags { get; } = new Dictionary<object, object>();

        public EdgeBase(Storage storage, Base @from, Base to)
        {
            _from = from;
            _to = to;
            Graph = storage;
        }

        public override string ToString()
        {
            return $"{_from} -> {_to}";
        }
    }
}