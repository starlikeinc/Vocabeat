using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LUIZ.InputSystem
{
    public interface IDelegateWrapper
    {
        void TryCastAndInvoke(object value);
    }

    public class DelegateWrapper<T> : IDelegateWrapper where T : struct
    {
        private Action<T> m_callback = null;

        //----------------------------------------------------
        public void Add(Action<T> action) => m_callback += action;
        public void Remove(Action<T> action) => m_callback -= action;
        public void Invoke(T value) => m_callback?.Invoke(value);
            
        public bool IsEmpty() => m_callback == null;

        //----------------------------------------------------
        // 비제네릭 호출 (object를 받아서 내부에서 캐스팅)
        void IDelegateWrapper.TryCastAndInvoke(object value)
        {
            if (value is T typedValue)
                Invoke(typedValue);
            else
                throw new InvalidCastException($"Invalid cast in DelegateWrapper. Expected {typeof(T)}, got {value.GetType()}");
        }
    }
    
    public class WrapperRegistry
    {
        private readonly Dictionary<string, IDelegateWrapper> m_dicWrappers = new();

        //----------------------------------------------------
        public bool TryGet<T>(string key, out DelegateWrapper<T> wrapper) where T : struct
        {
            if (m_dicWrappers.TryGetValue(key, out var obj) && obj is DelegateWrapper<T> typed)
            {
                wrapper = typed;
                return true;
            }

            wrapper = null;
            return false;
        }

        public DelegateWrapper<T> GetOrCreate<T>(string key) where T : struct
        {
            if (!TryGet<T>(key, out var wrapper))
            {
                wrapper = new DelegateWrapper<T>();
                m_dicWrappers[key] = wrapper;
            }

            return wrapper;
        }

        public void RemoveIfEmpty<T>(string key) where T : struct
        {
            if (TryGet<T>(key, out var wrapper) && wrapper.IsEmpty())
                m_dicWrappers.Remove(key);
        }

        public IDelegateWrapper GetRaw(string key)
        {
            return m_dicWrappers.TryGetValue(key, out var wrapper) ? wrapper : null;
        }

        public void Clear() => m_dicWrappers.Clear();
    }
}
