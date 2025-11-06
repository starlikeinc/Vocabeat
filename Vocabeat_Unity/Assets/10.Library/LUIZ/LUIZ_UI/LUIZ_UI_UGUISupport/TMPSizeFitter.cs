using UnityEngine;

namespace LUIZ.UI
{
    public class TMPSizeFitter : MonoBase
    {
        [SerializeField] private Vector2 SizeOffset;
        [SerializeField] private bool SizeWidth = true;
        [SerializeField] private bool SizeHeight = false;

        [SerializeField] private LText_TMP TMPInstance;

        private RectTransform m_rectTransform = null;

        //-----------------------------------------------
        protected override void OnUnityAwake()
        {
            m_rectTransform = transform as RectTransform;
        }

        //-------------------------------------------------
        public void OnEnable()
        {
            TMPInstance?.DoTMPChangedEventSubscribe(HandleTMPSizeFitterTextEvent);
        }

        public void OnDisable()
        {
            TMPInstance?.DoTMPChangedEventUnsubscribe(HandleTMPSizeFitterTextEvent);
        }

        //-------------------------------------------------------------------
        private void PrivTEMPSizeFitterReSizing()
        {
            Vector2 vecSize = m_rectTransform.sizeDelta;
            if (SizeWidth)
            {
                vecSize.x = TMPInstance.preferredWidth + SizeOffset.x;
            }
            if (SizeHeight)
            {
                vecSize.y = TMPInstance.preferredHeight + SizeOffset.y;
            }

            m_rectTransform.sizeDelta = vecSize;
        }

        private void HandleTMPSizeFitterTextEvent()
        {
            PrivTEMPSizeFitterReSizing();
        }
    }
}
