using MAS.Core.Contracts;

namespace MAS.Core.Compatibility.Contracts
{
    public interface IAbility
    {
        bool IsMutable { get; }
        CompatibilityType Compatible(IRequirement requirement);
        bool Compatible(IRequirement requirement, IScene scene);
    }

}