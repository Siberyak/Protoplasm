using System.Collections.Generic;

namespace Protoplasm.Collections
{
	public interface ILinksList<TLink> : IList<TLink>
	{ }

	public interface ILinksList<TLink, TOwner, TRelated> : ILinksList<TLink>
		where TLink : ILink<TOwner, TRelated>
	{
		TOwner Owner { get; }
	}
}