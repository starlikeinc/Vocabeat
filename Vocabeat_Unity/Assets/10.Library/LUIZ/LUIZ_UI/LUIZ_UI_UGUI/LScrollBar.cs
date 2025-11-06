using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI
{
    public class LScrollBar : Scrollbar
    {
        //ScrollRect가 임의로 ScrollBar의 Size를 조정해 주는 것을 해결
        [SerializeField] private bool UseFixedSize = false; public bool IsFixedSize() { return UseFixedSize; }

        [SerializeField, Range(0, 1)] private float FixedSize = 0f;

        //------------------------------------------------------------------
        internal void InterScrollRectRebuild()
        {
            size = FixedSize;
        }

        internal void InterScrollRectLateUpdate()
        {
            size = FixedSize;
        }
    }
}
