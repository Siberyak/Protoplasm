using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Protoplasm.ComponentModel
{
	public class MethodDescription : DescriptionBase<MethodDescriptionAttribute>
	{
		private readonly MethodBase _method;
		private readonly IEnumerable<ParamemterDescription> _parameters;
		protected internal MethodDescription(MethodBase method)
			: base(method)
		{
			_method = method;

			_parameters = method.GetParameters()
				.Select(x => new ParamemterDescription(x))
				//.Where(x => x.IsIdentified)
				.ToArray();
		}

		public IEnumerable<ParamemterDescription> Parameters
		{
			get { return _parameters; }
		}

		public override string ToString()
		{
			return string.Format("{0}, Parameters: [{1}]", base.ToString(), Parameters.Count());
		}

		public object Invoke(object obj, params object[] args)
		{
			return _method.Invoke((_method.IsStatic ? _method.DeclaringType : obj), args);
		}
	}
}