using System;
using EntityChange.Reflection;

namespace EntityChange.Fluent
{
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
        /// Fluent configuration for <see cref="ClassMapping"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity for the class mapping.</typeparam>
        /// <param name="builder">The fluent builder for <see cref="ClassMapping"/>.</param>
        /// <returns>
        /// A fluent builder to configure DataGenerator.
        /// </returns>
        /// <exception cref="ArgumentNullException">The <paramref name="builder"/> parameter is <see langword="null" />.</exception>
        public ConfigurationBuilder Entity<TEntity>(Action<ClassMappingBuilder<TEntity>> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var type = typeof(TEntity);
            var classMapping = GetClassMap(type);

            var mappingBuilder = new ClassMappingBuilder<TEntity>(classMapping);
            builder(mappingBuilder);

            return this;
        }


        /// <summary>
        /// Add the profile of type <typeparamref name="TProfile"/> to the configuration
        /// </summary>
        /// <typeparam name="TProfile">The type of the profile.</typeparam>
        /// <returns>
        /// A fluent builder to configure DataGenerator.
        /// </returns>
        public ConfigurationBuilder Profile<TProfile>() 
            where TProfile : IMappingProfile, new()
        {
            var profile = new TProfile();
            var type = profile.EntityType;
            var classMapping = GetClassMap(type);

            profile.Register(classMapping);

            return this;
        }


        private ClassMapping GetClassMap(Type type)
        {
            var classMapping = Configuration.Mapping.GetOrAdd(type, t =>
            {
                var typeAccessor = TypeAccessor.GetAccessor(t);
                var mapping = new ClassMapping(typeAccessor);
                return mapping;
            });

            return classMapping;
        }

    }
}
