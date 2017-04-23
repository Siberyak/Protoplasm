namespace MAS.Core.Compatibility.Contracts
{
    public interface IAbility
    {
        bool IsMutable { get; }
        CompatibilityType Compatible(IRequirement requirenet);
    }
}