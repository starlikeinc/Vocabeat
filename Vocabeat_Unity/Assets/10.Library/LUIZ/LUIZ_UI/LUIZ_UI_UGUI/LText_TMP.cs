using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace LUIZ.UI
{
    public class LText_TMP : TextMeshProUGUI
    {
        //로컬라이징 테이블이나 차트같이 다양한 참조 변환을 위한 브릿지
        private static Func<string, string> FuncTextTMPSet = null;

        private event Action m_delTMPChanged = null;

        //-------------------------------------------------------    
        public override string text
        {
            get => base.text;
            set
            {
                if (FuncTextTMPSet != null)
                {
                    string newText = FuncTextTMPSet(value);
                    if (string.IsNullOrWhiteSpace(newText))
                    {
                        Debug.LogWarning($"[LText_TMP] newtext is null. setting value instead : {value}", this.gameObject);
                        base.text = value;
                    }
                    else
                    {
                        base.text = FuncTextTMPSet(value);
                    }
                }
                else
                {
                    base.text = value;
                }
            }
        }

        //-------------------------------------------------------    
        public override void SetAllDirty()
        {
            base.SetAllDirty();

            if (m_delTMPChanged != null)
            {
                m_delTMPChanged.Invoke();
            }
        }

        //-------------------------------------------------------
        public void DoTMPChangedEventSubscribe(Action delEvent) { m_delTMPChanged += delEvent; }
        public void DoTMPChangedEventUnsubscribe(Action delEvent) { m_delTMPChanged -= delEvent; }

        public static void DoTMPRegisterSetEvent(Func<string, string> funcTextTMPSet)
        {
            if(FuncTextTMPSet != null)
            {
                Debug.LogWarning("[LText_TMP] FuncTextTMPSet already set!");
            }
            else
            {
                FuncTextTMPSet = funcTextTMPSet;
            }
        }
    }
}
