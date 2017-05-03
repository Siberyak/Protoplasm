using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.Calendars.Tests
{
    internal class ByDayInfosAdapter : ByDayInfosAdapter<int?>
    {
        public ByDayInfosAdapter(Calendars<DateTime, TimeSpan, int?>.ICalendar baseCalendar, IEnumerable<DayInfoBase> infos) 
            : base(8, 0, baseCalendar, infos)
        {
        }

        protected override int? DifferenceToData(TimeSpan difference)
        {
            return difference.Hours;
        }

        public override int? Include(int? a, int? b)
        {
            return a.HasValue || b.HasValue
                ? (a ?? 0) + (b ?? 0)
                : default(int?);
        }

        public override int? Exclude(int? a, int? b)
        {
            return a.HasValue || b.HasValue
                ? (a ?? 0) - (b ?? 0)
                : default(int?);
        }
    }
}