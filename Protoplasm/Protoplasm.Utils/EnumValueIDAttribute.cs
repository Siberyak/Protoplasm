using System;

namespace KG.SE2.Utils
{
    /// <summary>
    ///     определяет значение ID для значения
    /// </summary>
    public class EnumValueIDAttribute : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        ///     конструктор
        /// </summary>
        /// <param name="id"></param>
        public EnumValueIDAttribute(int id)
        {
            ID = id;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     значение ID
        /// </summary>
        public int ID { get; private set; }

        #endregion
    }
}