using System;
using System.Linq;
using MAS.Core.Contracts;
using Protoplasm.Calendars;

namespace Application1.Data
{
    public class ResourceAgentNegotiator : Negotiator<ResourceAgent>
    {
        private WorkSchedule _schedule;

        public ResourceAgentNegotiator(IScene scene, ResourceAgent agent) : base(scene, agent)
        {
        }

        public WorkSchedule Schedule => _schedule ?? (_schedule = RequestFromAgent<WorkSchedule>(Scene));


        public override IScene Variate(INegotiator respondent)
        {
            if (respondent.IsSatisfied)
                return null;

            Boundary boundary;
            WorkItemAmount amount;
            var totalSatisfaction = Scene.Satisfaction.Snapshot();
            var my = Satisfaction;

            if (!respondent.Request(out boundary) || !respondent.Request(out amount))
                return null;

            var scheduler = Schedule.Scheduler();

            if (boundary.Finish.Right.IsUndefined)
                boundary = boundary.FinishBefore(DateTime.Today.AddDays(5));

            var result = scheduler.FindAllocation(boundary.Start, boundary.Finish, boundary.TotalDuration, boundary.Duration, amount, SchedulerKind.LeftToRight)
                .ToArray();


            bool accepted;
            if (!respondent.Ask(result, out accepted) || !accepted)
                return null;

            Schedule.Allocate(result);
            
            var s = Scene.Satisfaction;
            if (s.CompareTo(totalSatisfaction) > 0)
                return Scene;


            //scheduler.FindAllocation();
            return null;
        }

        public override bool IsSatisfied => false;

    }
}