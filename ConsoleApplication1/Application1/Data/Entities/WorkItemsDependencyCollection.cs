using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.Calendars;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
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
}