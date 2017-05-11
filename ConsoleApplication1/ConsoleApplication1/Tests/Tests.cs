using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Akka.Actor;
using MAS.Core;
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
            var original = Calendars<DateTime, TimeSpan>.ToDuration;
            try
            {
                Calendars<DateTime, TimeSpan>.ToDuration = (from, to) => from.HasValue && to.HasValue ? to.Value - from.Value : default(TimeSpan?);

                WorkCalendarTests.WorkCalendars();

                CompetencesTests.TestCompetencesMatching();

                TestManagersAdd();
            }
            finally
            {
                Calendars<DateTime, TimeSpan>.ToDuration = original;
            }
        }

        private static PlanningEnvironment<DateTime, TimeSpan> TestManagersAdd()
        {
            var environment = new PlanningEnvironment<DateTime, TimeSpan>();

            //var baseCalendar = environment.CreateCalendar<TestCalendarItemType>(ByDayOfWeek, (a, b) => b, (a, b) => TestCalendarItemType.Unknown);

            var adapter = new ByDayOfWeekAvailabilityCalendarAdapter(8, 0, null);
            var baseCalendar = new AvailabilityCalendar(adapter);

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
                    Interval<DateTime>.Undefined,
                    Interval<DateTime>.Undefined,
                    Interval<TimeSpan>.New(TimeSpan.FromDays(1)),
                    Interval<TimeSpan>.New(TimeSpan.FromDays(1)),
                    Competences.New().AnyOf(Competences.New().AddKeyValue("C1",  7).AddKeyValue("C2", 7))
                );

            var wi2 = environment.WorkItems.CreateWorkItemAgent
                (
                    "wi2",
                    Interval<DateTime>.Undefined,
                    Interval<DateTime>.Undefined,
                    Interval<TimeSpan>.New(TimeSpan.FromDays(1)),
                    Interval<TimeSpan>.New(TimeSpan.FromDays(1)),
                    Competences.New().AddKeyValue("C1", 7)
                );

            var wi3 = environment.WorkItems.CreateWorkItemAgent
                (
                    "wi3",
                    Interval<DateTime>.Undefined,
                    Interval<DateTime>.Undefined,
                    Interval<TimeSpan>.New(TimeSpan.FromDays(1)),
                    Interval<TimeSpan>.New(TimeSpan.FromDays(1)),
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


            var sch1 = res1.Entity.Calendar.CreateSchedule(TimeSpan.FromMinutes(15));
            var left = DateTime.Today;
            var right = left.AddDays(10);
            AvailabilityData data = 4;

            sch1.Include(left.Left(true), right.Right(false), data);

            var sch = sch1.Allocated.Get(left.AddDays(-1), right.AddDays(1));
            var ava = sch1.Available.Get(left.AddDays(-1), right.AddDays(1));

            var arr1 = sch1.Allocated.DefinedItems();
            var arr2 = sch1.Available.DefinedItems();

            var tmp = res1.Entity.Calendar.Get(new DateTime(2017, 1, 1), left);



            var adc = new AmountedDataCalendar();

            adc.Get(left.AddDays(-2), right.AddDays(2));

            var adcsch = adc.CreateSchedule(TimeSpan.FromMinutes(15));

            adcsch.Include(left.Left(true), right.Right(false), AmountedData.Appoint("111", 1));
            adcsch.Include(left.AddDays(-1).Left(true), right.AddDays(1).Right(false), AmountedData.Appoint("222", 2));

            Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.CalendarItem[] adcarr1 = adcsch.Allocated.DefinedItems();
            var adcarr2 = adcsch.Available.DefinedItems();

            var root = new Scene();
            var branch1 = root.Branch();
            IScene resultScene;

            wi1.TrySatisfy(branch1, out resultScene);


            Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment>
                .CalculateAmount = (dur, availabilityData, amount) => new Appointment(amount.Appointee, 1);
            Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment>
                .AddAmount = (a, b) => new Appointment(a.Appointee, a.Value+b.Value);
            Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment>
                .SubstAmount = (a, b) => new Appointment(a.Appointee, a.Value - b.Value);

            Calendars<DateTime, TimeSpan>.OffsetToLeft = (dt, span) => dt?.Add(-(span ?? TimeSpan.Zero));
            Calendars<DateTime, TimeSpan>.OffsetToRight = (dt, span) => dt?.Add(span ?? TimeSpan.Zero);

            var tmpSch = adc.CreateSchedule(TimeSpan.FromMinutes(15));
            var scheduler = new Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment>
                (
                tmpSch, ProcessInstructionsRequest
                );

            var start = new Interval<DateTime>(left.Left(true));
            var finish = new Interval<DateTime>(left.Left(true), left.AddDays(1).Right(true));
            var totalDuration = new Interval<TimeSpan>(right: Point<TimeSpan>.Right(TimeSpan.FromHours(8)));
            var duration = new Interval<TimeSpan>(right: Point<TimeSpan>.Right(TimeSpan.FromHours(1)));
            var requiredAmount = new Appointment("111", 1);
            var kind = SchedulerKind.LeftToRight;

            // без горизонтов
            FindAllocationWithException<ArgumentException>(scheduler, null, null, totalDuration, duration, requiredAmount, kind);

            // [минимальное начало] правее [максимального окончания]
            FindAllocationWithException<ArgumentException>
                (
                    scheduler,
                    new Interval<DateTime>(right.Left(true)),
                    new Interval<DateTime>(right: left.Right(true)),
                    totalDuration,
                    duration,
                    requiredAmount,
                    kind
                );

            //определен только один горизонт и не определена длительность
            FindAllocationWithException<ArgumentException>(scheduler, null, finish, null, null, requiredAmount, kind);

            //определен только один горизонт и не определена максимальная длительность
            FindAllocationWithException<ArgumentException>(scheduler, null, finish, null, new Interval<TimeSpan>(Point<TimeSpan>.Left(TimeSpan.FromHours(1)), null), requiredAmount, kind);



            scheduler.FindAllocation(null, finish, totalDuration, null, requiredAmount, kind);


            scheduler.FindAllocation
                (
                start, 
                finish, 
                totalDuration,
                duration,
                requiredAmount, 
                kind
                );

            return environment;

        }

        static void FindAllocationWithException<T>(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment> scheduler, Interval<DateTime> start, Interval<DateTime> finish, Interval<TimeSpan> totalDuration, Interval<TimeSpan> duration, Appointment amount, SchedulerKind kind)
            where T : Exception
        {
            ExecuteWithException<T>(() => scheduler.FindAllocation(start, finish, totalDuration, duration, amount, kind));
        }
        static void ExecuteWithException<T>(Action action, Func<T, string> checkException = null)
            where T : Exception
        {
            try
            {
                action();
                Debug.Fail($"ожидалось исключение {typeof(T)}");
            }
            catch (Exception e)
            {
                if (!(e is T))
                {
                    Debug.Fail($"ожидалось исключение {typeof(T)}, текущее - {e.GetType()}");
                }

                var checkResult = checkException?.Invoke((T)e);
                if(string.IsNullOrEmpty(checkResult))
                    return;

                Debug.Fail(checkResult);
            }
        }

        private static IteratorInstructions ProcessInstructionsRequest(TimeSpan duration, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData data, Appointment requiredamount, out Appointment amount)
        {
            amount = new Appointment(requiredamount.Appointee, duration.TotalHours);
            return IteratorInstructions.Accept;
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

    public struct AmountedData : PlanningEnvironment<DateTime,TimeSpan>.IAvailabilityData
    {
        private readonly IEnumerable<Appointment> _appointments; 
        public int Available { get; }

        public AmountedData(int totalAmount) : this(new Appointment[0], totalAmount)
        {}

        private AmountedData(IEnumerable<Appointment> appointments, int available)
        {
            _appointments = appointments;
            Available = available;
        }

        PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData.Include(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData availabilityData)
        {
            var amountedData = ((AmountedData) availabilityData);
            return Include(amountedData);
        }

        public AmountedData Include(AmountedData amountedData)
        {
            var appointments = _appointments.Concat(amountedData._appointments)
                .GroupBy(x => x.Appointee)
                .Select(x => new Appointment(x.Key, x.Sum(y => y.Value)))
                .Where(x => x.Value != 0)
                .ToArray();

            var totalAmount = Available + amountedData.Available;

            return new AmountedData(appointments, totalAmount);
        }

        PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData.Exclude(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData availabilityData)
        {
            var amountedData = ((AmountedData)availabilityData);
            return Exclude(amountedData);
        }

        public AmountedData Exclude(AmountedData amountedData)
        {
            var appointments = _appointments.Concat(amountedData._appointments.Select(x => x.Invert()))
                .GroupBy(x => x.Appointee)
                .Select(x => new Appointment(x.Key, x.Sum(y => y.Value)))
                .Where(x => x.Value != 0)
                .ToArray();

            var totalAmount = Available - amountedData.Available;

            return new AmountedData(appointments, totalAmount);
        }

        public override string ToString()
        {
            var appointed = _appointments.Sum(x => x.Value);
            return $"статок: {Available}, Распределено (сумма): [{appointed} на {_appointments.Count()} 'кусков']";
        }

        public static AmountedData Appoint(string appointee, int amount)
        {
            return new AmountedData(new []{new Appointment(appointee, amount)}, amount);
        }
    }

    class Appointment : IComparable<Appointment>
    {
        public Appointment(string appointee, double value)
        {
            Value = value;
            Appointee = appointee;
        }

        public double Value { get; }
        public string Appointee { get; }

        public Appointment Invert()
        {
            return new Appointment(Appointee, -Value);
        }

        public int CompareTo(Appointment other)
        {
            if(Appointee != other?.Appointee)
                throw new ArgumentException($"{Appointee} != {other?.Appointee}", nameof(other));

            return Value.CompareTo(other?.Value);
        }
    }

    class AmountedDataCalendarItemsAdapter : Calendars<DateTime,TimeSpan,PlanningEnvironment<DateTime,TimeSpan>.IAvailabilityData>.CalendarAdapter
    {
        public override void Define(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime,TimeSpan>.IAvailabilityData>.ICalendarItems container, Point<DateTime> left, Point<DateTime> right)
        {
            container.Include(left, right, new AmountedData(10));
        }

        protected override DateTime Define(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.ICalendarItems container, DateTime left, DateTime right)
        {
            throw new NotSupportedException();
        }

        public override PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Include(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData a, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData b)
        {
            return a == null
                ? b
                : b == null
                    ? a
                    : a.Include(b);
        }

        public override PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Exclude(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData a, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData b)
        {
            return a == null
                ? b
                : b == null
                    ? a
                    : a.Exclude(b);
        }
    }

    public class AmountedDataCalendar : PlanningEnvironment<DateTime, TimeSpan>.AvailabilityCalendar
    {
        public AmountedDataCalendar() : base(new AmountedDataCalendarItemsAdapter())
        {
        }
    }

    public class AvailabilityCalendar : PlanningEnvironment<DateTime, TimeSpan>.AvailabilityCalendar
    {
        public AvailabilityCalendar(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.ICalendarItemsAdapter adapter) : base(adapter)
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

    public class Scene : IScene
    {
        private Satisfaction _satisfaction;

        public Scene()
        { }

        private Scene(IScene original)
        {
            Original = original;
        }

        public IScene Original { get; }

        public ISatisfaction Satisfaction => _satisfaction ?? (_satisfaction = new Satisfaction(((Scene)Original)._satisfaction?.Value ?? 0));

        private readonly List<INegotiator> _negotiators = new List<INegotiator>();
        public IEnumerable<INegotiator> Negotiators
        {
            get
            {
                lock (_negotiators)
                {
                    return _negotiators.ToArray();
                }
            }
        }

        public INegotiator Negotiator(IAgent agent)
        {
            lock (_negotiators)
            {
                var negotiator = _negotiators.FirstOrDefault(x => x.Agent == agent)
                    ?? CreateNegotiator(agent);

                return negotiator;
            }
        }

        private INegotiator CreateNegotiator(IAgent agent)
        {
            var negotiator = agent.Negotiator(this);
            _negotiators.Add(negotiator);
            return negotiator;
        }

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