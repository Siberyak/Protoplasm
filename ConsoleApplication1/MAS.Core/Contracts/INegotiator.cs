namespace MAS.Core.Contracts
{
    public interface INegotiator : IRespondent
    {
        IScene Scene { get; }

        ISatisfaction Satisfaction { get; }
    }
}