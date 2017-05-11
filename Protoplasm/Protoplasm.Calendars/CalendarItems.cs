using System;
using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {
        internal class CalendarItems : PointedIntervalsContainer<CalendarItem, TTime, TData>, ICalendarItems
        {
            public CalendarItems(Func<TData,TData,TData> includeData, Func<TData, TData, TData> excludeData, Func<TData, string> toDebugString = null)
                : base
                    (
                    (left, right, data) => new CalendarItem(left, right, data),
                    (a, b) => includeData(a, b),
                    (a, b) => excludeData(a, b),
                    toDebugString != null
                        ? data => toDebugString(data)
                        : default(DataToString)
                    )
            {
            }

            public INode<ICalendarItem> Find(Point<TTime> point)
            {
                var node = FindNode(x => x.Contains(point));
                return node;
            }

            void ICalendarItems.Include(Point<TTime> left, Point<TTime> right, TData data)
            {
                base.Include(left, right, data);
            }

            void ICalendarItems.Include(TTime? left, TTime? right, TData data, bool leftIncluded, bool rightIncluded)
            {
                base.Include(left, right, data, leftIncluded, rightIncluded);
            }

            void ICalendarItems.Exclude(Point<TTime> left, Point<TTime> right, TData data)
            {
                base.Exclude(left, right, data);
            }

            void ICalendarItems.Exclude(TTime? left, TTime? right, TData data, bool leftIncluded, bool rightIncluded)
            {
                base.Exclude(left, right, data, leftIncluded, rightIncluded);
            }

        }
    }
}