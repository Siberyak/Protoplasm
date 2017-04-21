using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.TestData
{
    public abstract class Ability : BaseAbility
    {
        protected Ability(MappingType mappingType)
        {
            MappingType = mappingType;
        }

        public MappingType MappingType { get; private set; }

        public override ConformType Conformable(BaseRequirement requirement)
        {
            return ConformType.Imposible;
        }
    }
}
