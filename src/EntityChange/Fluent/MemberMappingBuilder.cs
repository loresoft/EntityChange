using System;

namespace EntityChange.Fluent
{
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
        public MemberMappingBuilder<TEntity, TProperty> Equality(Func<object, object, bool> equalityFactory)
        {
            MemberMapping.Equality = equalityFactory;
            return this;
        }

        /// <summary>
        /// Sets the member value string formatter <see langword="delegate" />.
        /// </summary>
        /// <param name="formatterFactory">The member value string formatter factory.</param>
        /// <returns>
        /// Fluent builder for an entity property.
        /// </returns>
        public MemberMappingBuilder<TEntity, TProperty> Formatter(Func<object, string> formatterFactory)
        {
            MemberMapping.Formatter = formatterFactory;
            return this;
        }

        /// <summary>
        /// sets the member display name.
        /// </summary>
        /// <param name="value">The display name.</param>
        /// <returns>
        /// Fluent builder for an entity property.
        /// </returns>
        public MemberMappingBuilder<TEntity, TProperty> Display(string value)
        {
            MemberMapping.DisplayName = value;
            return this;
        }
    }
}