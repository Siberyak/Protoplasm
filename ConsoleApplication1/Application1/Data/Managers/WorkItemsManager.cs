using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Utils;

namespace Application1.Data
{
    public class WorkItemsManager : AgentsManager
    {
        public WorkItemAgent CreateWorkItemAgent(string caption, IReadOnlyCollection<Competence> competences)
        {
            var workItem = new WorkItem(caption, competences);
            var agent = new WorkItemAgent(this, workItem);
            return Initialize(agent);
        }

        protected override IEnumerable<IAbilitiesHolder> GetAbilitiesHolders()
        {
            return Enumerable.Empty<IAbilitiesHolder>();
        }

        protected override IEnumerable<IRequirementsHolder> GetRequirementsHolders()
        {
            return Agents;
        }

    }
}