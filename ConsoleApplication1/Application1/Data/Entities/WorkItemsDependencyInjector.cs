using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
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
}