using MAS.Core.Contracts;
using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public abstract class Negotiator<T> : Negotiator<T, Satisfaction>
        where T : IAgent
    {
        protected Negotiator(IScene scene, T agent) : base(scene, agent)
        {
        }

        protected override Satisfaction CreateSatisfaction(ISatisfaction original)
        {
            return new Satisfaction(0);
        }

        public override bool IsSatisfied => Satisfaction.Value > 0;
    }
}