using System;

namespace EntityChange.Fluent
{
    /// <summary>
    /// Fluent builder for an entity property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public class CollectionMappingBuilder<TEntity, TProperty>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionMappingBuilder{TEntity,TProperty}"/> class.
        /// </summary>
        /// <param name="memberMapping">The member mapping.</param>
        public CollectionMappingBuilder(MemberMapping memberMapping)
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
        /// Ignore this property during data generation.
        /// </summary>
        /// <param name="value">if set to <c>true</c> this property will be ignored.</param>
        /// <returns>
        /// Fluent builder for a collection property.
        /// </returns>
        public CollectionMappingBuilder<TEntity, TProperty> Ignore(bool value = true)
        {
            MemberMapping.Ignored = value;
            return this;
        }


        /// <summary>
        /// Sets the collection element equality <see langword="delegate" />.
        /// </summary>
        /// <param name="equalityFactory">The collection element equality <see langword="delegate" />.</param>
        /// <returns>
        /// Fluent builder for a collection property.
        /// </returns>
        public CollectionMappingBuilder<TEntity, TProperty> ElementEquality(Func<object, object, bool> equalityFactory)
        {
            MemberMapping.Equality = equalityFactory;
            return this;
        }

        /// <summary>
        /// Sets the collection element string formatter <see langword="delegate" />.
        /// </summary>
        /// <param name="formatterFactory">The collection element string formatter factory.</param>
        /// <returns>
        /// Fluent builder for a collection property.
        /// </returns>
        public CollectionMappingBuilder<TEntity, TProperty> ElementFormatter(Func<object, string> formatterFactory)
        {
            MemberMapping.Formatter = formatterFactory;
            return this;
        }


        /// <summary>
        /// Sets the collection element comparison option.
        /// </summary>
        /// <param name="comparison">The collection element comparison option.</param>
        /// <returns>
        /// Fluent builder for a collection property.
        /// </returns>
        public CollectionMappingBuilder<TEntity, TProperty> CollectionComparison(CollectionComparison comparison)
        {
            MemberMapping.CollectionComparison = comparison;
            return this;
        }

    }
}