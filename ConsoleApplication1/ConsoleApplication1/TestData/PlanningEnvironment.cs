using System;
using Protoplasm.Calendars;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
        where TTime : struct, IComparable<TTime>
        where TDuration : struct, IComparable<TDuration>
    {
        public static Func<TTime?, TTime?, TDuration?> ToDuration => Calendars<TTime, TDuration>.ToDuration;

        public static TTime Min(TTime a, TTime b)
        {
            return Calendars<TTime, TDuration>.Min(a, b);
        }
        public static TTime Max(TTime a, TTime b)
        {
            return Calendars<TTime, TDuration>.Max(a, b);
        }

        public PlanningEnvironment()
        {
            Resources = new ResourcesManager(this);
            WorkItems = new WorkItemsManager(this);
        }

        public ResourcesManager Resources { get; }
        public WorkItemsManager WorkItems { get; }
    }
}