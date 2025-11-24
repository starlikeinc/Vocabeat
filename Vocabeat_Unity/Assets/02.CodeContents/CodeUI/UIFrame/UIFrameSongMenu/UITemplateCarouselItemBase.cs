using System;
using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UITemplateCarouselItemBase : UITemplateItemBase
{
    /// <summary>이 슬롯이 현재 어떤 데이터 인덱스를 보여주는지</summary>
    public int BoundItemIndex { get; private set; } = -1;

    //-----------------------------------------------------------
    /// <summary>
    /// 캐러셀 쪽에서 공통으로 호출하는 상태 적용 함수.
    /// (인덱스, 포커스 여부, 스케일 등)
    /// </summary>
    public void ApplyCommonState(int itemIndex, bool isCenter, float focusedScale, float normalScale)
    {
        BoundItemIndex = itemIndex;

        RectTransform rt = transform as RectTransform;
        if (rt != null)
        {
            float scale = isCenter ? focusedScale : normalScale;
            rt.localScale = new Vector3(scale, scale, 1f);
        }

        // 자식 쪽에서 포커스 여부에 따라 색/효과 바꾸고 싶으면 이 훅을 쓰면 됨
        OnApplyFocusState(isCenter);
    }

    /// <summary>
    /// 포커스 여부에 따라 슬롯 비주얼을 바꾸고 싶을 때 사용하는 훅.
    /// </summary>
    protected virtual void OnApplyFocusState(bool isCenter) { }

    //-----------------------------------------------------------
    protected override void OnUITemplateItemReturn()
    {
        base.OnUITemplateItemReturn();

        BoundItemIndex = -1;

        // 기본 스케일 리셋
        RectTransform rt = transform as RectTransform;
        if (rt != null)
        {
            rt.localScale = Vector3.one;
        }
    }
}

