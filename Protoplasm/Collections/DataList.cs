using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Protoplasm.Collections
{
    public class DataList<T> : BindingListWrapper<T>
	{
		public DataList(bool processIBindingListItem = true) : this(new List<T>(), processIBindingListItem)
		{
		}

        public DataList(IList<T> list, bool processIBindingListItem = true) : base(list.OnStartCreatingList<T,T,DataList<T>>(processIBindingListItem), processIBindingListItem)
        {
            BindingListsObserver.OnListCreating<T,T,DataList<T>>(this, list, processIBindingListItem);
        }
	}

    public class DataListWithKeys<T, TKey> : DataList<T>
    {
        private Func<T, TKey> GetKey { get; set; }

        public DataListWithKeys(Func<T, TKey> getKey, bool processIBindingListItem = true)
            : this(new List<T>(), getKey, processIBindingListItem)
        {
            
        }

        public DataListWithKeys(IList<T> list, Func<T, TKey> getKey, bool processIBindingListItem = true)
            : base(list.OnStartCreatingList<T,T,DataListWithKeys<T,TKey>>(processIBindingListItem), processIBindingListItem)
        {
            GetKey = getKey;

            BindingListsObserver.OnListCreating<T, T, DataListWithKeys<T, TKey>>(this, list, default(bool?));

        }

        public bool Remove(TKey key)
        {
            return Remove(this[key]);
        }

        public bool ContainsKey(TKey key)
        {
            return BaseList.Any(x => Equals(GetKey(x), key));
        }

        public T this[TKey key]
        {
            get { return BaseList.FirstOrDefault(x => Equals(GetKey(x), key)); }
        }

        public ICollection<TKey> Keys
        {
            get { return BaseList.Select(x => GetKey(x)).ToArray(); }
        }
    }


    //public class DataListWithKeys<T, TKey> : DataList<T>
    //{
    //    private Func<T, TKey> GetKey { get; set; }
    //    readonly Dictionary<TKey, int> _dict = new Dictionary<TKey, int>();

    //    public DataListWithKeys(Func<T, TKey> getKey, bool processIBindingListItem = true)
    //        : this(new List<T>(),getKey, processIBindingListItem)
    //    {}

    //    public DataListWithKeys(IList<T> list, Func<T, TKey> getKey, bool processIBindingListItem = true)
    //        : base(list, processIBindingListItem)
    //    {
    //        GetKey = getKey;
    //        for (var i = 0; i < list.Count; i++)
    //            _dict.Add(GetKey(list[i]), i);
    //    }

    //    protected override void BaseList_Add(T item)
    //    {
    //        _dict.Add(GetKey(item), BaseListCount);
    //        base.BaseList_Add(item);
    //    }

    //    protected override object BaseList_AddNew()
    //    {
    //        var item = base.BaseList_AddNew();
    //        _dict.Add(GetKey((T) item), BaseListCount);
    //        return item;
    //    }

    //    protected override void BaseList_Clear()
    //    {
    //        base.BaseList_Clear();
    //        _dict.Clear();
    //    }

    //    protected override bool BaseList_Contains(T item)
    //    {
    //        return _dict.ContainsKey(GetKey(item));
    //    }

    //    protected override bool BaseList_Remove(T item)
    //    {
    //        var result = base.BaseList_Remove(item);
    //        _dict.Remove(GetKey(item));
    //        return result;
    //    }

    //    protected override void BaseList_RemoveAt(int index)
    //    {
    //        var item = BaseList_Get(index);
    //        _dict.Remove(GetKey(item));

    //        for (var i = index+1; i < BaseListCount; i++)
    //            _dict[GetKey(BaseList_Get(i))] = i - 1;

    //        base.BaseList_RemoveAt(index);
    //    }

    //    protected override int BaseList_IndexOf(T item)
    //    {
    //        var key = GetKey(item);
    //        return _dict.ContainsKey(key) ? _dict[key] : -1;
    //    }

    //    protected override void BaseList_Insert(int index, T item)
    //    {
    //        _dict.Add(GetKey(item), index);

    //        for (var i = index; i < BaseListCount; i++)
    //            _dict[GetKey(BaseList_Get(i))] = i + 1;

    //        base.BaseList_Insert(index, item);
    //    }

    //    protected override void BaseList_MoveTo(int index, T item)
    //    {
    //        throw new NotImplementedException();
    //        base.BaseList_MoveTo(index, item);
    //    }

    //    //protected override void BaseList_Set(int index, T value)
    //    //{
    //    //    var tmp = BaseList_Get(index);
    //    //    if(ReferenceEquals(tmp, value))
    //    //        return;
    //    //    base.BaseList_Set(index, value);

    //    //    BaseList_RemoveAt(index);
    //    //    BaseList_Insert(index, value);

    //    //    var item = this[index];

    //    //    _dict.Remove(GetKey(item));
    //    //    _dict.Add(GetKey(value), index);
    //    //    base.BaseList_Set(index, value);
    //    //}

    //    public bool ContainsKey(TKey key)
    //    {
    //        return _dict.ContainsKey(key);
    //    }

    //    public bool Remove(TKey key)
    //    {
    //        if (ContainsKey(key))
    //        {
    //            RemoveAt(_dict[key]);
    //            return true;
    //        }

    //        return false;
    //    }

    //    public T this[TKey key]
    //    {
    //        get { return ContainsKey(key) ? this[_dict[key]] : default(T); }
    //    }

    //    public ICollection<TKey> Keys
    //    {
    //        get { return _dict.Keys; }
    //    }
    //}
}
