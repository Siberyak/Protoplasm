using System;

namespace ConsoleApplication1
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ObfuscateDecimalAttribute : ObfuscateAttribute
    {
        protected override object GetObfuscatedValue(ObfuscationSequenceAttribute sequence, object data, object originalValue)
        {
            if (!(originalValue is decimal))
                return originalValue;

            return ((decimal)originalValue + 100) * 397 ;
        }
    }
}