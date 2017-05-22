using System;
using Protoplasm.Calendars;
using Protoplasm.Utils;

namespace Application1.Data
{
    public class WorkScheduler : Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.Scheduler<WorkItemAmount>
    {
        public WorkScheduler
            (
            Calendars<DateTime, TimeSpan, WorkItemSlotCollection>.ISchedule schedule, 
            RequestInstructionsDelegate requestInstructions, 
            RequestAmountDelegate requestAmount,
            RequestDurationByAmountDelegate requestDurationByAmount
            , RequestDataForAllocateDelegate requestDataForAllocateDelegate
            )
            : base
                (
                schedule,
                requestInstructions,
                requestAmount,
                requestDurationByAmount
                //, requestDataForAllocateDelegate
                )
        {
        }
    }
}