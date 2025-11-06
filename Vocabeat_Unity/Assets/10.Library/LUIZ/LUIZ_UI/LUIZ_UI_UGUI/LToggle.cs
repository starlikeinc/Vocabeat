using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace LUIZ.UI
{
    public class LToggle : LButton
    {
        [SerializeField] private bool DefaultOn = false;

        [SerializeField] private GameObject PivotOn;
        [SerializeField] private GameObject PivotOff;

        [SerializeField] private UnityEvent ToggleOnEvent;
        [SerializeField] private UnityEvent ToggleOffEvent;

        private bool m_isOn = false;              public bool IsToggleOn() { return m_isOn; }

        //----------------------------------------------------
        protected override void Awake()
        {
            base.Awake();
            m_isOn = DefaultOn;
        }

        protected override void Start()
        {
            base.Start();
            ProtToggleOnOff(m_isOn, false, false);
        }

        //----------------------------------------------------
        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            ProtToggleOnOff(!m_isOn, true);
        }

        //----------------------------------------------------
        public void DoToggleOnOff(bool isOn, bool isInvokeEvent = true)
        {
            ProtToggleOnOff(isOn, isInvokeEvent);
        }

        //----------------------------------------------------
        protected void ProtToggleOnOff(bool isOn, bool isInvokeEvent, bool isInvokeChildEvents = true)
        {
            if (PivotOn == null || PivotOff == null)
            {
                Debug.LogWarning("[LToggle] Pivot is NULL");
                return;
            }

            if(isOn == true)
            {
                PivotOn.SetActive(true);
                PivotOff.SetActive(false);

                if(isInvokeEvent == true)
                    ToggleOnEvent?.Invoke();
            }
            else
            {
                PivotOn.SetActive(false);
                PivotOff.SetActive(true);

                if (isInvokeEvent == true)
                    ToggleOffEvent?.Invoke();
            }

            if(isInvokeChildEvents == true)
                OnToggleOnOff(isOn, isInvokeEvent);

            m_isOn = isOn;
        }

        //----------------------------------------------------
        protected virtual void OnToggleOnOff(bool isOn, bool isInvokeEvent) { }
    }
}
