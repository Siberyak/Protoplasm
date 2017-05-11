using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.PointedIntervals;

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

        protected override Point<DateTime> DefineByBaseData(Calendars<DateTime, TimeSpan, T>.ICalendarItems container, Point<DateTime> left, Point<DateTime> right, T baseData)
        {
            if (!left.PointValue.HasValue)
                throw new ArgumentException("значение не определено", nameof(left));
            if (!right.PointValue.HasValue)
                throw new ArgumentException("значение не определено", nameof(right));

            left = Point<DateTime>.Left(left.Date(), true);
            right = Point<DateTime>.Right(right.Date(), false);

            var year = left.Year();

            var endOfYear = Point<DateTime>.Right(new DateTime(year+1, 1, 1), false);

            right = Point<DateTime>.Min(endOfYear, right);


            var infos = DayInfos.Where(x => x.Between(left.Value(), right.Value().AddMilliseconds(-1))).ToArray();

            container.Include(left, right, baseData);


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

            return right.AsLeft();
        }

        protected abstract T DifferenceToData(TimeSpan difference);
    }

    public static class DateTimePointExtender
    {
        public static Point<DateTime> Left(this DateTime value, bool included)
        {
            return Point<DateTime>.Left(value, included);
        }
        public static Point<DateTime> Right(this DateTime value, bool included)
        {
            return Point<DateTime>.Right(value, included);
        }

        public static DateTime Value(this Point<DateTime> point)
        {
            return point.PointValue.Value;
        }

        private static Point<DateTime> ChangeValue(this Point<DateTime> point, Func<DateTime, DateTime> changeValue, bool? included = null)
        {
            if (!point.PointValue.HasValue)
                return point;

            var value = changeValue(point.PointValue.Value);

            return point.Direction == PointDirection.Left
                ? Point<DateTime>.Left(value, included ?? point.Included)
                : Point<DateTime>.Right(value, included ?? point.Included);
        }

        public static Point<DateTime> Add(this Point<DateTime> point, TimeSpan value)
        {
            return point.ChangeValue(x => x.Add(value));
        }
        public static Point<DateTime> AddDays(this Point<DateTime> point, double value)
        {
            return point.ChangeValue(x => x.AddDays(value));
        }
        public static Point<DateTime> AddHours(this Point<DateTime> point, double value)
        {
            return point.ChangeValue(x => x.AddHours(value));
        }
        public static Point<DateTime> AddMilliseconds(this Point<DateTime> point, double value)
        {
            return point.ChangeValue(x => x.AddMilliseconds(value));
        }
        public static Point<DateTime> AddMinutes(this Point<DateTime> point, double value)
        {
            return point.ChangeValue(x => x.AddMinutes(value));
        }
        public static Point<DateTime> AddMonths(this Point<DateTime> point, int value)
        {
            return point.ChangeValue(x => x.AddMonths(value));
        }
        public static Point<DateTime> AddSeconds(this Point<DateTime> point, double value)
        {
            return point.ChangeValue(x => x.AddSeconds(value));
        }
        public static Point<DateTime> AddTicks(this Point<DateTime> point, long value)
        {
            return point.ChangeValue(x => x.AddTicks(value));
        }
        public static Point<DateTime> AddYears(this Point<DateTime> point, int value)
        {
            return point.ChangeValue(x => x.AddYears(value));
        }
        public static Point<DateTime> AsDate(this Point<DateTime> point, bool? included = null)
        {
            return point.ChangeValue(x => x.Date, included);
        }

        public static DateTime Date(this Point<DateTime> point)
        {
            return Value(point).Date; 
        }

        public static int Day(this Point<DateTime> point)
        {
            return Value(point).Day;
        }

        public static DayOfWeek DayOfWeek(this Point<DateTime> point)
        {
            return Value(point).DayOfWeek;
        }

        public static int DayOfYear(this Point<DateTime> point)
        {
            return Value(point).DayOfYear;
        }

        public static int Hour(this Point<DateTime> point)
        {
            return Value(point).Hour;
        }

        public static DateTimeKind Kind(this Point<DateTime> point)
        {
            return Value(point).Kind;
        }

        public static int Millisecond(this Point<DateTime> point)
        {
            return Value(point).Millisecond;
        }

        public static int Minute(this Point<DateTime> point)
        {
            return Value(point).Minute;
        }

        public static int Month(this Point<DateTime> point)
        {
            return Value(point).Month;
        }

        public static int Second(this Point<DateTime> point)
        {
            return Value(point).Second;
        }

        public static long Ticks(this Point<DateTime> point)
        {
            return Value(point).Ticks;
        }

        public static TimeSpan TimeOfDay(this Point<DateTime> point)
        {
            return Value(point).TimeOfDay;
        }

        public static int Year(this Point<DateTime> point)
        {
            return Value(point).Year;
        }
    }
}