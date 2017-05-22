using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.Utils;

namespace Application1.Data
{
    public struct WorkItemSlotCollection
    {

        public double Productivity { get; }
        public double AvailableProductivity { get; }

        private IEnumerable<WorkItemSlot> _slots;
        public IEnumerable<WorkItemSlot> Slots => _slots ?? (_slots = Enumerable.Empty<WorkItemSlot>());

        public WorkItemSlotCollection(double productivity, IEnumerable<WorkItemSlot> slots = null)
        {
            Productivity = productivity;
            _slots = slots;
            AvailableProductivity = Productivity;
            AvailableProductivity -= Slots.Sum(x => x.GrantedProductivity);
        }

        public WorkItemSlotCollection Include(WorkItemSlotCollection other)
        {
            var otherSlots = other.Slots;
            return CreateWorkItemSlotCollection(other, otherSlots);
        }

        public WorkItemSlotCollection Exclude(WorkItemSlotCollection other)
        {
            var otherSlots = other.Slots.Select(x => new WorkItemSlot(x.WorkItem, -x.GrantedProductivity));
            return CreateWorkItemSlotCollection(other, otherSlots);
        }

        private WorkItemSlotCollection CreateWorkItemSlotCollection(WorkItemSlotCollection other, IEnumerable<WorkItemSlot> otherSlots)
        {
            var slots = Slots.Concat(otherSlots).GroupBy(x => x.WorkItem).Select(x => new WorkItemSlot(x.Key, x.Sum(y => y.GrantedProductivity))).ToArray();
            var capacity = Productivity.Max(other.Productivity);
            return new WorkItemSlotCollection(capacity, slots);
        }

        public static readonly WorkItemAmount.MesureUnit Unit = WorkItemAmount.MesureUnit.PerHour;

        public static double TimeSpanToDouble(TimeSpan span)
        {
            switch (Unit)
            {
                case WorkItemAmount.MesureUnit.PerDay:
                    return span.TotalDays;
                case WorkItemAmount.MesureUnit.PerHour:
                    return span.TotalHours;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static TimeSpan DoubleToTimeSpan(double value)
        {
            switch (Unit)
            {
                case WorkItemAmount.MesureUnit.PerDay:
                    return TimeSpan.FromDays(value);
                case WorkItemAmount.MesureUnit.PerHour:
                    return TimeSpan.FromHours(value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            return $"P:{Productivity}, A:{AvailableProductivity}, S:{Slots.Count()}";
        }
    }
}