using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Utils;
using Protoplasm.Calendars;

namespace Application1.Data
{
    public class Resource : MembershipItem
    {

        public static readonly WorkCalendar WorkCalendar;

        static Resource()
        {
            WorkCalendar = new WorkCalendar(new WorkCalendarAdapter());
        }

        private readonly CompetencesAbility _competencesAbility;
        private readonly ScheduleAbility _scheduleAbility;
        public WorkSchedule Schedule => _scheduleAbility.Schedule;

        public IReadOnlyCollection<Competence> Competences => _competencesAbility.Competences;

        public Resource(string caption, IReadOnlyCollection<Competence> competences, params ResourcesGroup[] memberOf) 
            : base(caption, memberOf)
        {
            _competencesAbility = new CompetencesAbility(competences.Union(MembershipCompetences).ToArray());
            _scheduleAbility = new ScheduleAbility(WorkCalendar);
        }

        protected override IReadOnlyCollection<Ability> GenerateAbilities()
        {
            return new Ability[] { _competencesAbility, _scheduleAbility };
        }
    }
}