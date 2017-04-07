using System;
using System.Collections.Generic;
using System.Linq;

namespace Protoplasm.Utils
{
    /// <summary>
    /// </summary>
    public static class CommonEnumHelper
    {
        #region Static Fields

        private static readonly Dictionary<Type, Dictionary<string, object>> Enums =
            new Dictionary<Type, Dictionary<string, object>>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     EnumItem по id
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static EnumItem<T> GetValueByID<T>(string id)
        {
            return EnumHelper<T>.GetValueByID(id);
        }

        /// <summary>
        ///     id по значению
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetValueID<T>(T value)
        {
            return EnumHelper<T>.GetValueIDs()[value];
        }

        /// <summary>
        ///     словарь значений (x.ToString -> (object)x)
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static Dictionary<string, object> Values(Type enumType)
        {
            if (!Enums.ContainsKey(enumType))
            {
                var dictionary = Enum.GetValues(enumType).Cast<object>().ToDictionary(x => x.ToString());
                Enums.Add(enumType, dictionary);
            }

            return Enums[enumType];
        }

        #endregion
    }
}