using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.TestData
{
    public class Ability : BaseAbility
    {
        public Ability(MappingType mappingType)
        {
            MappingType = mappingType;
        }

        public MappingType MappingType { get; private set; }

        protected override bool Conformable(BaseRequirement requirement)
        {
            return false;
        }
    }



    public class Requirement : BaseRequirement
    {

        public Requirement(MappingType mappingType)
        {
            MappingType = mappingType;
        }

        public MappingType MappingType { get; private set; }

        protected override bool Conformable(BaseAbility ability)
        {
            return false;
        }
    }
}
