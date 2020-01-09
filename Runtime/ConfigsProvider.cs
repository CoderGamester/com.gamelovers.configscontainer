using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace

namespace GameLovers.ConfigsContainer
{
	/// <summary>
	/// Provides all the Game's config static data, including the game design data
	/// Has the imported data from the Universal Google Sheet file on the web
	/// </summary>
	public interface IConfigsProvider
	{
		/// <summary>
		/// Requests the Config of <typeparamref name="T"/> type and with the given <paramref name="id"/>
		/// </summary>
		T GetConfig<T>(int id);

		/// <summary>
		/// Requests the Config List of <typeparamref name="T"/> type
		/// </summary>
		IReadOnlyList<T> GetConfigsList<T>();

		/// <summary>
		/// Requests the Config Dictionary of <typeparamref name="T"/> type
		/// </summary>
		IReadOnlyDictionary<int, T> GetConfigsDictionary<T>();
	}
	
	/// <inheritdoc />
	public class ConfigsProvider : IConfigsProvider
	{
		private readonly IDictionary<Type, IEnumerable> _configs = new Dictionary<Type, IEnumerable>();

		/// <inheritdoc />
		public T GetConfig<T>(int id)
		{
			return GetConfigsDictionary<T>()[id];
		}

		/// <inheritdoc />
		public IReadOnlyList<T> GetConfigsList<T>()
		{
			return new List<T>(GetConfigsDictionary<T>().Values);
		}

		/// <inheritdoc />
		public IReadOnlyDictionary<int, T> GetConfigsDictionary<T>() 
		{
			return _configs[typeof(T)] as IReadOnlyDictionary<int, T>;
		}

		/// <summary>
		/// Adds the given <paramref name="configList"/> to the container
		/// </summary>
		public void AddConfigs<T>(IList<T> configList) where T : struct, IConfig
		{
			var dictionary = new Dictionary<int, T>();

			for (int i = 0; i < configList.Count; i++)
			{
				dictionary.Add(configList[i].ConfigId, configList[i]);
			}

			_configs.Add(typeof(T), new ReadOnlyDictionary<int, T>(dictionary));
		}
	}
}