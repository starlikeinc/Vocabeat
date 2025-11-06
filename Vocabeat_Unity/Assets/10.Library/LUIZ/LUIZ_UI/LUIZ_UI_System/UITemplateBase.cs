using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.UI
{
    public abstract class UITemplateBase : UIWidgetCanvasBase
    {
        [Header("[ UITemplate ]")]
        [SerializeField] protected UITemplateItemBase TemplateItem = null;

        private List<UITemplateItemBase> m_listTemplateItems = new List<UITemplateItemBase>();

        //----------------------------------------------------------
        protected override void OnUIWidgetInitializePost(UIFrameBase parentFrame)
        {
            base.OnUIWidgetInitializePost(parentFrame);

            if(TemplateItem != null)
            {
                TemplateItem.gameObject.SetActive(false);
            }
        }

        //----------------------------------------------------------
        public T DoTemplateRequestItem<T>(Transform parent = null) where T : UITemplateItemBase
        {
            return PrivRequestTemplateItem(parent) as T;
        }

        public UITemplateItemBase DoTemplateRequestItem(Transform parent = null)
        {
            return PrivRequestTemplateItem(parent);
        }

        public void DoUITemplateReturnAll()
        {
            foreach(UITemplateItemBase item in m_listTemplateItems)
            {
                PrivReturnTemplateItem(item);
            }
        }

        public void DoUITemplateReturn(UITemplateItemBase returnItem)
        {
            PrivReturnTemplateItem(returnItem);
        }

        //-----------------------------------------------------------
        private UITemplateItemBase PrivMakeTemplateItem(Transform parent)
        {
            UIFrameBase parentFrame = GetParentUIFrame();
            if(parentFrame == null)
            {
                Debug.LogError("[UITemplateBase] Template must be a child of UIFrame");
                return null;
            }

            GameObject newObject = Instantiate(TemplateItem.gameObject, parent, false);
            newObject.transform.localPosition = Vector3.zero;

            UITemplateItemBase newItem = newObject.GetComponent<UITemplateItemBase>();

            m_listTemplateItems.Add(newItem);
            parentFrame.InterUIFrameAddChildWidget(newItem);

            return newItem;
        }

        private UITemplateItemBase PrivRequestTemplateItem(Transform parent)
        {
            UITemplateItemBase requestItem = null;

            foreach(UITemplateItemBase item in m_listTemplateItems)
            {
                if(item.IsTemplateItemActive() == false)
                {
                    requestItem = item;
                    break;
                }
            }

            if(requestItem == null)
            {
                requestItem = PrivMakeTemplateItem(parent);
            }
            else
            {
                requestItem.transform.SetParent(parent, false);
                requestItem.transform.localPosition = Vector3.zero;
            }

            requestItem.InterUIWidgetParentWidget(this);
            requestItem.InterTemplateItemActivate();

            requestItem.DoUIWidgetShow();

            OnUITemplateRequestItem(requestItem);

            return requestItem;

        }

        private void PrivReturnTemplateItem(UITemplateItemBase returnItem)
        {
            UIFrameBase parentFrame = GetParentUIFrame();
            
            returnItem.InterTemplateItemReturn();
            returnItem.DoUIWidgetHide();

            returnItem.transform.SetParent(TemplateItem.transform.parent, false);

            parentFrame.InterUIFrameRemoveChildWidget(returnItem);

            OnUITemplateReturnItem(returnItem);
        }

        //-----------------------------------------------------------
        protected virtual void OnUITemplateRequestItem(UITemplateItemBase item) { }
        protected virtual void OnUITemplateReturnItem(UITemplateItemBase item) { }
    }
}
