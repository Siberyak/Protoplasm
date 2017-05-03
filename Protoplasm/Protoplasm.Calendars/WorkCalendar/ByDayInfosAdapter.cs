using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.Calendars
{
    public abstract class ByDayInfosAdapter<T> : Calendars<DateTime, TimeSpan, T>.CalendarAdapter<T>
    {
        protected readonly T DaylyWorkData;
        protected readonly T DaylyNotWorkData;
        protected readonly IEnumerable<DayInfoBase> DayInfos;

        protected ByDayInfosAdapter(T daylyWorkData, T daylyNotWorkData, Calendars<DateTime, TimeSpan, T>.ICalendar baseCalendar, IEnumerable<DayInfoBase> infos) : base(baseCalendar)
        {
            DaylyWorkData = daylyWorkData;
            DaylyNotWorkData = daylyNotWorkData;
            DayInfos = infos;
        }

        protected override DateTime DefineByBaseData(Calendars<DateTime, TimeSpan, T>.ICalendarItems container, DateTime left, DateTime end, T baseData)
        {
            var year = left.Year;

            var endOfYear = left.AddMonths(1 - left.Month).AddDays(1 - left.Day).AddYears(1).AddDays(-1);

            end = Calendars<DateTime, TimeSpan>.Min(endOfYear, end.AddDays(-1));

            var infos = DayInfos.Where(x => x.Between(left, end)).ToArray();


            container.Include(left, end.AddDays(1), baseData, rightIncluded: false);


            foreach (var info in infos)
            {
                var date = (info as YearlyHoliday)?.ToDate(year) ?? ((DayInfo)info).Date;

                if (info.IsWorkday)
                {
                    var difference = DifferenceToData(info.Difference);
                    var value = Include(Exclude(DaylyWorkData, Equals(baseData, default(T)) ? DaylyNotWorkData : baseData), difference);
                    container.Include(date, date.AddDays(1), value, rightIncluded: false);
                }
                else
                {
                    container.Exclude(date, date.AddDays(1), baseData, rightIncluded: false);
                }
            }

            return end.AddDays(1);
        }

        protected abstract T DifferenceToData(TimeSpan difference);
    }
}