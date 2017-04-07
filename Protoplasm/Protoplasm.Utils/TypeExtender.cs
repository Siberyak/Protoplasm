using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Protoplasm.Utils
{
    /// <summary>
    ///     хелпер для работы с атрибутами типов
    /// </summary>
    public static class TypeExtender
    {
        #region Static Fields

        private static readonly Dictionary<Type, string> DisplayNames = new Dictionary<Type, string>();

        private static readonly Dictionary<Type, object> _defaultValuesByType = new Dictionary<Type, object>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     версия сборки, в которой определен тип
        /// </summary>
        /// <param name="type"></param>
        /// <param name="withRevision"></param>
        /// <returns></returns>
        public static Version AssemblyVersion(this Type type, bool withRevision = false)
        {
            if (type == null)
            {
                return null;
            }

            var version = type.Assembly.GetName().Version;
            return withRevision ? version : new Version(version.Major, version.Minor, version.Build);
        }

        /// <summary>
        ///     атрибут нужного типа, заданный на типе. первый, если несколько
        /// </summary>
        /// <typeparam name="T">тип атрибута</typeparam>
        /// <param name="type">тип</param>
        /// <param name="inherit">с учетом наследования</param>
        /// <returns></returns>
        public static T Attribute<T>(this Type type, bool inherit = false)
            where T : Attribute
        {
            return type.Attributes<T>().FirstOrDefault();
        }

        /// <summary>
        ///     атрибуты нужного типа, заданные на типе
        /// </summary>
        /// <typeparam name="T">тип атрибута</typeparam>
        /// <param name="type">тип</param>
        /// <param name="inherit">с учетом наследования</param>
        /// <returns></returns>
        public static IEnumerable<T> Attributes<T>(this Type type, bool inherit = false)
            where T : Attribute
        {
            return type == null
                       ? new T[0]
                       : type.GetCustomAttributes(inherit).OfType<T>().ToArray();
        }

        /// <summary>
        ///     значение по умолчанию для типа (аналог default(...))
        /// </summary>
        /// <param name="type">тип</param>
        /// <returns></returns>
        public static object DefaultValue(this Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (!_defaultValuesByType.ContainsKey(type))
            {
                lock (_defaultValuesByType)
                {
                    if (!_defaultValuesByType.ContainsKey(type))
                    {
                        _defaultValuesByType.Add(type, type.IsValueType ? Activator.CreateInstance(type) : null);
                    }
                }
            }

            return _defaultValuesByType[type];
        }

        /// <summary>
        ///     DisplayName типа. (через DisplayNameAttribute)
        /// </summary>
        /// <param name="type">тип</param>
        /// <returns></returns>
        public static string DisplayName(this Type type)
        {
            return type == null ? null : _GetTypeDisplayName(type);
        }

        #endregion

        #region Methods

        private static string _GetTypeDisplayName(Type type)
        {
            lock (DisplayNames)
            {
                if (!DisplayNames.ContainsKey(type))
                {
                    var attr = type.Attribute<DisplayNameAttribute>();
                    var displayName = attr == null
                                          ? type.TypeName()
                                          : attr.DisplayName;

                    DisplayNames.Add(type, displayName);
                }
            }
            return DisplayNames[type];
        }

        #endregion
    }
}