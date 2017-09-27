using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.Logging.EventSourceBridge
{
    internal class ReadOnlyEmptyDictionary<K, V> : IDictionary<K, V>
    {
        public static readonly IDictionary<K, V> Instance = new ReadOnlyEmptyDictionary<K, V>();

        public V this[K key]
        {
            get => throw new KeyNotFoundException();
            set => throw new NotSupportedException("This dictionary is read-only");
        }

        public ICollection<K> Keys => Array.Empty<K>();

        public ICollection<V> Values => Array.Empty<V>();

        public int Count => 0;

        public bool IsReadOnly => true;

        private ReadOnlyEmptyDictionary() { }

        public void Add(K key, V value) => throw new NotSupportedException("This dictionary is read-only");

        public void Add(KeyValuePair<K, V> item) => throw new NotSupportedException("This dictionary is read-only");

        public bool Contains(KeyValuePair<K, V> item) => false;

        public bool ContainsKey(K key) => false;

        public bool Remove(K key) => false;

        public bool Remove(KeyValuePair<K, V> item) => false;

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => Enumerable.Empty<KeyValuePair<K, V>>().GetEnumerator();

        public void Clear()
        {
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            // Don't need to do anything, this dictionary is empty
        }

        public bool TryGetValue(K key, out V value)
        {
            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
