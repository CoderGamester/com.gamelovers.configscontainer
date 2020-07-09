using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <summary>
	/// A simple dictionary with the possibility to observe changes to it's elements defined <see cref="ObservableUpdateType"/> rules
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
		
		/// <summary>
		/// Requests this dictionary as a <see cref="IReadOnlyDictionary{TKey,TValue}"/>
		/// </summary>
		IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary { get; }
			
		/// <inheritdoc cref="Dictionary{TKey,TValue}.TryGetValue" />
		bool TryGetValue(TKey key, out TValue value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.ContainsKey" />
		bool ContainsKey(TKey key);
		
		/// <summary>
		/// Observes this dictionary with the given <paramref name="onUpdate"/> when the given <paramref name="key"/> data
		/// changes following the rule of the given <paramref name="updateType"/>
		/// </summary>
		void Observe(TKey key, ObservableUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <inheritdoc cref="Observe(TKey,GameLovers.ObservableUpdateType,System.Action{TKey,TValue})" />
		/// <remarks>
		/// It invokes the given <paramref name="onUpdate"/> method before starting to observe to this dictionary
		/// </remarks>
		void InvokeObserve(TKey id, ObservableUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <summary>
		/// Observes this dictionary with the given <paramref name="onUpdate"/> when any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void Observe(ObservableUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this dictionary with the given <paramref name="onUpdate"/> of the given <paramref name="key"/> data
		/// changes following the rule of the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(TKey key, ObservableUpdateType updateType, Action<TKey, TValue> onUpdate);
		
		/// <summary>
		/// Stops observing this dictionary with the given <paramref name="onUpdate"/> of any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(ObservableUpdateType updateType, Action<TKey, TValue> onUpdate);
		
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
		IDictionary<TKey, TValue> Dictionary { get; }

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Add" />
		void Add(TKey key, TValue value);

		/// <inheritdoc cref="Dictionary{TKey,TValue}.Remove" />
		bool Remove(TKey key);
	}

	/// <inheritdoc />
	public class ObservableDictionary<TKey, TValue> : IObservableDictionary<TKey, TValue>
		where TValue : struct
	{
		private readonly Func<IDictionary<TKey, TValue>> _dictionaryResolver;
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onAddActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onUpdateActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<TKey, IList<Action<TKey, TValue>>> _onRemoveActions = new Dictionary<TKey, IList<Action<TKey, TValue>>>();
		private readonly IDictionary<int, IList<Action<TKey, TValue>>> _genericUpdateActions = new Dictionary<int, IList<Action<TKey, TValue>>>
		{
			{(int) ObservableUpdateType.Added, new List<Action<TKey, TValue>>()},
			{(int) ObservableUpdateType.Removed, new List<Action<TKey, TValue>>()},
			{(int) ObservableUpdateType.Updated, new List<Action<TKey, TValue>>()}
		};

		/// <inheritdoc />
		public int Count => _dictionaryResolver().Count;
		/// <inheritdoc />
		public IReadOnlyDictionary<TKey, TValue> ReadOnlyDictionary => new ReadOnlyDictionary<TKey, TValue>(_dictionaryResolver());
		/// <inheritdoc />
		public IDictionary<TKey, TValue> Dictionary => _dictionaryResolver();
		
		private ObservableDictionary() {}

		public ObservableDictionary(Func<IDictionary<TKey, TValue>> dictionaryResolver)
		{
			_dictionaryResolver = dictionaryResolver;
		}

		/// <inheritdoc cref="Dictionary{TKey,TValue}.this" />
		public TValue this[TKey key]
		{
			get => _dictionaryResolver()[key];
			set
			{
				_dictionaryResolver()[key] = value;
 
				if (_onUpdateActions.TryGetValue(key, out var actions))
				{
					for (var i = 0; i < actions.Count; i++)
					{
						actions[i](key, value);
					}
				}

				var updates = _genericUpdateActions[(int) ObservableUpdateType.Updated];
				for (var i = 0; i < updates.Count; i++)
				{
					updates[i](key, value);
				}
			}
		}

		/// <inheritdoc />
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dictionaryResolver().TryGetValue(key, out value);
		}

		/// <inheritdoc />
		public bool ContainsKey(TKey key)
		{
			return _dictionaryResolver().ContainsKey(key);
		}

		/// <inheritdoc />
		public void Add(TKey key, TValue value)
		{
			_dictionaryResolver().Add(key, value);
			
			if (_onAddActions.TryGetValue(key, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](key, value);
				}
			}

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Added];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](key, value);
			}
		}

		/// <inheritdoc />
		public bool Remove(TKey key)
		{
			var ret = false;

			if (_dictionaryResolver().TryGetValue(key, out var value))
			{
				ret = true;

				_dictionaryResolver().Remove(key);
			}
			
			if (_onRemoveActions.TryGetValue(key, out var actions))
			{
				for (var i = 0; i < actions.Count; i++)
				{
					actions[i](key, value);
				}
			}

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Removed];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](key, value);
			}

			return ret;
		}

		/// <inheritdoc />
		public void Observe(TKey key, ObservableUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			switch (updateType)
			{
				case ObservableUpdateType.Added:
					if (!_onAddActions.TryGetValue(key, out var addList))
					{
						addList = new List<Action<TKey, TValue>>();
						
						_onAddActions.Add(key, addList);
					}
					
					addList.Add(onUpdate);
					break;
				case ObservableUpdateType.Updated:
					if (!_onUpdateActions.TryGetValue(key, out var updateList))
					{
						updateList = new List<Action<TKey, TValue>>();
						
						_onUpdateActions.Add(key, updateList);
					}
					
					updateList.Add(onUpdate);
					break;
				case ObservableUpdateType.Removed:
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
		public void InvokeObserve(TKey id, ObservableUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			onUpdate(id, _dictionaryResolver()[id]);
			
			Observe(id, updateType, onUpdate);
		}

		/// <inheritdoc />
		public void Observe(ObservableUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Add(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(TKey key, ObservableUpdateType updateType, Action<TKey, TValue> onUpdate)
		{
			switch (updateType)
			{
				case ObservableUpdateType.Added:
					if (_onAddActions.TryGetValue(key, out var addList))
					{
						addList.Remove(onUpdate);
					}
					break;
				case ObservableUpdateType.Updated:
					if (_onUpdateActions.TryGetValue(key, out var updateList))
					{
						updateList.Remove(onUpdate);
					}
					break;
				case ObservableUpdateType.Removed:
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
		public void StopObserving(ObservableUpdateType updateType, Action<TKey, TValue> onUpdate)
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