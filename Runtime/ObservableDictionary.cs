using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	public enum DictionaryUpdateType
	{
		Added,
		Updated,
		Removed
	}
	
	/// <summary>
	/// A simple dictionary with the possibility to observe changes to it's elements defined <see cref="DictionaryUpdateType"/> rules
	/// </summary>
	public interface IObservableDictionary
	{
	}

	/// <inheritdoc />
	public interface IObservableDictionaryCollection<TKey, out TValue> : IObservableDictionary
	{
		/// <summary>
		/// Observes this dictionary with the given <paramref name="onUpdate"/> when the given <paramref name="key"/> data
		/// changes following the rule of the given <paramref name="updateType"/>
		/// </summary>
		void Observe(TKey key, ListUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <summary>
		/// Observes this dictionary with the given <paramref name="onUpdate"/> when any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void Observe(ListUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this dictionary with the given <paramref name="onUpdate"/> of the given <paramref name="key"/> data
		/// changes following the rule of the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(TKey key, ListUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this dictionary with the given <paramref name="onUpdate"/> of any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(ListUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this dictionary updates for the given <paramref name="key"/>
		/// </summary>
		void StopObserving(TKey key);
	}

	/// <inheritdoc cref="IObservableDictionary" />
	public interface IObservableReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IObservableDictionaryCollection<TKey, TValue>
		where TValue : struct
	{
	}

	/// <inheritdoc cref="IObservableDictionary" />
	public interface IObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IObservableDictionaryCollection<TKey, TValue>
		where TValue : struct
	{
	}

	/// <inheritdoc cref="IObservableDictionary" />
	public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IObservableDictionary<TKey, TValue>, IObservableReadOnlyDictionary<TKey, TValue>
		where TValue : struct
	{
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onAddActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onUpdateActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onRemoveActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IReadOnlyDictionary<int, IList<Action<TKey, TValue>>> _genericUpdateActions = 
			new ReadOnlyDictionary<int, IList<Action<TKey, TValue>>>(new Dictionary<int, IList<Action<TKey, TValue>>>
			{
				{(int) ListUpdateType.Added, new List<Action<TKey, TValue>>()},
				{(int) ListUpdateType.Removed, new List<Action<TKey, TValue>>()},
				{(int) ListUpdateType.Updated, new List<Action<TKey, TValue>>()}
			});

		public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
		{
			foreach (var pair in dictionary)
			{
				Add(pair.Key, pair.Value);
			}
		}

		public ObservableDictionary(Func<TValue, TKey> referenceIdResolver, IList<TValue> list)
		{
			for (var i = 0; i < list.Count; i++)
			{
				Add(referenceIdResolver(list[i]), list[i]);
			}
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}.this" />
		public new TValue this[TKey key]
		{
			get => base[key];
			set
			{
				base[key] = value;
 
				if (_onUpdateActions.TryGetValue(key, out var actions))
				{
					for (var i = 0; i < actions.Count; i++)
					{
						actions[i](key, value);
					}
				}

				var updates = _genericUpdateActions[(int) ListUpdateType.Updated];
				for (var i = 0; i < updates.Count; i++)
				{
					updates[i](key, value);
				}
			}
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Add" />
		public new void Add(TKey key, TValue value)
		{
			base.Add(key, value);
			
			if (_onAddActions.TryGetValue(key, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](key, value);
				}
			}

			var updates = _genericUpdateActions[(int) ListUpdateType.Added];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](key, value);
			}
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Remove" />
		public new bool Remove(TKey key)
		{
			TryGetValue(key, out TValue value);

			if (base.Remove(key))
			{
				return true;
			}
			
			if (_onRemoveActions.TryGetValue(key, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](key, value);
				}
			}

			var updates = _genericUpdateActions[(int) ListUpdateType.Removed];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](key, value);
			}

			return false;
		}
		
		/// <inheritdoc />
		public void Observe(TKey key, ListUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			switch (updateType)
			{
				case ListUpdateType.Added:
					if (!_onAddActions.TryGetValue(key, out var addList))
					{
						addList = new List<Action<TKey, TValue>>();
						
						_onAddActions.Add(key, addList);
					}
					
					addList.Add(onUpdate);
					break;
				case ListUpdateType.Updated:
					if (!_onUpdateActions.TryGetValue(key, out var updateList))
					{
						updateList = new List<Action<TKey, TValue>>();
						
						_onUpdateActions.Add(key, updateList);
					}
					
					updateList.Add(onUpdate);
					break;
				case ListUpdateType.Removed:
					if (!_onRemoveActions.TryGetValue(key, out var removeList))
					{
						removeList = new List<Action<TKey, TValue>>();
						
						_onRemoveActions.Add(key, removeList);
					}
					
					removeList.Add(onUpdate);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(updateType), updateType, "Wrong update type");
			}
		}

		/// <inheritdoc />
		public void Observe(ListUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Add(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(TKey key, ListUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			switch (updateType)
			{
				case ListUpdateType.Added:
					if (_onAddActions.TryGetValue(key, out var addList))
					{
						addList.Remove(onUpdate);
					}
					break;
				case ListUpdateType.Updated:
					if (_onUpdateActions.TryGetValue(key, out var updateList))
					{
						updateList.Remove(onUpdate);
					}
					break;
				case ListUpdateType.Removed:
					if (_onRemoveActions.TryGetValue(key, out var removeList))
					{
						removeList.Remove(onUpdate);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(updateType), updateType, "Wrong update type");
			}
		}
		
		/// <inheritdoc />
		public void StopObserving(ListUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Remove(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(TKey key)
		{
			if (_onAddActions.TryGetValue(key, out var addList))
			{
				addList.Clear();

				_onAddActions.Remove(key);
			}
			if (_onUpdateActions.TryGetValue(key, out var updateList))
			{
				updateList.Clear();

				_onUpdateActions.Remove(key);
			}
			if (_onRemoveActions.TryGetValue(key, out var removeList))
			{
				removeList.Clear();

				_onRemoveActions.Remove(key);
			}
		}
	}
}