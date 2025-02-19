using EntityChange.Fluent;

namespace EntityChange;

/// <summary>
/// A <see langword="base"/> class for creating entity comparision profiles
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public abstract class EntityProfile<TEntity> : EntityMappingBuilder<TEntity>, IEntityProfile
    where TEntity : class
{
    /// <summary>
    /// Gets or sets the type of the entity.
    /// </summary>
    /// <value>
    /// The type of the entity.
    /// </value>
    Type IEntityProfile.EntityType => typeof(TEntity);

    /// <summary>
    /// Registers the specified class mapping.
    /// </summary>
    /// <param name="entityMapping">The class mapping.</param>
    void IEntityProfile.Register(EntityMapping entityMapping)
    {
        EntityMapping = entityMapping;
        Configure();
    }


    /// <summary>
    /// Configure the <typeparamref name="TEntity"/> mapping information.
    /// </summary>
    public abstract void Configure();
}