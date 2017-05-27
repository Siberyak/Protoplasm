using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Protoplasm.Calendars;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace Application1.Data
{
    public struct AllocationRequirement
    {
        public Boundary Boundary { get; private set; }
        public WorkItemAmount Amount { get; private set; }
        public SchedulerKind Kind { get; private set; }

        public AllocationRequirement(Boundary boundary, WorkItemAmount amount, SchedulerKind kind)
        {
            Boundary = boundary;
            Amount = amount;
            Kind = kind;
        }
    }

    public struct WorkItemAmount : IComparable<WorkItemAmount>
    {
        public enum MesureUnit
        {
            [DisplayFormat(DataFormatString = "чел.дн.")]
            [EnumDisplayName("чел.дн.")]
            PerDay = 0,
            [DisplayFormat(DataFormatString = "чел.час.")]
            [EnumDisplayName("чел.час.")]
            PerHour
        }
        static WorkItemAmount()
        {
            ArithmeticAdapter<WorkItemAmount>.Add = (a, b) => a + b;
            ArithmeticAdapter<WorkItemAmount>.Subst = (a, b) => a - b;
        }

        private WorkItem _workItem;
        private readonly double _amount;

        public readonly bool AllowBreaks;
        public readonly bool AllowCombine;

        private WorkItemAmount(WorkItem workItem, double amount, bool allowBreaks, bool allowCombine)
        {
            _workItem = workItem;
            _amount = amount;
            AllowBreaks = allowBreaks;
            AllowCombine = allowCombine;
        }

        public WorkItemAmount(WorkItem workItem, double amount, TimeSpan duration, bool allowBreaks, bool allowCombine) 
            : this(workItem, amount*WorkItemSlotCollection.TimeSpanToDouble(duration), allowBreaks, allowCombine)
        {
        }

        public override string ToString()
        {
            return $"WI:[{_workItem}], A:[{_amount}] {EnumHelper<MesureUnit>.GetDisplayNames()[WorkItemSlotCollection.Unit]}";
        }

        public int CompareTo(WorkItemAmount other)
        {
            var empty = default(WorkItemAmount);

            var isEmoty = Equals(this, empty);
            var otherIsEmpty = Equals(other, empty);

            if (isEmoty && otherIsEmpty)
                return 0;
            if (isEmoty)
                return 1;
            if (otherIsEmpty)
                return -1;


            if (_workItem != other._workItem)
                throw new ArgumentException(@"сравнение с amount-ом для 'левого' work-item-a", nameof(other));

            return _amount.CompareTo(other._amount);
        }

        public static AllocationInstruction ResponseInstruction(WorkItemSlotCollection data, WorkItemAmount requiredamount)
        {
            var allocated = data.Slots.Any();
            var noScope = Math.Abs(data.AvailableProductivity) < double.Epsilon;
            var allowCombine = requiredamount.AllowCombine;
            var allowBreaks = requiredamount.AllowBreaks;


            if (noScope)
            {
                return allowBreaks ? AllocationInstruction.Skip : AllocationInstruction.Reject;
            }

            if (!allocated)
                return AllocationInstruction.Accept;

            if (allowCombine)
            {
                return AllocationInstruction.Accept;
            }

            return allowBreaks ? AllocationInstruction.Skip : AllocationInstruction.Reject;
        }

        public static WorkItemAmount ResponseAmount(TimeSpan duration, WorkItemSlotCollection data, WorkItemAmount requiredamount)
        {
            return new WorkItemAmount(requiredamount._workItem, data.AvailableProductivity, duration, requiredamount.AllowBreaks, requiredamount.AllowCombine);
        }

        public static TimeSpan ResponseDurationByAmount(TimeSpan fullduration, WorkItemAmount fullamount, WorkItemAmount amount)
        {
            var toDouble = WorkItemSlotCollection.TimeSpanToDouble(fullduration);

            var value = toDouble / fullamount._amount*amount._amount;
            
            return WorkItemSlotCollection.DoubleToTimeSpan(value); 
        }

        public static WorkItemSlotCollection ResponseDataForAllocate(Interval<DateTime> interval, WorkItemSlotCollection originaldata, WorkItemAmount amount)
        {
            var duration = Calendars<DateTime, TimeSpan>.Duration(interval);
            var portion = amount._amount/WorkItemSlotCollection.TimeSpanToDouble(duration);
            return originaldata.Include(new WorkItemSlotCollection(0, new[] {new WorkItemSlot(amount._workItem, portion),}));
        }

        private static WorkItemAmount Calc(WorkItemAmount a, WorkItemAmount b, Func<double, double, double> calcAmount)
        {
            var awi = a._workItem ?? b._workItem;
            var bwi = b._workItem ?? a._workItem;

            if (awi != bwi)
                throw new ArithmeticException($"для '{typeof (WorkItemAmount).TypeName()}' допускается выполнение 'сложения' и 'вычитания' только если совпадают ссылки на '{typeof (WorkItem)}'");

            if (awi == null)
                return default(WorkItemAmount);

            var allowBreaks = a._workItem != null ? a.AllowBreaks : b.AllowBreaks;
            var allowCombine = a._workItem != null ? a.AllowCombine : b.AllowCombine;

            return new WorkItemAmount(awi, calcAmount(a._amount, b._amount), allowBreaks, allowCombine);
        }

        public static WorkItemAmount operator +(WorkItemAmount a, WorkItemAmount b)
        {
            return Calc(a, b, (x, y) => x + y);
        }

        public static WorkItemAmount operator -(WorkItemAmount a, WorkItemAmount b)
        {
            return Calc(a, b, (x, y) => x - y);
        }
    }
}