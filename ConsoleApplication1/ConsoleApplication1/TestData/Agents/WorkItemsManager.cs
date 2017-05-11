using System;
using System.Collections.Generic;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {
        public class WorkItemsManager : AgentsManager
        {
            private PlanningEnvironment<TTime, TDuration> _environment;
            public WorkItemsManager(PlanningEnvironment<TTime, TDuration> environment)
            {
                _environment = environment;
            }


            


            public WorkItemAgent CreateWorkItemAgent
                (
                string caption, 
                Interval<TTime> start, 
                Interval<TTime> finish,
                Interval<TDuration> calendarDuration,
                Interval<TDuration> workingDuration,
                IReadOnlyCollection<Competence> competences
                )
            {
                var workItem = new WorkItem(caption, start, finish, calendarDuration, workingDuration, competences);
                var agent = new WorkItemAgent(this, workItem);
                agent.Initialize();
                return agent;
            }
        }
    }
}