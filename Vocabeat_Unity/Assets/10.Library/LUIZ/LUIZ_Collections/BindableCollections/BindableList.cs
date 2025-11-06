using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.Collections
{
    public sealed class BindableList<T> : IEnumerable<T> 
    {
        private readonly List<T> m_list;
        private int m_suppressCnt;           // 알림 억제 카운터
        private bool m_isBatchDirty;         // 억제 중 변경 플래그
        
        //---------------------------------------------------------------
        //개별 변경 이벤트
        public event Action<int/*idx*/, T> OnItemAdded;
        public event Action<int/*idx*/, T, T> OnItemReplaced;
        public event Action<int/*idx*/, T> OnItemRemoved;
        public event Action OnCleared;

        //배치 변경 이벤트
        public event Action OnBatchChanged;

        public int Count => m_list.Count;
        public IReadOnlyList<T> Items => m_list;
        public T this[int index] => m_list[index];
        
        //---------------------------------------------------------------
        public BindableList(int capacity = 16) => m_list = new List<T>(capacity);
        
        //---------------------------------------------------------------
        public void Clear()
        {
            if (m_list.Count == 0) return;

            m_list.Clear();
            if (IsSuppressed) { m_isBatchDirty = true; return; }

            OnCleared?.Invoke();
        }

        public void Add(in T item)
        {
            m_list.Add(item);
            int idx = m_list.Count - 1;

            if (IsSuppressed)
            {
                m_isBatchDirty = true; 
                return;
            }
            OnItemAdded?.Invoke(idx, item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            using (SuppressNotifications())
            {
                foreach (var it in items)
                    Add(in it);
            }
            //SuppressNotifications 디스포즈 시 OnBatchChanged 자동 호출하고 있음
        }

        public bool Replace(int index, in T item)
        {
            if ((uint)index >= (uint)m_list.Count) 
                return false;

            T old = m_list[index];
            m_list[index] = item;

            if (IsSuppressed)
            {
                m_isBatchDirty = true; 
                return true;
            }

            OnItemReplaced?.Invoke(index, old, item);
            return true;
        }

        ///<summary>
        ///index의 원소를 제거 후 뒤의 모든 원소를 앞으로 한 칸씩 이동
        ///</summary>
        public bool RemoveAt(int index)
        {
            if ((uint)index >= (uint)m_list.Count) 
                return false;

            T removed = m_list[index];
            m_list.RemoveAt(index);

            if (IsSuppressed)
            {
                m_isBatchDirty = true; 
                return true;
            }

            OnItemRemoved?.Invoke(index, removed);
            return true;
        }

        ///<summary>
        ///가장 뒤의 원소를 index 위치로 옮기고 가장 뒤 원소를 제거
        ///리스트 재정렬을 피하기 때문에 빠르지만 이걸 호출한 순간부터 List의 순서가 보장되지 않음에 유의하세요
        ///</summary>
        public bool RemoveAtSwapBack(int index)
        {
            if ((uint)index >= (uint)m_list.Count)
                return false;

            int last = m_list.Count - 1;
            T removed = m_list[index];

            if (index != last)
            {
                var moved = m_list[last];
                m_list[index] = moved;
                m_list.RemoveAt(last);

                if (IsSuppressed)
                {
                    m_isBatchDirty = true;
                    return true;
                }
                
                OnItemReplaced?.Invoke(index, removed, moved);
                OnItemRemoved?.Invoke(last, removed);
            }
            else
            {
                m_list.RemoveAt(last);
                if (IsSuppressed)
                {
                    m_isBatchDirty = true;
                    return true;
                }
                
                OnItemRemoved?.Invoke(last, removed);
            }
            return true;
        }

        public bool TryGet(int index, out T v)
        {
            if ((uint)index >= (uint)m_list.Count)
            {
                v = default;
                return false;
            }
            
            v = m_list[index];
            return true;
        }
        
        public IDisposable SuppressNotifications()
        {
            m_suppressCnt++;
            return new Scope(() =>
            {
                m_suppressCnt--;
                if (m_suppressCnt == 0 && m_isBatchDirty)
                {
                    m_isBatchDirty = false;
                    OnBatchChanged?.Invoke();
                }
            });
        }

        //------------------------------------------------------
        private bool IsSuppressed => m_suppressCnt > 0;
        
        //------------------------------------------------------
        private sealed class Scope : IDisposable
        {
            private Action m_end;
            public Scope(Action end) => m_end = end;

            public void Dispose()
            {
                m_end?.Invoke();
                m_end = null;
            }
        }
        
        //---------------------------------------------------------------
        public IEnumerator<T> GetEnumerator() => m_list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); //비제네릭용
    }
}