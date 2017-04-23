using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.TestData
{
    public class CalendarTests
    {
        public static void Calendars()
        {
            // календарь по дням недели
            var calendar1 = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<TestCalendarItemType>(ByDayOfWeek, (a, b) => b, (a, b) => TestCalendarItemType.Unknown);
            var tmp1 = calendar1.Get(DateTime.Today, DateTime.Today.AddDays(10));
            var tmp2 = calendar1.Get(DateTime.Today.AddDays(20), DateTime.Today.AddDays(30));
            var tmp3 = calendar1.Get(DateTime.Today.AddDays(15), DateTime.Today.AddDays(35));

            // календарь доопределяет рабочее время
            // опирается на календарь по дням недели
            var calendar2 = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<TestCalendarItemType>(calendar1, ByWorkingTime, (a, b) => b, (a, b) => TestCalendarItemType.Unavalable);
            var tmp4 = calendar2.Get(DateTime.Today.AddDays(5), DateTime.Today.AddDays(15));

            // опирается на календарь рабочего времени 
            // конвертирует данные в другой тип
            var calendar3 = new Calendars<DateTime, TimeSpan, int?>.Calendar<TestCalendarItemType>
                (
                calendar2, DoDefineData,
                (a, b) => b, (a, b) => null
                );

            var tmp5 = calendar3.Get(DateTime.Today.AddYears(1), DateTime.Today.AddYears(1).AddDays(15));
        }

        private static void ByDayOfWeek(Calendars<DateTime, TimeSpan, TestCalendarItemType>.ICalendarItems container, Calendars<DateTime, TimeSpan, TestCalendarItemType>.CalendarItem toDefine)
        {
            var begin = toDefine.Left.PointValue.Value;
            var end = toDefine.Right.PointValue.Value;

            while (begin < end)
            {
                var available = TestCalendarItemType.Available;
                var date = begin.Date.AddDays(1);
                var dayOfWeek = begin.DayOfWeek;

                switch (dayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        date = date.AddDays(1);
                        available = TestCalendarItemType.Unavalable;
                        break;
                    case DayOfWeek.Sunday:
                        available = TestCalendarItemType.Unavalable;
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

        private static void ByWorkingTime(Calendars<DateTime, TimeSpan, TestCalendarItemType>.ICalendarItems container, Calendars<DateTime, TimeSpan, TestCalendarItemType>.CalendarItem toDefine)
        {
            var begin = toDefine.Left.PointValue.Value;
            var end = toDefine.Right.PointValue.Value;

            container.Include(begin, end, toDefine.Data, rightIncluded: false);
            if (toDefine.Data == TestCalendarItemType.Unavalable)
                return;

            while (begin < end)
            {
                var wantedEnd = ExcludeIfNeed(begin, end, 0, 9, container);
                wantedEnd = ExcludeIfNeed(begin, end, 13, 14, container) ?? wantedEnd;
                wantedEnd = ExcludeIfNeed(begin, end, 18, 24, container) ?? wantedEnd;


                begin = wantedEnd ?? begin.Date.AddDays(1);
            }
        }

        private static DateTime? ExcludeIfNeed(DateTime begin, DateTime end, int beginHour, int endHour,
            Calendars<DateTime, TimeSpan, TestCalendarItemType>.ICalendarItems container)
        {

            var wantedBegin = begin.Date.AddHours(beginHour);
            var wantedEnd = begin.Date.AddHours(endHour);


            if (wantedBegin > end)
                return null;
            if (wantedEnd < begin)
                return null;

            wantedBegin = PlanningEnvironment<DateTime, TimeSpan>.Max(begin, wantedBegin);
            wantedEnd = PlanningEnvironment<DateTime, TimeSpan>.Min(end, wantedEnd);

            container.Exclude(wantedBegin, wantedEnd, TestCalendarItemType.Unavalable, true, false);
            return wantedEnd;
        }

        private static void DoDefineData(Calendars<DateTime, TimeSpan, int?>.ICalendarItems container, Calendars<DateTime, TimeSpan, TestCalendarItemType>.CalendarItem todefine)
        {
            if (todefine.Data == TestCalendarItemType.Unknown)
                container.Exclude(todefine.Left, todefine.Right, null);
            else
                container.Include(todefine.Left, todefine.Right, todefine.Data == TestCalendarItemType.Available ? 1 : 0);
        }
    }

    public enum TestCalendarItemType
    {
        Unknown = 0,
        Available,
        Unavalable
    }



}