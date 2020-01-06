using System.Collections.Generic;

// ReSharper disable once CheckNamespace

namespace GameLovers.ConfigsContainer
{
	/// <summary>
	/// Implement this interface on config interface to be added to the <see cref="Configs"/>
	/// </summary>
	public interface IConfig
	{
		/// <summary>
		/// The Config Id. Also represents the index to request a config in <see cref="IConfigs.GetConfig{T}"/>
		/// </summary>
		int ConfigId { get; }
	}
	
	/// <summary>
	/// Generic container of the configs imported with a ConfigsImporter script
	/// The given <typeparamref name="T"/> type is the same of the config struct defined to be serialized in the scriptable object
	/// </summary>
	public interface IConfigsContainer<T> where T : IConfig
	{
		List<T> Configs { get; }
	}
}