using System.Collections.Generic;

// ReSharper disable once CheckNamespace

namespace GameLovers
{
	/// <inheritdoc cref="IIdList{TKey,TValue}" />
	/// <remarks>
	/// It is acts as a list with capabilities to observe it for changes within it's container
	/// </remarks>
	public class ObservableList<T> : IdList<int, T> where T : struct
	{
		public ObservableList(IList<T> list) : base(list.IndexOf, list)
		{
		}
	}
}