using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AssertExtension;
using Common.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Collections.Tests
{
	[TestClass]
	public class PredicatedListTests
	{
		readonly DataList<int> _original = new DataList<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
		[TestMethod]
		public void Create()
		{
			var list = new PredicatedList<int>(_original, x => x <= 5);
			AssertExt.AreEqual(list.Count(), 5);
		}
	
		[TestMethod]
		public void OriginalInsert()
		{

			var a = new []
			        	{
			        		new ListChangedEventArgs(ListChangedType.ItemAdded, 1),
			        		new ListChangedEventArgs(ListChangedType.ItemAdded, 6),
			        		new ListChangedEventArgs(ListChangedType.ItemDeleted, 6),
			        		new ListChangedEventArgs(ListChangedType.ItemDeleted, 1),
			        		new ListChangedEventArgs(ListChangedType.ItemAdded, 5),
			        		new ListChangedEventArgs(ListChangedType.ItemDeleted, 0),
			        		new ListChangedEventArgs(ListChangedType.ItemDeleted, 0),
			        		new ListChangedEventArgs(ListChangedType.ItemAdded, 0),
			        		new ListChangedEventArgs(ListChangedType.Reset, -1),
			        	};

			var q = new Queue<ListChangedEventArgs>(a);

			var list = new PredicatedList<int>(_original, x => x <= 5);
			list.ListChanged += (sender, args) => ProcessQueue(q, args);

			_original.Insert(1, 5);
			AssertExt.AreEqual(list.Count(), 6);
			AssertExt.AreEqual(a.Length-1, q.Count);

			_original.Add(5);
			AssertExt.AreEqual(list.Count(), 7);
			AssertExt.AreEqual(a.Length - 2, q.Count);

			_original.RemoveAt(_original.Count()-1);
			AssertExt.AreEqual(list.Count(), 6);
			AssertExt.AreEqual(a.Length - 3, q.Count);

			_original.Insert(0, 10);
			AssertExt.AreEqual(list.Count(), 6);
			AssertExt.AreEqual(a.Length - 3, q.Count);

			_original.Remove(5);
			AssertExt.AreEqual(list.Count(), 5);
			AssertExt.AreEqual(a.Length - 4, q.Count);

			_original[7] = 5;
			AssertExt.AreEqual(list.Count(), 6);
			AssertExt.AreEqual(a.Length - 5, q.Count);

			_original[1] = 10;
			AssertExt.AreEqual(list.Count(), 5);
			AssertExt.AreEqual(a.Length - 6, q.Count);

			_original[2] = 5;
			AssertExt.AreEqual(list.Count(), 5);
			AssertExt.AreEqual(a.Length - 8, q.Count);


			_original.Clear();
			AssertExt.AreEqual(list.Count(), 0);
		}

		private void ProcessQueue(Queue<ListChangedEventArgs> queue, ListChangedEventArgs e)
		{
			var we = queue.Dequeue();

			var listChangedType = we.ListChangedType == e.ListChangedType;
			var index = we.NewIndex == e.NewIndex;

			AssertExt.IsTrue(listChangedType && index);
		}
	}
}