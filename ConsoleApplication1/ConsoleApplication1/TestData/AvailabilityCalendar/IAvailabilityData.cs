namespace ConsoleApplication1.TestData
{
    public partial class PlanningEnvironment<TTime, TDuration>
    {

        public interface IAvailabilityData
        {
            IAvailabilityData Include(IAvailabilityData availabilityData);
            IAvailabilityData Exclude(IAvailabilityData availabilityData);
        }
    }
}