using System;
using System.Diagnostics;
using System.IO;
using MAS.Core.Compatibility;
using MAS.Core.Contracts;
using Protoplasm.Calendars;
using Protoplasm.Calendars.Tests;
using Protoplasm.PointedIntervals;


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

                WorkCalendarTests.WorkCalendars();

                CompetencesTests.TestCompetencesMatching();

                TestManagersAdd();
            }
            finally
            {
                Calendars<DateTime, TimeSpan>.GetOffset = original;
            }
        }

        private static PlanningEnvironment<DateTime, TimeSpan> TestManagersAdd()
        {
            var environment = new PlanningEnvironment<DateTime, TimeSpan>();

            //var baseCalendar = environment.CreateCalendar<TestCalendarItemType>(ByDayOfWeek, (a, b) => b, (a, b) => TestCalendarItemType.Unknown);

            var adapter = new ByDayOfWeekAvailabilityCalendarAdapter(8, 0, null);
            var baseCalendar = new AvailabilityCalendar1(adapter);

            var res1 = environment.Resources.CreateEmployeeAgent
                (
                    "E1",
                    Competences.New().AddKeyValue("курящий", true).AddKeyValue("C1", 10).AddKeyValue("C2", 5),
                    baseCalendar
                );

            var res2 = environment.Resources.CreateEmployeeAgent
                (
                    "E2",
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
                    Competences.New().AddKeyValue("C2", 7)
                );


            IScene scene = new Scene();

            var c1r = res1.Abilities().Compatible(wi1);
            var c1a = wi1.Requirements().Compatible(res1);

            Debug.Assert(c1r.Compatibility == CompatibilityType.DependsOnScene);
            Debug.Assert(ReferenceEquals(c1r, c1a));
            Debug.Assert(ReferenceEquals(c1r, res1.Abilities().Compatible(wi1)));
            Debug.Assert(ReferenceEquals(wi1.Requirements().Compatible(res1), c1a));

            var ok1 = c1a.Compatible(scene);

            var c2 = res2.Abilities().Compatible(wi1);
            Debug.Assert(c2.Compatibility == CompatibilityType.DependsOnScene);

            var c3 = res1.Abilities().Compatible(wi2);
            Debug.Assert(c3.Compatibility == CompatibilityType.DependsOnScene);
            var c4 = res2.Abilities().Compatible(wi2);
            Debug.Assert(c4.Compatibility == CompatibilityType.Never);

            var c5 = res1.Abilities().Compatible(wi3);
            Debug.Assert(c5.Compatibility == CompatibilityType.Never);
            var c6 = res2.Abilities().Compatible(wi3);
            Debug.Assert(c6.Compatibility == CompatibilityType.DependsOnScene);


            //res1.Entity.Calendar.CreateSchedule(new PlanningEnvironment<DateTime, TimeSpan>.AllocationsHelper());

            var tmp = res1.Entity.Calendar.Get(new DateTime(2017, 1, 1), DateTime.Today);

            return environment;

        }
    }


    public struct AvailabilityData : PlanningEnvironment<DateTime,TimeSpan>.IAvailabilityData
    {
        public override string ToString()
        {
            return $"{Value}";
        }

        public int Value;

        public AvailabilityData(int value)
        {
            Value = value;
        }

        public static implicit operator int(AvailabilityData data)
        {
            return data.Value;
        }
        public static implicit operator AvailabilityData(int value)
        {
            return new AvailabilityData(value);
        }

        public PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Include(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData availabilityData)
        {
            var value = this + (AvailabilityData)availabilityData;
            return new AvailabilityData(value);
        }

        public PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Exclude(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData availabilityData)
        {
            var value = this - (AvailabilityData)availabilityData;
            return new AvailabilityData(value);
        }
    }

 

    public class AvailabilityCalendar1 : PlanningEnvironment<DateTime, TimeSpan>.AvailabilityCalendar
    {
        public AvailabilityCalendar1(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.ICalendarAdapter adapter) : base(adapter)
        {
        }
    }

    class ByDayOfWeekAvailabilityCalendarAdapter : ByDayOfWeekAdapter<PlanningEnvironment<DateTime,TimeSpan>.IAvailabilityData>
    {
        public ByDayOfWeekAvailabilityCalendarAdapter(AvailabilityData daylyWorkData, AvailabilityData daylyNotWorkData, Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.ICalendar baseCalendar) : base(daylyWorkData, daylyNotWorkData, baseCalendar)
        {
        }

        public override PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Include(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData a, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData b)
        {
            return a == null ? b : a.Include(b);
        }

        public override PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Exclude(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData a, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData b)
        {
            return a == null ? b : a.Exclude(b);
        }
    }

    class Scene : IScene
    {
        private Satisfaction _satisfaction;

        public Scene()
        { }

        private Scene(IScene original)
        {
            Original = original;
        }

        public IScene Original { get; }

        public ISatisfaction Satisfaction => _satisfaction ?? (_satisfaction = new Satisfaction(((Scene)Original)._satisfaction.Value));

        public IScene Branch()
        {
            return new Scene(this);
        }

        public void MergeToOriginal()
        {
        }

    }

    public interface ISatisfaction<T> : ISatisfaction, IComparable<ISatisfaction<T>>
        where T : IComparable<T>
    {
        T Delta { get; }
        T Value { get; }
    }


    public abstract class Satisfaction<T> : ISatisfaction<T>
        where T : IComparable<T>
    {
        protected T _original;

        protected Satisfaction(T original)
        {
            _original = original;
        }

        public T Delta { get; set; }
        public abstract T Value { get; }

        public int CompareTo(ISatisfaction<T> other)
        {
            return other == null ? -1 : Value.CompareTo(other.Value);
        }

        public int CompareTo(ISatisfaction other)
        {
            return CompareTo(other as ISatisfaction<T>);
        }
    }



    public class Satisfaction : Satisfaction<double>
    {
        public Satisfaction(double original) : base(original)
        {
        }

        public override double Value => _original + Delta;
    }
}