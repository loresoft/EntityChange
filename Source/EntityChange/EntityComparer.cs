using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityChange.Extenstions;
using EntityChange.Fluent;
using EntityChange.Reflection;

namespace EntityChange
{
    public class EntityComparer
    {
        private readonly Stack<string> _pathStack;
        private readonly List<ChangeRecord> _changes;

        public EntityComparer() : this(Configuration.Default)
        {
        }

        public EntityComparer(Configuration configuration)
        {
            Configuration = configuration;
            _changes = new List<ChangeRecord>();
            _pathStack = new Stack<string>();
        }


        /// <summary>
        /// Gets the generator configuration.
        /// </summary>
        /// <value>
        /// The generator configuration.
        /// </value>
        public Configuration Configuration { get; }


        /// <summary>
        /// Configures the generator with specified fluent <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The fluent configuration builder <see langword="delegate"/>.</param>
        public void Configure(Action<ConfigurationBuilder> builder)
        {
            var configurationBuilder = new ConfigurationBuilder(Configuration);
            builder(configurationBuilder);
        }


        public IReadOnlyCollection<ChangeRecord> Compare<T>(T original, T current)
        {
            _changes.Clear();
            var type = typeof(T);

            CompareType(type, original, current);
            return _changes;
        }


        private void CompareType(Type type, object original, object current, ICompareOptions options = null)
        {
            // both null, nothing to compare
            if (original == null && current == null)
                return;

            Type keyType;
            Type elementType;

            if (type.IsArray)
                CompareArray(original, current, options);
            else if (original is IDictionary || current is IDictionary)
                CompareDictionary(original, current, options);
            else if (type.IsDictionary(out keyType, out elementType))
                CompareGenericDictionary(original, current, keyType, elementType, options);
            else if (original is IList || current is IList)
                CompareList(original, current, options);
            else if (type.IsList(out elementType))
                CompareEnumerable(original, current, options);
            else if (type.IsCollection())
                CompareEnumerable(original, current, options);
            else if (type.IsValueType || type == typeof(string))
                CompareValue(original, current, options);
            else
                CompareObject(type, original, current);
        }

        private void CompareObject(Type type, object original, object current)
        {
            if (CheckForNull(original, current))
                return;


            CompareProperties(type, original, current);
        }

        private void CompareProperties(Type type, object original, object current)
        {
            var classMapping = GetMapping(type);
            foreach (var memberMapping in classMapping.Members)
            {
                var originalValue = memberMapping.MemberAccessor.GetValue(original);
                var currentValue = memberMapping.MemberAccessor.GetValue(current);

                var path = memberMapping.MemberAccessor.Name;
                _pathStack.Push(path);

                var propertyType = memberMapping.MemberAccessor.MemberType.GetUnderlyingType();

                CompareType(propertyType, originalValue, currentValue, memberMapping);

                _pathStack.Pop();
            }
        }


        private void CompareDictionary(object original, object current, ICompareOptions options)
        {
            var originalDictionary = original as IDictionary;
            var currentDictionary = current as IDictionary;

            if (CheckForNull(originalDictionary, currentDictionary))
                return;

            CompareByKey(originalDictionary, currentDictionary, d => d.Keys, (d, k) => d[k]);
        }

        private void CompareGenericDictionary(object original, object current, Type keyType, Type elementType, ICompareOptions options)
        {
            var t = typeof(DictionaryWrapper<,>).MakeGenericType(keyType, elementType);
            var o = Activator.CreateInstance(t, original) as IDictionaryWrapper;
            var c = Activator.CreateInstance(t, current) as IDictionaryWrapper;

            if (CheckForNull(o, c))
                return;

            CompareByKey(o, c, d => d.GetKeys(), (d, k) => d.GetValue(k));
        }


        private void CompareArray(object original, object current, ICompareOptions options)
        {
            var originalArray = original as Array;
            var currentArray = current as Array;

            if (CheckForNull(originalArray, currentArray))
                return;

            if (options.CollectionComparison == CollectionComparison.ObjectEquality)
                CompareByEquality(originalArray, currentArray, options);
            else
                CompareByIndexer(originalArray, currentArray, t => t.Length, (t, i) => t.GetValue(i));
        }

        private void CompareList(object original, object current, ICompareOptions options)
        {
            var originalList = original as IList;
            var currentList = current as IList;

            if (CheckForNull(originalList, currentList))
                return;

            if (options.CollectionComparison == CollectionComparison.ObjectEquality)
                CompareByEquality(originalList, currentList, options);
            else
                CompareByIndexer(originalList, currentList, t => t.Count, (t, i) => t[i]);
        }

        private void CompareEnumerable(object original, object current, ICompareOptions options)
        {
            var originalEnumerable = original as IEnumerable;
            var currentEnumerable = current as IEnumerable;

            if (CheckForNull(originalEnumerable, currentEnumerable))
                return;

            if (options.CollectionComparison == CollectionComparison.ObjectEquality)
            {
                CompareByEquality(originalEnumerable, currentEnumerable, options);
                return;
            }

            // convert to object array
            var originalArray = originalEnumerable?.Cast<object>().ToArray();
            var currentArray = currentEnumerable?.Cast<object>().ToArray();

            if (CheckForNull(originalArray, currentArray))
                return;

            CompareByIndexer(originalArray, currentArray, t => t.Length, (t, i) => t.GetValue(i));
        }


        private void CompareValue(object original, object current, ICompareOptions options)
        {
            var compare = options?.Equality ?? Object.Equals;
            bool areEqual = compare(original, current);

            if (areEqual)
                return;

            CreateChange(ChangeOperation.Replace, original, current);
        }


        private void CompareByEquality(IEnumerable original, IEnumerable current, ICompareOptions options)
        {
            var originalList = original.Cast<object>().ToList();
            var currentList = current.Cast<object>().ToList();

            var currentPath = CurrentPath();
            var compare = options?.Equality ?? Object.Equals;


            _pathStack.Pop();
            for (int index = 0; index < currentList.Count; index++)
            {
                string p = $"{currentPath}[{index}]";

                var v = currentList[index];
                var o = originalList.FirstOrDefault(f => compare(f, v));
                if (o == null)
                {
                    // added item
                    CreateChange(ChangeOperation.Add, null, v, p);
                    continue;
                }

                // remove so can't be reused
                originalList.Remove(o);

                var t = o.GetType();

                _pathStack.Push(p);
                CompareType(t, o, v, options);
                _pathStack.Pop();
            }
            _pathStack.Push(currentPath);


            // removed items
            foreach (var v in originalList)
                CreateChange(ChangeOperation.Remove, v, null);
        }

        private void CompareByIndexer<T>(T originalList, T currentList, Func<T, int> countFactory, Func<T, int, object> valueFactory)
        {
            if (countFactory == null)
                throw new ArgumentNullException(nameof(countFactory));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            var currentPath = CurrentPath();

            var orginalCount = countFactory(originalList);
            var currentCount = countFactory(currentList);

            int commonCount = Math.Min(orginalCount, currentCount);

            // compare common items
            if (commonCount > 0)
            {
                _pathStack.Pop();
                for (int i = 0; i < commonCount; i++)
                {
                    string p = $"{currentPath}[{i}]";
                    var o = valueFactory(originalList, i);
                    var v = valueFactory(currentList, i);

                    // skip nulls
                    if (o == null && v == null)
                        continue;

                    // get dictionary value type
                    var t = o?.GetType() ?? v?.GetType();

                    _pathStack.Push(p);

                    CompareType(t, o, v);

                    _pathStack.Pop();
                }
                _pathStack.Push(currentPath);
            }

            // added items
            if (commonCount < currentCount)
            {
                for (int i = commonCount; i < currentCount; i++)
                {
                    string p = $"{currentPath}[{i}]";
                    var v = valueFactory(currentList, i);

                    CreateChange(ChangeOperation.Add, null, v, p);
                }
            }

            // removed items
            if (commonCount < orginalCount)
            {
                for (int i = commonCount; i < orginalCount; i++)
                {
                    string p = $"{currentPath}[{i}]";
                    var v = valueFactory(originalList, i);

                    CreateChange(ChangeOperation.Remove, v, null, p);
                }
            }

        }

        private void CompareByKey<T>(T originalDictionary, T currentDictionary, Func<T, IEnumerable> keysFactory, Func<T, object, object> valueFactory)
        {
            if (keysFactory == null)
                throw new ArgumentNullException(nameof(keysFactory));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            var originalKeys = keysFactory(originalDictionary).Cast<object>().ToList();
            var currentKeys = keysFactory(currentDictionary).Cast<object>().ToList();

            var currentPath = CurrentPath();

            // compare common keys
            var commonKeys = originalKeys.Intersect(currentKeys).ToList();
            _pathStack.Pop();
            foreach (var key in commonKeys)
            {
                string k = key.ToString();
                string p = $"{currentPath}[{k}]";

                // safe to use index because keys are common
                var o = valueFactory(originalDictionary, key);
                var v = valueFactory(currentDictionary, key);

                // skip nulls
                if (o == null && v == null)
                    continue;

                // get dictionary value type
                var t = o?.GetType() ?? v?.GetType();

                // adjust path for dictionary
                _pathStack.Push(p);

                CompareType(t, o, v);

                // restore path
                _pathStack.Pop();
            }
            _pathStack.Push(currentPath);

            // new key changes
            var addedKeys = currentKeys.Except(originalKeys).ToList();
            foreach (var key in addedKeys)
            {
                string k = key.ToString();
                string p = $"{currentPath}[{k}]";
                var v = valueFactory(currentDictionary, key);

                CreateChange(ChangeOperation.Add, null, v, p);
            }

            // removed key changes
            var removedKeys = originalKeys.Except(currentKeys).ToList();
            foreach (var key in removedKeys)
            {
                string k = key.ToString();
                string p = $"{currentPath}[{k}]";
                var v = valueFactory(originalDictionary, key);

                CreateChange(ChangeOperation.Remove, v, null, p);
            }

        }


        private bool CheckForNull(object original, object current)
        {
            // both null, nothing to compare
            if (original == null && current == null)
                return true;

            if (original == null)
            {
                CreateChange(ChangeOperation.Replace, null, current);
                return true;
            }

            if (current == null)
            {
                CreateChange(ChangeOperation.Remove, original, null);
                return true;
            }

            return false;
        }


        private string CurrentPath()
        {
            return _pathStack
                .ToArray()
                .Reverse()
                .ToDelimitedString(".");
        }

        private void CreateChange(ChangeOperation operation, object original, object current, string path = null)
        {
            var c = new ChangeRecord
            {
                Path = path ?? CurrentPath(),
                Operation = operation,
                OrginalValue = original,
                CurrentValue = current
            };

            _changes.Add(c);
        }


        private ClassMapping GetMapping(Type type)
        {
            var mapping = Configuration.Mapping
                .GetOrAdd(type, t => new ClassMapping(TypeAccessor.GetAccessor(type)));

            if (mapping.Mapped)
                return mapping;

            bool autoMap = mapping.AutoMap || Configuration.AutoMap;
            if (!autoMap)
                return mapping;

            // thread-safe initialization 
            lock (mapping.SyncRoot)
            {
                if (mapping.Mapped)
                    return mapping;

                var typeAccessor = mapping.TypeAccessor;

                var properties = typeAccessor.GetProperties()
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
                    if (memberMapping.Ignored || memberMapping.Equality != null)
                        continue;


                    // init here
                }

                mapping.Mapped = true;

                return mapping;
            }
        }

    }


    public interface IDictionaryWrapper
    {
        IEnumerable GetKeys();
        object GetValue(object key);
    }

    public class DictionaryWrapper<TKey, TValue> : IDictionaryWrapper
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }


        public IEnumerable GetKeys()
        {
            return _dictionary.Keys;
        }

        public object GetValue(object key)
        {
            TValue value;

            _dictionary.TryGetValue((TKey)key, out value);

            return value;
        }
    }
}
