using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI
{
    /// <summary>
    /// 캔버스를 사용하는 Widget의 경우 해당 클래스를 상속 받을것. 부모 UIFrame의 소트 오더에 맞춰 자동으로 소팅 오프셋을 맞춰준다.
    /// </summary>
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(Canvas))]
    public abstract class UIWidgetCanvasBase : UIWidgetBase
    {
        [Header("[ UIWidgetCanvas ]")]
        [SerializeField] private int SortOrderOffset = 1;       public int GetCanvasSortOrderOffset() { return SortOrderOffset; }

        private Canvas m_widgetCanvas = null;                   public Canvas GetWidgetCanvas() { return m_widgetCanvas; }

        //-------------------------------------------------------------------
        protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
        {
            base.OnUIWidgetInitialize(parentFrame);

            m_widgetCanvas = GetComponent<Canvas>();

            ProtChangeSortOrder();
        }

        protected override void OnUIWidgetShow()
        {
            base.OnUIWidgetShow();

            ProtChangeSortOrder();
        }

        protected override void OnUIWidgetParentFrameShow()
        {
            base.OnUIWidgetParentFrameShow();

            ProtChangeSortOrder();
        }

        //-------------------------------------------------------------------
        protected virtual void ProtChangeSortOrder()
        {
            Canvas parentCanvas = GetParentUIFrame().GetUIFrameCanvas();
            m_widgetCanvas.overrideSorting = true;
            m_widgetCanvas.sortingLayerName = parentCanvas.sortingLayerName;
            m_widgetCanvas.sortingOrder = parentCanvas.sortingOrder + SortOrderOffset;
        }

        //-------------------------------------------------------------------
    }
}
