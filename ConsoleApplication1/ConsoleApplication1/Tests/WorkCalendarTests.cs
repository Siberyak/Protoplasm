using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApplication1.TestData
{
    public class WorkCalendarTests
    {
        private static readonly Holidays Holidays = new Holidays
        {
            //===================================================================================
            // ��������� ����������� �������� ��� 
            //===================================================================================
            {1, 8, 1}, //   1-8 ������
            {23, 2}, //     23 �������
            {8, 3}, //      8 �����
            {1, 5}, //      1 ��� 
            {9, 5}, //      9 ��� 
            {12, 6}, //     12 ����
            {4, 11}, //     4 ������

            //===================================================================================
            // 2017 - �������� � ����������� 
            //===================================================================================
            new DateTime(2017, 2, 24), //           � ����������� 1 ������ �� ������� 24 �������
            new DateTime(2017, 5, 8), //            � ������� 7 ������ �� ����������� 8 ���
            new DateTime(2017, 11, 6), //           � ������� 4 ������ �� ����������� 6 ������

            {new DateTime(2017, 2, 22), -1}, //     ����������� ��������������� ������� ����
            {new DateTime(2017, 3, 7), -1}, //      ����������� ��������������� ������� ����
            {new DateTime(2017, 11, 3), -1}, //     ����������� ��������������� ������� ����
            //===================================================================================
        };

        public static void WorkCalendars()
        {

            // ��� �� Duration ���������� ��� ��� ������������ ������� ����� �� ������
            Calendars<DateTime, TimeSpan>.GetOffset = (a, b) => b - a;

            var byDayOfWeek = new ByDayOfWeek();
            var byDayInfos = new ByDayInfos(Holidays);

            // ������� ��������� ������� ���� - [��. - ��.]:(8 �.) + [��. - ����.]:(0 �.)
            var baseCalendar = new WorkCalendar(byDayOfWeek);

            // ��������� + �������� + ����������� ���������������
            var calendar = new WorkCalendar(baseCalendar, byDayInfos);

            // ����� ���������� �������
            var items1 = baseCalendar.Get(new DateTime(2017, 1, 1), DateTime.Today);
            var items2 = calendar.Get(new DateTime(2017, 1, 1), DateTime.Today);

            // ������� ��������� � ���� ������
            var result = calendar.Get(new DateTime(2017, 1, 1), new DateTime(2017, 1, 1).AddYears(1).AddDays(-1));

            // ����������� ������ ����� �� ������
            var hours1 = result.Where(x => x.Duration.HasValue).Sum(x => x.Duration.Value.Days * x.Data);

            // ������ �����
            var hours2 = calendar.WorkHours(2017);

            Debug.Assert(hours1 == hours2);
            Debug.Assert(hours2 == 1973);

        }
    }


    class WorkCalendar : Calendars<DateTime, TimeSpan, int?>.Calendar
    {

        public int WorkHours(int year)
        {
            var left = new DateTime(year, 1, 1);
            var items = Get(left, left.AddYears(1).AddDays(-1));

            var hours = items.Sum(Hours);

            return hours;
        }

        private static int Hours(Calendars<DateTime, TimeSpan, int?>.CalendarItem x)
        {
            var duration = x.Duration ?? (x.Right.PointValue - x.Left.PointValue) ?? TimeSpan.Zero;
            return duration.Days * (x.Data ?? 0);
        }


        public WorkCalendar(WorkCalendarHelper helper)
            : base(helper)
        {
        }

        public WorkCalendar(WorkCalendar prev, WorkCalendarHelper helper)
            : base(prev, helper)
        {
        }
    }

    abstract class WorkCalendarHelper : Calendars<DateTime, TimeSpan, int?>.Calendar.CalendarHelper
    {
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



    class ByDayInfos : WorkCalendarHelper
    {
        private readonly IEnumerable<DayInfoBase> _infos;

        public ByDayInfos(IEnumerable<DayInfoBase> infos)
        {
            _infos = infos;
        }

        protected override DateTime ProcessInterval(Calendars<DateTime, TimeSpan, int?>.ICalendarItems container, DateTime left, DateTime end, int? data)
        {
            var year = left.Year;

            var endOfYear = left.AddMonths(1 - left.Month).AddDays(1 - left.Day).AddYears(1).AddDays(-1);

            end = PlanningEnvironment<DateTime, TimeSpan>.Min(endOfYear, end.AddDays(-1));

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



    class ByDayOfWeek : WorkCalendarHelper
    {
        protected override DateTime ProcessInterval(Calendars<DateTime, TimeSpan, int?>.ICalendarItems container, DateTime left, DateTime end, int? data)
        {
            DateTime? right = left;
            var value = 0;
            var dayOfWeek = left.DayOfWeek;
            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    right = left.AddDays(1);
                    break;
                case DayOfWeek.Sunday:
                    break;
                default:
                    right = left.AddDays(5 - (int)dayOfWeek);
                    value = 8;
                    break;
            }

            right = PlanningEnvironment<DateTime, TimeSpan>.Min(right.Value, end).AddDays(1);

            container.Include(left, right, value, rightIncluded: false);

            return right.Value;
        }
    }

    public class DayInfo : DayInfoBase
    {
        public override TimeSpan Difference => _difference ?? base.Difference;

        public readonly DateTime Date;

        private readonly TimeSpan? _difference;
        public override bool IsWorkday { get; }

        public DayInfo(DateTime date, TimeSpan difference) : this(date, true)
        {
            _difference = difference;
        }

        public DayInfo(DateTime date, bool isWorkday)
        {
            Date = date;
            IsWorkday = isWorkday;
        }

        public override bool Between(DateTime left, DateTime right)
        {
            return left <= Date && Date <= right;
        }
    }

    public class Holidays : IEnumerable<DayInfoBase>
    {
        private readonly List<DayInfoBase> _holidays = new List<DayInfoBase>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<DayInfoBase> GetEnumerator()
        {
            return _holidays.GetEnumerator();
        }

        /// <summary>
        /// ��� ��������� ��������.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        public void Add(int day, int month)
        {
            _holidays.Add(new YearlyHoliday(day, month));
        }

        /// <summary>
        /// ��� ���������� ��������� ��������. ��������, 1-8 ������
        /// </summary>
        /// <param name="dayfrom"></param>
        /// <param name="dayTo"></param>
        /// <param name="month"></param>
        public void Add(int dayfrom, int dayTo, int month)
        {
            _holidays.AddRange(YearlyHoliday.Range(dayfrom, dayTo, month));
        }

        /// <summary>
        /// ��� ������������ ��������.
        /// </summary>
        /// <param name="date"></param>
        public void Add(DateTime date)
        {
            _holidays.Add(new DayInfo(date, false));
        }

        /// <summary>
        /// ��� ������� ���� � �������� �� ������� �������������. �������� ��� ��������������� ����
        /// </summary>
        /// <param name="date"></param>
        /// <param name="difference"></param>
        public void Add(DateTime date, int difference)
        {
            _holidays.Add(new DayInfo(date, TimeSpan.FromHours(difference)));
        }
    }

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

    public abstract class DayInfoBase
    {
        public virtual TimeSpan Difference { get; } = TimeSpan.Zero;
        public abstract bool IsWorkday { get; }
        public abstract bool Between(DateTime left, DateTime right);

    }
}