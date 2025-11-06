using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LUIZ.InputSystem
{
    internal class PollingEntry<T>  where T : struct
    {
        public InputAction Action { get; private set; }
        public T PrevValue { get; private set; }
        public IInputComparer<T> Comparer { get; private set; }
        public string ActionName { get; private set; }

        public bool IsActive { get; private set; } = false;
        
        //---------------------------
        public void Setup(string actionName, InputAction action, IInputComparer<T> comparer)
        {
            ActionName = actionName;
            Action = action;
            Comparer = comparer ?? CreateDefaultComparer();
            PrevValue = default;
        }
        
        public void Reset()
        {
            ActionName = null;
            Action = null;
            PrevValue = default;
            Comparer = null;
        }
        
        public void SetPrevValue(T value) => PrevValue = value;
        public void SetActive(bool isActive) => IsActive = isActive;
        
        public void SetComparer(IInputComparer<T> newComparer)
        {
            Comparer = newComparer ?? CreateDefaultComparer();
            PrevValue = default;
        }

        //---------------------------
        private static IInputComparer<T> CreateDefaultComparer()
        {
            if (typeof(T) == typeof(Vector2))
                return (IInputComparer<T>)new Vector2MoveComparer();

            if (typeof(T) == typeof(float))
                return (IInputComparer<T>)new FloatButtonComparer();
            //TODO : 나중에 추가 기본 컴페어러 작성?

            return new DefaultComparer();
        }

        private class DefaultComparer : IInputComparer<T>
        {
            public bool ShouldInvoke(T prev, T current)
                => !EqualityComparer<T>.Default.Equals(prev, current);
        }
    }
}