using System;
using Protoplasm.Calendars;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class WorkCalendarAdapter : Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.CalendarAdapter
    {
        public override void Define(Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.ICalendarItems container, Point<DateTime> left, Point<DateTime> right)
        {
            container.Include(left, right, new WorkItemSlotCollection(1));
        }

        protected override DateTime Define(Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.ICalendarItems container, DateTime left, DateTime right)
        {
            throw new NotImplementedException();
        }

        public override WorkItemSlotCollection Include(WorkItemSlotCollection a, WorkItemSlotCollection b)
        {
            return a.Include(b);
        }

        public override WorkItemSlotCollection Exclude(WorkItemSlotCollection a, WorkItemSlotCollection b)
        {
            return a.Exclude(b);
        }
    }
}