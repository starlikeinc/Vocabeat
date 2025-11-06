using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

namespace LUIZ.UI
{
    public class LToggleRadio : LToggle
    {
        private static readonly Dictionary<string, List<LToggleRadio>> c_dicRadioGroups = new Dictionary<string, List<LToggleRadio>>();

        [SerializeField] private string GroupName;

        //------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();
            RegisterRadioGroup(GroupName, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterRadioGroup(GroupName, this);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (IsToggleOn() == true)//이미 켜져있으면 눌리지 않는다.
                return;

            base.OnPointerClick(eventData);
        }

        //----------------------------------------------------
        protected override void OnToggleOnOff(bool isOn, bool isInvokeEvent)
        {
            if (isOn == true)
            {
                ChainHideRadioGroup(!isOn, isInvokeEvent);
            }
            else
            {
                ChainHideRadioGroup(!isOn, isInvokeEvent);
            }
        }
        
        //----------------------------------------------------
        internal void InterToggleChainOnOff(bool isOn, bool isInvokeEvent)
        {
            ProtToggleOnOff(isOn, isInvokeEvent, false);
        }
        
        //----------------------------------------------------
        private void ChainHideRadioGroup(bool isOn, bool isInvokeEvent)
        {
            if (c_dicRadioGroups.TryGetValue(GroupName, out List<LToggleRadio> group) == true)
            {
                foreach(LToggleRadio radio in group)
                {
                    if(this == radio)
                        continue;
                    
                    radio.InterToggleChainOnOff(isOn, isInvokeEvent);
                }
            }
        }

        //------------------------------------------------------
        private void RegisterRadioGroup(string groupName, LToggleRadio radioBtn)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return;

            if(c_dicRadioGroups.TryGetValue(groupName, out List<LToggleRadio> group) == true)
            {
                group.Add(radioBtn);
            }
            else
            {
                List<LToggleRadio> newGroup = new List<LToggleRadio>() { radioBtn };
                c_dicRadioGroups.Add(groupName, newGroup);
            }
        }

        private void UnregisterRadioGroup(string groupName, LToggleRadio radioBtn)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                return;

            if (c_dicRadioGroups.TryGetValue(groupName, out List<LToggleRadio> group) == true)
            {
                group.Remove(radioBtn);

                if(group.Count <= 0)
                {
                    c_dicRadioGroups.Remove(groupName);
                }
            }
        }
    }
}
