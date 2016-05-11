using System;

namespace EntityChange.Fluent
{
    /// <summary>
    /// Fluent builder for an entity property.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public class MemberConfigurationBuilder<TEntity, TProperty>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberConfigurationBuilder{TEntity, TProperty}"/> class.
        /// </summary>
        /// <param name="memberMapping">The member mapping.</param>
        public MemberConfigurationBuilder(MemberMapping memberMapping)
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
        /// Fluent builder for an entity property.
        /// </returns>
        public MemberConfigurationBuilder<TEntity, TProperty> Ignore(bool value = true)
        {
            MemberMapping.Ignored = value;
            return this;
        }


        public MemberConfigurationBuilder<TEntity, TProperty> Equality(Func<object, object, bool> equalityFactory)
        {
            MemberMapping.Equality = equalityFactory;
            return this;
        }
    }
}