using System.Collections;
using System.Linq.Expressions;

namespace EntityChange.Fluent;

/// <summary>
/// Fluent builder for <see cref="EntityMapping"/>.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class EntityMappingBuilder<TEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityMappingBuilder{TEntity}"/> class.
    /// </summary>
    protected EntityMappingBuilder()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityMappingBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="entityMapping">The class mapping.</param>
    public EntityMappingBuilder(EntityMapping entityMapping)
    {
        EntityMapping = entityMapping;
    }

    /// <summary>
    /// Gets or sets the class mapping.
    /// </summary>
    /// <value>
    /// The class mapping.
    /// </value>
    public EntityMapping EntityMapping { get; protected set; }


    /// <summary>
    /// Sets a value indicating whether to automatic map properties of the class.
    /// </summary>
    /// <param name="value"><c>true</c> to automatic map properties; otherwise, <c>false</c>.</param>
    /// <returns>A fluent builder for class mapping.</returns>
    public EntityMappingBuilder<TEntity> AutoMap(bool value = true)
    {
        EntityMapping.AutoMap = value;
        return this;
    }


    /// <summary>
    /// Start a fluent configuration for the specified <paramref name="property"/>.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="property">The source property to configure.</param>
    /// <returns>A fluent member builder for the specified property.</returns>
    public MemberMappingBuilder<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> property)
    {
        var propertyAccessor = EntityMapping.TypeAccessor.FindProperty(property);

        var memberMapping = EntityMapping.Members.Find(m => m.MemberAccessor.MemberInfo == propertyAccessor.MemberInfo);
        if (memberMapping == null)
        {
            memberMapping = new MemberMapping();
            memberMapping.MemberAccessor = propertyAccessor;

            EntityMapping.Members.Add(memberMapping);
        }

        var builder = new MemberMappingBuilder<TEntity, TProperty>(memberMapping);
        return builder;
    }
    
    /// <summary>
    /// Start a fluent configuration for the specified <paramref name="collection"/>.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="collection">The source property to configure.</param>
    /// <returns>A fluent member builder for the specified property.</returns>
    public CollectionMappingBuilder<TEntity, TProperty> Collection<TProperty>(Expression<Func<TEntity, TProperty>> collection)
        where TProperty : IEnumerable
    {
        var propertyAccessor = EntityMapping.TypeAccessor.FindProperty(collection);

        var memberMapping = EntityMapping.Members.Find(m => m.MemberAccessor.MemberInfo == propertyAccessor.MemberInfo);
        if (memberMapping == null)
        {
            memberMapping = new MemberMapping();
            memberMapping.MemberAccessor = propertyAccessor;

            EntityMapping.Members.Add(memberMapping);
        }

        var builder = new CollectionMappingBuilder<TEntity, TProperty>(memberMapping);
        return builder;
    }
}