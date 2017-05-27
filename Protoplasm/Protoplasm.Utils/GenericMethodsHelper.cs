using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protoplasm.Utils
{
    public static class GenericMethodsHelper
    {
        private static readonly Dictionary<Type, Dictionary<string, Dictionary<Type, MethodInfo>>> _methods
            = new Dictionary<Type, Dictionary<string, Dictionary<Type, MethodInfo>>>();

        public static TResult Invoke<T, TResult>(string name, Type argumentType, object target, params object[] args)
        {
            return Invoke<T, TResult>(name, argumentType, 0, target, args);
        }

        public static TResult Invoke<T, TResult>(string name, Type argumentType, BindingFlags flags, object target, params object[] args)
        {
            return (TResult) Get<T>(name, argumentType, flags).Invoke(target, args);
        }

        public static void Invoke<T>(string name, Type argumentType, object target, params object[] args)
        {
            Invoke<T>(name, argumentType, 0, target, args);
        }

        public static void Invoke<T>(string name, Type argumentType, BindingFlags flags, object target, params object[] args)
        {
            Get<T>(name, argumentType, flags).Invoke(target, args);
        }

        public static MethodInfo Get<T>(string name, Type argumentType, BindingFlags flags = 0)
        {
            if (flags == 0)
                flags = BindingFlags.Public | BindingFlags.Instance;

            var type = typeof(T);

            if (!_methods.ContainsKey(type))
            {
                lock (_methods)
                {
                    if (!_methods.ContainsKey(type))
                        _methods.Add(type, new Dictionary<string, Dictionary<Type, MethodInfo>>());
                }
            }

            if (!_methods[type].ContainsKey(name))
            {
                lock (_methods)
                {
                    if (!_methods[type].ContainsKey(name))
                    {
                        _methods[type].Add(name, new Dictionary<Type, MethodInfo>());
                    }
                }
            }

            if (!_methods[type][name].ContainsKey(argumentType))
            {
                lock (_methods)
                {
                    if (!_methods[type][name].ContainsKey(argumentType))
                    {
                        var method = type
                            .GetMethods(flags)
                            .First(x => x.Name == name && x.IsGenericMethod)
                            .GetGenericMethodDefinition()
                            .MakeGenericMethod(argumentType);
                        _methods[type][name].Add(argumentType, method);
                    }
                }
            }
            return _methods[type][name][argumentType];
        }
    }
}