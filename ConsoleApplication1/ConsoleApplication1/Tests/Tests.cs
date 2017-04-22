using System;
using System.IO;
using ConsoleApplication1.Intervals;

namespace ConsoleApplication1.TestData
{
    //public partial class PlanningEnvironment<TTime, TDuration>
    //{

    //}

    public class Tests
    {
        public static void Do()
        {
            PlanningEnvironment<DateTime, TimeSpan>.GetOffset = (from, to) => from.HasValue && to.HasValue ? to.Value - from.Value : default(TimeSpan?);

            CalendarTests.TestCalendarItems();

            CalendarTests.Calendars();

            CalendarTests.WorkCalendars();


            CompetencesTests.TestCompetencesMatching();

            TestManagersAdd();
        }

       #region TestCalendarItems

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

        static PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItem ExcludeIfNeed(DateTime begin, DateTime end, int beginHour, int endHour, PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItems container)
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

        #endregion

        private static PlanningEnvironment<DateTime, TimeSpan> TestManagersAdd()
        {
            var environment = new PlanningEnvironment<DateTime, TimeSpan>();

            var baseCalendar = environment.CreateCalendar<CalendarItemType>(ByDayOfWeek, (a, b) => b, (a, b) => CalendarItemType.Unknown);

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
                    Interval<DateTime?>.Empty,
                    Interval<DateTime?>.Empty,
                    Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
                    Competences.New().AnyOf(Competences.New().AddKeyValue("C1",  7).AddKeyValue("C2", 7))
                );

            var wi2 = environment.WorkItems.CreateWorkItemAgent
                (
                    Interval<DateTime?>.Empty,
                    Interval<DateTime?>.Empty,
                    Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
                    Competences.New().AddKeyValue("C1", 7)
                );

            var wi3 = environment.WorkItems.CreateWorkItemAgent
                (
                    Interval<DateTime?>.Empty,
                    Interval<DateTime?>.Empty,
                    Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
                    Competences.New().AddKeyValue("C2", 7)
                );


            var c1 = $"r1 -> wi1 : {r1.ConformableFor(wi1).ToConformType()}";
            var c2 = $"r2 -> wi1 : {r2.ConformableFor(wi1).ToConformType()}";

            var c3 = $"r1 -> wi2 : {r1.ConformableFor(wi2).ToConformType()}";
            var c4 = $"r2 -> wi2 : {r2.ConformableFor(wi2).ToConformType()}";

            var c5 = $"r1 -> wi3 : {r1.ConformableFor(wi3).ToConformType()}";
            var c6 = $"r2 -> wi3 : {r2.ConformableFor(wi3).ToConformType()}";


            return environment;

        }
    }
}