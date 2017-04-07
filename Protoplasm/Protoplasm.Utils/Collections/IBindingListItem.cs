using System;

namespace Protoplasm.Utils.Collections
{
	public interface IBindingListItem
	{
		event EventHandler Deleted;
		void OnDeleted();
	}
}