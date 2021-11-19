using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace TeamGleason.SpeakFaster.BasicKeyboard.Layout
{
    public class KeyCollection<T> : ICollection<T>
        where T : IndexObject
    {
        private readonly Dictionary<string, T> _dictionary = new Dictionary<string, T>();

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        public virtual void Add(T item)
        {
            Debug.Assert(!_dictionary.ContainsKey(item.Id));
            _dictionary.Add(item.Id, item);
        }

        public virtual void Clear()
        {
            _dictionary.Clear();
        }

        public bool Contains(T item)
        {
            var value = _dictionary.ContainsKey(item.Id);
            return value;
        }

        public void CopyTo(T[] array, int index)
        {
            _dictionary.Values.CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _dictionary.Values.GetEnumerator();
        }

        public virtual bool Remove(T item)
        {
            var value = _dictionary.Remove(item.Id);
            return value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[string key] => _dictionary[key];
    }
}
