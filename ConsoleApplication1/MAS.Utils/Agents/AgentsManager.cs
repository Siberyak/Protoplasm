using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Utils
{
    public abstract class AgentsManager : IAgentsManager
    {
        readonly List<IAgent> _agents = new List<IAgent>();
        protected readonly List<IAgentsManager> AbilitiesManagers = new List<IAgentsManager>();
        protected readonly List<IAgentsManager> RequirementsManagers = new List<IAgentsManager>();

        protected TAgent Initialize<TAgent>(TAgent agent)
            where TAgent : IManagedAgent
        {
            agent.Initialize();
            Add(agent);
            return agent;
        }

        protected void Add(IAgent agent)
        {
            if(!Add(_agents, agent))
                return;

            CollectCompatibilites(agent);
        }

        private void CollectCompatibilites(IAgent agent)
        {
            foreach (var manager in AbilitiesManagers)
            {
                manager.RegisterCompatibility((IRequirementsHolder)agent);
            }

            foreach (var manager in RequirementsManagers)
            {
                manager.RegisterCompatibility((IAbilitiesHolder)agent);
            }
        }

        public void RegisterCompatibility(IAbilitiesHolder abilities)
        {
            foreach (var requirements in GetRequirementsHolders())
            {
                Compatible(abilities, requirements);
            }
        }

        public void RegisterCompatibility(IRequirementsHolder requirements)
        {
            foreach (var abilities in GetAbilitiesHolders())
            {
                Compatible(abilities, requirements);
            }
        }

        public IEnumerable<IAgent> Agents => _agents.ToArray();

        private readonly List<IHoldersCompatibilityInfo> _compatibilities = new List<IHoldersCompatibilityInfo>();

        public IHoldersCompatibilityInfo Compatible(IAbilitiesHolder abilities, IRequirementsHolder requirements)
        {
            return Find(requirements, abilities)
                   ?? Generate(requirements, abilities);
        }

        public IHoldersCompatibilityInfo Find(IRequirementsHolder requirements, IAbilitiesHolder abilities)
        {
            return _compatibilities.FirstOrDefault(x => x.RequiremenetsHolder == requirements && x.AbilitiesHolder == abilities);
        }

        private IHoldersCompatibilityInfo Generate(IRequirementsHolder requirements, IAbilitiesHolder abilities)
        {
            lock (_compatibilities)
            {
                var info = Find(requirements, abilities);

                if (info != null)
                    return info;

                var requirementsAgentsManager = GetAgentsManager(requirements);
                var abilitiesAgentsManager = GetAgentsManager(abilities);

                // если кто-то из ходеров связан с IAgemtsManager, то вернет нам info-шку (или она уже есть, или создадут)
                // если нет - создаем  сами
                info = requirementsAgentsManager?.Add(requirements, abilities, null)
                       ?? abilitiesAgentsManager?.Add(requirements, abilities, null) 
                       ?? requirements.Compatibility(abilities);
                
                // ничего не нашли
                _compatibilities.Add(info);

                // освежаем, если надо
                requirementsAgentsManager?.Add(requirements, abilities, info);
                abilitiesAgentsManager?.Add(requirements, abilities, info);

                return info;
            }
        }

        IAgentsManager GetAgentsManager(object candidate)
        {
            var manager = candidate as IAgentsManager ?? (candidate as IManagedAgent)?.Manager;
            if (manager == this)
                manager = null;

            return manager;
        }

        IHoldersCompatibilityInfo IAgentsManager.Add(IRequirementsHolder requirements, IAbilitiesHolder abilities, IHoldersCompatibilityInfo info)
        {
            return Add(requirements, abilities, info);
        }

        protected IHoldersCompatibilityInfo Add(IRequirementsHolder requirements, IAbilitiesHolder abilities, IHoldersCompatibilityInfo info)
        {
            lock (_compatibilities)
            {
                // ищем у себя
                var local = Find(requirements, abilities);

                // есть и там и тут
                if (info != null && local != null)
                {
                    if (ReferenceEquals(info, local))
                        return info;

                    // заменяем тут
                    _compatibilities.Remove(local);
                    _compatibilities.Add(info);
                    return info;
                }

                // там есть, тут нету
                if (local == null && info != null)
                {
                    // добавляем сюда
                    _compatibilities.Add(info);
                    return info;
                }

                // есть тут (а там нету)
                if (local != null)
                    return local;

                // нет ни тут, и там
                local = requirements.Compatibility(abilities);
                _compatibilities.Add(local);
                return local;
            }
        }

        public IEnumerable<IHoldersCompatibilityInfo> this[IRequirementsHolder requirements]
        {
            get { return _compatibilities.Where(x => x.RequiremenetsHolder == requirements).ToArray(); }
        }

        protected abstract IEnumerable<IAbilitiesHolder> GetAbilitiesHolders();
        protected abstract IEnumerable<IRequirementsHolder> GetRequirementsHolders();

        public bool TrySatisfyRequirements(IScene scene, out IScene result, VariantKind kind)
        {
            return GetRequirementsHolders()
                .TrySatisfy(scene, out result, kind);
        }

        public IEnumerable<IScene> RequirementsSatisfactionVariants(IScene scene)
        {
            return GetRequirementsHolders()
                .SelectMany(resourceAgent => resourceAgent.SatisfactionVariants(scene));
        }

        public bool TrySatisfyAbilities(IScene scene, out IScene result, VariantKind kind)
        {
            return GetAbilitiesHolders()
                .TrySatisfy(scene, out result, kind);
        }

        public IEnumerable<IScene> AbilitiesSatisfactionVariants(IScene scene)
        {
            return GetAbilitiesHolders()
                .SelectMany(resourceAgent => resourceAgent.SatisfactionVariants(scene));
        }

        public void AddAbilitiesManager(IAgentsManager manager)
        {
            Add(AbilitiesManagers, manager);
        }
        public void AddRequirementsManager(IAgentsManager manager)
        {
            Add(RequirementsManagers, manager);
        }

        private static bool Add<T>(ICollection<T> list, T data)
            where T : class
        {
            if (data == null || list.Contains(data))
                return false;

            list.Add(data);
            return true;
        }

        public IEnumerator<IHoldersCompatibilityInfo> GetEnumerator()
        {
            return _compatibilities.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}