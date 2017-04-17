using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Protoplasm.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SequencedAttribute : Attribute
    {
        public static readonly SequencedAttribute Default = new SequencedAttribute();
        public long Current;

        public string DisplayName { get; set; }

        public virtual string DataDisplayName(object data)
        {
            return DisplayName ?? data?.GetType().DisplayName();
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ObfuscateAttribute : Attribute
    {
        private PropertyInfo _pi;

        public virtual void Obfuscate(SequencedAttribute sequence, object data)
        {
            var value = GetObfuscatedValue(sequence, data, _pi.GetValue(data));
            _pi.SetValue(data, value);
        }

        protected virtual object GetObfuscatedValue(SequencedAttribute sequence, object data, object originalValue)
        {
            return originalValue;
        }

        public ObfuscateAttribute SetProperty(PropertyInfo propertyInfo)
        {
            _pi = propertyInfo;
            return this;
        }
    }

    public class Obfuscator
    {
        static readonly Dictionary<Type, Obfuscator> _byType = new Dictionary<Type, Obfuscator>();
        private SequencedAttribute _sequencedAttribute;
        private ObfuscateAttribute[] _properties;

        public static void Obfuscate(object data)
        {
            if(data == null)
                return;

            var type = data.GetType();
            if(!_byType.ContainsKey(type))
                _byType.Add(type, new Obfuscator(type));

            var obfuscator = _byType[type];

            obfuscator.Process(data);
        }

        private Obfuscator(Type type)
        {
            _sequencedAttribute = type.Attribute<SequencedAttribute>() ?? new SequencedAttribute();
            _properties = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(x => x.Attribute<ObfuscateAttribute>()?.SetProperty(x))
                .Where(x => x != null)
                .ToArray();
        }

        private void Process(object data)
        {
            _sequencedAttribute.Current++;

            foreach (var attribute in _properties)
            {
                attribute.Obfuscate(_sequencedAttribute, data);
            }
        }
    }
}