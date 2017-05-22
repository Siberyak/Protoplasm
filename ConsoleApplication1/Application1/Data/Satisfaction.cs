using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAS.Core.Contracts;
using MAS.Utils;

namespace Application1.Data
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

        public void Change(double value)
        {
            Δ = value - Value;
        }
    }
}
