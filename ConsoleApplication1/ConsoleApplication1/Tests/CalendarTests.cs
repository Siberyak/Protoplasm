using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApplication1.TestData
{
    public class CalendarTests
    {
        private static readonly Holidays Holidays = new Holidays
        {
            {1, 8, 1}, //   1-8 €нвар€
            {23, 2}, //     23 феврал€
            {8, 3}, //      8 марта
            {1, 5}, //      1 ма€ 
            {9, 5}, //      9 ма€ 
            {12, 6}, //     12 июн€
            {4, 11}, //     4 но€бр€

            new DateTime(2017, 2, 24),
            new DateTime(2017, 5, 8),
            new DateTime(2017, 11, 6),

            {new DateTime(2017, 2, 22), -1}, //   сокращенный предпраздничный рабочий день
            {new DateTime(2017, 3, 7), -1}, //    сокращенный предпраздничный рабочий день
            {new DateTime(2017, 11, 3), -1}, //   сокращенный предпраздничный рабочий день
        };

        public static void WorkCalendars()
        {

            // что бы Duration вычисл€лс€ сам дл€ контрольного расчета часов по списку
            PlanningEnvironment<DateTime, TimeSpan>.GetOffset = (a, b) => b - a;

            var byDayOfWeek = new ByDayOfWeek();
            var byDayInfos = new ByDayInfos(Holidays);

            // базовый календарь рабочих дней - [пн. - пт.]:(8 ч.) + [сб. - вскр.]:(0 ч.)
            var baseCalendar = new WorkCalendar(byDayOfWeek);

            // праздники + переносы + укороченные предпраздничные
            var calendar = new WorkCalendar(baseCalendar, byDayInfos);

            // чисто посмотреть разницу
            var items1 = baseCalendar.Get(new DateTime(2017, 1, 1), DateTime.Today);
            var items2 = calendar.Get(new DateTime(2017, 1, 1), DateTime.Today);

            // искомый результат в виде списка
            var result = calendar.Get(new DateTime(2017, 1, 1), new DateTime(2017, 1, 1).AddYears(1).AddDays(-1));

            // контрольный расчет часов по списку
            var hours1 = result.Where(x => x.Duration.HasValue).Sum(x => x.Duration.Value.Days * x.Data);

            // расчет часов
            var hours2 = calendar.WorkHours(2017);

            Debug.Assert(hours1 == hours2);
            Debug.Assert(hours2 == 1973);

        }


        public static void Calendars()
        {
            // календарь по дн€м недели
            var calendar1 = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>(ByDayOfWeek, (a, b) => b, (a, b) => CalendarItemType.Unknown);
            var tmp1 = calendar1.Get(DateTime.Today, DateTime.Today.AddDays(10));
            var tmp2 = calendar1.Get(DateTime.Today.AddDays(20), DateTime.Today.AddDays(30));
            var tmp3 = calendar1.Get(DateTime.Today.AddDays(15), DateTime.Today.AddDays(35));

            // календарь доопредел€ет рабочее врем€
            // опираетс€ на календарь по дн€м недели
            var calendar2 = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>(calendar1, ByWorkingTime, (a, b) => b, (a, b) => CalendarItemType.Unavalable);
            var tmp4 = calendar2.Get(DateTime.Today.AddDays(5), DateTime.Today.AddDays(15));

            // опираетс€ на календарь рабочего времени 
            // конвертирует данные в другой тип
            var calendar3 = new PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.PrevBasedCalendar<CalendarItemType>
                (
                calendar2, DoDefineData,
                (a, b) => b, (a, b) => null
                );

            var tmp5 = calendar3.Get(DateTime.Today.AddYears(1), DateTime.Today.AddYears(1).AddDays(15));
        }

        private static void ByDayOfWeek(PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItems container, PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItem toDefine)
        {
            var begin = toDefine.Left.PointValue.Value;
            var end = toDefine.Right.PointValue.Value;

            while (begin < end)
            {
                var available = CalendarItemType.Available;
                var date = begin.Date.AddDays(1);
                var dayOfWeek = begin.DayOfWeek;

                switch (dayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        date = date.AddDays(1);
                        available = CalendarItemType.Unavalable;
                        break;
                    case DayOfWeek.Sunday:
                        available = CalendarItemType.Unavalable;
                        break;
                    default:
                        date = date.AddDays(5 - (int) dayOfWeek);
                        break;
                }

                container.Include
                    (
                        begin,
                        begin = PlanningEnvironment<DateTime, TimeSpan>.Min(date, end),
                        available,
                        rightIncluded: false
                    );
            }
        }

        private static void ByWorkingTime(PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItems container, PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItem toDefine)
        {
            var begin = toDefine.Left.PointValue.Value;
            var end = toDefine.Right.PointValue.Value;

            container.Include(begin, end, toDefine.Data, rightIncluded: false);
            if (toDefine.Data == CalendarItemType.Unavalable)
                return;

            while (begin < end)
            {
                var interval = ExcludeIfNeed(begin, end, 0, 9, container);
                interval = ExcludeIfNeed(begin, end, 13, 14, container) ?? interval;
                interval = ExcludeIfNeed(begin, end, 18, 24, container) ?? interval;


                begin = interval?.Right.PointValue ?? begin.Date.AddDays(1);
            }
        }

        private static PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItem ExcludeIfNeed(DateTime begin, DateTime end, int beginHour, int endHour,
            PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItems container)
        {

            var wantedBegin = begin.Date.AddHours(beginHour);
            var wantedEnd = begin.Date.AddHours(endHour);


            if (wantedBegin > end)
                return null;
            if (wantedEnd < begin)
                return null;

            wantedBegin = PlanningEnvironment<DateTime, TimeSpan>.Max(begin, wantedBegin);
            wantedEnd = PlanningEnvironment<DateTime, TimeSpan>.Min(end, wantedEnd);

            PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItem interval;
            //= container.NewInterval(Point<DateTime>.Right(wantedBegin), Point<DateTime>.Right(wantedEnd, false), CalendarItemType.Unavalable);
            container.Exclude(out interval, wantedBegin, wantedEnd, CalendarItemType.Unavalable, true, false);
            return interval;
        }

        private static void DoDefineData(PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.CalendarItems container, PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItem todefine)
        {
            if (todefine.Data == CalendarItemType.Unknown)
                container.Exclude(todefine.Left, todefine.Right, null);
            else
                container.Include(todefine.Left, todefine.Right, todefine.Data == CalendarItemType.Available ? 1 : 0);
        }

        public static void TestCalendarItems()
        {
            var container = new PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItems((a, b) => b, (a, b) => CalendarItemType.Unknown);
            container.Include(DateTime.Today, DateTime.Today.AddDays(1), CalendarItemType.Available);
            container.Include(DateTime.Now, DateTime.Now.AddHours(1), CalendarItemType.Available);

            var environment = new PlanningEnvironment<DateTime, TimeSpan>();


            //var request = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>(TODO, TODO, TODO).CalendarItem() {Begin = DateTime.Now.AddDays(-10), End = DateTime.Now.AddDays(12)};

            //var items0 = new[] {request};

            //var items1 = ByDayOfWeek(items0);
            //var items2 = ByWorkingTime(items1);
        }
    }

    public enum CalendarItemType
    {
        Unknown = 0,
        Available,
        Unavalable
    }


    class WorkCalendar : PlanningEnvironment<DateTime, TimeSpan>.Calendar<int?>
    {

        public int WorkHours(int year)
        {
            var left = new DateTime(year, 1, 1);
            var items = Get(left, left.AddYears(1).AddDays(-1));

            var hours = items.Sum(Hours);

            return hours;
        }

        private static int Hours(PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.CalendarItem x)
        {
            var duration = x.Duration ?? (x.Right.PointValue - x.Left.PointValue) ?? TimeSpan.Zero;
            return duration.Days*(x.Data ?? 0);
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

    abstract class WorkCalendarHelper : PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.HelperBase
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

        protected override DateTime ProcessInterval(PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.CalendarItems container, DateTime left, DateTime end, int? data)
        {
            var year = left.Year;

            var endOfYear = left.AddMonths(1 - left.Month).AddDays(1 - left.Day).AddYears(1).AddDays(-1);

            end = PlanningEnvironment<DateTime, TimeSpan>.Min(endOfYear, end.AddDays(-1));

            var infos = _infos.Where(x => x.Between(left, end)).ToArray();


            container.Include(left, end.AddDays(1), data, rightIncluded: false);


            foreach (var info in infos)
            {
                var date = (info as YearlyHoliday)?.ToDate(year) ?? ((DayInfo) info).Date;

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
        protected override DateTime ProcessInterval(PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.CalendarItems container, DateTime left, DateTime end, int? data)
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
                    right = left.AddDays(5 - (int) dayOfWeek);
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
        /// ƒл€ ежегодных выходных.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        public void Add(int day, int month)
        {
            _holidays.Add(new YearlyHoliday(day, month));
        }

        /// <summary>
        /// ƒл€ диапазонов ежегодных выходных. Ќапример, 1-8 €нвар€
        /// </summary>
        /// <param name="dayfrom"></param>
        /// <param name="dayTo"></param>
        /// <param name="month"></param>
        public void Add(int dayfrom, int dayTo, int month)
        {
            _holidays.AddRange(YearlyHoliday.Range(dayfrom, dayTo, month));
        }

        /// <summary>
        /// ƒл€ перенесенных выходных.
        /// </summary>
        /// <param name="date"></param>
        public void Add(DateTime date)
        {
            _holidays.Add(new DayInfo(date, false));
        }

        /// <summary>
        /// ƒл€ рабочих дней с отличной от базовой длительностью. Ќапример дл€ предпраздничных дней
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