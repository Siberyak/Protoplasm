using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Akka.Actor;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Contracts;
using MAS.Utils;
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

            throw new NotImplementedException();

            //wi1.TrySatisfy(branch1, out resultScene);

            Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment>
                .AddAmount = (a, b) => new Appointment(a.Appointee, a.Value+b.Value);
            Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment>
                .SubstAmount = (a, b) => new Appointment(a.Appointee, a.Value - b.Value);

            Calendars<DateTime, TimeSpan>.OffsetToLeft = (dt, span) => dt?.Add(-(span ?? TimeSpan.Zero));
            Calendars<DateTime, TimeSpan>.OffsetToRight = (dt, span) => dt?.Add(span ?? TimeSpan.Zero);

            Calendars<DateTime, TimeSpan>.AddDuration = (a, b) => a + b;
            Calendars<DateTime, TimeSpan>.SubstDuration = (a, b) => a - b;


            var tmpSch = adc.CreateSchedule(TimeSpan.FromMinutes(15));
            var scheduler = new Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<Appointment>
                (
                tmpSch, ProcessInstructionsRequest, ProcessAmountRequest, PrecessDurationByAmount
                //, ProcessDataForAllocate
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

            //определен один диапазон и не определена длительность
            FindAllocationWithException<ArgumentException>(scheduler, null, finish, null, null, requiredAmount, kind);

            //определен один диапазон и не определена максимальная длительность
            FindAllocationWithException<ArgumentException>(scheduler, null, finish, null, new Interval<TimeSpan>(Point<TimeSpan>.Left(TimeSpan.FromHours(1)), null), requiredAmount, kind);

            tmpSch.Exclude(left.AddDays(9).AddHours(14).Left(true), left.AddDays(9).AddHours(16).Right(false), AmountedData.Appoint("222", 1));


            TestTestDataScheduller();


            scheduler.FindAllocation
                (
                    new Interval<DateTime>(left.Left(true), left.AddDays(10).Right(true)),
                    new Interval<DateTime>(left.AddDays(10).Left(true), left.AddDays(20).Right(true)),
                    null,
                    new Interval<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(12), true, true),
                    requiredAmount,
                    kind
                );

            scheduler.FindAllocation(new Interval<DateTime>(left.Left(true), left.AddDays(10).Right(true)), finish, totalDuration, null, requiredAmount, kind);



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

        private static TimeSpan PrecessDurationByAmount(TimeSpan fullduration, Appointment fullamount, Appointment amount)
        {
            var k = amount.Value/fullamount.Value;
            return TimeSpan.FromTicks(Convert.ToInt64(Math.Round(fullduration.Ticks * k)));
        }

        private static PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData ProcessDataForAllocate(Interval<DateTime> interval, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData originaldata, Appointment reqiredamount)
        {
            if (!(originaldata is AmountedData))
                throw new ArgumentException("", nameof(originaldata));

            var data = (AmountedData)originaldata;

            var amount = 10 - data.Appointments.Sum(x => x.Value);
            return AmountedData.Appoint(reqiredamount.Appointee, (int) amount);
        }



        private static void TestTestDataScheduller()
        {
            SchedulerTester.Test();



            var calendar = new TestDataCalendar();
            var schedule = calendar.CreateSchedule(TimeSpan.FromMinutes(15));
            var scheduler = new TestDataScheduller(schedule);

            var left = DateTime.Today;

            // for reject
            schedule.Exclude
                (
                    left.AddDays(9).AddHours(14).Left(true),
                    left.AddDays(9).AddHours(15).Right(false),
                    new TestData("14-15")
                );

            // for skip
            schedule.Exclude
                (
                left.AddDays(9).AddHours(15).AddMinutes(20).Left(true),
                left.AddDays(9).AddHours(15).AddMinutes(40).Right(false),
                new TestData("-= ! =-")
                );

            // for reject
            schedule.Exclude
                (
                left.AddDays(9).AddHours(16).Left(true),
                left.AddDays(9).AddHours(17).Right(false),
                new TestData("16-17")
                );

            // for skip
            schedule.Exclude
                (
                left.AddDays(9).AddHours(19).Left(true),
                left.AddDays(9).AddHours(20).Right(false),
                new TestData("-= ! =-")
                );

            scheduler.FindAllocation
                (
                    new Interval<DateTime>(left.Left(true), left.AddDays(10).Right(true)),
                    new Interval<DateTime>(left.AddDays(10).Left(true), left.AddDays(20).Right(true)),
                    null,
                    new Interval<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(12), true, true),
                    new TestAppointment("123", 6), 
                    SchedulerKind.LeftToRight
                );

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

        private static Appointment ProcessAmountRequest(TimeSpan duration, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData data, Appointment requiredamount)
        {
            if (!(data is AmountedData))
                return null;

            return null;
        }

        private static AllocationInstruction ProcessInstructionsRequest(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData data, Appointment requiredamount)
        {
            if (!(data is AmountedData))
                return AllocationInstruction.Skip;
            
            var amountedData = ((AmountedData)data);
            if(amountedData.Appointments.Any(x => x.Appointee != requiredamount.Appointee))
                return AllocationInstruction.Reject;
            
            if(amountedData.Available <= 0)
                return AllocationInstruction.Skip;

            return AllocationInstruction.Accept;
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
        public IEnumerable<Appointment> Appointments { get; }

        public int Available { get; }

        public AmountedData(int totalAmount) : this(new Appointment[0], totalAmount)
        {}

        private AmountedData(IEnumerable<Appointment> appointments, int available)
        {
            Appointments = appointments;
            Available = available;
        }

        PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData.Include(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData availabilityData)
        {
            var amountedData = ((AmountedData) availabilityData);
            return Include(amountedData);
        }

        public AmountedData Include(AmountedData amountedData)
        {
            var appointments = Appointments.Concat(amountedData.Appointments)
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
            var appointments = Appointments.Concat(amountedData.Appointments.Select(x => x.Invert()))
                .GroupBy(x => x.Appointee)
                .Select(x => new Appointment(x.Key, x.Sum(y => y.Value)))
                .Where(x => x.Value != 0)
                .ToArray();

            var totalAmount = Available - amountedData.Available;

            return new AmountedData(appointments, totalAmount);
        }

        public override string ToString()
        {
            var appointed = Appointments.Sum(x => x.Value);
            return $"статок: {Available}, Распределено (сумма): [{appointed} на {Appointments.Count()} 'кусков']";
        }

        public static AmountedData Appoint(string appointee, int amount)
        {
            return new AmountedData(new []{new Appointment(appointee, amount)}, amount);
        }
    }



    class TestData : PlanningEnvironment<DateTime,TimeSpan>.IAvailabilityData
    {
        public override string ToString()
        {
            return $"'{Appointee}'";
        }

        public string Appointee;

        public TestData(string appointee)
        {
            Appointee = appointee;
        }

        public PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Include(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData availabilityData)
        {
            var testdata = availabilityData as TestData ?? new TestData(null);
            if (Appointee == null || Equals(Appointee, testdata.Appointee))
                return new TestData(testdata.Appointee);

            throw new NotSupportedException();
        }

        public PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData Exclude(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData availabilityData)
        {
            var testdata = availabilityData as TestData ?? new TestData(null);
            if (Appointee == null || Equals(Appointee, testdata.Appointee))
                return new TestData(null);

            throw new NotSupportedException();
        }
    }


    abstract class AvailabilityCalendarAdapter<T> : Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.CalendarAdapter
        where T : PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData
    {
        public override void Define(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.ICalendarItems container, Point<DateTime> left, Point<DateTime> right)
        {
            container.Include(left, right, GetDefinedData());
        }

        protected abstract T GetDefinedData();

        protected override DateTime Define(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.ICalendarItems container, DateTime left, DateTime right)
        {
            throw new NotImplementedException();
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

    class TestAppointment : IComparable<TestAppointment>
    {
        public override string ToString()
        {
            return $"'{Appointee}': {Hours} ч.";
        }

        public string Appointee;

        public double Hours;

        public TestAppointment()
        {
        }

        public TestAppointment(string appointee, double hours)
        {
            Appointee = appointee;
            Hours = hours;
        }

        public int CompareTo(TestAppointment other)
        {
            if(other.Appointee != Appointee)
                throw new NotSupportedException();
            return Hours.CompareTo(other.Hours);
        }
    }

    class TestDataCalendarAdapter : AvailabilityCalendarAdapter<TestData>
    {
        protected override TestData GetDefinedData()
        {
            return new TestData(null);
        }
    }

    class TestDataCalendar : PlanningEnvironment<DateTime, TimeSpan>.AvailabilityCalendar
    {
        public TestDataCalendar() : base(new TestDataCalendarAdapter())
        {
        }
    }

    class TestDataScheduller : Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.Scheduler<TestAppointment>
    {
        static TestDataScheduller()
        {
            AddAmount = Add;
            SubstAmount = Subst;
        }

        public TestDataScheduller(Calendars<DateTime, TimeSpan, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData>.ISchedule schedule) : base(schedule, ResponseInstructions, ResponseAmount, ResponseDuration
            //, ResponseDataForAllocate
            )
        {}

        private static TimeSpan ResponseDuration(TimeSpan fullduration, TestAppointment fullamount, TestAppointment amount)
        {
            var k = amount.Hours / fullamount.Hours;
            return TimeSpan.FromTicks(Convert.ToInt64(Math.Round(fullduration.Ticks * k)));
        }
        private static PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData ResponseDataForAllocate(Interval<DateTime> interval, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData originaldata, TestAppointment reqiredamount)
        {
            throw new NotImplementedException();
        }

        static TestAppointment Add(TestAppointment a, TestAppointment b)
        {
            if (a == null && b == null)
                return null;

            if (a == null)
                return b;

            if (b == null)
                return a;

            if (a.Appointee != b.Appointee)
                throw new NotSupportedException();

            return new TestAppointment(a.Appointee, a.Hours + b.Hours);
        }

        static TestAppointment Subst(TestAppointment a, TestAppointment b)
        {
            if (a == null && b == null)
                return null;

            if (a == null)
                return new TestAppointment(b.Appointee, -b.Hours);

            if (b == null)
                return a;

            if (a.Appointee != b.Appointee)
                throw new NotSupportedException();

            return new TestAppointment(a.Appointee, a.Hours - b.Hours);
        }

        private static AllocationInstruction ResponseInstructions(PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData data, TestAppointment requiredamount)
        {
            var testData = data as TestData;
            if(testData == null || requiredamount?.Appointee == null)
                throw new NotSupportedException();

            if (testData.Appointee == "-= ! =-")
                return AllocationInstruction.Skip;

            return testData.Appointee == null 
                ? AllocationInstruction.Accept 
                : AllocationInstruction.Reject;
        }

        private static TestAppointment ResponseAmount(TimeSpan duration, PlanningEnvironment<DateTime, TimeSpan>.IAvailabilityData data, TestAppointment requiredamount)
        {
            if(ResponseInstructions(data, requiredamount) != AllocationInstruction.Accept)
                throw new NotSupportedException();

            return new TestAppointment(requiredamount.Appointee, duration.TotalHours);
        }
    }

    public class Appointment : IComparable<Appointment>
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

    class AmountedDataCalendarItemsAdapter : AvailabilityCalendarAdapter<AmountedData>
    {
        protected override AmountedData GetDefinedData()
        {
            return new AmountedData(10);
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
}