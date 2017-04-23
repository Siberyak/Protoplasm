namespace MAS.Core
{
    /// <summary>
    /// Типы совместимости
    /// </summary>
    public enum CompatibilityType
    {
        /// <summary>
        /// Совместимость при любых обстоятельствах
        /// </summary>
        Always = 0,
        /// <summary>
        /// Зависит от состояния сцены
        /// </summary>
        DependsOnScene =1,
        /// <summary>
        /// Несовместимость при любых обстоятельствах
        /// </summary>
        Never =2
    }
}