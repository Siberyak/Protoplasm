using System.Collections.Generic;

namespace MAS.Core.Contracts
{
    public interface IScene
    {
        IScene Original { get; }

        IScene Branch();

        void MergeToOriginal();

        ISatisfaction Satisfaction { get; }

        IEnumerable<INegotiator> Negotiators { get; }
        

    }
}