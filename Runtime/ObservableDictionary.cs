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
		/// <summary>
		/// Requests the element count of this dictionary
		/// </summary>
		int Count { get; }
	}

	/// <inheritdoc />
	public interface IObservableDictionaryReader<TKey, TValue> : IObservableDictionary
	{
		/// <summary>
		/// Looks up and return the data that is associated with the given <paramref name="key"/>
		/// </summary>
		TValue this[TKey key] { get; }
			
		/// <inheritdoc cref="Dictionary{TKey,TValue}.TryGetValue" />
		bool TryGetValue(TKey key, out TValue value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.ContainsKey" />
		bool ContainsKey(TKey key);
		
		/// <summary>
		/// Requests this dictionary as a <see cref="IReadOnlyDictionary{TKey,TValue}"/>
		/// </summary>
		IReadOnlyDictionary<TKey, TValue> GetReadOnlyDictionary();
		
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

	/// <inheritdoc />
	public interface IObservableDictionary<TKey, TValue> : IObservableDictionaryReader<TKey, TValue>
		where TValue : struct
	{
		/// <summary>
		/// Changes the given <paramref name="key"/> in the dictionary.
		/// It will notify any observer listing to its data
		/// </summary>
		new TValue this[TKey key] { get; set; }

		/// <summary>
		/// Returns this dictionary reference as an <see cref="IDictionary{TKey,TValue}"/>
		/// </summary>
		IDictionary<TKey, TValue> GetDictionary();

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Add" />
		void Add(TKey key, TValue value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Remove" />
		bool Remove(TKey key);
	}

	/// <inheritdoc />
	public class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>
		where TValue : struct
	{
		private readonly IDictionary<TKey, TValue> _dictionary;
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onAddActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onUpdateActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onRemoveActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<int, IList<Action<TKey, TValue>>> _genericUpdateActions = new Dictionary<int, IList<Action<TKey, TValue>>>
		{
			{(int) ListUpdateType.Added, new List<Action<TKey, TValue>>()},
			{(int) ListUpdateType.Removed, new List<Action<TKey, TValue>>()},
			{(int) ListUpdateType.Updated, new List<Action<TKey, TValue>>()}
		};

		public int Count => _dictionary.Count;
		
		private ObservableDictionary() {}

		public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
		{
			_dictionary = dictionary;
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}.this" />
		public TValue this[TKey key]
		{
			get => _dictionary[key];
			set
			{
				_dictionary[key] = value;
 
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

		/// <inheritdoc />
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		/// <inheritdoc />
		public bool ContainsKey(TKey key)
		{
			return _dictionary.ContainsKey(key);
		}

		/// <inheritdoc />
		public IReadOnlyDictionary<TKey, TValue> GetReadOnlyDictionary()
		{
			return new ReadOnlyDictionary<TKey, TValue>(_dictionary);
		}

		/// <inheritdoc />
		public IDictionary<TKey, TValue> GetDictionary()
		{
			return _dictionary;
		}

		/// <inheritdoc />
		public void Add(TKey key, TValue value)
		{
			_dictionary.Add(key, value);
			
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

		/// <inheritdoc />
		public bool Remove(TKey key)
		{
			var ret = false;

			if (_dictionary.TryGetValue(key, out var value))
			{
				ret = true;

				_dictionary.Remove(key);
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

			return ret;
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