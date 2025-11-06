using UnityEngine;
using UnityEngine.EventSystems;

namespace LUIZ.UI
{
    public class UIPointerEventScaler : MonoBase, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private bool ScaleOnClick = false;
        [SerializeField, Range(0.5f, 1.5f)] private float ClickScaleRatio = 0.95f;

        [SerializeField] private bool ScaleOnHover = false;
        [SerializeField, Range(0.5f, 1.5f)] private float HoverScaleRatio = 1.03f;

        private Vector3 m_originalScale = Vector3.one;

        //------------------------------------------------------
        protected override void OnUnityAwake()
        {
            m_originalScale = this.transform.localScale;
        }

        //------------------------------------------------------
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ScaleOnHover == false)
                return;

            this.transform.localScale = m_originalScale * HoverScaleRatio;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (ScaleOnHover == false)
                return;

            this.transform.localScale = m_originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ScaleOnClick == false)
                return;

            this.transform.localScale = m_originalScale * ClickScaleRatio;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (ScaleOnClick == false)
                return;

            this.transform.localScale = m_originalScale;
        }
    }
}