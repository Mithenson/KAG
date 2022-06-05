using System;
using System.Collections;
using System.Collections.Generic;
using KAG.Unity.Common.Observables;

namespace KAG.Unity.Common.DataBindings
{
	public class ListDataBinding : IDataBinding
	{
		public bool IsActive { get; private set; }
		
		protected readonly IObservableList _sourceList;
		protected readonly IDataBindingConverter _converter;
		protected readonly IList _targetList;
		
		public ListDataBinding(IObservableList sourceList, IDataBindingConverter converter, IList targetList, bool isActive = true)
		{
			_sourceList = sourceList;
			_converter = converter;
			_targetList = targetList;
			
			IMP_SetActive(isActive);
		}

		public void SetActive(bool value)
		{
			if (IsActive == value)
				return;

			IMP_SetActive(value);
		}
		private void IMP_SetActive(bool value)
		{
			if (!value)
			{
				IsActive = false;
				
				_sourceList.OnElementAdded -= OnElementAdded;
				_sourceList.OnElementInserted -= OnElementInserted;
				_sourceList.OnElementRemoved -= OnElementRemoved;
				_sourceList.OnCleared -= OnCleared;
			}
			else
			{
				IsActive = true;

				foreach (var item in _sourceList)
				{
					if (!_converter.TryConvert(item, out var convertedItem))
						throw GetConversionException(item);
					
					_targetList.Add(convertedItem);
				}
				
				_sourceList.OnElementAdded += OnElementAdded;
				_sourceList.OnElementInserted += OnElementInserted;
				_sourceList.OnElementRemoved += OnElementRemoved;
				_sourceList.OnCleared += OnCleared;
			}
		}

		protected virtual void OnElementAdded(object _, ListElementAddedEventArgs args)
		{
			if (!_converter.TryConvert(args.Item, out var convertedItem))
				throw GetConversionException(args.Item);
			
			_targetList.Add(convertedItem);
		}

		protected virtual void OnElementInserted(object _, ListElementInsertedEventArgs args)
		{
			if (!_converter.TryConvert(args.Item, out var convertedItem))
				throw GetConversionException(args.Item);
			
			_targetList.Insert(args.Index, convertedItem);
		}

		protected virtual void OnElementRemoved(object _, ListElementRemovedEventArgs args) =>
			_targetList.RemoveAt(args.Index);

		protected virtual void OnCleared(object _) =>
			_targetList.Clear();
		
		private InvalidOperationException GetConversionException(object item) =>
			throw new InvalidOperationException($"Conversion failed for `{nameof(item)}={item}`, this isn't allowed for collections.");

		public void Dispose()
		{
			IMP_SetActive(false);
			
			if (_converter is IDisposable disposableConverter)
				disposableConverter.Dispose();
		}
	}
	
	public sealed class ListDataBinding<TSource, TTarget> : ListDataBinding
	{
		public ListDataBinding(ObservableList<TSource> sourceList, IDataBindingConverter<TSource, TTarget> converter, IList<TTarget> targetList, bool isActive = true) 
			: base(sourceList, converter, (IList)targetList, isActive) { }
	}
}