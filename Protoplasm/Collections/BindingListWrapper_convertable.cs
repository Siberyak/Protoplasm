using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Protoplasm.Collections
{
	public class BindingListWrapper<TBase, TResult> : IBindingList<TResult>
	{
		protected class FakePropertyDescriptor : PropertyDescriptor
		{
			public FakePropertyDescriptor(string name) : base(name, new Attribute[0])
			{
			}

			#region Overrides of PropertyDescriptor

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object component)
			{
				throw new NotImplementedException();
			}

			public override void ResetValue(object component)
			{
				throw new NotImplementedException();
			}

			public override void SetValue(object component, object value)
			{
				throw new NotImplementedException();
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}

			public override Type ComponentType
			{
				get { return typeof(BindingListWrapper<TBase, TResult>); }
			}

			public override bool IsReadOnly
			{
				get { return true; }
			}

			public override Type PropertyType
			{
				get { return typeof(object); }
			}

			#endregion
		}

        protected virtual bool ProcessIBindingListItem { get; set; }
		protected Func<TBase, TResult> _baseToResult;
		protected Func<TResult, TBase> _resultToBase;
		private IBindingList<TBase> _innerList;
		protected IList<TBase> BaseList { get { return _innerList; } }


        public BindingListWrapper(IList<TBase> baseList, Func<TBase, TResult> baseToResult, Func<TResult, TBase> resultToChild, bool processIBindingListItem = true)
        {
            baseList.OnStartCreatingList<TBase, TResult, BindingListWrapper<TBase, TResult>>(processIBindingListItem);

            _baseToResult = baseToResult;
            _resultToBase = resultToChild;
            ProcessIBindingListItem = processIBindingListItem;
            UpdateInnerList(baseList);

            BindingListsObserver.OnListCreating<TBase, TResult, BindingListWrapper<TBase, TResult>>(this, baseList, processIBindingListItem);
        }

		public event BeforeRemoveEventHandler BeforeRemoveItem;

		protected void OnBeforeRemoveItem(int index)
		{
			if (BeforeRemoveItem == null)
				return;

			BeforeRemoveItem(this, new BeforeRemoveEventArgs(index));
		}

		public void SuspendEvents()
		{
			_innerList.SuspendEvents();
		}

		public void ResumeEvents(bool raiseReset)
		{
            _innerList.ResumeEvents(raiseReset);
		}

		public bool RaiseListChangedEvents
		{
			get { return _innerList.RaiseListChangedEvents; }
		}

		public void raiseReset()
		{
			OnListChanged(ListChangedType.Reset, -1, -1);
		}


        /// <summary>
        /// обновляет внутренний список и кидает событие ListChanged Reset
        /// </summary>
        /// <param name="list"></param>
		protected virtual void UpdateInnerList(IList<TBase> list)
		{
            if(ReferenceEquals(_innerList, list))
                return;

			OnInnerListChanging();

			_innerList = GetInnerListInstance(list);

			if(OnInnerListChanged())
                OnListChanged(ListChangedType.Reset, -1, -1);
		}

        /// <summary>
        /// если внутренний список есть - подписывается на события
        /// </summary>
        /// <returns>признак того, что надо кинуть ListChanged Reset</returns>
        protected virtual bool OnInnerListChanged()
	    {
	        _innerList.ListChanged += OnInnerListChanged;
            foreach (var x in BaseList)
                SubscribeDeleted(x as IBindingListItem);
            return true;
	    }

        /// <summary>
        /// если внутренний список есть - отписывается от всех событий
        /// </summary>
	    protected virtual void OnInnerListChanging()
	    {
	        if (BaseList != null)
	        {
	            _innerList.ListChanged -= OnInnerListChanged;
	            foreach (var x in BaseList)
	                UnsubscribeDeleted(x as IBindingListItem);
	        }
	    }

	    protected virtual TResult BaseToResult(TBase @base)
		{
			var result = _baseToResult(@base);
			ResubscribePropertyChanged(result as INotifyPropertyChanged);
			return result;

		}

        static readonly Dictionary<Type, FieldInfo> _npch = new Dictionary<Type, FieldInfo>();

	    protected virtual void ResubscribePropertyChanged(INotifyPropertyChanged npc)
		{
			if (npc == null)
				return;

	        var npce = npc as INotifyPropertyChangedExtended;
	        if (npce != null)
            {
                if (!npce.IsSubscribed(OnBOPropertyChanged))
                    npce.PropertyChanged += OnBOPropertyChanged;

                return;
            }

            var type = npc.GetType();
            if (!_npch.ContainsKey(type))
            {
                lock (_npch)
                {
                    if (!_npch.ContainsKey(type))
                    {
                        _npch.Add(type, GetNPCHEvent(type));
                    }
                }
            }

            if (_npch[type] == null)
            {
                npc.PropertyChanged -= OnBOPropertyChanged;
                npc.PropertyChanged += OnBOPropertyChanged;
            }
            else
            {
                PropertyChangedEventHandler eh = OnBOPropertyChanged;
                var value = (PropertyChangedEventHandler)_npch[type].GetValue(npc);
                if(value != null && !value.GetInvocationList().Contains(eh))
                    npc.PropertyChanged += OnBOPropertyChanged;
            }
		}

	    private static FieldInfo GetNPCHEvent(Type type)
	    {
            var fields = type
                .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.FieldType == typeof(PropertyChangedEventHandler))
                .ToArray()
                ;

	        return fields.Length == 1 ? fields[0] : null;
	    }

	    protected void OnBOPropertyChanged(object sender, PropertyChangedEventArgs e)
		{

			var contract = ResultToBase((TResult)sender);
			if (!BaseList_Contains(contract))
				((INotifyPropertyChanged)sender).PropertyChanged -= OnBOPropertyChanged;
			else
			{
				if (e.PropertyName == ">>")
					SuspendEvents();
				else if (e.PropertyName == "<<" && !RaiseListChangedEvents)
				{
					ResumeEvents(false);
				    var index = BaseList_IndexOf(contract);
				    OnListChanged(ListChangedType.ItemChanged, index, index);
				}
				else if (RaiseListChangedEvents)
					OnListChanged(ListChangedType.ItemChanged, BaseList_IndexOf(contract), new FakePropertyDescriptor(e.PropertyName));
			}
		}

		protected virtual TBase ResultToBase(TResult result)
		{
			return _resultToBase(result);
		}

		protected virtual IBindingList<TBase> GetInnerListInstance(IList<TBase> list)
		{
			return list as IBindingList<TBase> ?? new SimpleDataList<TBase>(list, ProcessIBindingListItem);
		}

		protected virtual void UnsubscribeDeleted(IBindingListItem item)
		{
		    if (!ProcessIBindingListItem || item == null)
				return;

			item.Deleted -= ItemDeletd;
		}

        protected virtual void SubscribeDeleted(IBindingListItem item)
		{
            if (!ProcessIBindingListItem || item == null)
                return;

			item.Deleted += ItemDeletd;
		}

		protected virtual void ItemDeletd(object sender, EventArgs e)
		{
			((IBindingListItem)sender).Deleted -= ItemDeletd;
			BaseList_Remove((TBase) sender);
		}

		protected virtual void OnInnerListChanged(object sender, ListChangedEventArgs e)
		{
			switch (e.ListChangedType)
			{
				case ListChangedType.Reset:
				case ListChangedType.ItemAdded:
				case ListChangedType.ItemDeleted:
				case ListChangedType.ItemMoved:
					OnListChanged(e.ListChangedType, e.NewIndex, e.OldIndex);
					break;
				case ListChangedType.ItemChanged:
					OnListChanged(e.ListChangedType, e.NewIndex, null/*new FakePropertyDescriptor("")*/);
					break;
				case ListChangedType.PropertyDescriptorAdded:
					break;
				case ListChangedType.PropertyDescriptorDeleted:
					break;
				case ListChangedType.PropertyDescriptorChanged:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region BaseList Methods

		protected virtual IEnumerator<TBase> BaseList_GetEnumerator()
		{
			return BaseList.GetEnumerator();
		}

		protected virtual void BaseList_Add(TBase item)
		{
			SubscribeDeleted(item as IBindingListItem);
			BaseList.Add(item);
		}

		protected virtual object BaseList_AddNew()
		{
			var value = _innerList.AddNew();
			SubscribeDeleted(value as IBindingListItem);
			return value;
		}

		protected virtual void BaseList_Clear()
		{
			SuspendEvents();
			try
			{
			    foreach (var x in BaseList)
			        UnsubscribeDeleted(x as IBindingListItem);
				BaseList.Clear();
			}
			finally
			{
				ResumeEvents(true);
			}
		}

		protected virtual bool BaseList_Contains(TBase item)
		{
            return BaseList.Contains(item);
		}

		protected virtual void BaseList_CopyTo(TBase[] array, int arrayIndex)
		{
            BaseList.CopyTo(array, arrayIndex);
		}

		protected virtual bool BaseList_Remove(TBase item)
		{
			if (!BaseList_Contains(item))
			{
				OnListChanged(ListChangedType.Reset, -1, -1);
				return false;
			}

			UnsubscribeDeleted(item as IBindingListItem);
			OnBeforeRemoveItem(BaseList_IndexOf(item));
            return BaseList.Remove(item);
		}

		protected virtual void BaseList_RemoveAt(int index)
		{
			UnsubscribeDeleted(BaseList[index] as IBindingListItem);
			OnBeforeRemoveItem(index);
			BaseList.RemoveAt(index);
		}

		protected virtual int BaseListCount
		{
			get { return BaseList.Count; }
		}

		protected virtual bool BaseListIsReadOnly
		{
			get { return BaseList.IsReadOnly; }
		}

		protected virtual bool BaseListIsFixedSize
		{
			get { return _innerList.IsFixedSize; }
		}

		protected virtual int BaseList_IndexOf(TBase item)
		{
            return BaseList.IndexOf(item);
		}

		protected virtual void BaseList_Insert(int index, TBase item)
		{
			SubscribeDeleted(item as IBindingListItem);
            BaseList.Insert(index, item);
		}

		protected virtual void BaseList_MoveTo(int index, TBase item)
		{
            if (!BaseList.Contains(item))
				return;

			BaseList_Remove(item);
			BaseList_Insert(index, item);
		}

		protected virtual TBase BaseList_Get(int index)
		{
			return BaseList[index];
		}

        protected virtual void Set(int index, TBase value)
        {
            SuspendEvents();

            BaseList_Set(index, value);

            ResumeEvents(false);
            OnListChanged(ListChangedType.ItemChanged, index, index);

            //UnsubscribeDeleted(BaseList[index] as IBindingListItem);
            //BaseList[index] = value;
            //SubscribeDeleted(BaseList[index] as IBindingListItem);
        }
		protected virtual void BaseList_Set(int index, TBase value)
		{
            var tmp = BaseList_Get(index);
            if (ReferenceEquals(tmp, value))
                return;

            BaseList_RemoveAt(index);
			BaseList_Insert(index, value);
		}

		#endregion

		#region ResultList methods

		public virtual IEnumerator<TResult> GetEnumerator()
		{
			return new EnumeratorWithConvertation<TBase, TResult>(BaseList, BaseToResult);
		}

		public virtual void Add(TResult item)
		{
			var value = ResultToBase(item);
			BaseList_Add(value);
		}

		public virtual bool Contains(TResult item)
		{
			return BaseList_Contains(ResultToBase(item));
		}

		public virtual void CopyTo(TResult[] array, int arrayIndex)
		{
			BaseList.Select(BaseToResult).ToArray().CopyTo(array, arrayIndex);
		}

		public virtual void Insert(int index, TResult item)
		{
			var value = ResultToBase(item);
			BaseList_Insert(index, value);
		}

		public virtual void Clear()
		{
			BaseList_Clear();
		}

		public virtual int IndexOf(TResult item)
		{
			return BaseList_IndexOf(ResultToBase(item));
		}

		public virtual bool Remove(TResult item)
		{
			var value = ResultToBase(item);
			return BaseList_Remove(value);
		}

		public virtual void RemoveAt(int index)
		{
			BaseList_RemoveAt(index);
		}

		public virtual TResult this[int index]
		{
			get
			{
				return BaseToResult(BaseList_Get(index));
			}
			set
			{
				Set(index, ResultToBase(value));
			}
		}

		public virtual bool IsReadOnly
		{
			get { return BaseListIsReadOnly; }
		}

		public virtual bool IsFixedSize
		{
			get { return BaseListIsFixedSize; }
		}

		public virtual object SyncRoot
		{
			get { return _innerList.SyncRoot; }
		}

		public virtual bool IsSynchronized
		{
			get { return _innerList.IsSynchronized; }
		}

		#endregion

		#region Implementation of IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of ICollection

		void ICollection.CopyTo(Array array, int index)
		{
			BaseList.Select(BaseToResult).ToArray().CopyTo(array, index);
		}

		int ICollection<TResult>.Count
		{
			get { return BaseList.Count; }
		}

		bool ICollection<TResult>.IsReadOnly
		{
			get { return IsReadOnly; }
		}

		int ICollection.Count
		{
			get { return BaseList.Count; }
		}

		#endregion

		#region Implementation of IList

		int IList.Add(object value)
		{
			var item = ResultToBase((TResult)value);
			BaseList_Add(item);

			return BaseListCount - 1;
		}

		bool IList.Contains(object value)
		{
			return Contains((TResult)value);
		}

		void ICollection<TResult>.Clear()
		{
			Clear();
		}

		int IList.IndexOf(object value)
		{
			return IndexOf((TResult)value);
		}

		void IList.Insert(int index, object value)
		{
			Insert(index, (TResult)value);
		}

		void IList.Remove(object value)
		{
			Remove((TResult)value);
		}

		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		object IList.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (TResult)value; }
		}

		#endregion

		#region Implementation of IBindingList

		object IBindingList.AddNew()
		{
			return ResultList_AddNew();
		}

		protected virtual object ResultList_AddNew()
		{
			return BaseToResult((TBase)BaseList_AddNew());
		}

		public virtual void AddIndex(PropertyDescriptor property)
		{
			_innerList.AddIndex(property);
		}

		public virtual void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			_innerList.ApplySort(property, direction);
		}

		public virtual int Find(PropertyDescriptor property, object key)
		{
			return _innerList.Find(property, key);
		}

		public virtual void RemoveIndex(PropertyDescriptor property)
		{
			_innerList.RemoveIndex(property);
		}

		public virtual void RemoveSort()
		{
			_innerList.RemoveSort();
		}

		public virtual bool AllowNew
		{
			get { return false; }
		}

		public virtual bool AllowEdit
		{
			get { return true; }
		}

		public virtual bool AllowRemove
		{
			get { return true; }
		}

		public virtual bool SupportsChangeNotification
		{
			get { return _innerList.SupportsChangeNotification; }
		}

		public virtual bool SupportsSearching
		{
			get { return _innerList.SupportsSearching; }
		}

		public virtual bool SupportsSorting
		{
			get { return _innerList.SupportsSorting; }
		}

		public virtual bool IsSorted
		{
			get { return _innerList.IsSorted; }
		}

		public virtual PropertyDescriptor SortProperty
		{
			get { return _innerList.SortProperty; }
		}

		public virtual ListSortDirection SortDirection
		{
			get { return _innerList.SortDirection; }
		}

		public event ListChangedEventHandler ListChanged;

		#endregion

		protected virtual void OnListChanged(ListChangedType changedType, int newIndex, PropertyDescriptor pd)
		{
			OnListChanged(new ListChangedEventArgs(changedType, newIndex, pd));
		}

		protected virtual void OnListChanged(ListChangedType changedType, int newIndex, int oldIndex)
		{
			OnListChanged(new ListChangedEventArgs(changedType, newIndex, oldIndex));
		}

		public virtual TBase this[TResult result]
		{
			get
			{
				var @base = ResultToBase(result);
				return BaseList.FirstOrDefault(x => Equals(x, @base));
			}
		}


		public void RaiseItemChanged(ListChangedEventArgs e)
		{
			OnListChanged(e);
		}

		protected void OnListChanged(ListChangedEventArgs e)
		{
			if (!RaiseListChangedEvents) 
				return;

            BindingListsObserver.OnListChanged(this, e);

            if (ListChanged != null)
			    ListChanged(this, e);
		}

	}

    public class SimpleDataList<T> : BindingList<T>, IBindingList<T>
    {
        protected virtual bool ProcessIBindingListItem { get; set; }

        public SimpleDataList(bool processIBindingListItem)
        {
            ProcessIBindingListItem = processIBindingListItem;
        }

        public SimpleDataList(IList<T> list, bool processIBindingListItem)
            : base(list ?? new List<T>())
        {
            ProcessIBindingListItem = processIBindingListItem;
        }

        public event BeforeRemoveEventHandler BeforeRemoveItem;

        protected void OnBeforeRemoveItem(int index)
        {
            if (BeforeRemoveItem == null)
                return;

            BeforeRemoveItem(this, new BeforeRemoveEventArgs(index));
        }

        readonly Stack<bool> _raiseListChangedEventsInfo = new Stack<bool>();

        public void SuspendEvents()
        {
            _raiseListChangedEventsInfo.Push(RaiseListChangedEvents);
            RaiseListChangedEvents = false;
        }

        public void ResumeEvents(bool raiseReset)
        {
            RaiseListChangedEvents = _raiseListChangedEventsInfo.Pop();
            if (RaiseListChangedEvents && raiseReset)
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected void UnsubscribeDeleted(IBindingListItem item)
        {
            if (!ProcessIBindingListItem || item == null)
                return;

            item.Deleted -= ItemDeletd;
        }

        protected void SubscribeDeleted(IBindingListItem item)
        {
            if (!ProcessIBindingListItem || item == null)
                return;

            item.Deleted += ItemDeletd;
        }

        protected virtual void ItemDeletd(object sender, EventArgs e)
        {
            ((IBindingListItem)sender).Deleted -= ItemDeletd;
            Remove((T)sender);
        }

        protected override object AddNewCore()
        {
            return base.AddNewCore();
        }

        public override void CancelNew(int itemIndex)
        {
            base.CancelNew(itemIndex);
        }

        public override void EndNew(int itemIndex)
        {
            base.EndNew(itemIndex);
        }

        protected override void SetItem(int index, T item)
        {
            if (ReferenceEquals(this[index], item))
                return;

            SuspendEvents();

            Remove(this[index]);
            InsertItem(index, item);

            ResumeEvents(false);
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index, index));

            //UnsubscribeDeleted(this[index] as IBindingListItem);
            //base.SetItem(index, item);
            //SubscribeDeleted(this[index] as IBindingListItem);
        }

        protected override void RemoveItem(int index)
        {
            UnsubscribeDeleted(this[index] as IBindingListItem);
            OnBeforeRemoveItem(index);
            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, T item)
        {
            SubscribeDeleted(item as IBindingListItem);
            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            SuspendEvents();
            foreach (var item in Items)
                UnsubscribeDeleted(item as IBindingListItem);

            base.ClearItems();
            ResumeEvents(true);
        }
    }

    
}
