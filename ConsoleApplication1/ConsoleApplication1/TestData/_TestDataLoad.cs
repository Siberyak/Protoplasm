using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ConsoleApplication1.Intervals;

namespace ConsoleApplication1.TestData
{
    //public partial class PlanningEnvironment<TTime, TDuration>
    //{

    //}

    public class _TestDataLoad
    {
        public static void Do()
        {
            PlanningEnvironment<DateTime, TimeSpan>.GetOffset = (from, to) => from.HasValue && to.HasValue ? to.Value - from.Value : default(TimeSpan?);

            TestCalendars();

            TestCalendarItems();

            TestCompetencesMatching();

            TestManagersAdd();
        }

        private static void TestCalendars()
        {
            var calendar1 = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>(ByDayOfWeek, (a, b) => b, (a, b) => CalendarItemType.Unknown);
            var tmp1 = calendar1.Get(DateTime.Today, DateTime.Today.AddDays(10));
            var tmp2 = calendar1.Get(DateTime.Today.AddDays(20), DateTime.Today.AddDays(30));
            var tmp3 = calendar1.Get(DateTime.Today.AddDays(15), DateTime.Today.AddDays(35));

            var calendar2 = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>(calendar1, ByWorkingTime, (a, b) => b, (a, b) => CalendarItemType.Unavalable);
            var tmp4 = calendar2.Get(DateTime.Today.AddDays(5), DateTime.Today.AddDays(15));

            var calendar3 = new PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.PrevBasedCalendar<CalendarItemType>
                (
                calendar2,
                DoDefineData,
                (a, b) => b, (a, b) => null
                );
            var tmp5 = calendar3.Get(DateTime.Today.AddYears(1), DateTime.Today.AddYears(1).AddDays(15));
        }

        private static void DoDefineData(PlanningEnvironment<DateTime, TimeSpan>.Calendars<int?>.CalendarItems container, PlanningEnvironment<DateTime, TimeSpan>.Calendars<CalendarItemType>.CalendarItem todefine)
        {
            if (todefine.Data == CalendarItemType.Unknown)
                container.Exclude(todefine.Left, todefine.Right, null);
            else
                container.Include(todefine.Left, todefine.Right, todefine.Data == CalendarItemType.Available? 1 : 0);
        }

        #region TestCalendarItems

        private static void TestCalendarItems()
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


        private static void TestCompetencesMatching()
        {
            var d = new Department("D");
            var d1 = new Department("d1", d);
            var d11 = new Department("d11", d1);
            var d12 = new Department("d12", d1);


            var r2 = new Role("r2");
            var d2 = new Department("d2", d);

            var c3 = new Department("c3", d);
            var po = new Role("po");

            IEnumerable<Competence> competences;

            var byDepartment = Competences.New().AddKey("что-то просто в наличии").AddKeyValue("2", 1).MemberOf(out competences, d11);
            Competence byDepartmentRequiredResult = competences.ToArray()[0];

            Competence byKeyRequiredResult;
            var byKey = Competences.New().AddKeyValue("2", 1).MemberOf(d2).AddKey("фигня какая-то", out byKeyRequiredResult);

            Competence byKeyValueRequiredResult;
            var byKeyValue = Competences.New().AddKey("что-то просто в наличии").AddKeyValue("2", 3, out byKeyValueRequiredResult).MemberOf(c3);


            var byRole = Competences.New().AddKey("что-то просто в наличии").AddKeyValue("1", 3).MemberOf(out competences, c3, r2);
            var byRoleRequiredResult = competences.ToArray()[1];


            var notForAll = Competences.New().AddKey("что-то просто в наличии").AddKeyValue("1", 3).MemberOf(c3, po);

            Competence by_11_result;
            Competence by_12_result;
            var by_11_12 = Competences.New().AddKey("что-то просто в наличии").AddKeyValue("1", 3).MemberOf(c3, po).AddKeyValue(11, "11", out by_11_result).AddKeyValue(12, 12, out by_12_result);

            var required = Competences.New().AnyOf(Competences.New().AddKey("фигня какая-то").AddKeyValue("2", 2).MemberOf(d1).MemberOf(r2).AddKey(11));


            Check(required, byDepartment, true, byDepartmentRequiredResult);
            Check(required, byKey, true, byKeyRequiredResult);
            Check(required, byKeyValue, true, byKeyValueRequiredResult);
            Check(required, byRole, true, byRoleRequiredResult);
            Check(required, notForAll, false);

            required = required.AddKeyValue(12, 10);
            Check(required, by_11_12, true, by_11_result, by_12_result);
        }

        private static void Check(Competences required, Competences current, bool requiredAcceptable, params Competence[] requiredResults)
        {
            IEnumerable<CompetenceMatchingResult> result;
            var acceptable = required.Acceptable(current, out result);

            Debug.Assert(acceptable == requiredAcceptable);

            if (!requiredAcceptable)
                return;


            var competences = result.Select(x => x.Result).ToArray();
            var except = requiredResults.Except(competences).ToArray();
            Debug.Assert(except.Length == 0);
        }

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