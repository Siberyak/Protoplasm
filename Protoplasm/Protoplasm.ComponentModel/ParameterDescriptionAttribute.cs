using System;

namespace Protoplasm.ComponentModel
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class ParameterDescriptionAttribute : DescriptionBaseAttribute
	{
		public ParameterDescriptionAttribute(IdentifierBase identificator, string description) : base(identificator, description)
		{
		}

		public ParameterDescriptionAttribute(string id, string description) : base(id, description)
		{
		}
	}
}