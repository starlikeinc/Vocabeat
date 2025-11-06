using System;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.Collections
{
    public interface IBindableDictionary<TKey, TValue>
    {
        event Action<TKey, TValue> Added;
        event Action<TKey, TValue, TValue> Replaced;
        event Action<TKey, TValue> Removed;
        event Action Cleared;
    }

    //-----------------------------------------------------------------
    public sealed class BindableDictionary<TKey, TValue> : IBindableDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> m_dic;

        //-----------------------------------------------------------------
        public BindableDictionary(int capacity = 32, IEqualityComparer<TKey> comparer = null)
            => m_dic = new Dictionary<TKey, TValue>(capacity, comparer ?? EqualityComparer<TKey>.Default);

        //-----------------------------------------------------------------
        public event Action<TKey, TValue> Added;
        public event Action<TKey, TValue, TValue> Replaced;
        public event Action<TKey, TValue> Removed;
        public event Action Cleared;

        //-----------------------------------------------------------------
        public bool TryGetValue(TKey key, out TValue value) => m_dic.TryGetValue(key, out value);
        public IReadOnlyDictionary<TKey, TValue> Items => m_dic;
        public int Count => m_dic.Count;

        public bool TryAdd(TKey key, in TValue value)
        {
            if (m_dic.ContainsKey(key)) return false;
            m_dic.Add(key, value);
            Added?.Invoke(key, value);
            return true;
        }

        public void AddOrReplace(TKey key, in TValue value)
        {
            if (m_dic.TryGetValue(key, out var old))
            {
                m_dic[key] = value;
                Replaced?.Invoke(key, old, value);
            }
            else
            {
                m_dic.Add(key, value);
                Added?.Invoke(key, value);
            }
        }

        public bool Remove(TKey key)
        {
            if (m_dic.TryGetValue(key, out var old))
            {
                m_dic.Remove(key);
                Removed?.Invoke(key, old);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            if (m_dic.Count == 0) return;
            m_dic.Clear();
            Cleared?.Invoke();
        }
    }
}
