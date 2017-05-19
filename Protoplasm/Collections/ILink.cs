namespace Protoplasm.Collections
{
	public interface ILink<TOwner, TRelated>
	{
		TOwner Owner { get; }
		TRelated Related { get; }
	}
}