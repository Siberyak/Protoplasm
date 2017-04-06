using System;
using System.Collections.Generic;
using System.Linq;

namespace KG.SE2.Utils
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumItem<T> : IEquatable<EnumItem<T>>, IComparable, IComparable<EnumItem<T>>, IComparable<T>, IEquatable<T>
    {
        #region Constructors and Destructors

        static EnumItem()
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException(typeof(T).FullName + " ������ ���� IsEnum");
            }

            //Items = EnumHelper<T>.GetByAttribute<T, EnumItem<T>, EnumDisplayNameAttribute>((value, attributes) => new KeyValuePair<T, EnumItem<T>>(value, attributes.Length > 0 ? new EnumItem<T>(value, attributes[0].DisplayName) : new EnumItem<T>(value)));

            var displayNames = EnumHelper<T>.GetDisplayNames();
            var valuePairs = displayNames.ToList();
            Items = valuePairs.ToDictionary(pair => pair.Key, pair => new EnumItem<T>(pair.Key, pair.Value));
        }

        private EnumItem(T value)
        {
            Value = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="displayName"></param>
        public EnumItem(T value, string displayName)
            : this(value)
        {
            DisplayName = displayName;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     DisplayName
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     ������� x => EnumItem(x)
        /// </summary>
        public static Dictionary<T, EnumItem<T>> Items { get; }

        /// <summary>
        ///     ��������
        /// </summary>
        public T Value { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     ������������� �� DisplayName ����� EnumItem
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EnumItem<T>> GetAllItems()
        {
            return Items.Values.OrderBy(x => x.DisplayName).ToArray();
        }

        /// <summary>
        ///     ����� EnumItem, ��������������� ������ ����������
        /// </summary>
        /// <param name="prototypes">���������</param>
        /// <returns></returns>
        public static IEnumerable<EnumItem<T>> GetItems(params T[] prototypes)
        {
            return prototypes.Select(prototype => Items[prototype]);
        }

        /// <summary>
        ///     ��������� ���� EnumItem-��
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(EnumItem<T> a, EnumItem<T> b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return Equals(a.Value, b.Value);
        }

        /// <summary>
        ///     ����������� DBNull -> null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator EnumItem<T>(DBNull value)
        {
            return null;
        }

        /// <summary>
        ///     ����������� ���� EnumItem-��
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(EnumItem<T> a, EnumItem<T> b)
        {
            return !(a == b);
        }

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(EnumItem<T> other)
        {
            return other == this;
        }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as EnumItem<T>);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// ���������� ������� ������ � ������ �������� ���� �� ����.
        /// </summary>
        /// <returns>
        /// ��������, �����������, ����� ������������� ������� ������������ ��������. ����������� ������������ �������� ��������� ����. ��������  ��������  ������ ����  �������� ����� ������� ������ �������� ��������� <paramref name="other"/>. Zero  �������� ����� ������� ����� �������� ��������� <paramref name="other"/>.  ������ ����.  �������� ����� ������� ������ �������� ��������� <paramref name="other"/>.
        /// </returns>
        /// <param name="other">������, ������� ��������� �������� � ������ ��������.</param>
        int IComparable<T>.CompareTo(T other)
        {
            return CompareToEnumValue(other);
        }

        private int CompareToEnumValue(T other)
        {
            return ((IComparable)Value).CompareTo(other);
        }

        /// <summary>
        /// ���������, ����� �� ������� ������ ������� ������� ���� �� ����.
        /// </summary>
        /// <returns>
        /// true, ���� ������� ������ ����� ��������� <paramref name="other"/>, � ��������� �����堗 false.
        /// </returns>
        /// <param name="other">������, ������� ��������� �������� � ������ ��������.</param>
        bool IEquatable<T>.Equals(T other)
        {
            return Equals(Value, other);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => DisplayName ?? Value.ToString();

        #endregion

        #region Methods

        int IComparable.CompareTo(object obj)
        {
            return obj is T ? CompareToEnumValue((T)obj) : CompareTo(obj as EnumItem<T>);
        }

        int IComparable<EnumItem<T>>.CompareTo(EnumItem<T> other)
        {
            return CompareTo(other);
        }

        private int CompareTo(EnumItem<T> other)
        {
            return other == null ? -1 : String.Compare(DisplayName, other.DisplayName, StringComparison.Ordinal);
        }

        #endregion
    }
}