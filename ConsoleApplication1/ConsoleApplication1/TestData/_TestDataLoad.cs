using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.TestData
{
    //public partial class PlanningEnvironment<TTime, TDuration>
    //{

    //}

    public class _TestDataLoad
    {
        public static void Do()
        {
            var environment = new PlanningEnvironment<DateTime, TimeSpan>();

            var d = new Department("D");
            var d1 = new Department("d1", d);
            var d11 = new Department("d11", d1);
            var d12 = new Department("d12", d1);


            var r2 = new Role("r2");
            var d2 = new Department("d2", d);

            var c3 = new Department("c3", d);
            var po = new Role("po");

            var byDepartment = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("2", 1)
                .MemberOf(d11);

            var byKey = Competences.New()
                .AddKeyValue("2", 1)
                .MemberOf(d2)
                .AddKey("фигня какая-то")
                ;

            var byKeyValue = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("2", 3)
                .MemberOf(c3);

            var byRole = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("1", 3)
                .MemberOf(c3, r2);

            var notForAll = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("1", 3)
                .MemberOf(c3, po);

            var by_11_12 = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("1", 3)
                .MemberOf(c3, po)
                .AddKeyValue(11, "11")
                .AddKeyValue(12, 12);

            var required = Competences.New()
                .AnyOf
                (
                    Competences.New()
                        .AddKey("фигня какая-то")
                        .AddKeyValue("2", 2)
                        .MemberOf(d1)
                        .MemberOf(r2)
                        .AddKey(11)
                );


            IEnumerable<CompetenceMatchingResult> result;
            var acceptable = required.Acceptable(byDepartment, out result);
            acceptable = required.Acceptable(byKey, out result);
            acceptable = required.Acceptable(byKeyValue, out result);
            acceptable = required.Acceptable(byRole, out result);
            acceptable = required.Acceptable(notForAll, out result);

            required = required.AddKeyValue(12, 10);
            acceptable = required.Acceptable(by_11_12, out result);


            environment.Resources.Add
                (
                    "res 1",
                    Competence.Set("", 1), 
                    new PlanningEnvironment<DateTime, TimeSpan>.Calendar(), 
                    c3, po
                );

            environment.Resources.Add
                (
                    "res 2",
                    Competence.Set("", 1),
                    new PlanningEnvironment<DateTime, TimeSpan>.Calendar(),
                    r2, d2
                );

            //===================================================

            environment.WorkItems.Add
                (
                    Interval<DateTime?>.Empty,
                    Interval<DateTime?>.Empty,
                    Interval<TimeSpan?>.New(TimeSpan.FromDays(1)),
                    Competences.New().AnyOf(Competence.Set("", 2).MemberOf(d2, c3))
                );

        }
    }

}