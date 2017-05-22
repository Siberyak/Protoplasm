using System;
using Protoplasm.Calendars;

namespace Application1.Data
{
    public class WorkCalendar : WorkCalendar<WorkItemSlotCollection>
    {

        static WorkCalendar()
        {
            if(Calendars<DateTime, TimeSpan>.OffsetToLeft == null)
            {
                Calendars<DateTime, TimeSpan>.OffsetToLeft = (dt, span) => dt?.Add(-(span ?? TimeSpan.Zero));
                Calendars<DateTime, TimeSpan>.OffsetToRight = (dt, span) => dt?.Add(span ?? TimeSpan.Zero);

                Calendars<DateTime, TimeSpan>.AddDuration = (a, b) => a + b;
                Calendars<DateTime, TimeSpan>.SubstDuration = (a, b) => a - b;

                Calendars<DateTime, TimeSpan>.ToDuration = (a, b) => b - a;
            }
        }

        public WorkCalendar(Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.ICalendarItemsAdapter adapter) : base(adapter)
        {
        }

        protected override WorkItemSlotCollection Amount(int days, WorkItemSlotCollection data)
        {
            throw new NotImplementedException();
        }
    }
}