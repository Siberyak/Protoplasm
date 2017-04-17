using System;

namespace ConsoleApplication1
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ObfuscationSequenceAttribute : Attribute
    {
        public long Current { get; set; }

        public string DisplayName { get; set; }

        public virtual string DataDisplayName(object data)
        {
            return DisplayName ?? data?.GetType().DisplayName();
        }
    }
}