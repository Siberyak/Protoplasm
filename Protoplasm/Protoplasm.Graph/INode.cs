using System.Collections.Generic;

namespace Protoplasm.Graph
{
    public interface INode
    {
        IGraph Graph { get; }
        IEnumerable<IEdge> References { get; }
        IEnumerable<IEdge> BackReferences { get; }
        Dictionary<object, object> Tags { get; }

    }
}