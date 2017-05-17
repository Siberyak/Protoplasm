using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace Protoplasm.Calendars
{
    public static class SchedulerTester
    {
        public static void Test()
        {
            Test1();
            Test2();
        }

        private static void Test1()
        {
            var config = new SchConfig();

            for (var i = 7; i <= 14; i += 7)
            {
                config.Schedule.Include(i - 1, i + 1, new TestData(true), rightIncluded: false);
            }

            config.Schedule.Include(8, 13, new TestData("zxc", 4), rightIncluded: false);
            config.Schedule.Include(16, 60, new TestData("zxc", 6), rightIncluded: false);

            config.Schedule.Include(60, 62, new TestData(true), rightIncluded: false);
            config.Schedule.Include(67, 69, new TestData(true), rightIncluded: false);

            var sch = new Sch(config);

            sch.FindAllocation
                (
                    new Interval<double>(5, 100, true, true),
                    new Interval<double>(5, 100, true, true),
                    new Interval<double>(5, 9, true, false),
                    new Interval<double>(5, 10, true, true),
                    new TestDataAmount($"amount {7*8}", 7 * 8, true, true),
                    SchedulerKind.LeftToRight
                );

            sch.FindAllocation
                (
                    new Interval<double>(5, 100, true, true),
                    new Interval<double>(5, 100, true, true),
                    new Interval<double>(5, 9, true, false),
                    new Interval<double>(5, 10, true, true),
                    new TestDataAmount($"amount {6 * 8}", 6 * 8, true, true),
                    SchedulerKind.LeftToRight
                );

        }

        private static void Test2()
        {
            var config = new SchConfig();

            for (var i = 7; i <= 100; i += 7)
            {
                config.Schedule.Include(i - 1, i + 1, new TestData(true), rightIncluded: false);
            }

            config.Schedule.Include(8, 13, new TestData("zxc", 6), rightIncluded: false);

            var sch = new Sch(config);
            sch.FindAllocation
                (
                    new Interval<double>(5, 100, true, true),
                    new Interval<double>(5, 100, true, true),
                    new Interval<double>(5, 9, true, false),
                    new Interval<double>(5, 10, true, true),
                    new TestDataAmount("123", 7 * 8, true, true),
                    SchedulerKind.LeftToRight
                );
        }
    }
    class TestDataAmount : IComparable<TestDataAmount>
    {
        static TestDataAmount()
        {
            DataAdapter<TestDataAmount>.Add = (a, b) => a + b;
            DataAdapter<TestDataAmount>.Subst = (a, b) => a - b;
        }

        public override string ToString()
        {
            return $"'{Appointee}': {Amount}";
        }

        public readonly string Appointee;
        public readonly double Amount;
        public readonly bool AllowBreaks;
        public readonly bool AllowCombine;

        public TestDataAmount(string appointee, double amount, bool allowBreaks, bool allowCombine)
        {
            Appointee = appointee;
            Amount = amount;
            AllowBreaks = allowBreaks;
            AllowCombine = allowCombine;
        }

        public int CompareTo(TestDataAmount other)
        {
            if (other == null)
                return -1;

            if(Appointee != other.Appointee)
                throw new NotSupportedException();

            return Amount.CompareTo(other.Amount);
        }


        private static string Normalize(ref TestDataAmount a, ref TestDataAmount b)
        {
            var appointee = a?.Appointee ?? b.Appointee;
            var allowCombine = a?.AllowCombine ?? b.AllowCombine;
            var allowBreaks = a?.AllowBreaks ?? b.AllowBreaks;

            a = a ?? new TestDataAmount(appointee, 0, allowBreaks, allowCombine);
            b = b ?? new TestDataAmount(appointee, 0, allowBreaks, allowCombine);

            if (a.Appointee != b.Appointee)
                throw new NotSupportedException();
            return appointee;
        }

        public static TestDataAmount operator +(TestDataAmount a, TestDataAmount b)
        {
            if (a == null && b == null)
                return null;

            var appointee = Normalize(ref a, ref b);

            return new TestDataAmount(appointee, a.Amount + b.Amount, a.AllowBreaks, a.AllowCombine);
        }
        public static TestDataAmount operator -(TestDataAmount a, TestDataAmount b)
        {
            if (a == null && b == null)
                return null;

            var appointee = Normalize(ref a, ref b);

            return new TestDataAmount(appointee, a.Amount - b.Amount, a.AllowBreaks, a.AllowCombine);
        }

    }

    class TestData
    {
        public readonly bool Skipped;

        public readonly List<string> Slots = new List<string>(new string[8]);

        public TestData(bool skipped = false)
        {
            Skipped = skipped;
        }

        public TestData(string appointee, int slots) : this(GenerateSlots(appointee, slots))
        { }

        public static string[] GenerateSlots(string appointee, int slots)
        {
            var result = new string[8];
            for (int i = 0; i < slots; i++)
            {
                result[i] = appointee;
            }
            return result;
        }

        public TestData(string[] slots, bool skipped = false) : this(skipped)
        {
            //if(slots.Length != 8)
            //    throw new NotSupportedException();
            Slots = new List<string>(slots);
        }

        public static TestData Include(TestData a, TestData b)
        {
            if (a == null && b==null)
                return null;

            if (a == null)
                return b;

            if (b == null)
                return a;

            if (a.Skipped)
                return a;

            if (b.Skipped)
                return b;

            var slots = new List<string>(a.Slots);

            foreach (var slot in b.Slots.Where(x => x != null))
            {
                var index = slots.FindIndex(x => x == null);
                if (index == -1)
                    throw new NotSupportedException();

                slots[index] = slot;
            }

            return new TestData(slots.ToArray());
        }

        public static TestData Exclude(TestData a, TestData b)
        {
            if (a == null && b == null)
                return null;

            if (a == null)
                return b;

            if (b == null)
                return a;

            if (a.Skipped)
                return a;
            if (b.Skipped)
                return b;

            var slots = new List<string>(a.Slots);

            foreach (var slot in b.Slots.Where(x => x != null))
            {
                var index = slots.FindIndex(x => x == slot);
                if (index == -1)
                {
                    index = slots.FindIndex(x => x == null);
                    if(index == -1)
                        throw new NotSupportedException();
                    slots.RemoveAt(index);
                }
                else
                    slots[index] = null;
            }

            return new TestData(slots.ToArray());
        }

        public override string ToString()
        {
            var empty = $"Empty: {Slots.Count(x => x == null)}";
            var list = new List<string> {empty};
            list.AddRange(Slots.Where(x => x != null).GroupBy(x => x).OrderBy(x => x).Select(x => $"'{x.Key}':{x.Count()}"));
            
            return Skipped ? "[Skipped]" 
                : $"[{string.Join(", ", list)}]";
        }
    }

    class C : Calendars<double, double, TestData>.Calendar
    {
        static C()
        {
            Calendars<double, double>.ToDuration = (a, b) => b - a;
            Calendars<double, double>.AddDuration = (a, b) => a + b;
            Calendars<double, double>.OffsetToLeft = (a, b) => a-b;
            Calendars<double, double>.OffsetToRight = (a, b) => a+b;
        }

        public C() : base(new CA())
        {
        }
    }
    class CA : Calendars<double, double, TestData>.CalendarAdapter
    {
        public override void Define(Calendars<double, double, TestData>.ICalendarItems container, Point<double> left, Point<double> right)
        {
            container.Include(left, right, new TestData());
        }

        protected override double Define(Calendars<double, double, TestData>.ICalendarItems container, double left, double right)
        {
            throw new NotImplementedException();
        }

        public override TestData Include(TestData a, TestData b)
        {
            return TestData.Include(a,b);
        }

        public override TestData Exclude(TestData a, TestData b)
        {
            return TestData.Exclude(a, b);
        }
    }

    class SchConfig
    {
        public Calendars<double, double, TestData>.ISchedule Schedule { get; } = CreateEmptySchedule();

        private static Calendars<double, double, TestData>.ISchedule CreateEmptySchedule()
        {
            return new C().CreateSchedule(1);
        }

        public IteratorInstruction ProcessRequestInstructions(TestData data, TestDataAmount requiredamount)
        {
            if(data.Skipped)
                return IteratorInstruction.Skip;

            var emptySlots = data.Slots.Count(x => x == null);
            var appointeeSlots = data.Slots.Count(x => x == requiredamount.Appointee);

            if (emptySlots == 0)
            {
                return requiredamount.AllowBreaks ? IteratorInstruction.Reject : IteratorInstruction.Skip;
            }

            if (emptySlots != data.Slots.Count && !requiredamount.AllowCombine)
            {
                return IteratorInstruction.Reject;
            }

            return IteratorInstruction.Accept;
        }

        public TestDataAmount ProcessRequestAmount(double duration, TestData data, TestDataAmount requiredamount)
        {
            double amount;
            if (data.Skipped)
                amount = 0;
            else
            {
                var slots = data.Slots.Count(x => x == null);
                amount = slots * duration;
            }
            return new TestDataAmount(requiredamount.Appointee, amount, requiredamount.AllowBreaks, requiredamount.AllowCombine);
        }

        public double ProcessRequestDurationByAmount(double fullduration, TestDataAmount fullamount, TestDataAmount amount)
        {
            return fullduration/fullamount.Amount* amount.Amount;
        }
    }

    class Sch : Calendars<double, double,TestData>.Scheduler<TestDataAmount>
    {
        public Sch() : this(new SchConfig())
        { }

        public Sch(SchConfig config)
            : base(config.Schedule, config.ProcessRequestInstructions, config.ProcessRequestAmount, config.ProcessRequestDurationByAmount)
        {
        }



    }


}