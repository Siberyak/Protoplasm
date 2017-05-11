using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleApplication1.TestData
{
    public static class CompetencesTests
    {
        public static void TestCompetencesMatching()
        {
            var d = new PlanningEnvironment<DateTime, TimeSpan>.Department("D");
            var d1 = new PlanningEnvironment<DateTime, TimeSpan>.Department("d1", d);
            var d11 = new PlanningEnvironment<DateTime, TimeSpan>.Department("d11", d1);
            var d12 = new PlanningEnvironment<DateTime, TimeSpan>.Department("d12", d1);


            var r2 = new Role("r2");
            var d2 = new PlanningEnvironment<DateTime, TimeSpan>.Department("d2", d);

            var c3 = new PlanningEnvironment<DateTime, TimeSpan>.Department("c3", d);
            var po = new Role("po");

            IEnumerable<Competence> competences;

            var byDepartment = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("2", 1)
                .MemberOf(out competences, d11);

            Competence byDepartmentRequiredResult = competences.ToArray()[0];

            Competence byKeyRequiredResult;
            var byKey = Competences.New()
                .AddKeyValue("2", 1)
                .MemberOf(d2)
                .AddKey("фигня какая-то", out byKeyRequiredResult);

            Competence byKeyValueRequiredResult;
            var byKeyValue = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("2", 3, out byKeyValueRequiredResult)
                .MemberOf(c3);


            var byRole = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("1", 3)
                .MemberOf(out competences, c3, r2);
            var byRoleRequiredResult = competences.ToArray()[1];


            var notForAll = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("1", 3)
                .MemberOf(c3, po);

            Competence by_11_result;
            Competence by_12_result;
            var by_11_12 = Competences.New()
                .AddKey("что-то просто в наличии")
                .AddKeyValue("1", 3)
                .MemberOf(c3, po)
                .AddKeyValue(11, "11", out by_11_result)
                .AddKeyValue(12, 12, out by_12_result);

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
    }
}