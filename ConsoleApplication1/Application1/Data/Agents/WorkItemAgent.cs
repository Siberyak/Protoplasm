using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.Calendars;

namespace Application1.Data
{
    [DisplayName("WI-Агент")]
    public class WorkItemAgent : ManagedEntityAgent<WorkItem>, IRequirementsHolder
    {
        static readonly HandlersStorage<WorkItemAgent> Handlers = new HandlersStorage<WorkItemAgent>();

        static WorkItemAgent()
        {
            //Handlers.Requested(agent => agent.Boundary);
            //Handlers.Requested(agent => agent.RequiredAmount);
            Handlers.Asked<DependenciesRequest, NegotiatorState>((agent, request) => agent.DependenciesState(request.Scene));
            Handlers.Asked<DependenciesRequest, Boundary[]>((agent, request) => agent.DependenciesBoundaries(request.Scene));
        }

        private Boundary[] DependenciesBoundaries(IScene scene)
        {
            return WorkItemsManager.DependenciesBoundaries(scene, Entity, Kind);
        }

        private NegotiatorState DependenciesState(IScene scene)
        {
            return WorkItemsManager.DependenciesState(scene, Entity, Kind);
        }


        //public Boundary Boundary => Entity.Boundary;
        //public WorkItemAmount RequiredAmount => Entity.RequiredAmount;
        public SchedulerKind Kind { get; set; }

        public int Priority => Entity.Priority;

        public WorkItemAgent(WorkItemsManager manager, WorkItem entity) : base(manager, entity)
        {
        }

        WorkItemsManager WorkItemsManager => (WorkItemsManager) Manager;

        public override void Initialize()
        {
            Entity.Lock();
        }

        protected override INegotiator Negotiator(IScene scene)
        {
            return new WorkItemAgentNegotiator(scene, this);
        }

        public override bool Request<TData>(out TData data)
        {
            return Handlers.Request(this, out data) || base.Request(out data);
        }

        public override bool Ask<TQuestion, TAnswer>(TQuestion question, out TAnswer answer)
        {
            return Handlers.Ask(this, question, out answer);
        }
    }
}