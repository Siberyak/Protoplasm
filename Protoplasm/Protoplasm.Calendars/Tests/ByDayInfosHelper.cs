using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.Calendars.Tests
{
    internal class ByDayInfosHelper : WorkCalendarHelper
    {
        private readonly IEnumerable<DayInfoBase> _infos;

        public ByDayInfosHelper(IEnumerable<DayInfoBase> infos)
        {
            _infos = infos;
        }

        protected override DateTime ProcessInterval(Calendars<DateTime, TimeSpan, int?>.ICalendarItems container, DateTime left, DateTime end, int? data)
        {
            var year = left.Year;

            var endOfYear = left.AddMonths(1 - left.Month).AddDays(1 - left.Day).AddYears(1).AddDays(-1);

            end = Calendars<DateTime, TimeSpan>.Min(endOfYear, end.AddDays(-1));

            var infos = _infos.Where(x => x.Between(left, end)).ToArray();


            container.Include(left, end.AddDays(1), data, rightIncluded: false);


            foreach (var info in infos)
            {
                var date = (info as YearlyHoliday)?.ToDate(year) ?? ((DayInfo)info).Date;

                if (info.IsWorkday)
                {
                    var value = 8 + info.Difference.Hours - (data ?? 0);
                    container.Include(date, date.AddDays(1), value, rightIncluded: false);
                }
                else
                {
                    container.Exclude(date, date.AddDays(1), data, rightIncluded: false);
                }
            }

            return end.AddDays(1);
        }
    }
}