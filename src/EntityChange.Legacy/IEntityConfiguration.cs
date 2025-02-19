using System.Collections.Concurrent;

using EntityChange.Fluent;

namespace EntityChange;

/// <summary>
/// An interface defining the entity configuration.
/// </summary>
public interface IEntityConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether to automatic map properties of the class by default.
    /// </summary>
    /// <value>
    ///   <c>true</c> to automatic map properties; otherwise, <c>false</c>.
    /// </value>
    bool AutoMap { get; set; }

    /// <summary>
    /// Gets the mapped class definitions.
    /// </summary>
    /// <value>
    /// The mapped class definitions.
    /// </value>
    ConcurrentDictionary<Type, EntityMapping> Mapping { get; }

    /// <summary>
    /// Configures the comparison with specified fluent <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The fluent configuration builder <see langword="delegate"/>.</param>
    void Configure(Action<ConfigurationBuilder> builder);

    /// <summary>
    /// Gets the <see cref="EntityMapping"/> for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to get mapping for.</param>
    /// <returns><see cref="EntityMapping"/> for the specified type.</returns>
    EntityMapping GetMapping(Type type);

    /// <summary>
    /// Registers the specified profile to the configuration.
    /// </summary>
    /// <param name="profile">The profile.</param>
    /// <returns></returns>
    EntityConfiguration Register(IEntityProfile profile);
}
