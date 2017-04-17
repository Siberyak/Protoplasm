using System;
using System.Reflection;

namespace ConsoleApplication1
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ObfuscateAttribute : Attribute
    {
        private PropertyInfo _pi;

        public virtual void Obfuscate(ObfuscationSequenceAttribute sequence, object data)
        {
            var value = GetObfuscatedValue(sequence, data, _pi.GetValue(data));
            _pi.SetValue(data, value);
        }

        protected virtual object GetObfuscatedValue(ObfuscationSequenceAttribute sequence, object data, object originalValue)
        {
            return originalValue;
        }

        public ObfuscateAttribute SetProperty(PropertyInfo propertyInfo)
        {
            _pi = propertyInfo;
            return this;
        }
    }
}