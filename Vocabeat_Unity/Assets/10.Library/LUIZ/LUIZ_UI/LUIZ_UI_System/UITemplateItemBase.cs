using UnityEngine;

namespace LUIZ.UI
{
    public class UITemplateItemBase : UIWidgetBase
    {
        private bool m_isTemplateItemActive = false;        public bool IsTemplateItemActive() { return m_isTemplateItemActive; }

        private UITemplateBase m_parentTemplate = null;

        //-----------------------------------------------------------
        protected override void OnUIWidgetParentWidget(UIWidgetBase parentWidget)
        {
            base.OnUIWidgetParentWidget(parentWidget);
            m_parentTemplate = parentWidget as UITemplateBase;
        }

        //-----------------------------------------------------------
        public void DoTemplateItemReturn()
        {
            if (m_isTemplateItemActive == true)
            {
                m_parentTemplate.DoUITemplateReturn(this);
            }
        }

        //-----------------------------------------------------------
        internal void InterTemplateItemActivate()
        {
            m_isTemplateItemActive = true;
            OnUITemplateItemActivate();
        }

        internal void InterTemplateItemReturn()
        {
            m_isTemplateItemActive = false;
            OnUITemplateItemReturn();
        }

        //-----------------------------------------------------------
        protected virtual void OnUITemplateItemActivate() { }
        protected virtual void OnUITemplateItemReturn() { }
    }
}
