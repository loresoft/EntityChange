using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EntityChange.Extenstions;
using EntityChange.Fluent;
using EntityChange.Reflection;

namespace EntityChange
{
    /// <summary>
    /// A class defining the configuration.
    /// </summary>
    public class Configuration
    {
        private static readonly Lazy<Configuration> _current = new Lazy<Configuration>(() => new Configuration());

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            Mapping = new ConcurrentDictionary<Type, EntityMapping>();
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
        public ConcurrentDictionary<Type, EntityMapping> Mapping { get; }


        /// <summary>
        /// Configures the comparison with specified fluent <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The fluent configuration builder <see langword="delegate"/>.</param>
        public void Configure(Action<ConfigurationBuilder> builder)
        {
            var configurationBuilder = new ConfigurationBuilder(this);
            builder(configurationBuilder);
        }

        /// <summary>
        /// Gets the <see cref="EntityMapping"/> for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to get mapping for.</param>
        /// <returns><see cref="EntityMapping"/> for the specified type.</returns>
        public EntityMapping GetMapping(Type type)
        {
            var mapping = Mapping
                .GetOrAdd(type, t => new EntityMapping(TypeAccessor.GetAccessor(type)));

            if (mapping.Mapped)
                return mapping;

            bool autoMap = mapping.AutoMap || AutoMap;
            if (!autoMap)
                return mapping;

            // thread-safe initialization 
            lock (mapping.SyncRoot)
            {
                if (mapping.Mapped)
                    return mapping;

                var typeAccessor = mapping.TypeAccessor;

                var properties = typeAccessor
                    .GetProperties()
                    .Where(p => p.HasGetter && p.HasSetter);

                foreach (var property in properties)
                {
                    // get or create member
                    var memberMapping = mapping.Members.FirstOrDefault(m => m.MemberAccessor.Name == property.Name);
                    if (memberMapping == null)
                    {
                        memberMapping = new MemberMapping { MemberAccessor = property };
                        mapping.Members.Add(memberMapping);
                    }


                    // skip already mapped fields
                    if (memberMapping.Ignored || memberMapping.DisplayName.HasValue())
                        continue;

                    // lookup display name
                    var displayName = property.MemberInfo.GetCustomAttribute(typeof(DisplayNameAttribute), true) as DisplayNameAttribute;
                    if (displayName != null)
                    {
                        memberMapping.DisplayName = displayName.DisplayName;
                        continue;
                    }

                    var display = property.MemberInfo.GetCustomAttribute(typeof(DisplayAttribute), true) as DisplayAttribute;
                    if (display != null)
                    {
                        memberMapping.DisplayName = display.Name;
                        continue;
                    }

                    memberMapping.DisplayName = property.Name.ToSpacedWords();
                }

                mapping.Mapped = true;

                return mapping;
            }
        }


        /// <summary>
        /// Gets the default configuration.
        /// </summary>
        /// <value>
        /// The default configuration.
        /// </value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Configuration Default => _current.Value;
    }
}