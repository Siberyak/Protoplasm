using System.Collections.Generic;

namespace Protoplasm.Utils.Graph
{
    public interface INode
    {
        IGraph Graph { get; }
        IEnumerable<IEdge> References { get; }
        IEnumerable<IEdge> BackReferences { get; }
        Dictionary<object, object> Tags { get; }

    }
}