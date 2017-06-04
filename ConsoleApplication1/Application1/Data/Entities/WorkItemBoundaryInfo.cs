using System;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class WorkItemBoundaryInfo
    {
        public Interval<DateTime> Start;
        public Interval<DateTime> Finish;

        public WorkItemBoundaryInfo()
        {
        }

        public WorkItemBoundaryInfo(Interval<DateTime> start, Interval<DateTime> finish)
        {
            Start = start;
            Finish = finish;
        }
    }
}