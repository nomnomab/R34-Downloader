using System.Collections.Generic;
using System.Linq;

namespace DataTypes
{
    public class Batch<T, T2> where T : class
    {
        public int Count => list.Count;
        private List<KeyValuePair<T, T2>> list { get; set; }

        public T2 this[int index] => list[index].Value;
        public T2 this[T key] => list.FirstOrDefault(x=>x.Key == key).Value;

        public void Add(T a, T2 b)
        {
            KeyValuePair<T, T2> pair = new KeyValuePair<T, T2>(a, b);
            list.Add(pair);
        }

        public bool Remove(T a)
        {
            foreach(KeyValuePair<T, T2> pair in list)
                if(pair.Key == a) return list.Remove(pair);
            return false;
        }

        public void Reorder(T[] a)
        {
            T2[] v = GetValues(a);
            for (int i = 0; i < a.Length; i++) list[i] = new KeyValuePair<T, T2>(a[i], v[i]);
        }

        public void Clear()
        {
            list.Clear();
        }

        public T[] GetKeys()
        {
            return list.Select(x => x.Key).ToArray();
        }

        public T2[] GetValues()
        {
            return list.Select(x => x.Value).ToArray();
        }

        public T2[] GetValues(T[] a)
        {
            T2[] b = new T2[a.Length];
            for (int i = 0; i < a.Length; i++) b[i] = this[a[i]];
            return b;
        }

        public Batch()
        {
            list = new List<KeyValuePair<T, T2>>();
        }
    }
}