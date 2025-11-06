using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LUIZ.Collections
{
    //LinkedList는 Node제거시 O(1)이고 재정렬 없어서 좋기는한데
    //의외로 삽입 삭제가 빈번하면 LinkedListNode가 ref 타입이라 GC압박이 될 수 있음
    //IndexLinkedList는 배열 인덱스 기반으로 작성해두어서 LinkedListNode 이용 X
    //인덱스 기반 제거로 AddLast, AddFirst, InsertAfter, InsertBefore 전부 O(1)이다.
    
    //[주의!!!]
    //인덱스 기반 탐색을 이용한 구조라
    //인덱스를 통한 접근, for문으로 값을 순환하는건 실제 List의 순서가 아니다! ex) 리스트[리스트.Count - 1] 이 실제 마지막 데이터 값이 아니라는 뜻임.
    //실제 순서에 기반하여 값을 가져오고 싶다면 foreach를 돌거나 클래스 하단에 구현된 열거자를 직접 받아올 것!!!! 
    
    //마지막 값이나 첫번쨰 값을 받아오고 싶다면 리스트[Head], 리스트[Tail] 이런식으로 해야함
    //아니면 걍 편의 메서드 쪽에 있는거 쓸 것
    public sealed class IndexLinkedList<T> : IEnumerable<T>
    {
        private const int c_NULL = -1;

        private int[] m_next;           //i의 다음 노드 인덱스(없으면 c_NULL)
        private int[] m_prev;           //i의 이전 노드 인덱스(없으면 c_NULL)
        private T[] m_aryValues;        //i번 노드 값(T)
        private bool[] m_isAlive;       //i 슬롯 사용 중 여부
        
        private int m_freeHeadIdx;     //next[]를 링크로 재사용
        private int m_head = c_NULL;
        private int m_tail = c_NULL;
        
        private int m_count = 0;
        private int m_version = 0;      //열거자 무효화 감지용임

        //----------------------------------------------------------------------
        public IndexLinkedList(int initialCapacity = 16)
        {
            if (initialCapacity < 1)
                initialCapacity = 1;
            
            m_next = new int[initialCapacity];
            m_prev = new int[initialCapacity];
            m_aryValues = new T[initialCapacity];
            m_isAlive = new bool[initialCapacity];
            
            BuildFreeList(0, initialCapacity);
        }

        //----------------------------------------------------------------------
        public int Count => m_count;
        
        public bool IsEmpty => m_count == 0;
        
        public int FirstIndex => m_head;
        public int LastIndex => m_tail;

        //----------------------------------------------------------------------
        //인덱스 접근 
        //주의 !!!!!
        //length-1 같은 접근이나 for문 돌면서 가져오는 접근은 실제 순서대로 정렬된 값이 아님!!!!!!
        public ref T this[int index]
        {
            get
            {
                EnsureAlive(index);
                return ref m_aryValues[index];
            }
        }

        public T GetValue(int index)
        {
            EnsureAlive(index);
            return m_aryValues[index];
        }

        //----------------------------------------------------------------------
        //연결 탐색용
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next(int index)
        {
            EnsureAlive(index);
            return m_next[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Prev(int index)
        {
            EnsureAlive(index);
            return m_prev[index];
        }
        
        //----------------------------------------------------------------------
        //--- 편의 메서드 O(1)-------------------------------
        public bool TryPeekFirst(out T value)
        {
            if (m_head == c_NULL) { value = default!; return false; }
            value = m_aryValues[m_head];
            return true;
        }
        public bool TryPeekLast(out T value)
        {
            if (m_tail == c_NULL) { value = default!; return false; }
            value = m_aryValues[m_tail];
            return true;
        }

        public bool RemoveFirst(bool clearValue = false)
        {
            if (m_head == c_NULL) return false;
            Remove(m_head, clearValue);
            return true;
        }
        public bool RemoveLast(bool clearValue = false)
        {
            if (m_tail == c_NULL) return false;
            Remove(m_tail, clearValue);
            return true;
        }

        //값까지 돌려주는 Pop
        public bool TryPopFirst(out int index, out T value, bool clearValue = false)
        {
            if (m_head == c_NULL) { index = c_NULL; value = default!; return false; }
            index = m_head; value = m_aryValues[index];
            Remove(index, clearValue);
            return true;
        }
        public bool TryPopLast(out int index, out T value, bool clearValue = false)
        {
            if (m_tail == c_NULL) { index = c_NULL; value = default!; return false; }
            index = m_tail; value = m_aryValues[index];
            Remove(index, clearValue);
            return true;
        }

        //----------------------------------------------------------------------
        //맨 뒤 추가 O(1)
        public int AddLast(in T value) => InsertInternal(m_tail, after: true, value);
        //맨 앞 추가 O(1)
        public int AddFirst(in T value) => InsertInternal(m_head, after: false, value);

        //기준 노드 뒤에 삽입 O(1)
        public int InsertAfter(int index, in T value)
        {
            EnsureAlive(index);
            return InsertInternal(index, after: true, value);
        }
        //기준 노드 앞에 삽입 O(1)
        public int InsertBefore(int index, in T value)
        {
            EnsureAlive(index);
            return InsertInternal(index, after: false, value);
        }

        //제거 O(1)
        public void Remove(int index, bool clearValue = false)
        {
            EnsureAlive(index);

            int n = m_next[index];
            int p = m_prev[index];

            if (p != c_NULL) m_next[p] = n;
            else m_head = n;
            if (n != c_NULL) m_prev[n] = p;
            else m_tail = p;

            if (clearValue)
                m_aryValues[index] = default!;
            
            m_isAlive[index] = false;

            //next를 스택 링크로 사용하기 위해서..
            m_next[index] = m_freeHeadIdx;
            m_prev[index] = c_NULL;
            m_freeHeadIdx = index;

            m_count--;
            m_version++;
        }

        //전체 초기화 (용량은 유지함)
        public void Clear(bool clearValues = false)
        {
            if (clearValues)
                Array.Clear(m_aryValues, 0, m_aryValues.Length);

            Array.Clear(m_isAlive, 0, m_isAlive.Length);

            m_head = m_tail = c_NULL;
            m_count = 0;
            m_version++;

            BuildFreeList(0, m_aryValues.Length);
        }

        //----------------------------------------------------------------------
        //-------- 내부 구현 --------
        private int InsertInternal(int anchor, bool after, in T value)
        {
            int idx = AllocSlot();
            m_aryValues[idx] = value;
            m_isAlive[idx] = true;

            if (m_count == 0)
            {
                m_head = m_tail = idx;
                m_next[idx] = m_prev[idx] = c_NULL;
            }
            else if (anchor == c_NULL)
            {
                //anchor가 NULL이면
                //after=false => AddFirst
                //after=true => AddLast와 동일
                if (after) //AddLast
                {
                    m_prev[idx] = m_tail;
                    m_next[idx] = c_NULL;
                    m_next[m_tail] = idx;
                    m_tail = idx;
                }
                else //AddFirst
                {
                    m_prev[idx] = c_NULL;
                    m_next[idx] = m_head;
                    m_prev[m_head] = idx;
                    m_head = idx;
                }
            }
            else
            {
                if (after)
                {
                    int n = m_next[anchor];
                    m_prev[idx] = anchor;
                    m_next[idx] = n;
                    m_next[anchor] = idx;
                    if (n != c_NULL) m_prev[n] = idx;
                    else m_tail = idx;
                }
                else
                {
                    int p = m_prev[anchor];
                    m_next[idx] = anchor;
                    m_prev[idx] = p;
                    m_prev[anchor] = idx;
                    if (p != c_NULL) m_next[p] = idx;
                    else m_head = idx;
                }
            }

            m_count++;
            m_version++;
            return idx;
        }

        private int AllocSlot()
        {
            if (m_freeHeadIdx == c_NULL)
            {
                Grow(); //용량 키우기...
            }

            int idx = m_freeHeadIdx;
            m_freeHeadIdx = m_next[idx];
            
            //안전을 위해 초기화
            m_next[idx] = m_prev[idx] = c_NULL;
            return idx;
        }

        private void Grow()
        {
            int oldCap = m_aryValues.Length;
            int newCap = Math.Max(1, oldCap << 1);

            Array.Resize(ref m_aryValues, newCap);
            Array.Resize(ref m_next, newCap);
            Array.Resize(ref m_prev, newCap);
            Array.Resize(ref m_isAlive, newCap);

            //새 영역 연결
            BuildFreeList(oldCap, newCap);
        }

        private void BuildFreeList(int startInclusive, int capacity)
        {
            //start ~ capacity-1 를 스택으로......
            //[start] -> [start+1] -> ... -> [cap-1] -> 현재 freeHead 이런식임
            for (int i = startInclusive; i < capacity; i++)
            {
                m_next[i] = (i + 1 < capacity) ? i + 1 : m_freeHeadIdx;
                m_prev[i] = c_NULL;
                m_isAlive[i] = false;
            }

            m_freeHeadIdx = startInclusive;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureAlive(int index)
        {
            if ((uint)index >= (uint)m_aryValues.Length || !m_isAlive[index])
                throw new IndexOutOfRangeException("[ IndexLinkedList ] Invalid or freed index!!!");
        }

        //----------------------------------------------------------------------
        //------ T값을 도는 열거자 ----------------------------------------------
        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<T>
        {
            private readonly IndexLinkedList<T> m_list;
            private readonly int m_version;
            private int m_currentIdx;   // 현재 노드의 인덱스
            private T m_current;        // 현재 값 (T)

            //--------------------------------------------
            internal Enumerator(IndexLinkedList<T> list)
            {
                m_list = list;
                m_version = list.m_version;
                m_currentIdx = c_NULL;
                m_current = default!;
            }

            //--------------------------------------------
            public T Current => m_current;
            object IEnumerator.Current => m_current!;

            //--------------------------------------------
            public bool MoveNext()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("Collection was modified during enumeration.");

                if (m_currentIdx == c_NULL)
                {
                    m_currentIdx = m_list.m_head;
                }
                else
                {
                    m_currentIdx = (m_currentIdx != c_NULL) ? m_list.m_next[m_currentIdx] : c_NULL;
                }

                if (m_currentIdx == c_NULL)
                {
                    m_current = default!;
                    return false;
                }

                m_current = m_list.m_aryValues[m_currentIdx];
                return true;
            }

            public void Reset()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("[ IndexLinkedList ] Collection modified during enumeration!!!!");
                m_currentIdx = c_NULL;
                m_current = default!;
            }

            public void Dispose() { }
        }
        
        //---------------------------------------------------------------------------
        //---index를 도는 열거자-------------------------------------------------------
        public IndicesEnumerable Indices() => new IndicesEnumerable(this);

        public readonly struct IndicesEnumerable : IEnumerable<int>
        {
            private readonly IndexLinkedList<T> m_list;
            public IndicesEnumerable(IndexLinkedList<T> list) => m_list = list;

            public IndicesEnumerator GetEnumerator() => new IndicesEnumerator(m_list);
            IEnumerator<int> IEnumerable<int>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct IndicesEnumerator : IEnumerator<int>
        {
            private readonly IndexLinkedList<T> m_list;
            private readonly int m_version;
            private int m_currentIdx;
            
            //-----------------------------------------------
            public int Current { get; private set; }
            object IEnumerator.Current => Current;

            //-----------------------------------------------
            internal IndicesEnumerator(IndexLinkedList<T> list)
            {
                m_list = list;
                m_version = list.m_version;
                m_currentIdx = c_NULL;
                Current = c_NULL;
            }

            //-----------------------------------------------
            public bool MoveNext()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("Collection was modified during enumeration.");

                if (m_currentIdx == c_NULL)
                    m_currentIdx = m_list.m_head;
                else
                    m_currentIdx = (m_currentIdx != c_NULL) ? m_list.m_next[m_currentIdx] : c_NULL;

                Current = m_currentIdx;
                return m_currentIdx != c_NULL;
            }

            public void Reset()
            {
                if (m_version != m_list.m_version)
                    throw new InvalidOperationException("Collection was modified during enumeration.");
                m_currentIdx = c_NULL;
                Current = c_NULL;
            }

            public void Dispose() { }
        }
    }
}