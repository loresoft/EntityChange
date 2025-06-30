namespace EntityChange.Fluent;

/// <summary>
/// Fluent builder for an entity property.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TProperty">The type of the property.</typeparam>
public class MemberMappingBuilder<TEntity, TProperty>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberMappingBuilder{TEntity,TProperty}"/> class.
    /// </summary>
    /// <param name="memberMapping">The member mapping.</param>
    public MemberMappingBuilder(MemberMapping memberMapping)
    {
        MemberMapping = memberMapping;
    }

    /// <summary>
    /// Gets the current member mapping.
    /// </summary>
    /// <value>
    /// The current member mapping.
    /// </value>
    public MemberMapping MemberMapping { get; }


    /// <summary>
    /// Ignore this property during entity comparison.
    /// </summary>
    /// <param name="value">if set to <c>true</c> this property will be ignored.</param>
    /// <returns>
    /// Fluent builder for an entity property.
    /// </returns>
    public MemberMappingBuilder<TEntity, TProperty> Ignore(bool value = true)
    {
        MemberMapping.Ignored = value;
        return this;
    }

    /// <summary>
    /// Sets the member equality <see langword="delegate" />.
    /// </summary>
    /// <param name="equalityFactory">The member equality <see langword="delegate" />.</param>
    /// <returns>
    /// Fluent builder for an entity property.
    /// </returns>
    public MemberMappingBuilder<TEntity, TProperty> Equality(Func<TProperty?, TProperty?, bool> equalityFactory)
    {
        if (equalityFactory == null)
            MemberMapping.Equality = null;
        else
            MemberMapping.Equality = (left, right) => equalityFactory(
                left is not null ? (TProperty)left : default,
                right is not null ? (TProperty)right : default
            );

        return this;
    }

    /// <summary>
    /// Sets the member value string formatter <see langword="delegate" />.
    /// </summary>
    /// <param name="formatterFactory">The member value string formatter factory.</param>
    /// <returns>
    /// Fluent builder for an entity property.
    /// </returns>
    public MemberMappingBuilder<TEntity, TProperty> Formatter(Func<TProperty?, string?> formatterFactory)
    {
        if (formatterFactory == null)
            MemberMapping.Formatter = null;
        else
            MemberMapping.Formatter = v => formatterFactory(v is TProperty property ? property : default);

        return this;
    }

    /// <summary>
    /// sets the member display name.
    /// </summary>
    /// <param name="value">The display name.</param>
    /// <returns>
    /// Fluent builder for an entity property.
    /// </returns>
    public MemberMappingBuilder<TEntity, TProperty> Display(string? value)
    {
        MemberMapping.DisplayName = value;
        return this;
    }

    /// <summary>
    /// sets the member display name using the specified factory.
    /// </summary>
    /// <param name="displayFactory">The display factory.</param>
    /// <returns>
    /// Fluent builder for an entity property.
    /// </returns>
    public MemberMappingBuilder<TEntity, TProperty> Display(Func<MemberMapping, string?> displayFactory)
    {
        if (displayFactory == null)
            MemberMapping.DisplayName = null;
        else
            MemberMapping.DisplayName = displayFactory(MemberMapping);
        return this;
    }

}
