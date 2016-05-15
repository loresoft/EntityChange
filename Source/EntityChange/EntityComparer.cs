using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityChange.Extenstions;
using EntityChange.Fluent;
using EntityChange.Reflection;

namespace EntityChange
{
    /// <summary>
    /// A class to compare two entities generating a change list. 
    /// </summary>
    public class EntityComparer
    {
        private readonly Stack<string> _pathStack;
        private readonly Stack<string> _displayStack;
        private readonly List<ChangeRecord> _changes;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComparer"/> class.
        /// </summary>
        public EntityComparer() : this(Configuration.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityComparer"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public EntityComparer(Configuration configuration)
        {
            Configuration = configuration;
            _changes = new List<ChangeRecord>();
            _pathStack = new Stack<string>();
            _displayStack = new Stack<string>();
        }


        /// <summary>
        /// Gets the generator configuration.
        /// </summary>
        /// <value>
        /// The generator configuration.
        /// </value>
        public Configuration Configuration { get; }


        /// <summary>
        /// Compares the specified <paramref name="original"/> and <paramref name="current"/> entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="original">The original entity.</param>
        /// <param name="current">The current entity.</param>
        /// <returns>A list of changes.</returns>
        public IReadOnlyCollection<ChangeRecord> Compare<TEntity>(TEntity original, TEntity current)
        {
            _changes.Clear();
            var type = typeof(TEntity);

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
            else if (type.IsCollection())
                CompareCollection(original, current, options);
            else if (type.IsValueType || type == typeof(string))
                CompareValue(original, current, options);
            else
                CompareObject(type, original, current);
        }

        private void CompareObject(Type type, object original, object current)
        {
            if (CheckForNull(original, current))
                return;


            var classMapping = Configuration.GetMapping(type);
            foreach (var memberMapping in classMapping.Members)
            {
                var accessor = memberMapping.MemberAccessor;

                var originalValue = accessor.GetValue(original);
                var currentValue = accessor.GetValue(current);

                var propertyName = accessor.Name;
                var displayName = memberMapping.DisplayName ?? accessor.Name.ToSpacedWords();

                _pathStack.Push(propertyName);
                _displayStack.Push(displayName);

                var propertyType = accessor.MemberType.GetUnderlyingType();

                CompareType(propertyType, originalValue, currentValue, memberMapping);

                _displayStack.Pop();
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
            // TODO improve this, currently slow due to CreateInstance usage
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

            if (options?.CollectionComparison == CollectionComparison.ObjectEquality)
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

            if (options?.CollectionComparison == CollectionComparison.ObjectEquality)
                CompareByEquality(originalList, currentList, options);
            else
                CompareByIndexer(originalList, currentList, t => t.Count, (t, i) => t[i]);
        }

        private void CompareCollection(object original, object current, ICompareOptions options)
        {
            var originalEnumerable = original as IEnumerable;
            var currentEnumerable = current as IEnumerable;

            if (CheckForNull(originalEnumerable, currentEnumerable))
                return;

            if (options?.CollectionComparison == CollectionComparison.ObjectEquality)
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
            var currentName = CurrentName();
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
                    CreateChange(ChangeOperation.Add, null, v, p, currentName);
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

        private string CurrentName()
        {
            if (_pathStack.Count > 0)
                return _pathStack.Peek();

            return string.Empty;
        }

        private string CurrentDisplay()
        {
            if (_displayStack.Count > 0)
                return _displayStack.Peek();

            if (_pathStack.Count > 0)
                return _pathStack.Peek();

            return string.Empty;
        }

        private void CreateChange(ChangeOperation operation, object original, object current, string path = null, string name = null)
        {
            var c = new ChangeRecord();
            c.PropertyName = name ?? CurrentName();
            c.DisplayName = CurrentDisplay();
            c.Path = path ?? CurrentPath();
            c.Operation = operation;
            c.OriginalValue = original;
            c.CurrentValue = current;

            _changes.Add(c);
        }

    }
}
