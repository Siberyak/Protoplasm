using System.Collections.Generic;

namespace ProcessResearch
{
    public class ProcessInfo : OperationInfo
    {
        public List<OperationInfo> Operations { get; } = new List<OperationInfo>();
    }
}