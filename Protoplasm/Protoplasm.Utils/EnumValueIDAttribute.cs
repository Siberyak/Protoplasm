using System;

namespace KG.SE2.Utils
{
    /// <summary>
    ///     ���������� �������� ID ��� ��������
    /// </summary>
    public class EnumValueIDAttribute : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        ///     �����������
        /// </summary>
        /// <param name="id"></param>
        public EnumValueIDAttribute(int id)
        {
            ID = id;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     �������� ID
        /// </summary>
        public int ID { get; private set; }

        #endregion
    }
}