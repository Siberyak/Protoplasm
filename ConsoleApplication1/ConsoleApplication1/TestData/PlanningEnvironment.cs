using System;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
        where TTime : struct, IComparable<TTime>
        where TDuration : struct, IComparable<TDuration>
    {

        public ResourcesManager Resources { get; }
        public WorkItemsManager WorkItems { get; }
    }
}