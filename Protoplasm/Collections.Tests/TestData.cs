using System;
using Common.Collections;

namespace Collections.Tests
{
	class TestData : IBindingListItem
	{
		public Guid ID = Guid.NewGuid();

		void IBindingListItem.OnDeleted()
		{
			if (_deleted == null)
				return;

			_deleted(this, EventArgs.Empty);
		}

		private EventHandler _deleted;
		event EventHandler IBindingListItem.Deleted
		{
			add { _deleted += value; }
			remove { _deleted -= value; }
		}

		public void Delete()
		{
			((IBindingListItem)this).OnDeleted();
		}
	}
}