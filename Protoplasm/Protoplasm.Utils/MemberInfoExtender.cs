using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protoplasm.Utils
{
    /// <summary>
    ///     ������ ��� ������ � ���������� ������ ����
    /// </summary>
    public static class MemberInfoExtender
    {
        #region Public Methods and Operators

        /// <summary>
        ///     ������� ������� ���� ����� ����. ������, ���� ���������
        /// </summary>
        /// <param name="memberInfo">���� ����</param>
        /// <param name="inherit">� ������ ������������</param>
        /// <typeparam name="T">��� ��������</typeparam>
        /// <returns></returns>
        public static T Attribute<T>(this ICustomAttributeProvider memberInfo, bool inherit = false) //where T : Attribute
        {
            return memberInfo.Attributes<T>(inherit).FirstOrDefault();
        }

        /// <summary>
        ///     ������� ������� ���� ����� ����. ������, ���� ���������
        /// </summary>
        /// <param name="memberInfo">���� ����</param>
        /// <param name="attributeType">��� ��������</param>
        /// <param name="inherit">� ������ ������������</param>
        /// <returns></returns>
        public static Attribute Attribute(this ICustomAttributeProvider memberInfo, Type attributeType, bool inherit = false)
        {
            return memberInfo.Attributes(attributeType, inherit).FirstOrDefault();
        }

        /// <summary>
        ///     �������� ������� ���� ����� ����.
        /// </summary>
        /// <param name="memberInfo">���� ����</param>
        /// <param name="inherit">� ������ ������������</param>
        /// <typeparam name="T">��� ��������</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> Attributes<T>(this ICustomAttributeProvider memberInfo, bool inherit = false) //where T : Attribute
        {
            return memberInfo.GetCustomAttributes(inherit).OfType<T>().ToArray();
        }

        /// <summary>
        ///     �������� ������� ���� ����� ����
        /// </summary>
        /// <param name="memberInfo">���� ����</param>
        /// <param name="attributeType">��� ��������</param>
        /// <param name="inherit">� ������ ������������</param>
        /// <returns></returns>
        public static IEnumerable<Attribute> Attributes(this ICustomAttributeProvider memberInfo, Type attributeType, bool inherit = false)
        {
            return memberInfo.GetCustomAttributes(attributeType, inherit).OfType<Attribute>().ToArray();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="component"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetValue<T>(this MemberInfo member, object component, out T value)
        {
            value = default(T);

            object memberValue;
            if (member is PropertyInfo)
                memberValue = ((PropertyInfo)member).GetValue(component);
            else if (member is FieldInfo)
                memberValue = ((FieldInfo)member).GetValue(component);
            else if (member is MethodInfo)
                memberValue = ((MethodInfo)member).Invoke(component, new object[0]);
            else
                return false;

            bool valueResolved = memberValue is T;
            value = valueResolved ? (T)memberValue : default(T);
            return valueResolved;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Type MemberType(this MemberInfo memberInfo)
        {
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Method:
                    return ((MethodInfo)memberInfo).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)memberInfo).PropertyType;
                case MemberTypes.Constructor:
                    return memberInfo.DeclaringType;
                case MemberTypes.Field:
                    return ((FieldInfo)memberInfo).FieldType;
                default:
                    throw new InvalidOperationException();
            }
        }
        #endregion
    }
}