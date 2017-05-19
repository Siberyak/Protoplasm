using System;
using System.Collections.Generic;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class CalendarAbility : Ability
        {
            public IAvailabilityCalendar Calendar { get; }

            public CalendarAbility(IAvailabilityCalendar calendar)
            {
                Calendar = calendar;
            }

            public override CompatibilityType Compatible(IRequirement requirement)
            {
                // TODO: в целях оптимизации надо бы проверить календарь на наличие необходимых ворк-тайпов
                // но потом... всё потом... пока что не критично...
                return CompatibilityType.DependsOnScene;
            }

            public override bool Compatible(IRequirement requirement, IScene scene)
            {
                throw new System.NotSupportedException();
            }

            public override IAbility ToScene()
            {
                return new ScheduleAbility(this);
            }
        }
    }
}