using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Protoplasm.Graph
{
    public interface IEdge
    {
        //string Caption { get; }
        IGraph Graph { get; }
        INode From { get; }
        INode To { get; }

        //RelationTypes RelationType { get; }
        //Multiplicity Multiplicity { get; }
        bool IsBackreference { get; }

        Dictionary<object, object> Tags { get; } 
	}
}