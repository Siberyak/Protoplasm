namespace Protoplasm.Utils.Collections
{
	public interface ILink<TOwner, TRelated>
	{
		TOwner Owner { get; }
		TRelated Related { get; }
	}
}