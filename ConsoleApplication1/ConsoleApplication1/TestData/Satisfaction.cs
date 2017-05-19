namespace ConsoleApplication1.TestData
{
    public class Satisfaction : Satisfaction<double>
    {
        public Satisfaction(double original) : base(original)
        {
        }

        public override double Value => _original + Delta;
    }
}