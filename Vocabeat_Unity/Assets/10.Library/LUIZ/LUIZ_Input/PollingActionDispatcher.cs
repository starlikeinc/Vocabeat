using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LUIZ.InputSystem
{
    internal class PollingActionDispatcher
    {
        private abstract class PollingGroupBase
        {
            public abstract void Update();
        }

        private class PollingGroup<T> : PollingGroupBase where T : struct
        {
            private readonly Dictionary<string, PollingEntry<T>> m_dicEntries = new();
            private readonly Action<string, T> m_onInvoke;
            private readonly Action<string> m_onFail;

            //---------------------------------
            public PollingGroup(Action<string, T> onInvoke, Action<string> onFail = null)
            {
                m_onInvoke = onInvoke;
                m_onFail = onFail;
            }
            
            //---------------------------------
            public void Register(string actionName, InputAction action, IInputComparer<T> comparer = null)
            {
                if (!m_dicEntries.ContainsKey(actionName))
                {
                    var entry = new PollingEntry<T>();
                    entry.Setup(actionName, action, comparer);
                    m_dicEntries[actionName] = entry;
                }
                else
                {
                    Debug.LogWarning($"[PollingGroup<{typeof(T).Name}>] '{actionName}' already registered.");
                }
            }
            public void Unregister(string actionName)
            {
                if (m_dicEntries.TryGetValue(actionName, out var entry))
                    entry.Reset();
                
                m_dicEntries.Remove(actionName);
            }
            
            public void SetComparer(string actionName, IInputComparer<T> comparer)
            {
                if (m_dicEntries.TryGetValue(actionName, out var entry))
                    entry.SetComparer(comparer);
            }
            
            public bool TryGetEntry(string actionName, out PollingEntry<T> entry)
            {
                return m_dicEntries.TryGetValue(actionName, out entry);
            }
            
            public override void Update()
            {
                foreach (var entry in m_dicEntries.Values)
                {
                    if (!entry.IsActive) continue;
                    
                    var action = entry.Action;
                    if (action == null || !action.enabled)
                    {
                        m_onFail?.Invoke(entry.ActionName);
                        continue;
                    }
                    T value = action.ReadValue<T>();
                    if (entry.Comparer.ShouldInvoke(entry.PrevValue, value))
                    {
                        m_onInvoke?.Invoke(entry.ActionName, value);
                        entry.SetPrevValue(value);
                    }
                }
            }
        }

        //------------------------------------------------
        //타입별 그룹
        private readonly Dictionary<Type, PollingGroupBase> m_dicGroups = new();
        
        //------------------------------------------------
        public IEnumerable<Type> GetRegisteredTypes() => m_dicGroups.Keys;
        
        public void Register<T>(string actionName, InputAction action,  Action<string, T> onChanged, IInputComparer<T> comparer = null, Action<string> onFail = null) where T : struct
        {
            var type = typeof(T);
            if (!m_dicGroups.TryGetValue(type, out var baseGroup))
            {
                var newGroup = new PollingGroup<T>(onChanged, onFail);
                m_dicGroups[type] = newGroup;
                newGroup.Register(actionName, action, comparer);
            }
            else if (baseGroup is PollingGroup<T> group)
            {
                group.Register(actionName, action, comparer);
            }
            else
            {
                Debug.LogError($"[PollingActionDispatcher] Type mismatch in group for type {type}");
            }
        }
        public void Unregister<T>(string actionName) where T : struct
        {
            var type = typeof(T);
            if (m_dicGroups.TryGetValue(type, out var baseGroup) && baseGroup is PollingGroup<T> group)
            {
                group.Unregister(actionName);
            }
        }

        public void UpdateAllTypes()
        {
            foreach (var group in m_dicGroups.Values)
                group.Update();
        }

        public void UpdateType<T>() where T : struct
        {
            if (m_dicGroups.TryGetValue(typeof(T), out var baseGroup) && baseGroup is PollingGroup<T> group)
                group.Update();
        }
        
        public void SetComparer<T>(string actionName, IInputComparer<T> comparer) where T : struct
        {
            if (m_dicGroups.TryGetValue(typeof(T), out var baseGroup) && baseGroup is PollingGroup<T> group)
                group.SetComparer(actionName, comparer);
            else
                Debug.LogWarning($"[PollingActionDispatcher] Cannot set comparer. No polling group found for {typeof(T)} or group type mismatch.");
        }
        
        public bool TryGetPollingEntry<T>(string actionName, out PollingEntry<T> entry) where T : struct
        {
            entry = null;
            if (m_dicGroups.TryGetValue(typeof(T), out var baseGroup) && baseGroup is PollingGroup<T> group)
                return group.TryGetEntry(actionName, out entry);
            return false;
        }
    }
}