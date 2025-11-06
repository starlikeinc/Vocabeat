using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

namespace LUIZ.InputSystem
{
    [CreateAssetMenu(fileName = "InputSO", menuName = "LUIZ/ScriptableObjects/InputSO")]
    public class InputSO : ScriptableObject
    {
        private readonly Dictionary<string, InputAction> m_dicCachedActions = new();
        
        private readonly WrapperRegistry m_wrappersPerformed = new();
        private readonly WrapperRegistry m_wrappersPolling = new();

        private readonly PollingActionDispatcher m_pollingDispatcher = new();
        
        private Func<string, InputAction> m_resolveAction;
        
        //-----------------------------------------------------------------------
        public event Action<EInputDeviceType> OnLastInputDeviceChanged;
        public event Action<EInputDeviceType, InputDeviceChange> OnInputDeviceConnectionEvent;
        
        //-------------------------------------------------------------------------
        public void DoSubscribeAction<T>(string actionName, Action<T> callback, bool isPolling = false) where T : struct
        {
            var wrappers = isPolling ? m_wrappersPolling : m_wrappersPerformed;

            if (wrappers.TryGet<T>(actionName, out var typedWrapper))
            {
                typedWrapper.Add(callback);
                return;
            }

            var createdWrapper = wrappers.GetOrCreate<T>(actionName);
            createdWrapper.Add(callback);

            if (isPolling)
                PrivSubscribePolling(actionName, createdWrapper);
            else
                PrivAttachPerformedHandler(actionName, typedWrapper);
        }

        public void DoUnsubscribeAction<T>(string actionName, Action<T> callback, bool isPolling = false) where T : struct
        {
            var wrappers = isPolling ? m_wrappersPolling : m_wrappersPerformed;

            if (wrappers.TryGet<T>(actionName, out var wrapper))
            {
                wrapper.Remove(callback);
                //polling 쪽이고 더 이상 구독자가 없으면 polling entry 제거
                if (isPolling && wrapper.IsEmpty())
                {
                    m_pollingDispatcher.Unregister<T>(actionName);
                    wrappers.RemoveIfEmpty<T>(actionName);
                }
            }
        }
        
        /// <summary> UI를 통해서 Action를 조작하게 될때 (모바일 등) 이 함수를 이용하여 이벤트를 트리거 함 </summary>
        public void DoInvokeEvent<T>(string key, T value, bool isPolling = false) where T : struct
        {
            var wrappers = isPolling ? m_wrappersPolling : m_wrappersPerformed;
            var wrapper = wrappers.GetRaw(key);

            if (wrapper != null)
            {
                try { wrapper.TryCastAndInvoke(value); }
                catch (Exception ex) { Debug.LogError($"[InputSO] Error in DoInvokeEvent '{key}': {ex}"); }
            }
            else
                Debug.LogWarning($"[InputSO] Action '{key}' not registered (isPolling={isPolling}).");
        }
        
        public void SetPollingComparer<T>(string actionName, IInputComparer<T> comparer) where T : struct
        {
            m_pollingDispatcher.SetComparer(actionName, comparer);
        }
        
        //-----------------------------------------------------------------------
        internal void Init(Func<string, InputAction> resolveAction)
        {
            m_resolveAction = resolveAction;
            m_dicCachedActions.Clear();
        }

        internal void InterUpdatePolling()
        {
            m_pollingDispatcher.UpdateAllTypes();
        }

        internal void InterInvokeLastInputDeviceChanged(EInputDeviceType deviceType) 
            => OnLastInputDeviceChanged?.Invoke(deviceType);
        internal void InterInvokeInputDeviceConnectionEvent(EInputDeviceType deviceType, InputDeviceChange change) 
            => OnInputDeviceConnectionEvent?.Invoke(deviceType, change);
        
        //----------------------------------------------------------
        private void PrivSubscribePolling<T>(string actionName, DelegateWrapper<T> wrapper) where T : struct
        {
            var action = PrivGetOrCacheAction(actionName);
            if (action == null)
            {
                Debug.LogError($"[InputSO] Cannot register polling. Action '{actionName}' not found.");
                return;
            }

            m_pollingDispatcher.Register<T>(actionName, action, PrivInvokePollingAction);

            action.started += _ =>
            {
                if (m_pollingDispatcher.TryGetPollingEntry<T>(actionName, out var entry))
                    entry.SetActive(true);
            };
            action.canceled += _ =>
            {
                if (m_pollingDispatcher.TryGetPollingEntry<T>(actionName, out var entry))
                    entry.SetActive(false);
            };
        }
        
        private void PrivInvokePollingAction<T>(string actionName, T value) where T : struct
        {
            if (m_wrappersPolling.TryGet<T>(actionName, out var wrapper))
                wrapper.Invoke(value);
        }

        private void PrivAttachPerformedHandler<T>(string actionName, DelegateWrapper<T> wrapper) where T : struct
        {
            var action = PrivGetOrCacheAction(actionName);
            if (action == null)
                return;

            action.performed += ctx =>
            {
                try
                {
                    var value = ctx.ReadValue<T>();
                    wrapper.Invoke(value);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[InputSO] Error in performed callback '{actionName}': {ex}");
                }
            };
        }
        
        private InputAction PrivGetOrCacheAction(string actionName)
        {
            if (m_dicCachedActions.TryGetValue(actionName, out var cached))
                return cached;

            var action = m_resolveAction?.Invoke(actionName);
            if (action != null)
                m_dicCachedActions[actionName] = action;
            else
                Debug.LogWarning($"[InputSO] Failed to resolve InputAction '{actionName}'");

            return action;
        }
    }
}
