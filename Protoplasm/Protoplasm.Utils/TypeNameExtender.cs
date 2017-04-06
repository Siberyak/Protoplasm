using System;
using System.Collections.Generic;
using System.Linq;

namespace KG.SE2.Utils
{
    /// <summary>
    ///     хелпер для работы с именами типов
    /// </summary>
    public static class TypeNameExtender
    {
        #region Static Fields

        private static readonly Dictionary<Type, string> Names = new Dictionary<Type, string>();

        private static readonly Dictionary<Type, string> FullNames = new Dictionary<Type, string>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     полное название типа
        /// </summary>
        /// <param name="type">тип</param>
        /// <returns></returns>
        public static string FullTypeName(this Type type)
        {
            return type == null ? null : _GetTypeName(type, x => x.FullName, FullTypeName, FullNames);
        }

        /// <summary>
        ///     название типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeName(this Type type)
        {
            return type == null ? null : _GetTypeName(type, x => x.Name, TypeName, Names);
        }

        #endregion

        #region Methods

        private static string _GetTypeName(
            Type type,
            Func<Type, string> getName,
            Func<Type, string> getFullName,
            Dictionary<Type, string> dict)
        {
            lock (dict)
            {
                if (dict.ContainsKey(type))
                {
                    return dict[type];
                }

                var value = type.IsGenericType
                                ? string.Format(
                                    "{0}<{1}>",
                                    getName(type.GetGenericTypeDefinition()).Split('`')[0],
                                    string.Join(", ", type.GetGenericArguments().Select(getFullName)))
                                : getName(type);

                dict.Add(type, value);
                return dict[type];
            }
        }

        #endregion
    }
}