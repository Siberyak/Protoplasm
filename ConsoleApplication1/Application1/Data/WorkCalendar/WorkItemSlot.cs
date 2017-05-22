namespace Application1.Data
{
    public class WorkItemSlot
    {
        public WorkItemSlot(WorkItem workItem, double grantedProductivity)
        {
            WorkItem = workItem;
            GrantedProductivity = grantedProductivity;
        }

        public WorkItem WorkItem { get; }

        public double GrantedProductivity { get; }
    }
}