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
        /// Ignore this property during data generation.
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


        public MemberMappingBuilder<TEntity, TProperty> Equality(Func<object, object, bool> equalityFactory)
        {
            MemberMapping.Equality = equalityFactory;
            return this;
        }

        public MemberMappingBuilder<TEntity, TProperty> Display(string value)
        {
            MemberMapping.DisplayName = value;
            return this;
        }
    }
}