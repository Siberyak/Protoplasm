using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protoplasm.Utils
{
    /// <summary>
    ///     ������ ��� ������ � enum-���
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public static class EnumHelper<TEnum>
    {
        #region Static Fields

        private static Dictionary<TEnum, string> _displayNames;

        private static Dictionary<TEnum, string> _valueIDs;

        private static TEnum[] _flags;

        #endregion

        #region Constructors and Destructors

        static EnumHelper()
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
            {
                throw new ArgumentException(type.FullName + " - �� enum");
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     ������� ���� - �������� (����� ��������� �������)
        /// </summary>
        /// <param name="func">������� ��������� ���� ����-�������� ��� �������� enum-�</param>
        /// <typeparam name="TKey">��� �����</typeparam>
        /// <typeparam name="TValue">��� ��������</typeparam>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Get<TKey, TValue>(Func<TEnum, FieldInfo, KeyValuePair<TKey, TValue>> func)
        {
            var type = typeof(TEnum);
            var fields = GetFields(type);

            var ret = new Dictionary<TKey, TValue>();

            fields.ForEach(
                info =>
                    {
                        var value = (TEnum)info.GetValue(type);
                        var pair = func.Invoke(value, info);
                        if ((typeof(TKey).IsValueType && typeof(TValue).IsValueType)
                            || !Equals(pair, default(KeyValuePair<TKey, TValue>)))
                        {
                            ret.Add(pair.Key, pair.Value);
                        }
                    });

            return ret;
        }

        /// <summary>
        ///     ������� ���� - �������� (����� ��������� �������)
        /// </summary>
        /// <param name="func">������� ��������� ���� ����-�������� �� ������� ��������� �������� enum-�</param>
        /// <typeparam name="TKey">��� �����</typeparam>
        /// <typeparam name="TValue">��� ��������</typeparam>
        /// <typeparam name="TAttribute">��� ��������������� ���������</typeparam>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetByAttribute<TKey, TValue, TAttribute>(
            Func<TEnum, TAttribute[], KeyValuePair<TKey, TValue>> func)
            where TAttribute : Attribute
        {
            return GetByAttribute(func, @enum => default(KeyValuePair<TKey, TValue>));
        }

        /// <summary>
        ///     ������� ���� - �������� (����� ��������� �������)
        /// </summary>
        /// <param name="func">������� ��������� ���� ����-�������� �� ������� ��������� �������� enum-�</param>
        /// <param name="funcIfNotAttributed">
        ///     ������� ��������� ���� ����-�������� �� �������� enum-�, ��� �������� �� ����������
        ///     ���������
        /// </param>
        /// <typeparam name="TKey">��� �����</typeparam>
        /// <typeparam name="TValue">��� ��������</typeparam>
        /// <typeparam name="TAttribute">��� ��������������� ���������</typeparam>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> GetByAttribute<TKey, TValue, TAttribute>(
            Func<TEnum, TAttribute[], KeyValuePair<TKey, TValue>> func,
            Func<TEnum, KeyValuePair<TKey, TValue>> funcIfNotAttributed)
            where TAttribute : Attribute
        {
            return Get(
                (@enum, info) =>
                    {
                        var value = (TEnum)info.GetValue(info.DeclaringType);
                        var arr = info.Attributes<TAttribute>().ToArray();
                        return arr.Length == 0
                                   ? funcIfNotAttributed.Invoke(value)
                                   : func.Invoke(value, arr);
                    });
        }

        /// <summary>
        ///     ������� �������� - DisplayName (����� EnumDisplayNameAttribute)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<TEnum, string> GetDisplayNames()
        {
            return _displayNames ?? (_displayNames = GetByAttribute<TEnum, string, EnumDisplayNameAttribute>
                                                         (
                                                             (@enum, attributes) =>
                                                             new KeyValuePair<TEnum, string>(
                                                                 @enum,
                                                                 attributes[0].DisplayName ?? @enum.ToString()),
                                                             @enum => new KeyValuePair<TEnum, string>(@enum, @enum.ToString())
                                                         ));
        }

        /// <summary>
        ///     ������� ��������� ������������� (x.ToString() => x)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, TEnum> GetEnumNames()
        {
            return Enum.GetValues(typeof(TEnum))
                       .Cast<TEnum>()
                       .ToDictionary(value => value.ToString(), value => value);
        }

        /// <summary>
        ///     �������� ��������
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TEnum> GetFlags()
        {
            return _flags ?? (_flags = GetFlagsValues());
        }

        /// <summary>
        ///     EnumItem �� id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static EnumItem<TEnum> GetValueByID(string id)
        {
            var x = GetValueIDs().FirstOrDefault(pair => pair.Value == id);
            return Equals(x, default(KeyValuePair<TEnum, string>)) ? null : new EnumItem<TEnum>(x.Key, GetDisplayNames()[x.Key]);
        }

        /// <summary>
        ///     ������� �������� - ID (����� EnumValueIDAttribute)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<TEnum, string> GetValueIDs()
        {
            return _valueIDs ?? (_valueIDs = GetByAttribute<TEnum, string, EnumValueIDAttribute>
                                                 (
                                                     (@enum, attributes) =>
                                                     new KeyValuePair<TEnum, string>(@enum, attributes[0].ID.ToString())
                                                 ));
        }

        /// <summary>
        ///     ��� �������� - ��������� �� ����� ��������
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<TEnum> Split(TEnum value)
        {
            var converted = Convert.ToInt64(value);
            var values =
                GetFlags()
                    .Select(x => new { EnumValue = x, Value = Convert.ToInt64((object)x) })
                    .Where(x => (converted | x.Value) == converted)
                    .ToArray();
            return values.Sum(x => x.Value) == converted ? values.Select(x => x.EnumValue).ToArray() : new TEnum[0];
        }

        #endregion

        #region Methods

        private static List<FieldInfo> GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Static | BindingFlags.Public).ToList();
        }

        private static TEnum[] GetFlagsValues()
        {
            Type type = typeof(TEnum);

            if (TypeExtender.Attribute<FlagsAttribute>(type) == null)
            {
                return new TEnum[0];
            }

            var values = GetFields(type)
                .Select(x => (TEnum)x.GetValue(type))
                .Select(x => new { EnumValue = x, Value = Convert.ToInt64(x) })
                .ToArray();

            var max = values.Max(x => x.Value);

            long current = 1;
            var tmp = values.SkipWhile(x => x.Value < current).ToArray();
            var flags = new List<TEnum>();

            while (current <= max)
            {
                var v = tmp.FirstOrDefault();
                if (v == null)
                {
                    break;
                }

                if (v.Value == current)
                {
                    flags.Add(v.EnumValue);
                }

                current *= 2;
                tmp = tmp.SkipWhile(x => x.Value < current).ToArray();
            }

            return flags.ToArray();
        }
        #endregion
    }

}