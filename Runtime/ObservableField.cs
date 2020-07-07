using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <summary>
	/// A field with the possibility to observe changes to it's elements defined <see cref="ObservableUpdateType"/> rules
	/// </summary>
	public interface IObservableFieldReader<out T>
	{
		/// <summary>
		/// The field value
		/// </summary>
		T Value { get; }
		
		/// <summary>
		/// Observes this field with the given <paramref name="onUpdate"/> when any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void Observe(ObservableUpdateType updateType, Action<T> onUpdate);
		
		/// <summary>
		/// Stops observing this field with the given <paramref name="onUpdate"/> of any data changes following the rule of
		/// the given <paramref name="updateType"/>
		/// </summary>
		void StopObserving(ObservableUpdateType updateType, Action<T> onUpdate);
	}

	/// <inheritdoc />
	public interface IObservableField<T> : IObservableFieldReader<T>
	{
		/// <summary>
		/// The field value with possibility to be changed
		/// </summary>
		new T Value { get; set; }
	}
	
	/// <inheritdoc />
	public class ObservableField<T> : IObservableField<T>
	{
		private readonly Func<T> _fieldResolver;
		private readonly Action<T> _fieldSetter;
		private readonly IReadOnlyDictionary<int, IList<Action<T>>> _genericUpdateActions = 
			new ReadOnlyDictionary<int, IList<Action<T>>>(new Dictionary<int, IList<Action<T>>>
			{
				{(int) ObservableUpdateType.Added, new List<Action<T>>()},
				{(int) ObservableUpdateType.Removed, new List<Action<T>>()},
				{(int) ObservableUpdateType.Updated, new List<Action<T>>()}
			});

		/// <inheritdoc cref="IObservableField{T}.Value" />
		public T Value
		{
			get => _fieldResolver();
			set => _fieldSetter(value);
		}
		
		private ObservableField() {}
 
		public ObservableField(Func<T> fieldResolver, Action<T> fieldSetter)
		{
			_fieldResolver = fieldResolver;
			_fieldSetter = fieldSetter;
		}

		/// <inheritdoc />
		public void Observe(ObservableUpdateType updateType, Action<T> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Add(onUpdate);
		}

		/// <inheritdoc />
		public void StopObserving(ObservableUpdateType updateType, Action<T> onUpdate)
		{
			_genericUpdateActions[(int) updateType].Remove(onUpdate);
		}
	}
}