using System;
using System.Linq;
using System.Reflection;

namespace Protoplasm.ComponentModel
{
	public abstract class DescriptionBase<TAttribute>
		where TAttribute : DescriptionBaseAttribute
	{
		protected TAttribute _attribute;

		protected DescriptionBase(ICustomAttributeProvider attributeProvider)
		{
			if (attributeProvider == null)
				throw new ArgumentNullException("attributeProvider");

			_attribute = attributeProvider.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>().FirstOrDefault();
		}

		public string Description
		{
			get { return IsIdentified? _attribute.Description : null; }
		}

		public IdentifierBase Identifier
		{
			get { return IsIdentified ? _attribute.Identifier : null; }
		}

		public bool IsIdentified { get { return _attribute != null; } }

		public override string ToString()
		{
			return string.Format("Description: [{0}], Identifier: [{1}]", Description, Identifier);
		}


	}
}