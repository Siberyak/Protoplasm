using System;

namespace Protoplasm.ComponentModel
{
	public abstract class DescriptionBaseAttribute : Attribute
	{
		private DescriptionBaseAttribute(string description)
		{
			Description = description;
		}


		protected DescriptionBaseAttribute(IdentifierBase identificator, string description) : this(description)
		{
			Identifier = identificator;
		}

		protected DescriptionBaseAttribute(string id, string description)
			: this((StringIdentifier)id, description)
		{}

		public string Description { get; private set; }

		public IdentifierBase Identifier { get; private set; }
	}
}