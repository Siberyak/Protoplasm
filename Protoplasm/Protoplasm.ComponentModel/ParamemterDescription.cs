using System.Reflection;

namespace Protoplasm.ComponentModel
{
	public class ParamemterDescription : DescriptionBase<ParameterDescriptionAttribute>
	{
		public ParamemterDescription(ParameterInfo parameter) : base(parameter)
		{
		}
	}
}