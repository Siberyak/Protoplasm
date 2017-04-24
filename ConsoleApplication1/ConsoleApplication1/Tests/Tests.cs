using System;
using System.IO;
using Protoplasm.Calendars;
using Protoplasm.Calendars.Tests;


namespace ConsoleApplication1.TestData
{
    //public partial class PlanningEnvironment<TTime, TDuration>
    //{

    //}

    public class Tests
    {
        public static void Do()
        {
            var original = Calendars<DateTime, TimeSpan>.GetOffset;
            try
            {
                Calendars<DateTime, TimeSpan>.GetOffset = (from, to) => from.HasValue && to.HasValue ? to.Value - from.Value : default(TimeSpan?);

                CalendarTests.Calendars();

                WorkCalendarTests.WorkCalendars();

                CompetencesTests.TestCompetencesMatching();

                TestManagersAdd();
            }
            finally
            {
                Calendars<DateTime, TimeSpan>.GetOffset = original;
            }
        }

       #region TestCalendarItems

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

        static DateTime? ExcludeIfNeed(DateTime begin, DateTime end, int beginHour, int endHour, Calendars<DateTime, TimeSpan, TestCalendarItemType>.ICalendarItems container)
        {

            var wantedBegin = begin.Date.AddHours(beginHour);
            var wantedEnd = begin.Date.AddHours(endHour);


            if (wantedBegin > end)
                return null;
            if (wantedEnd < begin)
                return null;

            wantedBegin = PlanningEnvironment<DateTime, TimeSpan>.Max(begin, wantedBegin);
            wantedEnd = PlanningEnvironment<DateTime, TimeSpan>.Min(end, wantedEnd);

            Calendars<DateTime, TimeSpan, TestCalendarItemType>.CalendarItem interval;
            //= container.NewInterval(Point<DateTime>.Right(wantedBegin), Point<DateTime>.Right(wantedEnd, false), CalendarItemType.Unavalable);
            container.Exclude(wantedBegin, wantedEnd, TestCalendarItemType.Unavalable, true, false);
            return wantedEnd;
        }

        #endregion

        private static PlanningEnvironment<DateTime, TimeSpan> TestManagersAdd()
        {
            var environment = new PlanningEnvironment<DateTime, TimeSpan>();

            var baseCalendar = environment.CreateCalendar<TestCalendarItemType>(ByDayOfWeek, (a, b) => b, (a, b) => TestCalendarItemType.Unknown);

            var r1 = environment.Resources.CreateEmployeeAgent
                (
                    "R1",
                    Competences.New().AddKeyValue("курящий", true).AddKeyValue("C1", 10).AddKeyValue("C2", 5),
                    baseCalendar
                );

            var r2 = environment.Resources.CreateEmployeeAgent
                (
                    "R2",
                    Competences.New().AddKeyValue("курящий", false).AddKeyValue("C1", 5).AddKeyValue("C2", 10),
                    baseCalendar
                );

            ////===================================================

            var wi1 = environment.WorkItems.CreateWorkItemAgent
                (
                    "wi1",
                    Interval<DateTime?>.Empty,
                    Interval<DateTime?>.Empty,
                    Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
                    Competences.New().AnyOf(Competences.New().AddKeyValue("C1",  7).AddKeyValue("C2", 7))
                );

            var wi2 = environment.WorkItems.CreateWorkItemAgent
                (
                    "wi2",
                    Interval<DateTime?>.Empty,
                    Interval<DateTime?>.Empty,
                    Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
                    Competences.New().AddKeyValue("C1", 7)
                );

            var wi3 = environment.WorkItems.CreateWorkItemAgent
                (
                    "wi3",
                    Interval<DateTime?>.Empty,
                    Interval<DateTime?>.Empty,
                    Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
                    Competences.New().AddKeyValue("C2", 7).MemberOf()
                );


            var c1 = r1.Compatible(wi1);
            var c2 = r2.Compatible(wi1);

            var c3 = r1.Compatible(wi2);
            var c4 = r2.Compatible(wi2);

            var c5 = r1.Compatible(wi3);
            var c6 = r2.Compatible(wi3);


            return environment;

        }
    }
}