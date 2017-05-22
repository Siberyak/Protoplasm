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
        public ResourceAgent CreateResource(string caption, IReadOnlyCollection<Competence> competences, params ResourcesGroup[] memberOf)
        {
            var department = new Resource(caption, competences, memberOf);
            var agent = new ResourceAgent(this, department);
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
    }
}