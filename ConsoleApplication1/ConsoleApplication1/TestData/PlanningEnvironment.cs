using System;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
        where TTime : struct, IComparable<TTime>
        where TDuration : struct, IComparable<TDuration>
    {

        public static Func<TTime?, TTime?, TDuration?> GetOffset;

        public static TTime Min(TTime a, TTime b)
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }
        public static TTime Max(TTime a, TTime b)
        {
            return a.CompareTo(b) >= 0 ? a : b;
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