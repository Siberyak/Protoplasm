namespace MAS.Core.Compatibility.Contracts
{
    public interface IRequirement
    {
        bool IsMutable { get; }
        CompatibilityType Compatible(IAbility ability);
    }
}