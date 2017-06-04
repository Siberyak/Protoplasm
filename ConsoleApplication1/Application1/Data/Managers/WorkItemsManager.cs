using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;
using MAS.Utils;
using Protoplasm.Calendars;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;
using IAllocation = Protoplasm.Calendars.Calendars<System.DateTime, System.TimeSpan, Application1.Data.WorkItemSlotCollection>.Scheduler<Application1.Data.WorkItemAmount>.IAllocation;

namespace Application1.Data
{
    public class WorkItemsManager : AgentsManager
    {
        private readonly List<WorkItemsDependencyCollection> _dependencies = new List<WorkItemsDependencyCollection>();

        public WorkItemAgent Get(WorkItem workItem)
        {
            if (workItem == null)
                return null;

            return Agents.OfType<WorkItemAgent>().FirstOrDefault(x => x.Entity == workItem)
                   ?? CreateWorkItemAgent(workItem);

        }

        public WorkItem CreateWorkItem(string caption, int priority)
        {
            var workItem = new WorkItem(caption, priority);
            return workItem;
        }

        private WorkItemAgent CreateWorkItemAgent(WorkItem workItem)
        {
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

        public void Show(Scene scene)
        {
            foreach (var agent in Agents)
            {
                var negotiator = scene.Negotiator(agent);
                var state = negotiator.State;
                
                $"{negotiator.Satisfaction, -20}".Write(state == NegotiatorState.Satisfied ? ConsoleColor.Green : state == NegotiatorState.Ready ? ConsoleColor.Yellow : ConsoleColor.Blue);
                $"| {agent, -40}".Write();
                if (state == NegotiatorState.Satisfied)
                {
                    IAllocation[] allocation;
                    negotiator.Request(out allocation);
                    var left = allocation.Min(x => x.Left);
                    var right = allocation.Max(x => x.Right);
                    $"| {left.Interval(right)}".Write();
                }

                "".WriteLine();
            }
        }

        public WorkItemsDependencyInjector DependencyInjector => new WorkItemsDependencyInjector(_dependencies);

        public class WorkItemsDependencyInjector
        {
            readonly List<WorkItemsDependencyCollection> _collection;

            public WorkItemsDependencyInjector(List<WorkItemsDependencyCollection> collection)
            {
                _collection = collection;
            }

            private WorkItem _primary;
            private WorkItemsInterval _primaryInterval;
            private WorkItem _secondary;
            private WorkItemsInterval _secondaryInterval;
            private Interval<TimeSpan> _delay = new Interval<TimeSpan>();

            public WorkItemsDependencyInjector Primary(WorkItem workItem, WorkItemsInterval interval)
            {
                _primary = workItem;
                _primaryInterval = interval;
                return this;
            }
            public WorkItemsDependencyInjector Secondary(WorkItem workItem, WorkItemsInterval interval)
            {
                _secondary = workItem;
                _secondaryInterval = interval;
                return this;
            }
            public WorkItemsDependencyInjector MinDelay(TimeSpan? value)
            {
                _delay = Point<TimeSpan>.Left(value).Interval(_delay.Right);
                return this;
            }
            public WorkItemsDependencyInjector MaxDelay(TimeSpan? value)
            {
                _delay = _delay.Left.Interval(Point<TimeSpan>.Right(value));
                return this;
            }

            public WorkItemsDependencyInjector After()
            {
                return MinDelay(TimeSpan.Zero);
            }
            public WorkItemsDependencyInjector Before()
            {
                return MaxDelay(TimeSpan.Zero);
            }

            public WorkItemsDependencyInjector Reset()
            {
                _primary = null;
                _secondary = null;
                _delay = Interval<TimeSpan>.Undefined;
                return this;
            }

            public bool Inject()
            {
                lock (_collection)
                {
                    if (Validate() != ValidationResult.Valid)
                        return false;

                    var dep = _collection.FirstOrDefault(x => x.Primary == _primary && x.Secondary == _secondary);
                    if (dep == null)
                        _collection.Add(dep = new WorkItemsDependencyCollection(_primary, _secondary));

                    dep.Add(_secondaryInterval, _delay, _primaryInterval);

                    return true;
                }
            }

            public ValidationResult Validate()
            {
                lock (_collection)
                {
                    var result = ValidationResult.Valid;
                    if (_delay.IsUndefined)
                        result |= ValidationResult.DelayNotSet;
                    if (_primary == null)
                        result |= ValidationResult.PrimaryNotSet;
                    if (_secondary == null)
                        result |= ValidationResult.SecondaryNotSet;
                    if (_secondary == _primary)
                        result |= ValidationResult.PrimaryEqualsSecondary;

                    //if (result == ValidationResult.Valid)
                    //{
                    //    var dep = _collection.FirstOrDefault(x => x.Primary == _primary && x.Secondary == _secondary);
                    //    if (dep?.Contains(_secondaryInterval, _primaryInterval) == true)
                    //        result |= ValidationResult.Exists;
                    //}

                    return result;
                }
            }

            [Flags]
            public enum ValidationResult
            {
                Valid = 0,
                PrimaryNotSet = 1,
                PrimaryEqualsSecondary = 2,
                SecondaryNotSet = 4,
                DelayNotSet = 8,
                Exists = 16
            }

        }

        public NegotiatorState DependenciesState(IScene scene, WorkItem workItem, SchedulerKind kind)
        {
            IEnumerable<WorkItem> wis;
            switch (kind)
            {
                case SchedulerKind.LeftToRight:
                    {
                        wis = _dependencies.Where(x => x.Secondary == workItem).Select(x => x.Primary);
                        break;
                    }
                case SchedulerKind.RightToLeft:
                    {
                        wis = _dependencies.Where(x => x.Primary == workItem).Select(x => x.Secondary);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }


            var agents = wis.Select(x => Agents.OfType<IEntityAgent<WorkItem>>().First(a => a.Entity == x)).ToArray();
            var negotiators = agents.Select(scene.Negotiator).ToArray();
            var states = negotiators.Select(n => n.State).ToArray();

            if (states.Length == 0)
                return NegotiatorState.Ready;

            return states.All(x => x == NegotiatorState.Satisfied) ? NegotiatorState.Ready : NegotiatorState.NotReady;
        }

        public Boundary[] DependenciesBoundaries(IScene scene, WorkItem workItem, SchedulerKind kind)
        {
            var agents = Agents.OfType<IEntityAgent<WorkItem>>().ToArray();

            IEnumerable<WorkItemsDependencyCollection> deps;
            Func<WorkItemsDependencyCollection, WorkItem> getWorkItem;
            Func<WorkItemsDependencyCollection, Interval<DateTime>, Boundary[]> getBoundary;
            switch (kind)
            {
                case SchedulerKind.LeftToRight:
                        deps = _dependencies.Where(x => x.Secondary == workItem);
                        getWorkItem = x => x.Primary;
                        getBoundary = (x, interval) => x.BySecondary(interval);
                        break;
                case SchedulerKind.RightToLeft:
                        deps = _dependencies.Where(x => x.Primary == workItem);
                        getWorkItem = x => x.Secondary;
                        getBoundary = (x, interval) => x.ByPrimary(interval);
                        break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }

            var result = new List<Boundary>();
            foreach (var dep in deps)
            {
                var wi = getWorkItem(dep);
                var agent = agents.First(x => x.Entity == wi);
                IAllocation[] allocations;
                scene.Negotiator(agent).Request(out allocations);
                var left = allocations.Min(x => x.Left);
                var right = allocations.Max(x => x.Right);
                var interval = left.Interval(right);
                var boundaries = getBoundary(dep, interval);
                result.AddRange(boundaries);
            }

            return result.ToArray();
        }
    }
}