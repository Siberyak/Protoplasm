using System;
using System.Collections.Generic;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        public class Schedule : ISchedule, ICalendarItemsProvider
        {
            public TDuration MinDuration { get; }
            private readonly ICalendarItemsAdapter _adapter;
            private readonly MutableCalendar _available;
            private readonly CalendarItems _allocated;

            public ICalendar Available => _available;
            public ICalendarItemsAdapter Adapter => _adapter;
            public ICalendarItemsProvider Allocated => this;

            public Schedule(ICalendarItemsAdapter adapter, TDuration minDuration)
            {
                MinDuration = minDuration;
                if (adapter == null)
                    throw new ArgumentNullException(nameof(adapter));

                _adapter = adapter;
                _allocated = new CalendarItems(_adapter.Include, _adapter.Exclude, _adapter.ToDebugString);
                _available = new MutableCalendar(_adapter);
            }

            public INode<ICalendarItem> Find(Point<TTime> point)
            {
                return _available.Find(point);
            }

            private Schedule(Schedule original) : this(original._adapter, original.MinDuration)
            {
                var items = original._allocated.DefinedItems();
                foreach (var item in items)
                {
                    Include(item.Left, item.Right, item.Data);
                }
            }

            public ISchedule Clone()
            {
                return new Schedule(this);
            }

            public IEnumerable<CalendarItem> Get(TTime left, TTime right, bool leftIncluded = false, bool rightIcluded = false)
            {
                return Get(Point<TTime>.Left(left, leftIncluded), Point<TTime>.Right(right, rightIcluded));
            }

            public IEnumerable<CalendarItem> Get(Point<TTime> @from, Point<TTime> to)
            {
                var calendarItems = _allocated.Get(@from, to);
                return calendarItems;
            }

            public void Include(Point<TTime> left, Point<TTime> right, TData data)
            {
                _allocated.Include(left, right, data);
                _available.Allocate(left, right, data);
            }

            public void Include(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
            {
                _allocated.Include(left, right, data, leftIncluded, rightIncluded);
                _available.Allocate(left, right, data, leftIncluded, rightIncluded);
            }

            public void Exclude(Point<TTime> left, Point<TTime> right, TData data)
            {
                _allocated.Exclude(left, right, data);
                _available.Deallocate(left, right, data);
            }

            public void Exclude(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
            {
                _allocated.Exclude(left, right, data, leftIncluded, rightIncluded);
                _available.Deallocate(left, right, data, leftIncluded, rightIncluded);
            }

            public CalendarItem[] DefinedItems()
            {
                return _allocated.DefinedItems();
            }
        }

        internal sealed class MutableCalendar : Calendar
        {
            public MutableCalendar(ICalendarItemsAdapter adapter) : base(adapter)
            {
            }

            public void Allocate(Point<TTime> left, Point<TTime> right, TData data)
            {
                Define(left, right);
                Items.Include(left, right, data);
            }

            public void Deallocate(Point<TTime> left, Point<TTime> right, TData data)
            {
                Define(left, right);
                Items.Exclude(left, right, data);
            }

            public void Deallocate(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
            {
                Deallocate(Point<TTime>.Left(left, leftIncluded), Point<TTime>.Right(right, rightIncluded), data);
            }

            public void Allocate(TTime? left, TTime? right, TData data, bool leftIncluded = true, bool rightIncluded = true)
            {
                Allocate(Point<TTime>.Left(left, leftIncluded), Point<TTime>.Right(right, rightIncluded), data);
            }
        }
    }
}