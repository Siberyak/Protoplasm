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

        protected override ConformResult Conformable(BaseRequirement requirement)
        {
            return ConformResult.Empty;
        }
    }
}
