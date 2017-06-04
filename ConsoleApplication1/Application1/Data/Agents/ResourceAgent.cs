using System.ComponentModel;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.Calendars;

namespace Application1.Data
{
    [DisplayName("R-Агент")]
    public class ResourceAgent : ManagedEntityAgent<Resource>, IAbilitiesHolder
    {
        static readonly HandlersStorage<ResourceAgent> Handlers = new HandlersStorage<ResourceAgent>();

        static ResourceAgent()
        {
            Handlers.Requested(agent => agent.Entity.Schedule);
        }

        public ResourceAgent(ResourcesManager manager, Resource entity) : base(manager, entity)
        {
            
        }

        public override void Initialize()
        {}

        protected override INegotiator Negotiator(IScene scene)
        {
            return new ResourceAgentNegotiator(scene, this);
        }

        public override bool Request<TData>(out TData data)
        {
            return Handlers.Request(this, out data) || base.Request(out data);
        }
    }
}