using System;

namespace Protoplasm.ComponentModel
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class MethodDescriptionAttribute : DescriptionBaseAttribute
	{
		public MethodDescriptionAttribute(IdentifierBase identificator, string description) : base(identificator, description)
		{
		}

		public MethodDescriptionAttribute(string id, string description) : base(id, description)
		{
		}
	}
}