using System;
using System.Collections;
using System.Text;
using AssertExtension;
using Common.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Collections.Tests
{
	[TestClass]
	public class DataListWithKeysTests
	{
		private DataListWithKeys<TestData, Guid> _list1;
		private DataListWithKeys<TestData, Guid> _list2;
		private DataListWithKeys<TestData, Guid> _list3;
		private TestData _testData1;
		private TestData _testData2;
		private TestData _testData3;

		[TestInitialize]
		public void TestInitialize()
		{
			_list1 = new DataListWithKeys<TestData, Guid>(data => data.ID);
			_list2 = new DataListWithKeys<TestData, Guid>(data => data.ID);
			_list3 = new DataListWithKeys<TestData, Guid>(data => data.ID);

			_testData1 = new TestData();
			_testData2 = new TestData();
			_testData3 = new TestData();

			_list1.Add(_testData1);
			_list1.Add(_testData2);
			_list1.Add(_testData3);

			_list2.Add(_testData3);
			_list2.Add(_testData1);
			_list2.Add(_testData2);
		}

		[TestMethod]
		public void RemoveFromAllListsOnDeleted()
		{
			_testData1.Delete();

			AssertExt.AreEqual(_list1.Count, _list2.Count, "получили листы разной длинны");
			AssertExt.AreEqual(_list1.Count, 2, "элемент не удалился");
		}

		[TestMethod]
		public void AddContainedItem()
		{
			AssertExt.IsThrowingException(() => _list1.Add(_testData2));

			//AssertExt.AreEqual(_list1.Count, _list2.Count, "получили листы разной длинны");
			//AssertExt.AreEqual(_list1.Count, 2, "элемент не удалился");
		}

		[TestMethod]
		public void GetByKey()
		{
			AssertExt.IsTrue(_list1.ContainsKey(_testData1.ID));
			AssertExt.IsTrue(_list1.Keys.Contains(_testData1.ID));
			AssertExt.AreSame(_list1[_testData1.ID], _testData1);
			AssertExt.AreSame(_list1[0], _testData1);

			AssertExt.IsFalse(_list1.ContainsKey(Guid.Empty));
			AssertExt.IsFalse(_list1.Keys.Contains(Guid.Empty));
			AssertExt.IsNull(_list1[Guid.Empty]);
		}

		[TestMethod]
		public void RemoveByKey()
		{
			_list1.Remove(_testData1.ID);

			AssertExt.AreEqual(_list1.Count, 2);
			AssertExt.IsNull(_list1[_testData1.ID]);
			AssertExt.AreNotSame(_list1[0], _testData1);
			AssertExt.IsFalse(_list1.ContainsKey(_testData1.ID));
			AssertExt.IsFalse(_list1.Keys.Contains(_testData1.ID));
		}
	}
}
