using System;
using System.Collections;
using System.Collections.Generic;

namespace KAG.Unity.Common.Observables
{
	public sealed class ObservableList<T> : IObservableList, IList<T>, IReadOnlyList<T>
	{
		public event ListElementAddedEventHandler OnElementAdded;
		public event ListElementInsertedEventHandler OnElementInserted;
		public event ListElementRemovedEventHandler OnElementRemoved;
		public event ListClearedEventHandler OnCleared;
		
		public int Count => _implementation.Count;
		public bool IsReadOnly => ((IList)_implementation).IsReadOnly;
		public bool IsFixedSize => ((IList)_implementation).IsFixedSize;
		public bool IsSynchronized => ((ICollection)_implementation).IsSynchronized;
		public object SyncRoot => ((ICollection)_implementation).SyncRoot;
		
		private List<T> _implementation;

		public ObservableList() => 
			_implementation = new List<T>();
		public ObservableList(int capacity) => 
			_implementation = new List<T>(capacity);
		public ObservableList(IEnumerable<T> enumerable) => 
			_implementation = new List<T>(enumerable);
		
		object IList.this[int index]
		{
			get => ((IList)_implementation)[index];
			set => ((IList)_implementation)[index] = value;
		}
		public T this[int index]
		{
			get => _implementation[index];
			set => _implementation[index] = value;
		}
		
		public bool Contains(object value) => 
			((IList)_implementation).Contains(value);
		public bool Contains(T item) => 
			_implementation.Contains(item);
		
		public int IndexOf(object value) => 
			((IList)_implementation).IndexOf(value);
		public int IndexOf(T item) => 
			_implementation.IndexOf(item);
		
		public int Add(object value)
		{
			var output = ((IList)_implementation).Add(value);
			OnElementAdded?.Invoke(this, new ListElementAddedEventArgs(value));

			return output;
		}
		public void Add(T item)
		{
			_implementation.Add(item);
			OnElementAdded?.Invoke(this, new ListElementAddedEventArgs(item));
		}

		public void Insert(int index, object value)
		{
			((IList)_implementation).Insert(index, value);
			OnElementInserted?.Invoke(this, new ListElementInsertedEventArgs(value, index));
		}
		public void Insert(int index, T item)
		{
			_implementation.Insert(index, item);
			OnElementInserted?.Invoke(this, new ListElementInsertedEventArgs(item, index));
		}
		
		public void Remove(object value)
		{
			var asNonGenericList = (IList)_implementation;
			var index = asNonGenericList.IndexOf(value);
			asNonGenericList.RemoveAt(index);
			
			OnElementRemoved?.Invoke(this, new ListElementRemovedEventArgs(value, index));
		}
		public bool Remove(T item)
		{
			var index = IndexOf(item);
			if (index == -1)
				return false;

			_implementation.RemoveAt(index);
			OnElementRemoved?.Invoke(this, new ListElementRemovedEventArgs(item, index));
			return true;
		}

		public void RemoveAt(int index)
		{
			var item = _implementation[index];
			_implementation.RemoveAt(index);
			
			OnElementRemoved?.Invoke(this, new ListElementRemovedEventArgs(item, index));
		}

		public void Clear()
		{
			_implementation.Clear();
			OnCleared?.Invoke(this);
		}

		public void CopyTo(Array array, int index) => 
			((ICollection)_implementation).CopyTo(array, index);
		public void CopyTo(T[] array, int arrayIndex) => 
			_implementation.CopyTo(array, arrayIndex);
		
		IEnumerator IEnumerable.GetEnumerator() => 
			((IEnumerable)_implementation).GetEnumerator();
		public IEnumerator<T> GetEnumerator() => 
			_implementation.GetEnumerator();
	}
}