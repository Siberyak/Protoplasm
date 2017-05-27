using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core;
using MAS.Core.Contracts;
using Protoplasm.Calendars;
using IAllocation = Protoplasm.Calendars.Calendars<System.DateTime, System.TimeSpan, Application1.Data.WorkItemSlotCollection>.Scheduler<Application1.Data.WorkItemAmount>.IAllocation;


namespace Application1.Data
{
    class WorkItemAgentNegotiator : Negotiator<WorkItemAgent>
    {
        private static HandlersStorage<WorkItemAgentNegotiator> Handlers;
        private IAllocation[] _allocations
            = new IAllocation[0];

        static WorkItemAgentNegotiator()
        {
            Handlers = new HandlersStorage<WorkItemAgentNegotiator>();
            Handlers.Asked<IAllocation[], bool>((negotiator, allocations) => negotiator.Accept(allocations));
            Handlers.Requested(x => x._allocations);
            Handlers.Requested(x => x.AllocationRequirements);
        }

        public AllocationRequirement[] AllocationRequirements
        {
            get
            {
                var boundary = Agent.Boundary;
                Boundary[] restrictions;
                Agent.Ask(new DependenciesRequest(Scene), out restrictions);

                foreach (var restriction in restrictions)
                {
                    boundary = boundary.Restrict(restriction);
                }



                return boundary.IsCorrect
                    ? new[]{new AllocationRequirement(boundary, Agent.RequiredAmount, Agent.Kind)}
                    : new AllocationRequirement[0];
            }
        }

        private bool Accept(IAllocation[] allocations)
        {
            var accept = allocations.Length > 0;
            if (accept)
            {
                _allocations = allocations;
                Satisfaction.Change(Agent.Priority);
            }

            return accept;
        }

        public WorkItemAgentNegotiator(IScene scene, WorkItemAgent agent) : base(scene, agent)
        {
            Agent[Scene.Original]?.Request(out _allocations);
        }

        public override IEnumerable<IScene> Variants(INegotiator respondent)
        {
            IEnumerable<IScene> allocations;
            return !respondent.Ask((INegotiator)this, out allocations) 
                ? Enumerable.Empty<IScene>() 
                : allocations;
        }

        protected override void MergeToOriginal(INegotiator original)
        {
            var negotiator = (WorkItemAgentNegotiator)original;
            negotiator._allocations = _allocations;
        }

        protected override bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            return Handlers.Ask(this, question, out answer) || base.Ask(question, out answer);
        }

        protected override bool Request<TData>(out TData data)
        {
            return Handlers.Request(this, out data);
        }

        public override NegotiatorState State =>
            Satisfaction.Value > 0
                ? NegotiatorState.Satisfied
                : ReadyByDependencies()
                    ? NegotiatorState.Ready
                    : NegotiatorState.NotReady;


        private bool ReadyByDependencies()
        {
            NegotiatorState state;
            if (!Agent.Ask(new DependenciesRequest(Scene), out state))
                return false;

            return state != NegotiatorState.NotReady;
        }
    }

    public class DependenciesRequest
    {
        public IScene Scene { get; }

        public DependenciesRequest(IScene scene)
        {
            Scene = scene;
        }
    }

}