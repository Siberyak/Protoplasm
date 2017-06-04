using System;
using System.Collections.Generic;
using System.ComponentModel;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;

namespace Application1.Data
{
    public class WorkItem : Entity
    {
        private readonly List<WorkItemBoundaryInfo> _boundaryInfos;
        private readonly List<ResourceRequirementInfo> _resourceRequirementInfos;

        public WorkItem(string caption, int priority) : base(caption)
        {
            _boundaryInfos = new List<WorkItemBoundaryInfo>();
            _resourceRequirementInfos = new List<ResourceRequirementInfo>();

            Priority = priority;
        }
        public int Priority { get; }

        private bool _locked;

        public void Lock()
        {
            _locked = true;
        }

        public void Add(WorkItemBoundaryInfo item)
        {
            if(_locked)
                throw new Exception("Locked!");
            _boundaryInfos.Add(item);
        }

        public void AddRange(IEnumerable<WorkItemBoundaryInfo> collection)
        {
            if (_locked)
                throw new Exception("Locked!");
            _boundaryInfos.AddRange(collection);
        }

        public void Add(ResourceRequirementInfo item)
        {
            if (_locked)
                throw new Exception("Locked!");
            _resourceRequirementInfos.Add(item);
        }

        public void AddRange(IEnumerable<ResourceRequirementInfo> collection)
        {
            if (_locked)
                throw new Exception("Locked!");
            _resourceRequirementInfos.AddRange(collection);
        }

        //public Boundary Boundary => _boundaryRequirement.Boundary;
        //public WorkItemAmount RequiredAmount => new WorkItemAmount(this, 8, TimeSpan.FromHours(1), false, false);


        

        protected override IReadOnlyCollection<Requirement> GenerateRequirements()
        {
            var requirements = new List<Requirement>();
            foreach (var boundaryInfo in _boundaryInfos)
            {
                foreach (var requirementInfo in _resourceRequirementInfos)
                {
                    requirements.Add(new WorkItemRequirementsPart(boundaryInfo, requirementInfo));
                }
            }

            return requirements.ToArray();

            return new Requirement[]
            {
                //_boundaryRequirement,
                //_competencesRequirement,
            };
        }
    }

    class WorkItemRequirementsPart : Requirement
    {
        private WorkItemBoundaryInfo _boundaryInfo;
        private ResourceRequirementInfo _requirementInfo;

        public WorkItemRequirementsPart(WorkItemBoundaryInfo boundaryInfo, ResourceRequirementInfo requirementInfo)
        {
            _boundaryInfo = boundaryInfo;
            _requirementInfo = requirementInfo;
        }

        public override CompatibilityType Compatible(IAbility ability)
        {
            throw new NotImplementedException();
        }

        public override bool Compatible(IAbility ability, IScene scene)
        {
            throw new NotImplementedException();
        }

        public IRequirementsHolder AsRequirementsHolder(IAgentsManager manager)
        {
            var boundary = Boundary.Empty;
            boundary = boundary
                .SetStart(_boundaryInfo.Start)
                .SetFinish(_boundaryInfo.Finish)
                .SetTotalDuration(_requirementInfo.TotalDuration)
                .SetDuration(_requirementInfo.Duration);

            var requirements = new []
            {
                new BoundaryRequirement(boundary),
                _requirementInfo.Resource != null
                    ? (IRequirement)new ResourceRequirement(_requirementInfo.Resource)
                    : new CompetencesRequirement(_requirementInfo.Competences)
            };

            return new WorkItemPartAgent(manager, requirements);
        }

        [DisplayName(@"WIPart-Агент")]
        class WorkItemPartAgent : ManagedAgent
        {
            public WorkItemPartAgent(IAgentsManager manager, IReadOnlyCollection<IRequirement> requirements) : base(manager)
            {
                Requirements = requirements;
            }

            public override IReadOnlyCollection<IRequirement> Requirements { get; }
            public override IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities)
            {
                throw new NotImplementedException();
            }

            public override bool Compatible(IAbilitiesHolder abilities, IScene scene)
            {
                throw new NotImplementedException();
            }

            public override IRequirement ToScene(IRequirement requirement)
            {
                return requirement;
            }

            public override IReadOnlyCollection<IAbility> Abilities { get; } = new IAbility[0];
            public override IHoldersCompatibilityInfo Compatible(IRequirementsHolder requirements)
            {
                throw new NotImplementedException();
            }

            public override bool Compatible(IRequirementsHolder requirements, IScene scene)
            {
                throw new NotImplementedException();
            }

            public override IAbility ToScene(IAbility ability)
            {
                throw new NotImplementedException();
            }

            public override void Initialize()
            {
                throw new NotImplementedException();
            }

            protected override INegotiator Negotiator(IScene scene)
            {
                throw new NotImplementedException();
            }
        }
    }
}