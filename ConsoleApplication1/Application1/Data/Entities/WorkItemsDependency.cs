using System;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class WorkItemsDependency
    {
        public WorkItemsDependency(WorkItemsInterval primaryInterval, WorkItemsInterval secondaryInterval, Interval<TimeSpan> delay)
        {
            PrimaryInterval = primaryInterval;
            SecondaryInterval = secondaryInterval;
            Delay = delay;
        }

        public WorkItemsInterval PrimaryInterval { get; set; }
        public WorkItemsInterval SecondaryInterval { get; set; }
        public Interval<TimeSpan> Delay { get; set; }

        public override string ToString()
        {
            return $"prim.{PrimaryInterval} -> {Delay} -> sec.{SecondaryInterval}";
        }
    }
}