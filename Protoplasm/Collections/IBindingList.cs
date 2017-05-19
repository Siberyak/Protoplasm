using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Protoplasm.Collections
{
	public class BeforeRemoveEventArgs : EventArgs
	{
		public BeforeRemoveEventArgs(int index)
		{
			Index = index;
		}

		public int Index { get; private set; }
	}

	public delegate void BeforeRemoveEventHandler(object sender, BeforeRemoveEventArgs e);

	public interface IBindingList<TItem> : IBindingList, IList<TItem>
	{
		event BeforeRemoveEventHandler BeforeRemoveItem;
		bool RaiseListChangedEvents { get; }
		void SuspendEvents();
		void ResumeEvents(bool raiseReset = true);
	}

    //public static class BindingListEventsListener
    //{
    //    private static readonly Stack<object> _stack = new Stack<object>();
    //    public static void Start()
    //    {
    //        lock (_stack)
    //            _stack.Push(null);
    //    }

    //    public static void Stop()
    //    {
    //        lock (_stack)
    //            _stack.Push(null);
    //    }

    //    public static bool IsListen { get { lock (_stack) return _stack.Any(); } }

    //    //public static void Write<TSender>()
    //}


	public static class EnumerableExtender
	{
        public static IBindingList<T> ToBindingList<T>(this IEnumerable<T> collection, bool processIBindingListItem = true)
		{
			if (collection == null)
				return new DataList<T>(processIBindingListItem);

			if (collection is IBindingList<T>)
				return (IBindingList<T>) collection;

			if (collection is List<T>)
				return new DataList<T>((IList<T>) collection, processIBindingListItem);

			return new DataList<T>(collection.ToList(), processIBindingListItem);
		}
	}
}