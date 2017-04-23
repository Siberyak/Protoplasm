using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface IRequirementsCompatibilityInfo
    {
        IRequirementsHolder Holder { get; }
    }
}