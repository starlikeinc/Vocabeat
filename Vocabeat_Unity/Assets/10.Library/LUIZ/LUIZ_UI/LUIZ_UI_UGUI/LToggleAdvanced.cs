using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace LUIZ.UI
{
    /// <summary>
    /// 토글이 on off 2개의 상태가 아니라 N개의 상태를 가지게 표현하고 싶을때 이용하는 토글?버튼이다.(누를때마다 순환하는 구조)
    /// UIStateVisualHandler와 이용해도 좋음. 대신 메모리에 좋은 구조가 아니라 적절히 필요한 곳에서만 쓸것.....
    /// </summary>
    public class LToggleAdvanced : LButton
    {
        [System.Serializable]
        private class ToggleData
        {
            public string StateName;
            public UnityEvent ToggleEvent;
        }

        [SerializeField] private string DefaultVisualState;
        [SerializeField] private ToggleData[] ToggleDataAry;

        private int m_curStateIndex = 0;                       public int GetToggleCurrentStateIndex() { return m_curStateIndex; }

        private IIndexSelector m_customIndexSelector = null;   public void SetCustomIndexSelector(IIndexSelector selector) { m_customIndexSelector = selector; }
        
        //----------------------------------------------------------
        protected override void Start()
        {
            base.Start();
#if UNITY_EDITOR
            if (Application.isPlaying)
                DoToggleOnOff(DefaultVisualState, false);
#else
            DoToggleOnOff(DefaultVisualState, false);
#endif
        }

        //----------------------------------------------------------
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            int nextIndex;
            if (m_customIndexSelector == null)
            {
                //기본으로 Forward 이용
                nextIndex = IndexSelectors.Forward.GetNextIndex(m_curStateIndex, ToggleDataAry.Length);
            }
            else
            {
                nextIndex = m_customIndexSelector.GetNextIndex(m_curStateIndex, ToggleDataAry.Length);
            }

            DoToggleOnOff(nextIndex, true);
        }

        //----------------------------------------------------------
        public void DoToggleOnOff(string stateName, bool isInvokeEvent = true)
        {
            if (PrivTryGetToggleStateData(stateName, out int stateIndex, out ToggleData toggleData))
            {
                PrivToggleState(toggleData, isInvokeEvent);
                m_curStateIndex = stateIndex;
            }
        }

        public void DoToggleOnOff(int stateIndex, bool isInvokeEvent = true)
        {
            if(PrivTryGetToggleStateData(stateIndex, out ToggleData toggleData))
            {
                PrivToggleState(toggleData, isInvokeEvent);
                m_curStateIndex = stateIndex;
            }
        }
        
        //----------------------------------------------------------
        private void PrivToggleState(ToggleData toggleData, bool isInvokeEvent)
        {
            if (isInvokeEvent == true)
                toggleData.ToggleEvent?.Invoke();
        }

        private bool PrivTryGetToggleStateData(int stateIndex, out ToggleData toggleData)
        {
            bool isSuccess = false;
            toggleData = null;

            if (stateIndex >= 0 && stateIndex < ToggleDataAry.Length)
            {
                toggleData = ToggleDataAry[stateIndex];
                isSuccess = true;
            }

            if (isSuccess == false)
            {
                Debug.LogError($"[LToggleAdvanced] ObjName : {this.gameObject.name} StateIndex : {stateIndex} not Valid");
            }

            return isSuccess;
        }

        private bool PrivTryGetToggleStateData(string stateName, out int stateIndex, out ToggleData toggleData)
        {
            bool isSuccess = false;
            toggleData = null;
            stateIndex = 0;

            if (string.IsNullOrWhiteSpace(stateName))
                return false;

            for (int i = 0; i < ToggleDataAry.Length; i++)
            {
                if (ToggleDataAry[i].StateName == stateName)
                {
                    toggleData = ToggleDataAry[i];
                    stateIndex = i;
                    isSuccess = true;
                    break;
                }
            }

            if (isSuccess == false)
            {
                Debug.LogError($"[LToggleAdvanced] ObjName : {this.gameObject.name} StateName : {stateName} not Valid");
            }

            return isSuccess;
        }
    }
}
