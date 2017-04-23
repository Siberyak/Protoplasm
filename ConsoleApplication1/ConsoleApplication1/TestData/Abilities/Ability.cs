using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;

namespace ConsoleApplication1.TestData
{
    public abstract class Ability : BaseAbility
    {
        protected Ability(MappingType mappingType)
        {
            MappingType = mappingType;
        }

        public MappingType MappingType { get; private set; }

        public override CompatibilityType Compatible(IRequirement requirement)
        {
            return CompatibilityType.Never;
        }
    }
}
