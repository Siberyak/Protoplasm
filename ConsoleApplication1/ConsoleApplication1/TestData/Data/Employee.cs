using System;
using System.Collections.Generic;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {

        public class Employee : MembershipItem
        {
            private readonly CompetencesAbility _competencesAbility;
            private readonly CalendarAbility _calendarAbility;

            public Calendar Calendar => _calendarAbility.Calendar;
            public IReadOnlyCollection<Competence> Competences => _competencesAbility.Competences;

            public Employee(string caption, IReadOnlyCollection<Competence> competences, Calendar calendar, params MembershipItemsContainer[] memberOf) 
                : base(caption, memberOf)
            {
                _competencesAbility = new CompetencesAbility(competences);
                _calendarAbility = new CalendarAbility(calendar);
            }

            protected override IReadOnlyCollection<Ability> GenerateAbilities()
            {
                return new Ability[] {_competencesAbility, _calendarAbility};
            }
        }

    }
}