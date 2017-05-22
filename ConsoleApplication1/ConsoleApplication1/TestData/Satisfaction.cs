using MAS.Core.Contracts;
using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public class Satisfaction : Satisfaction<double>
    {
        public Satisfaction(double original) : base(original)
        {
        }

        public override double Value => _original + Δ;

        public override ISatisfaction Snapshot()
        {
            return new Satisfaction(_original) { Δ = Δ };
        }
    }
}