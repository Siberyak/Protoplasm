using System;

namespace Protoplasm.Collections
{
	public interface IBindingListItem
	{
		event EventHandler Deleted;
		void OnDeleted();
	}
}