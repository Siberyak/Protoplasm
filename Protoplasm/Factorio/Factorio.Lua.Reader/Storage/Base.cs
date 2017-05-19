using System.Collections.Generic;
using System.Linq;
using Factorio.Lua.Reader.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protoplasm.Graph;

namespace Factorio.Lua.Reader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Base : INode, IStoragedData
    {
        IGraph INode.Graph => Graph;

        public IEnumerable<IEdge> References => Graph.Edges.Where(x => x.From == this);
        public IEnumerable<IEdge> BackReferences => Graph.Edges.Where(x => x.To == this);
        public Dictionary<object, object> Tags { get; } = new Dictionary<object, object>();
        public IMutableDataGraph Graph { get; protected set; }
        public object NodeData()
        {
            return null;
        }

        protected JToken _token;

        public JToken Token => _token;

        public virtual void SetToken(JToken token)
        {
            _token = token;
        }

        public virtual void ProcessLinks()
        { }

        public void SetGraph(IMutableDataGraph graph)
        {
            Graph = graph;
        }

        public Storage Storage => (Storage) Graph;
    }
}