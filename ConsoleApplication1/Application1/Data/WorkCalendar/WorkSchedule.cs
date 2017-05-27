using System;
using System.Collections.Generic;
using Protoplasm.Calendars;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class WorkSchedule
    {
        readonly Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.ISchedule _schedule;

        public WorkSchedule(Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.ISchedule schedule)
        {
            _schedule = schedule;
        }

        public WorkSchedule Clone()
        {
            lock (this)
            {
                return new WorkSchedule(_schedule.Clone());
            }
        }


        public WorkScheduler Scheduler()
        {
            lock (this)
            {
                return new WorkScheduler
                    (
                    _schedule,
                    WorkItemAmount.ResponseInstruction,
                    WorkItemAmount.ResponseAmount,
                    WorkItemAmount.ResponseDurationByAmount,
                    WorkItemAmount.ResponseDataForAllocate
                    );
            }
        }

        private void Allocate(Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.Scheduler<WorkItemAmount>.IAllocation allocation)
        {
            if(allocation.Instruction != AllocationInstruction.Accept)
                return;
            
            var data = WorkItemAmount.ResponseDataForAllocate(allocation.Left.Interval(allocation.Right), allocation.CalendarData, allocation.Amount.Value);
            _schedule.Include(allocation.Left, allocation.Right, data);
        }

        public void Allocate(IEnumerable<Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.Scheduler<WorkItemAmount>.IAllocation> allocations)
        {
            lock (this)
            {
                foreach (var allocation in allocations)
                {
                    Allocate(allocation);
                }
            }
        }
    }
}