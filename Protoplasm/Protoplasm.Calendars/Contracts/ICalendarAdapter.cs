using Protoplasm.PointedIntervals;

namespace Protoplasm.Calendars
{
    public static partial class Calendars<TTime, TDuration, TData>
    {

        public interface ICalendarAdapter : ICalendarItemsAdapter//: ICalendarItemsAdapter
        {
            IAbstractCalendar BaseCalendar { get; }

            void Define(ICalendarItems container, Point<TTime> left, Point<TTime> right);
        }
    }
}