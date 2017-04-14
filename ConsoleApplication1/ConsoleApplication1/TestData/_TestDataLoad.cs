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

        }

        #region TestCalendarItems

        private static void TestCalendarItems()
        {
            var container = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItems((a, b) => b, (a, b) => CalendarItemType.Unknown);
            container.Include(DateTime.Today, DateTime.Today.AddDays(1), CalendarItemType.Available);
            container.Include(DateTime.Now, DateTime.Now.AddHours(1), CalendarItemType.Available);

            var environment = new PlanningEnvironment<DateTime, TimeSpan>();


            //var request = new PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>(TODO, TODO, TODO).CalendarItem() {Begin = DateTime.Now.AddDays(-10), End = DateTime.Now.AddDays(12)};

            //var items0 = new[] {request};

            //var items1 = ByDayOfWeek(items0);
            //var items2 = ByWorkingTime(items1);
        }


        private static void ByDayOfWeek(PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItems container, PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItem toDefine)
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

        private static void ByWorkingTime(PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItems container, PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItem toDefine)
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

        static PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItem ExcludeIfNeed(DateTime begin, DateTime end, int beginHour, int endHour, PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItems container)
        {

            var wantedBegin = begin.Date.AddHours(beginHour);
            var wantedEnd = begin.Date.AddHours(endHour);


            if (wantedBegin > end)
                return null;
            if (wantedEnd < begin)
                return null;

            wantedBegin = PlanningEnvironment<DateTime, TimeSpan>.Max(begin, wantedBegin);
            wantedEnd = PlanningEnvironment<DateTime, TimeSpan>.Min(end, wantedEnd);

            PlanningEnvironment<DateTime, TimeSpan>.Calendar<CalendarItemType>.CalendarItem interval;
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

        private static void TestManagersAdd()
        {
            //environment.Resources.Add
            //    (
            //        "res 1",
            //        Competence.Set("", 1), 
            //        new PlanningEnvironment<DateTime, TimeSpan>.Calendar(), 
            //        c3, po
            //    );

            //environment.Resources.Add
            //    (
            //        "res 2",
            //        Competence.Set("", 1),
            //        new PlanningEnvironment<DateTime, TimeSpan>.Calendar(),
            //        r2, d2
            //    );

            ////===================================================

            //environment.WorkItems.Add
            //    (
            //        Interval<DateTime?>.Empty,
            //        Interval<DateTime?>.Empty,
            //        Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
            //        Competences.New().AnyOf(Competence.Set("", 2).MemberOf(d2, c3))
            //    );
            return;
        }
    }
}