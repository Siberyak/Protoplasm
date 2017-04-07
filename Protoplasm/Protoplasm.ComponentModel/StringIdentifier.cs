using System;

namespace Protoplasm.ComponentModel
{
	public class StringIdentifier : IdentifierBase
	{
		public StringIdentifier(string id)
		{
			if(string.IsNullOrWhiteSpace(id))
				throw new ArgumentException("id is null or is white space", "id");

			ID = id;
		}

		public string ID { get; private set; }

		protected override bool Equals(IdentifierBase other)
		{
			var identificator = other as StringIdentifier;
			return identificator != null && identificator.ID == ID;
		}

		public override string ToString()
		{
			return ID;
		}
	}
}