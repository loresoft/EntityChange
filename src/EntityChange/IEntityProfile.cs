using System;

namespace EntityChange
{
    /// <summary>
    /// An <see langword="interface"/> for entity comparision profiles.
    /// </summary>
    public interface IEntityProfile
    {
        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        Type EntityType { get; }
        
        /// <summary>
        /// Registers the specified class mapping.
        /// </summary>
        /// <param name="entityMapping">The class mapping.</param>
        void Register(EntityMapping entityMapping);
    }
}