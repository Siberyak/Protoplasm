using System;
using System.Collections.Generic;
using System.Linq;
using MAS.Utils;
using Protoplasm.Calendars;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class WorkItem : Entity
    {
        private readonly BoundaryRequirement _boundaryRequirement;
        private readonly CompetencesRequirement _competencesRequirement;

        public WorkItem(string caption, int priority, IReadOnlyCollection<Competence> competences) : base(caption)
        {
            Priority = priority;
            _competencesRequirement = new CompetencesRequirement(competences);

            var boundary = Boundary.Empty;

            boundary = boundary
                .StartAfter(DateTime.Today)
                .FinishBefore(DateTime.Today.AddHours(24))
                .NotLonger(TimeSpan.FromHours(16))
                ;

            _boundaryRequirement = new BoundaryRequirement(boundary);
        }

        public Boundary Boundary => _boundaryRequirement.Boundary;
        public WorkItemAmount RequiredAmount => new WorkItemAmount(this, 8, TimeSpan.FromHours(1), false, false);

        public int Priority { get; }

        protected override IReadOnlyCollection<Requirement> GenerateRequirements()
        {
            return new Requirement[]
            {
                _boundaryRequirement,
                _competencesRequirement,
            };
        }
    }

    public class WorkItemsDependencyInjector
    {
        List<WorkItemsDependencyCollection> _collection;

        public WorkItemsDependencyInjector(List<WorkItemsDependencyCollection> collection)
        {
            _collection = collection;
        }

        private WorkItem _primary;
        private WorkItemsInterval _primaryInterval;
        private WorkItem _secondary;
        private WorkItemsInterval _secondaryInterval;
        private Interval<TimeSpan> _dealy = new Interval<TimeSpan>();

        public WorkItemsDependencyInjector Reset()
        {
            _primary = null;
            _secondary = null;
            _dealy = new Interval<TimeSpan>();
            return this;
        }

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
            return this;
        }
        public WorkItemsDependencyInjector MaxDelay(TimeSpan? value)
        {
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

                dep.Add(_secondaryInterval, _dealy, _primaryInterval);

                return true;
            }
        }

        public ValidationResult Validate()
        {
            lock (_collection)
            {
                var result = ValidationResult.Valid;
                if (_dealy.IsUndefined)
                    result |= ValidationResult.DelayNotSet;
                if (_primary == null)
                    result |= ValidationResult.PrimaryNotSet;
                if (_secondary == null)
                    result |= ValidationResult.SecondaryNotSet;
                if (_secondary == _primary)
                    result |= ValidationResult.PrimaryEqualsSecondary;

                if (result == ValidationResult.Valid)
                {
                    var dep = _collection.FirstOrDefault(x => x.Primary == _primary && x.Secondary == _secondary);
                    if (dep?.Contains(_secondaryInterval, _primaryInterval) == true)
                        result |= ValidationResult.Exists;
                }

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



    public class WorkItemsDependencyCollection
    {
        private static readonly Func<WorkItemsDependency, WorkItemsInterval, WorkItemsInterval, bool> Predicate =
            (x, primaryInterval, secondaryInterval) =>
                x.PrimaryInterval == primaryInterval
                && x.SecondaryInterval == secondaryInterval;

        private readonly List<WorkItemsDependency> _dependencies;

        public WorkItem Primary { get; set; }
        public WorkItem Secondary { get; set; }

        public WorkItemsDependencyCollection(WorkItem primary, WorkItem secondary)
        {
            Primary = primary;
            Secondary = secondary;
            _dependencies = new List<WorkItemsDependency>();
        }

        public void Add(WorkItemsInterval secondaryInterval, Interval<TimeSpan> delay, WorkItemsInterval primaryInterval)
        {
            lock (_dependencies)
            {
                _dependencies.RemoveAll(x => Predicate(x, primaryInterval, secondaryInterval));
                _dependencies.Add(new WorkItemsDependency(primaryInterval, secondaryInterval, delay));
            }
        }

        public bool Contains(WorkItemsInterval secondaryInterval, WorkItemsInterval primaryInterval)
        {
            lock (_dependencies)
            {
                return _dependencies.Any(x => Predicate(x, primaryInterval, secondaryInterval));
            }
        }


        private static Point<DateTime> ByInterval(WorkItemsInterval kind, Interval<DateTime> interval)
        {
            return kind == WorkItemsInterval.Start ? interval.Left : interval.Right;
        }

        public Boundary[] BySecondary(Interval<DateTime> interval)
        {
            lock (_dependencies)
            {

                var result = new List<Boundary>();
                foreach (var dependency in _dependencies)
                {
                    var boundary = new Boundary();

                    var primaryInterval = dependency.PrimaryInterval;
                    var secondaryInterval = dependency.SecondaryInterval;
                    var delay = dependency.Delay;

                    var point = ByInterval(primaryInterval, interval);

                    switch (secondaryInterval)
                    {
                        case WorkItemsInterval.Start:
                            {
                                if (delay.Min.IsDefined)
                                {
                                    var left = point.OffsetToRight(delay.Min);
                                    boundary = boundary.StartAfter(left.Value(), false);
                                }

                                if (delay.Max.IsDefined)
                                {
                                    var right = point.OffsetToRight(delay.Max);
                                    boundary = boundary.StartBefore(right.Value(), false);
                                }

                            }
                            break;
                        case WorkItemsInterval.Finish:
                            {
                                if (delay.Min.IsDefined)
                                {
                                    var left = point.OffsetToRight(delay.Min);
                                    boundary = boundary.FinishAfter(left.Value(), false);
                                }
                                if (delay.Max.IsDefined)
                                {
                                    var right = point.OffsetToRight(delay.Max);
                                    boundary = boundary.FinishBefore(right.Value(), false);
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    result.Add(boundary);
                }

                return result.ToArray();
            }
        }

        public Boundary[] ByPrimary(Interval<DateTime> interval)
        {
            return new Boundary[0];
        }
    }

    public class WorkItemsDependency
    {
        public WorkItemsDependency(WorkItemsInterval primaryInterval, WorkItemsInterval secondaryInterval, Interval<TimeSpan> delay)
        {
            PrimaryInterval = primaryInterval;
            SecondaryInterval = secondaryInterval;
            Delay = delay;
        }

        public WorkItemsInterval PrimaryInterval { get; set; }
        public WorkItemsInterval SecondaryInterval { get; set; }
        public Interval<TimeSpan> Delay { get; set; }

        public override string ToString()
        {
            return $"prim.{PrimaryInterval} -> {Delay} -> sec.{SecondaryInterval}";
        }
    }

    public enum WorkItemsInterval
    {
        Start,
        Finish,
    }
}