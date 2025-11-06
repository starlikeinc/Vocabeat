using System;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.Collections
{
    public sealed class BindableProperty<T>
    {
        private T m_value;
        private readonly IEqualityComparer<T> m_comparer;

        //-------------------------------------------------------
        public event Action<T> OnChanged;

        public T Value
        {
            get => m_value;
            set
            {
                if (!m_comparer.Equals(m_value, value))
                {
                    m_value = value;
                    OnChanged?.Invoke(m_value);
                }
            }
        }

        //-------------------------------------------------------
        public BindableProperty(T defaultValue = default, IEqualityComparer<T> comparer = null)
        {
            m_comparer = comparer ?? EqualityComparer<T>.Default;
            m_value = defaultValue;
        }

        //-------------------------------------------------------
        //초기 바인딩, 강제 리프레시용
        public void ForceNotify() => OnChanged?.Invoke(m_value);

        //값만 바꾸고 알림 억제
        public void SetSilently(T v) => m_value = v;

        //편의 구독 (해제 누수 방지함)
        public IDisposable Subscribe(Action<T> handler)
        {
            OnChanged += handler;
            return new Subscription(() => OnChanged -= handler);
        }

        //-------------------------------------------------------
        private sealed class Subscription : IDisposable
        {
            private Action m_dispose;
            public Subscription(Action d) => m_dispose = d;

            public void Dispose()
            {
                m_dispose?.Invoke();
                m_dispose = null;
            }
        }
    }
}
