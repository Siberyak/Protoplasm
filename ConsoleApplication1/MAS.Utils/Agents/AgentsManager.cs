using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using MAS.Core;
using MAS.Core.Compatibility;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using Protoplasm.Utils;

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

        protected virtual void CollectCompatibilites(IAgent agent)
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

        public virtual IEnumerable<IHoldersCompatibilityInfo> RegisterCompatibility(IAbilitiesHolder abilities)
        {
            return GetRequirementsHolders().Select(requirements => Compatible(abilities, requirements)).ToArray();
        }

        public virtual IEnumerable<IHoldersCompatibilityInfo> RegisterCompatibility(IRequirementsHolder requirements)
        {
            return GetAbilitiesHolders().Select(abilities => Compatible(abilities, requirements)).ToArray();
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

        protected IHoldersCompatibilityInfo Generate(IRequirementsHolder requirements, IAbilitiesHolder abilities)
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
                
                // добавляем
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

            var current = scene;

            var holders = GetRequirementsHolders();
            foreach (var holder in holders)
            {
                var success = holder.TrySatisfy(scene, out result, kind);
                if (success)
                    current = result;
            }

            result = current;
            return current != scene;


            return GetRequirementsHolders()
                .TrySatisfy(scene, out result, kind);
        }

        public IEnumerable<IScene> RequirementsSatisfactionVariants(IScene scene)
        {
            return GetRequirementsHolders()
                .SelectMany(resourceAgent => resourceAgent.SatisfactionVariants(scene));
        }

        public IScene SAbilities(IScene scene, VariantKind kind)
        {
            var scenes = SatisfyAbilities(scene);
            var result = Best(scenes, kind);
            return result;
        }

        private IScene Best(IEnumerable<IScene> scenes, VariantKind kind)
        {
            ConsoleExtender.WriteLine("-= checking variants =-");
            ConsoleExtender.WriteLine("");
            ConsoleExtender.WriteLine($"kind: {kind}");

            ConsoleExtender.WriteLine("");
            var timeHolder = ConsoleExtender.Holder.Get("");
            ConsoleExtender.WriteLine("");
            var variantsHolder = ConsoleExtender.Holder.Get("");
            var bestHolder = ConsoleExtender.Holder.Get("");
            var currentHolder = ConsoleExtender.Holder.Get("");

            var stopwatch = Stopwatch.StartNew();

            var cnt = 0;
            IScene best = null;
            foreach (var scene in scenes)
            {
                var elapsed = stopwatch.Elapsed;
                timeHolder.SetValue($"time left: {elapsed.ToString(@"hh\:mm\:ss")}        ");

                var persec = Math.Round(cnt++ / elapsed.TotalSeconds, 1);
                variantsHolder.SetValue($"checked variants: {cnt}, performance: {persec} per sec.         ");
                currentHolder.SetValue($"\t current: {scene}        ");
                if (best == null || scene.Satisfaction.CompareTo(best.Satisfaction) > 0)
                {
                    best = scene;
                    bestHolder.SetValue($"\t    best: {best}       ");

                    if (kind == VariantKind.First)
                        break;
                }
            }
            ConsoleExtender.WriteLine("-= done =-");
            return best;
        }

        public IEnumerable<IScene> SatisfyAbilities(IScene scene)
        {

            var holders = GetAbilitiesHolders().Parallel();

            var variants = holders.SelectMany(holder => holder.SatisfactionVariants(scene, false).Parallel()).Parallel();
            foreach (var first in variants)
            {
                var any = false;
                foreach (var second in SatisfyAbilities(first).Parallel())
                {
                    any = true;
                    yield return second;
                }

                if (!any)
                    yield return first;
            }
        }


        public bool TrySatisfyAbilities(IScene scene, out IScene result, VariantKind kind)
        {
            var current = scene;

            var holders = GetAbilitiesHolders();
            foreach (var holder in holders)
            {
                var success = holder.TrySatisfy(scene, out result, kind);
                if (success)
                    current = result;
            }

            result = current;
            return current != scene;
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