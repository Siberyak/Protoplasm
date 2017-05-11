using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface IHolder
    {
        INegotiator this[IScene scene] { get; }
    }
}