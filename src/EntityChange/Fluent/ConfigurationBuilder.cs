using EntityChange.Reflection;

namespace EntityChange.Fluent;

/// <summary>
/// Fluent <see cref="Configuration"/> builder.
/// </summary>
public class ConfigurationBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationBuilder"/> class.
    /// </summary>
    /// <param name="configuration">The configuration to update.</param>
    public ConfigurationBuilder(Configuration configuration)
    {
        Configuration = configuration;
    }


    /// <summary>
    /// Gets the current configuration.
    /// </summary>
    /// <value>
    /// The current configuration.
    /// </value>
    public Configuration Configuration { get; }

    /// <summary>
    /// Sets a value indicating whether to automatic map properties of the entity by default
    /// </summary>
    /// <param name="value"><c>true</c> to automatic map properties; otherwise, <c>false</c>.</param>
    /// <returns>
    /// A fluent builder to configure comparison.
    /// </returns>
    public ConfigurationBuilder AutoMap(bool value = true)
    {
        Configuration.AutoMap = value;
        return this;
    }

    /// <summary>
    /// Fluent configuration for <see cref="EntityMapping"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity for the class mapping.</typeparam>
    /// <param name="builder">The fluent builder for <see cref="EntityMapping"/>.</param>
    /// <returns>
    /// A fluent builder to configure comparison.
    /// </returns>
    /// <exception cref="ArgumentNullException">The <paramref name="builder"/> parameter is <see langword="null" />.</exception>
    public ConfigurationBuilder Entity<TEntity>(Action<EntityMappingBuilder<TEntity>> builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var type = typeof(TEntity);
        var classMapping = GetClassMap(type);

        var mappingBuilder = new EntityMappingBuilder<TEntity>(classMapping);
        builder(mappingBuilder);

        return this;
    }


    /// <summary>
    /// Add the profile of type <typeparamref name="TProfile"/> to the configuration
    /// </summary>
    /// <typeparam name="TProfile">The type of the profile.</typeparam>
    /// <returns>
    /// A fluent builder to configure comparison.
    /// </returns>
    public ConfigurationBuilder Profile<TProfile>()
        where TProfile : IEntityProfile, new()
    {
        var profile = new TProfile();
        return Profile(profile);
    }

    /// <summary>
    /// Add the specified <paramref name="profile" /> to the configuration
    /// </summary>
    /// <param name="profile">The profile to add to the configuration.</param>
    /// <returns>
    /// A fluent builder to configure comparison.
    /// </returns>
    public ConfigurationBuilder Profile(IEntityProfile profile)
    {
        var type = profile.EntityType;
        var classMapping = GetClassMap(type);

        profile.Register(classMapping);

        return this;
    }


    private EntityMapping GetClassMap(Type type)
    {
        var classMapping = Configuration.Mapping.GetOrAdd(type, t =>
        {
            var typeAccessor = TypeAccessor.GetAccessor(t);
            var mapping = new EntityMapping(typeAccessor) { AutoMap = Configuration.AutoMap };
            return mapping;
        });

        return classMapping;
    }

}
