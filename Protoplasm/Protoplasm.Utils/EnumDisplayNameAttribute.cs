using System;

namespace KG.SE2.Utils
{
    /// <summary>
    ///     определяет DisplayName для значения
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// </summary>
        public EnumDisplayNameAttribute()
            : this(null)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="displayName"></param>
        public EnumDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// </summary>
        public virtual string DisplayName { get; private set; }

        #endregion
    }
}