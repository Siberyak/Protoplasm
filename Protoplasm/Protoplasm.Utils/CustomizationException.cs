using System;

namespace Protoplasm.Utils
{
    public class CustomizationException : Exception
    {
        public Type Customizing { get; }

        public CustomizationException(Type customizing, string message, Exception innerException) : base(message, innerException)
        {
            Customizing = customizing;
        }

        public CustomizationException(Type customizing, string message) : base(message)
        {
            Customizing = customizing;
        }

        public static void Throw(Type type, Exception inner = null)
        {
            Throw(type, $"Ошибка кастомизации. Тип '{type.TypeName()}'", inner);
        }

        public static void Throw(Type type, string message, Exception inner = null)
        {
            if (inner == null)
                throw new CustomizationException(type, message);

            throw new CustomizationException(type, message, inner);
        }

        public static void Throw<T>(Exception inner = null)
        {
            Throw(typeof(T), inner);
        }
        public static void Throw<T>(string message, Exception inner = null)
        {
            Throw(typeof(T), message, inner);
        }
    }
}