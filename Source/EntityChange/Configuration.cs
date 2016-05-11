using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using EntityChange.Fluent;

namespace EntityChange
{
    /// <summary>
    /// A class defining the configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            Mapping = new ConcurrentDictionary<Type, ClassMapping>();
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether to automatic map properties of the class by default.
        /// </summary>
        /// <value>
        ///   <c>true</c> to automatic map properties; otherwise, <c>false</c>.
        /// </value>
        public bool AutoMap { get; set; }

        /// <summary>
        /// Gets the mapped class definitions.
        /// </summary>
        /// <value>
        /// The mapped class definitions.
        /// </value>
        public ConcurrentDictionary<Type, ClassMapping> Mapping { get; }


        /// <summary>
        /// Configures the generator with specified fluent <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The fluent configuration builder <see langword="delegate"/>.</param>
        public void Configure(Action<ConfigurationBuilder> builder)
        {
            var configurationBuilder = new ConfigurationBuilder(this);
            builder(configurationBuilder);
        }


        private static readonly Lazy<Configuration> _current = new Lazy<Configuration>(() => new Configuration());

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Configuration Default => _current.Value;


    }
}