using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConsoleApplication1
{
    public class Obfuscator
    {
        static readonly Dictionary<Type, Obfuscator> _byType = new Dictionary<Type, Obfuscator>();
        private ObfuscationSequenceAttribute _obfuscationSequenceAttribute;
        private ObfuscateAttribute[] _properties;

        public static void Obfuscate(object data)
        {
            if (data == null)
                return;

            var type = data.GetType();
            if (!_byType.ContainsKey(type))
                _byType.Add(type, new Obfuscator(type));

            var obfuscator = _byType[type];

            obfuscator.Process(data);
        }

        private Obfuscator(Type type)
        {
            _obfuscationSequenceAttribute = type.Attribute<ObfuscationSequenceAttribute>() ?? new ObfuscationSequenceAttribute();
            _properties = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance)
                .Select(x => MemberInfoExtender.Attribute<ObfuscateAttribute>(x)?.SetProperty(x))
                .Where(x => x != null)
                .ToArray();
        }

        private void Process(object data)
        {
            _obfuscationSequenceAttribute.Current++;

            foreach (var attribute in _properties)
            {
                attribute.Obfuscate(_obfuscationSequenceAttribute, data);
            }
        }
    }
}