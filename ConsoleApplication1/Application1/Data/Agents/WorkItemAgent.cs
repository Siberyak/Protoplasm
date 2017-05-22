using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using MAS.Core;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.Calendars;

namespace Application1.Data
{
    [DisplayName("WorkItem-Агент")]
    public class WorkItemAgent : ManagedEntityAgent<WorkItem>
    {
        static readonly HandlersStorage<WorkItemAgent> Handlers = new HandlersStorage<WorkItemAgent>();

        static WorkItemAgent()
        {
            Handlers.Requested(agent => agent.Boundary);
            Handlers.Requested(agent => agent.RequiredAmount);
        }


        private Boundary Boundary => Entity.Boundary;
        public WorkItemAmount RequiredAmount => Entity.RequiredAmount;

        public WorkItemAgent(WorkItemsManager manager, WorkItem entity) : base(manager, entity)
        {
        }


        public override void Initialize()
        {

        }

        protected override INegotiator Negotiator(IScene scene)
        {
            return new WorkItemNegotiator(scene, this);
        }

        public override bool Request<TData>(out TData data)
        {
            return Handlers.Request(this, out data) || base.Request(out data);
        }
    }
}