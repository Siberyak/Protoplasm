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

        private static PlanningEnvironment<DateTime, TimeSpan>.CalendarItem GetCalendarItemDelegated(PlanningEnvironment<DateTime, TimeSpan>.CalendarItem baseItem, DateTime date,
            params PlanningEnvironment<DateTime, TimeSpan>.Calendar.GenerateCalendarItem[] funcs)
        {
            foreach (var func in funcs)
            {
                baseItem = func(baseItem, date);
            }

            return baseItem;
        }

        //private static PlanningEnvironment<DateTime, TimeSpan>.CalendarItem GetTimedCalendarItem(PlanningEnvironment<DateTime, TimeSpan>.CalendarItem baseItem, DateTime date)
        //{
        //    if (baseItem == null || Equals(baseItem.Data, false))
        //    {
        //        return baseItem;
        //    }

        //    DateTime begin = date.Date.AddHours(9);
        //    DateTime end = date.Date.AddHours(18);

        //    var isWorkTime = date >= begin && date <= end;


        //    if (isWorkTime)
        //    {
        //        begin = PlanningEnvironment<DateTime, TimeSpan>.Max(begin, baseItem.Begin);
        //        end = PlanningEnvironment<DateTime, TimeSpan>.Min(end, baseItem.End);
        //    }
        //    else
        //    {
        //        if (date < begin)
        //        {
        //            end = begin;
        //            begin = baseItem.Begin;
        //        }
        //        else
        //        {
        //            begin = end;
        //            end = baseItem.End;
        //        }
        //    }

        //    return new PlanningEnvironment<DateTime, TimeSpan>.CalendarItem {Begin = begin, End = end, Data = isWorkTime};
        //}

        private static PlanningEnvironment<DateTime, TimeSpan>.CalendarItem GetCalendarItem(DateTime date)
        {
            DayOfWeek dayOfWeek = date.DayOfWeek;
            var begin = date.Date;
            var end = begin;
            var available = CalendarItemType.Available;

            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    end = date.AddDays(1);
                    available = CalendarItemType.Unavalable;
                    break;
                case DayOfWeek.Sunday:
                    available = CalendarItemType.Unavalable;
                    break;
                default:
                    end = date.AddDays(5 - (int)dayOfWeek);
                    break;
                    
            }

            return new PlanningEnvironment<DateTime, TimeSpan>.CalendarItem() {Begin = begin, End = end, Data = available};
        }


        public static void Do()
        {
            TestIntervals();

            TestCalendarItems();


            TestCompetencesMatching();


            TestManagersAdd();
        }

        private static void TestIntervals()
        {
            var container = new PointedIntervalsContainer<int, int>((a, b) => a + b, (a, b) => a - b);

            try
            {
                container.Include(1, 1, 8, true, false);
            }
            catch (Exception e)
            {

            }
            try
            {
                container.Include(1, 1, 8, false, true);
            }
            catch (Exception e)
            {

            }

            container.Include(0, 100, 8);


            container.Include(200, 300, 8);
            container.Include(190, 200, 8);
            container.Include(300, 310, 8);


            container.Include(20, 60, 2);
            container.Include(40, 80, 2);

            container.Exclude(50, 60, 2);
            container.Exclude(70, 80, 1);
            container.Exclude(80, 90, 1);
            try
            {
                container.Exclude(45, 45, 12, false, false);
            }
            catch (Exception e)
            {
                
            }
            container.Exclude(47, 47, 12);

            var tmp = container.ToArray();
        }

        private static void TestCalendarItems()
        {
            var container = new PointedIntervalsContainer<DateTime, CalendarItemType>((a, b) => b, (a, b) => CalendarItemType.Unknown);
            container.Include(DateTime.Today, DateTime.Today.AddDays(1), CalendarItemType.Available);
            container.Include(DateTime.Now, DateTime.Now.AddHours(1), CalendarItemType.Available);

            var environment = new PlanningEnvironment<DateTime, TimeSpan>();


            var request = new PlanningEnvironment<DateTime, TimeSpan>.CalendarItem() {Begin = DateTime.Now.AddDays(-10), End = DateTime.Now.AddDays(12)};

            var items0 = new[] {request};

            var items1 = ByDayOfWeek(items0);
            var items2 = ByWorkingTime(items1);
        }


        private static IEnumerable<PlanningEnvironment<DateTime, TimeSpan>.CalendarItem> ByDayOfWeek(IEnumerable<PlanningEnvironment<DateTime, TimeSpan>.CalendarItem> baseitems)
        {
            var items = baseitems.ToArray();

            var container = new PointedIntervalsContainer<DateTime, CalendarItemType>((a, b) => b, (a, b) => CalendarItemType.Unknown);

            // ====================
            // test points includings
            // ====================
            // normal
            //var days = 5;
            //container.Include(DateTime.Today.AddDays(days), DateTime.Today.AddDays(days+1), CalendarItemType.Available, false, false);
            //container.Include(DateTime.Today.AddDays(days + 1), DateTime.Today.AddDays(days + 2), CalendarItemType.Available, false, false);

            // exception
            //days = 10;
            //container.Include(DateTime.Today.AddDays(days), DateTime.Today.AddDays(days + 1), CalendarItemType.Available);
            //container.Include(DateTime.Today.AddDays(days + 1), DateTime.Today.AddDays(days + 2), CalendarItemType.Available, false);
            
            // error in splitting: don't need split
            //container.Include(DateTime.Today, DateTime.Today.AddDays(1), CalendarItemType.Available);
            //container.Include(DateTime.Now, DateTime.Now, CalendarItemType.Available);
            // ====================

            foreach (var item in items)
            {
                var begin = item.Begin;
                var end = item.End;

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
                            date = date.AddDays(5 - (int)dayOfWeek);
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


            var tmp = container.ToArray();

            items = tmp.Select
                (
                    x =>
                        new PlanningEnvironment<DateTime, TimeSpan>.CalendarItem
                        {
                            Begin = x.Left.PointValue.Value,
                            End = x.Right.PointValue.Value,
                            Data = x.Data
                        })
                .ToArray();

            return items;
        }


        private static IEnumerable<PlanningEnvironment<DateTime, TimeSpan>.CalendarItem> ByWorkingTime(IEnumerable<PlanningEnvironment<DateTime, TimeSpan>.CalendarItem> baseitems)
        {
            var items = baseitems.ToArray();

            var container = new PointedIntervalsContainer<DateTime, CalendarItemType>((a, b) => b, (a, b) => CalendarItemType.Unavalable);

            foreach (var item in items)
            {
                var begin = item.Begin;
                var end = item.End;

                container.Include(begin, end, item.Data, rightIncluded: false);
                if(item.Data == CalendarItemType.Unavalable)
                    continue;

                while (begin < end)
                {
                    PointedInterval<DateTime, CalendarItemType> interval;
                    if (NeedExclude(begin, end, 0, 9, out interval))
                        container.Exclude(interval);

                    if (NeedExclude(begin, end, 13, 14, out interval))
                        container.Exclude(interval);

                    if (NeedExclude(begin, end, 18, 24, out interval))
                        container.Exclude(interval);

                    begin = interval?.Right.PointValue.Value ?? begin.Date.AddDays(1);
                }
            }

            var tmp = container.ToArray();
            items = tmp.Select
                (
                    x =>
                        new PlanningEnvironment<DateTime, TimeSpan>.CalendarItem
                        {
                            Begin = x.Left.PointValue.Value,
                            End = x.Right.PointValue.Value,
                            Data = x.Data
                        })
                .ToArray();

            return items;
        }

        static bool NeedExclude(DateTime begin, DateTime end, int beginHour, int endHour, out PointedInterval<DateTime, CalendarItemType> interval)
        {

            var wantedBegin = begin.Date.AddHours(beginHour);
            var wantedEnd = begin.Date.AddHours(endHour);

            interval = null;

            if (wantedBegin > end)
                return false;
            if (wantedEnd < begin)
                return false;

            wantedBegin = PlanningEnvironment<DateTime, TimeSpan>.Max(begin, wantedBegin);
            wantedEnd = PlanningEnvironment<DateTime, TimeSpan>.Min(end, wantedEnd);

            interval = new PointedInterval<DateTime, CalendarItemType>(wantedBegin, true, wantedEnd, false, CalendarItemType.Unavalable);
            return true;
        }

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