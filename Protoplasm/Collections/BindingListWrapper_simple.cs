using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Protoplasm.Collections
{
	public class BindingListWrapper<TItem> : BindingListWrapper<TItem, TItem>
	{
		public BindingListWrapper(IList<TItem> baseList, bool processIBindingListItem = true)
            : base(baseList.OnStartCreatingList<TItem, TItem, BindingListWrapper<TItem, TItem>>(processIBindingListItem), x => x, x => x, processIBindingListItem)
		{
            BindingListsObserver.OnListCreating<TItem, TItem, BindingListWrapper<TItem,TItem>>(this, baseList, processIBindingListItem);
		}

		protected override void OnInnerListChanged(object sender, ListChangedEventArgs e)
		{
			if(e.ListChangedType == ListChangedType.ItemChanged)
				OnListChanged(e);
			else
				base.OnInnerListChanged(sender, e);
		}
	}
}