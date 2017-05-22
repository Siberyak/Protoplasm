using System;
using MAS.Core;
using MAS.Core.Contracts;
using Protoplasm.Calendars;

namespace Application1.Data
{
    class WorkItemNegotiator : Negotiator<WorkItemAgent>
    {
        private static HandlersStorage<WorkItemNegotiator> Handlers;

        static WorkItemNegotiator()
        {
            Handlers = new HandlersStorage<WorkItemNegotiator>();
            Handlers.Asked<Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.Scheduler<WorkItemAmount>.IAllocation[], bool>((negotiator, allocations) => negotiator.Accept(allocations));
        }

        private bool Accept(Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.Scheduler<WorkItemAmount>.IAllocation[] allocations)
        {
            var accept = allocations.Length > 0;
            if (accept)
            {
                Satisfaction.Change(1);
            }

            return accept;
        }

        public WorkItemNegotiator(IScene scene, WorkItemAgent agent) : base(scene, agent)
        {
        }

        public override IScene Variate(INegotiator respondent)
        {
            throw new NotImplementedException();
        }

        public override bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            return Handlers.Ask(this, question, out answer) || base.Ask(question, out answer);
        }

        public override bool IsSatisfied => Satisfaction.Value > 0;

    }

}