using System;
using System.Collections.Generic;

namespace Protoplasm.Calendars.Tests
{
    public class YearlyHoliday : DayInfoBase
    {

        public readonly int Day;
        public readonly int Month;

        public YearlyHoliday(int day, int month)
        {
            Day = day;
            Month = month;
        }

        public override bool IsWorkday => false;

        public override bool Between(DateTime left, DateTime right)
        {
            return left.Month <= Month && left.Day <= Day && right.Month >= Month && right.Day >= Day;
        }

        public static IEnumerable<YearlyHoliday> Range(int fromDay, int toDay, int month)
        {
            while (fromDay <= toDay)
            {
                yield return new YearlyHoliday(fromDay++, month);
            }
        }

        public DateTime ToDate(int year)
        {
            return new DateTime(year, Month, Day);
        }
    }
}