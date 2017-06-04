using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class ResourcesManager : AgentsManager
    {
        public ResourceAgent Get(Resource resource)
        {
            if (resource == null)
                return null;

            return Agents.OfType<ResourceAgent>().FirstOrDefault(x => x.Entity == resource)
                   ?? CreateResourceAgent(resource);
        }

        public Resource CreateResource(string caption, IReadOnlyCollection<Competence> competences, bool createAgent = false)
        {
            var memberOf = new ResourcesGroup[0];
            var resource = new Resource(caption, competences, memberOf);
            if (createAgent)
                CreateResourceAgent(resource);
            return resource;
        }

        private ResourceAgent CreateResourceAgent(Resource resource)
        {
            var agent = new ResourceAgent(this, resource);
            return Initialize(agent);
        }

        protected override IEnumerable<IAbilitiesHolder> GetAbilitiesHolders()
        {
            return Agents;
        }

        protected override IEnumerable<IRequirementsHolder> GetRequirementsHolders()
        {
            return Enumerable.Empty<IRequirementsHolder>();
        }

        public void Show(Scene scene)
        {
            foreach (var agent in Agents)
            {
                var negotiator = scene.Negotiator(agent);
                Console.WriteLine($"{negotiator.Satisfaction, -20}| {agent, -40}");
            }
        }
    }
}