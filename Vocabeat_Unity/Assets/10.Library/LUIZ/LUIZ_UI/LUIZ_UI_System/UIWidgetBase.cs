using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI
{
    public abstract class UIWidgetBase : MonoBase
    {
        /// <summary>
        /// DoUIWidgetShow,Hide 에서 GraphicOnly = true로 사용하기 위한 옵션
        /// </summary>
        protected bool m_isCollectChildGraphics = false;

        private RectTransform   m_rectTransform = null;     protected RectTransform    GetRectTransform()  { return m_rectTransform; }
        private UIFrameBase     m_parentFrame   = null;     protected UIFrameBase      GetParentUIFrame()  { return m_parentFrame; }
        private UIWidgetBase    m_ownerWidget   = null;     protected UIWidgetBase     GetOwnerWidget()    { return m_ownerWidget; }

        private bool m_isShow = true;                       public bool IsShow() { return m_isShow; }
        
        private List<Graphic> m_listChildGraphics = null;
        private readonly List<UIWidgetBase>  m_listChildWidget = new List<UIWidgetBase>();

        //----------------------------------------------------------------
        /// <summary>
        /// isEnableCanvasOnly = true 일 경우 위젯의 SetActive(true) 가 아닌 하위 모든 Graphic의 Graphic.enabled = true 한다.
        /// 오브젝트가 켜져있어 Update가 돌아 감에 유의, 오히려 이터레이팅이 클 수 있음
        /// 특정 상황에만 사용할 것 
        /// </summary>
        public void DoUIWidgetShow(bool isEnableGraphicOnly = false)
        {
            ShowHideWidget(true, isEnableGraphicOnly);
        }

        /// <summary>
        /// isDisableGraphicOnly = true 일 경우  위젯의 SetActive(false) 가 아닌 하위 모든 Graphic의 Graphic.enabled = false 한다.
        /// 오브젝트가 켜져있어 Update가 돌아 감에 유의, 오히려 이터레이팅이 클 수 있음
        /// 특정 상황에만 사용할 것
        /// </summary>
        public void DoUIWidgetHide(bool isDisableGraphicOnly = false)
        {
            ShowHideWidget(false, isDisableGraphicOnly);
        }

        //----------------------------------------------------------------
        protected IEnumerable<UIWidgetBase> ProtUIWidgetGetChildWidgets()
        {
            return m_listChildWidget;
        }

        //----------------------------------------------------------------
        internal void InterUIWidgetInitialize(UIFrameBase parentFrame)
        {
            m_isShow = this.gameObject.activeSelf;

            m_parentFrame = parentFrame;
            m_rectTransform = GetComponent<RectTransform>();

            CollectChildWidgetGraphics();

            OnUIWidgetInitialize(parentFrame);
        }

        internal void InterUIWidgetInitializePost(UIFrameBase parentFrame)
        {
            OnUIWidgetInitializePost(parentFrame);
        }

        internal void InterUIWidgetParentWidget(UIWidgetBase parentWidget)
        {
            m_ownerWidget = parentWidget;
            OnUIWidgetParentWidget(parentWidget);
        }

        internal void InterUIWidgetGraphicShow()
        {
            ShowHideGraphic(true);
        }

        internal void InterUIWidgetGraphicHide()
        {
            ShowHideGraphic(false);
        }

        internal void InterUIWidgetParentFrameShow()
        {
            OnUIWidgetParentFrameShow();
        }

        internal void InterUIWidgetParentFrameHide()
        {
            OnUIWidgetParentFrameHide();
        }

        //----------------------------------------------------------------
        private void ShowHideWidget(bool isShow, bool isGraphicOnly)
        {
            if (isGraphicOnly == false)
            {
                if (isShow == m_isShow)
                {
                    //Debug.LogWarning($"[UIWidgetBase] {this.gameObject.name} isShow already {isShow}");
                    return;
                }
                ShowHideGameObject(isShow);
            }
            else
            {
                if (m_listChildGraphics == null)
                {
                    Debug.LogError($"[UIWidgetBase] ListChildGraphics is null. please enable CollectChildGraphics = true. Will use SetActive for show hide." );
                    ShowHideGameObject(isShow);
                }
                else
                {
                    ShowHideGraphic(isShow);
                }
            }
        }

        private void ShowHideGameObject(bool isShow)
        {
            m_isShow = isShow;
            this.gameObject.SetActive(isShow);

            if (isShow == true)
                OnUIWidgetShow();
            else
                OnUIWidgetHide();
        }

        private void ShowHideGraphic(bool isShow)
        {
            for (int i = 0; i < m_listChildGraphics.Count; i++)
            {
                m_listChildGraphics[i].enabled = isShow;
            }

            for (int i = 0; i < m_listChildWidget.Count; i++)
            {
                if (isShow == true)
                    m_listChildWidget[i].InterUIWidgetGraphicShow();
                else
                    m_listChildWidget[i].InterUIWidgetGraphicHide();
            }

            if (isShow == true)
                OnUIWidgetGraphicShow();
            else
                OnUIWidgetGraphicHide();
        }

        private void CollectChildWidgetGraphics()
        {
            if(m_isCollectChildGraphics == true)
            {
                m_listChildGraphics = new List<Graphic>();

                Graphic graphic = this.gameObject.GetComponent<Graphic>();
                if (graphic != null)
                {
                    m_listChildGraphics.Add(graphic);
                }
            }

            PrivCollectChildRecursive(this.transform);
        }

        private void PrivCollectChildRecursive(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                UIWidgetBase childWidget = childTransform.GetComponent<UIWidgetBase>();

                if (childWidget != null)
                {
                    childWidget.InterUIWidgetParentWidget(this);
                    m_listChildWidget.Add(childWidget);
                }
                else
                {
                    if (m_isCollectChildGraphics == true)
                    {
                        Graphic graphic = childTransform.GetComponent<Graphic>();
                        if (graphic != null)
                        {
                            m_listChildGraphics.Add(graphic);
                        }
                    }

                    PrivCollectChildRecursive(childTransform);
                }
            }
        }
        //----------------------------------------------------------------
        /// <summary> UIFrame에 등록될때 호출. Awake 처럼 이용 </summary>
        protected virtual void OnUIWidgetInitialize(UIFrameBase parentFrame) { }
        /// <summary> Initialize가 완료된 이후 호출. Start 처럼 이용 </summary>
        protected virtual void OnUIWidgetInitializePost(UIFrameBase parentFrame) { }

        protected virtual void OnUIWidgetParentWidget(UIWidgetBase parentWidget) { }

        /// <summary> 직접 ShowHide 되었을때 호출되는 이벤트 </summary>
        protected virtual void OnUIWidgetShow() { }
        /// <summary> 직접 ShowHide 되었을때 호출되는 이벤트 </summary>
        protected virtual void OnUIWidgetHide() { }

        protected virtual void OnUIWidgetGraphicShow() { }
        protected virtual void OnUIWidgetGraphicHide() { }

        /// <summary> 부모 Frame이 ShowHide 되었을때 호출되는 이벤트 </summary>
        protected virtual void OnUIWidgetParentFrameShow() { }
        /// <summary> 부모 Frame이 ShowHide 되었을때 호출되는 이벤트 </summary>
        protected virtual void OnUIWidgetParentFrameHide() { }

        //----------------------------------------------------------------
        //기타 Rect 관련 편의 기능
        public void SetUIPositionX(float X)
        {
            m_rectTransform.anchoredPosition = new Vector3(X, m_rectTransform.anchoredPosition.y);
        }

        public void SetUIPositionY(float Y)
        {
            m_rectTransform.anchoredPosition = new Vector3(m_rectTransform.anchoredPosition.x, Y);
        }

        public void SetUIPosition(float X, float Y)
        {
            m_rectTransform.anchoredPosition = new Vector2(X, Y);
        }

        public void SetUIPosition(Vector2 position)
        {
            m_rectTransform.anchoredPosition = position;
        }

        public void SetUIPositionMoveX(float X)
        {
            m_rectTransform.localPosition = new Vector2(m_rectTransform.localPosition.x + X, m_rectTransform.localPosition.y);
        }

        public void SetUIPositionMoveY(float Y)
        {
            m_rectTransform.localPosition = new Vector2(m_rectTransform.localPosition.x, m_rectTransform.localPosition.y + Y);
        }

        public void SetUISiblingLowest()
        {
            m_rectTransform.SetSiblingIndex(0);
        }

        public void SetUISiblingTopMost()
        {
            Transform parentTransform = transform.parent;
            if (parentTransform != null)
            {
                m_rectTransform.SetSiblingIndex(parentTransform.childCount - 1);
            }
            else
            {
                Debug.LogError($"[UIWidgetBase] Fail to set {this.gameObject.name} as sibiling Topmost. widget has no parent");
            }
        }

        public void SetUIWidth(float width)
        {
            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void SetUIHeight(float height)
        {
            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public void SetUISize(Vector2 size)
        {
            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            m_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public void SetUIPivot(Vector2 pivot)
        {
            m_rectTransform.pivot = pivot;
        }

        //-------------------------------------------------------------------------
        public float GetUIWidth()
        {
            return m_rectTransform.rect.width;
        }

        public float GetUIHeight()
        {
            return m_rectTransform.rect.height;
        }

        public Vector2 GetUISize()
        {
            return new Vector2(m_rectTransform.rect.width, m_rectTransform.rect.height);
        }

        public Vector2 GetUIPosition()
        {
            return m_rectTransform.anchoredPosition;
        }

        public float GetUIPositionX()
        {
            return m_rectTransform.anchoredPosition.x;
        }

        public float GetUIPositionY()
        {
            return m_rectTransform.anchoredPosition.y;
        }

        public Vector2 GetUIPositionLeftTop()
        {
            return GetUIPositionLeftTop(m_rectTransform);
        }

        //----------------------------------------------------------------
        public static Vector2 GetUIPositionCenter(RectTransform rectTransform)
        {
            return GetUIPositionFromPivot(rectTransform, new Vector2(0.5f, 0.5f));
        }

        public static Vector2 GetUIPositionLeftTop(RectTransform rectTransform)
        {
            return GetUIPositionFromPivot(rectTransform, new Vector2(0f, 1f));
        }

        public static Vector2 GetUIPositionFromPivot(RectTransform rectTransform, Vector2 pivot)
        {
            Vector2 vecPosition = rectTransform.anchoredPosition;
            vecPosition.x += (pivot.x - rectTransform.pivot.x) * rectTransform.rect.width;
            vecPosition.y += (pivot.y - rectTransform.pivot.y) * rectTransform.rect.height;
            return vecPosition;
        }


    }
}
