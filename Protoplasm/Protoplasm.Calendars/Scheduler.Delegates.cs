using System;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public partial class Scheduler<TAmount>
            where TAmount : IComparable<TAmount>
        {

            public delegate IteratorInstruction RequestInstructionsDelegate(TData data, TAmount requiredAmount);

            public delegate TAmount RequestAmountDelegate(TDuration duration, TData data, TAmount requiredAmount);

            public delegate TDuration RequestDurationByAmountDelegate(TDuration fullDuration, TAmount fullAmount, TAmount amount);

            public delegate TData RequestDataForAllocateDelegate(Interval<TTime> interval, TData originalData, TAmount reqiredAmount);
        }
    }
}