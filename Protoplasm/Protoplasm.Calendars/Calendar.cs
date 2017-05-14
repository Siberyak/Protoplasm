using System;
using System.Collections.Generic;
using System.Linq;
using Protoplasm.PointedIntervals;
using Protoplasm.Utils;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        private class Translator : CalendarAdapter<TData>
        {
            public Translator(ICalendar baseCalendar) : base(baseCalendar)
            {
            }

            protected override Point<TTime> DefineByBaseData(ICalendarItems container, Point<TTime> left, Point<TTime> right, TData baseData)
            {
                container.Include(left, right, baseData);
                return right.AsLeft();
            }

            public override TData Include(TData a, TData b)
            {
                return _baseCalendar.Adapter.Include(a, b);
            }

            public override TData Exclude(TData a, TData b)
            {
                return _baseCalendar.Adapter.Exclude(a, b);
            }
        }
        public class Calendar : ICalendar
        {
            public ICalendarItemsAdapter Adapter { get; }
            public ICalendarAdapter CalendarAdapter => Adapter as ICalendarAdapter;
            public ISchedule CreateSchedule(TDuration epsilon)
            {
                return new Schedule(new Translator(this), epsilon);
            }

            private readonly CalendarItems _calendarItems;
            protected ICalendarItems Items => _calendarItems;

            public Calendar(ICalendarItemsAdapter adapter)
            {
                Adapter = adapter;

                // проверить на закольцовывание... на вс€кий случай....
                if (CalendarAdapter?.BaseCalendar?.FullChain().Contains(this) == true)
                    throw new ArgumentException("try to loop detected", nameof(Adapter));

                _calendarItems = new CalendarItems(Adapter.Include, Adapter.Exclude, Adapter.ToDebugString);
            }

            public IAbstractCalendar[] FullChain()
            {
                var chain = new IAbstractCalendar[] { this };

                if (CalendarAdapter?.BaseCalendar != null)
                    chain = CalendarAdapter.BaseCalendar.FullChain().Union(chain).ToArray();

                return chain;
            }

            public IEnumerable<CalendarItem> Get(TTime left, TTime right, bool leftIncluded = false, bool rightIcluded = false)
            {
                return Get(Point<TTime>.Left(left, leftIncluded), Point<TTime>.Right(right, rightIcluded));
            }

            public virtual IEnumerable<CalendarItem> Get(Point<TTime> left, Point<TTime> right)
            {
                Define(left, right);

                var calendarItems = _calendarItems.Get(left, right);
                return calendarItems;
            }


            public INode<ICalendarItem> Find(Point<TTime> point)
            {
                var node = _calendarItems.Find(point);

                if (!NodeIsUndefined(node) || CalendarAdapter == null)
                    return node;
                
                Define(point.AsLeft(true), point.AsRight(true));
                node = _calendarItems.Find(point);

                return node;
            }

            protected static bool NodeIsUndefined(INode<ICalendarItem> node)
            {
                return node?.Value == null || Equals(node.Value.Data, default(TData));
            }

            protected virtual void Define(Point<TTime> left, Point<TTime> right)
            {
                if (CalendarAdapter == null)
                    return;

                var node = _calendarItems.FindNode(x => x.Contains(left));

                while (node != null && node.Value.Left <= right)
                {
                    if (NodeIsUndefined(node))
                    {
                        var prev = node.Previous;
                        var prevprev = prev?.Previous;

                        var undefinedInterval = node.Value.Intersect(left, right);
                        Define(undefinedInterval);

                        if (prev?.Alive != true)
                            prev = prevprev;

                        node = (prev ?? _calendarItems.FirstNode).Next;
                        if (Equals(node.Value.Data, default(TData)))
                            node = node.Next;
                    }

                    node = node?.Next;
                }
            }

            private void Define(ICalendarItem undefinedInterval)
            {
                if (undefinedInterval == null)
                    throw new ArgumentNullException(nameof(undefinedInterval));
                if (CalendarAdapter == null)
                    throw new NotSupportedException();

                CalendarAdapter.Define(_calendarItems, undefinedInterval.Left, undefinedInterval.Right);
            }

            public CalendarItem[] DefinedItems()
            {
                return _calendarItems.DefinedItems();
            }

            public override string ToString()
            {
                return $"{GetType().TypeName()}";
            }
        }

    }
}