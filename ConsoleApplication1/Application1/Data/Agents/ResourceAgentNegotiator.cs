using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core;
using MAS.Core.Contracts;
using Protoplasm.Calendars;
using Protoplasm.Utils;
using IAllocation = Protoplasm.Calendars.Calendars<System.DateTime, System.TimeSpan, Application1.Data.WorkItemSlotCollection>.Scheduler<Application1.Data.WorkItemAmount>.IAllocation;

namespace Application1.Data
{
    public class ResourceAgentNegotiator : Negotiator<ResourceAgent>
    {
        private static readonly HandlersStorage<ResourceAgentNegotiator> Handlers = new HandlersStorage<ResourceAgentNegotiator>();

        static ResourceAgentNegotiator()
        {
            // запрос на поиск возможного размещения
            Handlers.Asked<INegotiator, IEnumerable<IScene>>((negotiator, respondent) => negotiator.Variants(respondent));
            
            // команда применить размещение к расписанию
            Handlers.Told<IAllocation[]>((negotiator, allocations) => negotiator.Allocate(allocations));

            Handlers.Requested(x => x.Schedule);
        }

        public ResourceAgentNegotiator(IScene scene, ResourceAgent agent) : base(scene, agent)
        {
            Schedule = GetSchedule();
        }

        public WorkSchedule Schedule { get; private set; }

        private WorkSchedule GetSchedule()
        {
            WorkSchedule schedule = null;
            if (Scene.Original == null)
            {
                if(Agent.Request(out schedule))
                    return schedule.Clone();

                throw new Exception("не удалось получить расписание");
            }

            if (Scene.Original.Negotiator(Agent).Request(out schedule))
                return schedule.Clone();

            throw new Exception("не удалось получить расписание");

        }


        public override IEnumerable<IScene> Variants(INegotiator respondent)
        {
            if (respondent.State != NegotiatorState.Ready)
                yield break;

            var allocationsVariants = FindAllocations(respondent).Parallel();
            foreach (var allocations in allocationsVariants)
            {
                var branch = Scene.Branch();
                var branchedSelf = branch.Negotiator(Agent);
                var branchedRespondent = branch.Negotiator(respondent.Agent);

                bool accepted;
                if (!branchedRespondent.Ask(allocations, out accepted) || !accepted)
                    continue;

                if (!branchedSelf.Tell(allocations))
                    continue;

                yield return branch;
            }
        }

        private void Allocate(IAllocation[] allocations)
        {
            lock(Schedule)
            {
                Schedule.Allocate(allocations);
                Satisfaction.Δ += allocations.Where(x => x.Instruction == AllocationInstruction.Accept).Sum(x => x.Duration.Value.TotalHours);
            }
        }

        private IEnumerable<IAllocation[]> FindAllocations(INegotiator respondent)
        {
            
            AllocationRequirement[] requirements;
            if (!respondent.Request(out requirements))
                yield break;

            var scheduler = Schedule.Scheduler();
            foreach (var requirement in requirements.Parallel())
            {
                var boundary = requirement.Boundary;
                var amount = requirement.Amount;
                SchedulerKind kind = requirement.Kind;

                var allocations = scheduler.FindAllocation(boundary.Start, boundary.Finish, boundary.TotalDuration, boundary.Duration, amount, kind)
                    .ToArray();

                if(allocations.Length == 0)
                    continue;

                yield return allocations;
            }
        }

        protected override void MergeToOriginal(INegotiator original)
        {
            var negotiator = (ResourceAgentNegotiator) original;
            negotiator.Schedule = Schedule;
            negotiator.Satisfaction = Satisfaction;
        }

        public override NegotiatorState State => NegotiatorState.Ready;

        protected override bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            return Handlers.Ask(this, question, out answer) || base.Ask(question, out answer);
        }

        public override bool Tell<TMessage>(TMessage message)
        {
            return Handlers.Tell(this, message) || base.Tell(message);
        }

        protected override bool Request<TData>(out TData data)
        {
            return Handlers.Request(this, out data) || base.Request(out data);
        }
    }
}