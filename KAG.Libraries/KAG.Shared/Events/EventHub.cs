using System;
using System.Collections.Generic;

namespace KAG.Shared.Events
{
	public sealed class EventHub
	{
		#region Nested types

		private abstract class Wrapper
		{
			public int UsageCount { get; set;}
			
			public abstract void Invoke(object sender, object rawEventArgs);
			public abstract void Register(object rawDel);
			public abstract void Unregister(object del);
		}
		private sealed class Wrapper<TArgs> : Wrapper
			where TArgs : EventArgs
		{
			private event Action<object, TArgs> _handler;
	
			public override void Invoke(object sender, object rawEventArgs)
			{
				if (!(rawEventArgs is TArgs eventArgs))
				{
					throw new InvalidCastException(
						$"Cannot invoke `{nameof(_handler)}={_handler.Method.Name}` with `{nameof(rawEventArgs)}={rawEventArgs}`. "
						+ $"The expected argument type is {typeof(TArgs).Name}.");
				}

				Invoke(sender, eventArgs);
			}
			public void Invoke(object sender, TArgs eventArgs) =>
				_handler?.Invoke(sender, eventArgs);
			
			public override void Register(object rawDel)
			{
				if (!(rawDel is Action<object, TArgs> del))
					throw new InvalidCastException($"Cannot register `{nameof(rawDel)}={rawDel}` as it's not an Action<object, {typeof(TArgs).Name}>.");

				Register(del);
			}
			public void Register(Action<object, TArgs> del) =>
				_handler += del;
			
			public override void Unregister(object rawDel)
			{
				if (!(rawDel is Action<object, TArgs> del))
					throw new InvalidCastException($"Cannot unregister `{nameof(rawDel)}={rawDel}` as it's not an Action<object, {typeof(TArgs).Name}>.");

				Unregister(del);
			}
			public void Unregister(Action<object, TArgs> del) =>
				_handler -= del;
		}

		#endregion

		private Dictionary<ushort, Type> _wrapperMappings;
		private Dictionary<Type, Wrapper> _wrappers;

		public EventHub()
		{
			_wrapperMappings = new Dictionary<ushort, Type>();
			_wrappers = new Dictionary<Type, Wrapper>();
		}

		public void Define<TArgs>(ushort evtKey) 
			where TArgs : EventArgs
		{
			if (_wrapperMappings.ContainsKey(evtKey))
				throw new InvalidOperationException($"An definition already exists for `{nameof(evtKey)}={evtKey}`.");

			if (!_wrappers.TryGetValue(typeof(TArgs), out var wrapper))
			{
				wrapper = new Wrapper<TArgs>();
				_wrappers.Add(typeof(TArgs), wrapper);
			}

			wrapper.UsageCount++;
			_wrapperMappings.Add(evtKey, typeof(TArgs));
		}

		public bool Remove(ushort evtKey)
		{
			if (!_wrapperMappings.TryGetValue(evtKey, out var argType))
				return false;

			_wrapperMappings.Remove(evtKey);

			var wrapper = _wrappers[argType];
			wrapper.UsageCount--;

			if (wrapper.UsageCount == 0)
				_wrappers.Remove(argType);
			
			return true;
		}
		
		public void Subscribe<TArgs>(ushort evtKey, Action<object, TArgs> del)
			where TArgs : EventArgs
		{
			var wrapper = GetWrapper<TArgs>(evtKey);
			wrapper.Register(del);
		}
		
		public void Unsubscribe<TArgs>(ushort evtKey, Action<object, TArgs> del)
			where TArgs : EventArgs
		{
			var wrapper = GetWrapper<TArgs>(evtKey);
			wrapper.Unregister(del);
		}

		public void Invoke<TArgs>(ushort evtKey, object sender, TArgs args)
			where TArgs : EventArgs
		{
			var wrapper = GetWrapper<TArgs>(evtKey);
			wrapper.Invoke(sender, args);
		}

		private Wrapper<TArgs> GetWrapper<TArgs>(ushort evtKey)
			where TArgs : EventArgs
		{
			if (!_wrapperMappings.TryGetValue(evtKey, out var argType))
				throw new InvalidOperationException($"No definition exists for `{nameof(evtKey)}={evtKey}`.");

			if (argType != typeof(TArgs))
				throw new InvalidOperationException($"The existing definition for `{nameof(evtKey)}={evtKey}` maps to {argType.Name} & not {typeof(TArgs).Name}.");

			return (Wrapper<TArgs>)_wrappers[argType];
		}
	}
}