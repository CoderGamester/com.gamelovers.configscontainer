using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <summary>
	/// A list with the possibility to observe changes to it's elements defined <see cref="ObservableUpdateType"/> rules
	/// </summary>
	public interface IObservableListReader
	{
		/// <summary>
		/// Requests the list element count
		/// </summary>
		int Count { get; }
	}
	
	/// <inheritdoc />
	/// <remarks>
	/// Read only observable list interface
	/// </remarks>
	public interface IObservableListReader<out T> :IObservableListReader where T : struct
	{
		/// <summary>
		/// Looks up and return the data that is associated with the given <paramref name="index"/>
		/// </summary>
		T this[int index] { get; }
		
		/// <summary>
		/// Requests this list as a <see cref="IReadOnlyList{T}"/>
		/// </summary>
		IReadOnlyList<T> ReadOnlyList { get; }
		
		/// <summary>
		/// Observes this list with the given <paramref name="onUpdate"/> when any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void Observe(ObservableUpdateType updateType, Action<int, T> onUpdate);
		
		/// <summary>
		/// Stops observing this list with the given <paramref name="onUpdate"/> of any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(ObservableUpdateType updateType, Action<int, T> onUpdate);
	}

	/// <inheritdoc />
	public interface IObservableList<T> : IObservableListReader<T> where T : struct
	{
		/// <summary>
		/// Changes the given <paramref name="index"/> in the list. If the data does not exist it will be added.
		/// It will notify any observer listing to its data
		/// </summary>
		new T this[int index] { get; set; }

		/// <summary>
		/// Returns this list reference as an <see cref="IList{T}"/>
		/// </summary>
		IList<T> List { get; }
		
		/// <summary>
		/// Add the given <paramref name="data"/> to the list.
		/// It will notify any observer listing to its data
		/// </summary>
		void Add(T data);
		
		/// <summary>
		/// Removes the data associated with the given <paramref name="index"/>
		/// </summary>
		/// <exception cref="IndexOutOfRangeException">
		/// Thrown if the given <paramref name="index"/> is out of the range of the list size
		/// </exception>
		void Remove(int index);
	}
	
	/// <inheritdoc />
	public class ObservableList<T> : IObservableList<T> where T : struct
	{
		private readonly Func<IList<T>> _listResolver;
		private readonly IReadOnlyDictionary<int, IList<Action<int, T>>> _genericUpdateActions = 
			new ReadOnlyDictionary<int, IList<Action<int, T>>>(new Dictionary<int, IList<Action<int, T>>>
			{
				{(int) ObservableUpdateType.Added, new List<Action<int, T>>()},
				{(int) ObservableUpdateType.Removed, new List<Action<int, T>>()},
				{(int) ObservableUpdateType.Updated, new List<Action<int, T>>()}
			});

		/// <inheritdoc cref="IObservableList{T}.this" />
		public T this[int index]
		{
			get => _listResolver()[index];
			set
			{
				_listResolver()[index] = value;
				
				var updates = _genericUpdateActions[(int) ObservableUpdateType.Updated];
				for (var i = 0; i < updates.Count; i++)
				{
					updates[i](i, value);
				}
			}
		}
		
		/// <inheritdoc />
		public int Count => _listResolver().Count;
		/// <inheritdoc />
		public IList<T> List => _listResolver();
		/// <inheritdoc />
		public IReadOnlyList<T> ReadOnlyList => new ReadOnlyCollection<T>(_listResolver());
		
		public ObservableList(Func<IList<T>> listResolver)
		{
			_listResolver = listResolver;
		}
		
		/// <inheritdoc />
		public void Add(T data)
		{
			_listResolver().Add(data);

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Added];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](i, data);
			}
		}

		/// <inheritdoc />
		public void Remove(int index)
		{
			var data = _listResolver()[index];
			
			_listResolver().RemoveAt(index);

			var updates = _genericUpdateActions[(int) ObservableUpdateType.Removed];
			for (var i = 0; i < updates.Count; i++)
			{
				updates[i](i, data);
			}
		}

		/// <inheritdoc />
		public void Observe(ObservableUpdateType updateType, Action<int, T> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Add(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(ObservableUpdateType updateType, Action<int, T> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Remove(onUpdate);
		}
	}
}