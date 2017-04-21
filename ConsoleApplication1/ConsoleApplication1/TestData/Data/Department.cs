using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class Department : MembershipItemsContainer
        {
            public Department(string caption, params Department[] memberOf) : base(caption, memberOf)
            {
            }


            protected override IReadOnlyCollection<Ability> GenerateAbilities()
            {
                var data = Members.Select(x => new {Member = x, Calendar = x.Abilities.OfType<CalendarAbility>().FirstOrDefault() } );
                return new Ability[] {};
            }
        }
    }
}