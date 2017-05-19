using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Protoplasm.Collections
{
	public class InheritedList<TParent, TChild> : TransformList<TChild, TParent>
		where TChild : TParent
	{
        public InheritedList(bool processIBindingListItem = true)
			: base(child => (TParent)child, parent => (TChild)parent, processIBindingListItem)
		{
		}

        public InheritedList(IList<TChild> collection, bool processIBindingListItem = true)
			: base(collection, child => (TParent)child, parent => (TChild)parent, processIBindingListItem)
		{
		}
	}


	


}