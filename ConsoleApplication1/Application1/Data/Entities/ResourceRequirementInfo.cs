using System;
using MAS.Utils;
using Protoplasm.PointedIntervals;

namespace Application1.Data
{
    public class ResourceRequirementInfo
    {
        public Resource Resource;
        public Competences Competences;
        public TimeSpan Offset;
        public Interval<TimeSpan> Duration;
        public Interval<TimeSpan> TotalDuration;

        public ResourceRequirementInfo(Resource resource, Competences competences, TimeSpan offset, Interval<TimeSpan> duration, Interval<TimeSpan> totalDuration)
        {
            Resource = resource;
            Competences = competences;
            Offset = offset;
            Duration = duration;
            TotalDuration = totalDuration;
        }
    }
}