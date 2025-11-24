using DG.Tweening;
using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;
using System;
using UnityEngine.UI;

/// <summary>
/// 캐러셀 공통 베이스.
/// - UITemplateBase를 상속
/// - 슬롯 타입 TItem, 데이터 타입 TData는 자식에서 결정
/// - 순환/인덱스/애니메이션/레이아웃 로직만 담당
/// </summary>
/// <typeparam name="TItem">캐러셀 슬롯 타입 (UITemplateCarouselItemBase 상속)</typeparam>
/// <typeparam name="TData">실제 데이터 타입 (예: UnitData, WeaponData 등)</typeparam>
public abstract class UITemplateCarouselBase<TItem, TData> : UITemplateBase where TItem : UITemplateCarouselItemBase
{
    [Header("[ Carousel Layout ]")]
    [SerializeField] protected RectTransform SlotRoot;
    [SerializeField] protected int VisibleSlotCount = 5;     // 홀수 추천
    [SerializeField] protected float SlotSpacing = 260f;     // X 간격
    [SerializeField] protected float FocusedScale = 1.2f;
    [SerializeField] protected float NormalScale = 1.0f;

    [Header("[ Carousel Animation ]")]
    [SerializeField] protected float MoveDuration = 0.25f;
    [SerializeField] protected Ease MoveEase = Ease.OutCubic;

    [Header("[ Carousel Buttons ]")]
    [SerializeField] protected Button BtnPrev;
    [SerializeField] protected Button BtnNext;

    /// <summary>실제 데이터 개수 (자식이 제공)</summary>
    protected int m_itemCount = 0;

    /// <summary>현재 강조(중앙) 중인 데이터 인덱스</summary>
    protected int m_centerItemIndex = 0;

    protected readonly List<TItem> m_listSlots = new List<TItem>();
    protected readonly List<Tween> m_moveTweens = new List<Tween>();

    protected bool m_isAnimating = false;
    protected Tween m_moveTween = null;

    protected int CenterSlotIndex => VisibleSlotCount / 2;


    /// <summary>중앙 인덱스가 바뀔 때 알리는 이벤트 (데이터 같이 넘김)</summary>
    public event Action<int, TData> OnCenterChanged;

    //-----------------------------------------------------------
    #region Unity & 초기화

    protected override void OnUIWidgetInitializePost(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitializePost(parentFrame);

        if (SlotRoot == null && TemplateItem != null)
        {
            SlotRoot = TemplateItem.transform.parent as RectTransform;
        }

        if (BtnPrev != null) BtnPrev.onClick.AddListener(OnClickPrev);
        if (BtnNext != null) BtnNext.onClick.AddListener(OnClickNext);

        CreateSlots();
        RefreshAll();
    }

    private void OnDestroy()
    {
        if (BtnPrev != null) BtnPrev.onClick.RemoveListener(OnClickPrev);
        if (BtnNext != null) BtnNext.onClick.RemoveListener(OnClickNext);

        foreach (var s in m_listSlots)
        {
            if (s != null)
                OnUnsubscribeSlotEvents(s);
        }

        StopAnimation();
    }

    #endregion

    //-----------------------------------------------------------
    #region 추상/오버라이드 지점 (자식이 구현)

    /// <summary>
    /// 현재 캐러셀에 바인딩할 데이터 개수.
    /// 자식 클래스에서 자신의 리스트 길이를 반환하도록 구현.
    /// </summary>
    protected abstract int GetItemCount();

    /// <summary>
    /// index에 해당하는 데이터를 반환. (0 ~ ItemCount-1)
    /// </summary>
    protected abstract TData GetItemData(int index);

    /// <summary>
    /// 하나의 슬롯에 데이터를 그리는 부분. 아이콘/텍스트/잠금 등 전부 여기.
    /// </summary>
    protected abstract void BindItemVisual(TItem slot, int itemIndex, TData data, bool isCenter);

    /// <summary>
    /// 슬롯 생성 직후 (이벤트 구독 등) 자식에 넘겨주고 싶으면 여기서 처리.
    /// </summary>
    protected virtual void OnSlotCreated(TItem slot) { }

    /// <summary>
    /// 슬롯 제거/프레임 파괴 시 이벤트 해제하고 싶으면 여기서 처리.
    /// </summary>
    protected virtual void OnUnsubscribeSlotEvents(TItem slot) { }

    /// <summary>
    /// 중앙 인덱스가 변경됐을 때 자식에서 추가 동작을 하고 싶으면 오버라이드.
    /// </summary>
    protected virtual void OnCenterIndexChanged(int newIndex, TData data) { }

    #endregion

    //-----------------------------------------------------------
    #region 외부 API

    /// <summary>
    /// 외부에서 "데이터가 바뀌었다" 라고 알려줄 때 호출.
    /// (리스트 교체 후 다시 그릴 때)
    /// </summary>
    public void RefreshAll(int? overrideCenterIndex = null)
    {
        m_itemCount = Mathf.Max(0, GetItemCount());
        if (m_itemCount <= 0)
        {
            m_centerItemIndex = 0;
            RefreshImmediate();
            return;
        }

        if (overrideCenterIndex.HasValue)
            m_centerItemIndex = WrapIndex(overrideCenterIndex.Value, m_itemCount);
        else
            m_centerItemIndex = WrapIndex(m_centerItemIndex, m_itemCount);

        StopAnimation();
        RefreshImmediate();
        RaiseCenterChanged();
    }

    public int GetCurrentCenterIndex() => m_centerItemIndex;

    public void MoveTo(int targetIndex)
    {
        if (m_itemCount <= 0) return;

        targetIndex = WrapIndex(targetIndex, m_itemCount);
        int delta = GetShortestDelta(m_centerItemIndex, targetIndex, m_itemCount);
        MoveBy(delta);
    }

    #endregion

    //-----------------------------------------------------------
    #region 슬롯 생성/관리

    protected void CreateSlots()
    {
        DoUITemplateReturnAll();
        m_listSlots.Clear();

        if (SlotRoot == null)
        {
            Debug.LogError("[UITemplateCarouselBase] SlotRoot is null");
            return;
        }

        for (int i = 0; i < VisibleSlotCount; i++)
        {
            TItem slot = DoTemplateRequestItem<TItem>(SlotRoot);
            if (slot == null)
            {
                Debug.LogError("[UITemplateCarouselBase] Fail to request slot item");
                continue;
            }

            m_listSlots.Add(slot);
            OnSlotCreated(slot);
        }
    }

    #endregion

    //-----------------------------------------------------------
    #region 입력 처리

    protected virtual void OnClickPrev() => MoveBy(-1);
    protected virtual void OnClickNext() => MoveBy(+1);

    /// <summary>
    /// 자식 슬롯에서 클릭 이벤트를 받은 경우, 이 메서드를 호출해주면
    /// 해당 슬롯의 BoundItemIndex를 기준으로 이동.
    /// </summary>
    protected void OnClickSlot(TItem slot)
    {
        if (m_itemCount <= 0) return;
        if (slot == null) return;

        int targetIndex = slot.BoundItemIndex;
        if (targetIndex < 0) return;

        if (targetIndex == m_centerItemIndex)
        {
            // 이미 중앙이면, 별도 "선택" 행동은 자식에서 알아서 처리해도 됨.
            OnCenterSlotClicked(targetIndex, GetItemData(targetIndex));
            return;
        }

        int delta = GetShortestDelta(m_centerItemIndex, targetIndex, m_itemCount);
        MoveBy(delta);
    }

    /// <summary>
    /// 중앙 슬롯을 다시 클릭했을 때의 동작 (선택/확정 등) 이 필요하면 자식에서 오버라이드.
    /// </summary>
    protected virtual void OnCenterSlotClicked(int centerIndex, TData data) { }

    #endregion

    //-----------------------------------------------------------
    #region 이동/애니메이션

    public void MoveBy(int delta)
    {
        if (m_itemCount <= 0) return;
        if (delta == 0) return;

        // 1) 이미 이동 중이면 "지금 애니메이션을 끝까지 완료" 시킨다.
        //    => 이전 targetIndex 기준으로 centerIndex, 레이아웃, SlotRoot가 딱 정리된 상태가 됨.
        if (m_isAnimating && m_moveTween != null && m_moveTween.IsActive())
        {
            m_moveTween.Complete();   // OnComplete 콜백이 즉시 실행됨
        }

        // 2) 새 target 인덱스 계산 (이제 m_centerItemIndex는 직전 이동이 끝난 값)
        int targetIndex = WrapIndex(m_centerItemIndex + delta, m_itemCount);
        TData targetData = GetItemData(targetIndex);

        // 3) 논리상 centerIndex를 "새 target"으로 먼저 바꾼 뒤,
        //    그 기준으로 레이아웃/포커스/비주얼을 재배치
        m_centerItemIndex = targetIndex;
        RefreshImmediate();

        // 4) 프레임 쪽에 "선택된 인덱스/데이터"를 바로 알려줌
        OnCenterChanged?.Invoke(targetIndex, targetData);
        OnCenterIndexChanged(targetIndex, targetData);

        // 5) 애니메이션이 불가능한 환경이면 여기서 끝
        if (SlotRoot == null ||
            Mathf.Approximately(SlotSpacing, 0f) ||
            Mathf.Approximately(MoveDuration, 0f))
        {
            return;
        }

        m_isAnimating = true;

        float y = SlotRoot.anchoredPosition.y;

        // *** 핵심 포인트 ***
        // "목표 레이아웃" 상태에서 SlotRoot를 delta * SlotSpacing 만큼 옆으로 밀어놓고
        // 0으로 미끄러져 오게 한다.
        //
        // 예) center 0 → 1 (delta=+1)
        //  - RefreshImmediate 후:  [-1,0,1,2,3] 배치, center=1
        //  - SlotRoot.x = +SlotSpacing 으로 두면
        //    화면 상엔 여전히 [-2,-1,0,1,2]처럼 보임(사람 눈엔 변화 없음)
        //  - 거기서 0까지 슬라이드하면서 오른쪽에서 index 3이 서서히 등장
        float fromX = delta * SlotSpacing;
        float toX = 0f;

        SlotRoot.anchoredPosition = new Vector2(fromX, y);

        m_moveTween = SlotRoot
            .DOAnchorPosX(toX, MoveDuration)
            .SetEase(MoveEase)
            .OnComplete(() =>
            {
                m_isAnimating = false;
                m_moveTween = null;

                // 혹시 남은 오차 정리
                SlotRoot.anchoredPosition = new Vector2(0f, y);

                // floating 오차나 스케일 트윗 등으로 어긋난 것이 있을 수 있으니 한 번 더 스냅
                RefreshImmediate();
            });
    }

    protected void RefreshImmediate()
    {
        if (m_listSlots.Count == 0) return;

        if (m_itemCount <= 0)
        {
            foreach (var s in m_listSlots)
                if (s != null) s.gameObject.SetActive(false);
            return;
        }

        float y = 0f;

        for (int slotIndex = 0; slotIndex < m_listSlots.Count; slotIndex++)
        {
            var slot = m_listSlots[slotIndex];
            if (slot == null) continue;

            slot.gameObject.SetActive(true);

            int offsetFromCenter = slotIndex - CenterSlotIndex;    // -2,-1,0,1,2 ...
            int itemIndex = WrapIndex(m_centerItemIndex + offsetFromCenter, m_itemCount);
            bool isCenter = (slotIndex == CenterSlotIndex);

            RectTransform rt = slot.transform as RectTransform;
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(offsetFromCenter * SlotSpacing, y);
            }

            // 공통 상태 (인덱스/스케일) 적용
            slot.ApplyCommonState(itemIndex, isCenter, FocusedScale, NormalScale);

            // 실제 비주얼 바인딩 (자식이 구현)
            TData data = GetItemData(itemIndex);
            BindItemVisual(slot, itemIndex, data, isCenter);
        }
    }

    protected void StopAnimation()
    {
        if (m_moveTween != null && m_moveTween.IsActive())
        {
            // OnComplete를 호출하지 않고 그냥 끊고,
            // 현재 centerIndex 기준으로 다시 스냅한다.
            m_moveTween.Kill();
            m_moveTween = null;
        }

        m_isAnimating = false;

        if (SlotRoot != null)
        {
            Vector2 pos = SlotRoot.anchoredPosition;
            SlotRoot.anchoredPosition = new Vector2(0f, pos.y);
        }

        // 현재 m_centerItemIndex 기준으로 레이아웃 다시 구성
        RefreshImmediate();
    }

    protected void RaiseCenterChanged()
    {
        if (m_itemCount <= 0) return;

        TData data = GetItemData(m_centerItemIndex);
        OnCenterChanged?.Invoke(m_centerItemIndex, data);
        OnCenterIndexChanged(m_centerItemIndex, data);
    }

    #endregion

    //-----------------------------------------------------------
    #region 유틸

    protected static int WrapIndex(int index, int count)
    {
        if (count <= 0) return 0;
        index %= count;
        if (index < 0) index += count;
        return index;
    }

    protected static int GetShortestDelta(int from, int to, int count)
    {
        if (count <= 0) return 0;

        from = WrapIndex(from, count);
        to = WrapIndex(to, count);

        int diff = to - from;
        int wrappedDiff = diff;

        if (Mathf.Abs(diff) > count / 2)
        {
            if (diff > 0)
                wrappedDiff = diff - count;
            else
                wrappedDiff = diff + count;
        }

        return wrappedDiff;
    }

    private void StartSlideAnimation(int delta)
    {
        // 애니메이션에 필요한 값 체크
        if (Mathf.Approximately(SlotSpacing, 0f)) return;
        if (Mathf.Approximately(MoveDuration, 0f)) return;

        m_isAnimating = true;

        // 혹시 남아있을지 모를 이전 트윈 정리
        foreach (var t in m_moveTweens)
        {
            if (t != null && t.IsActive())
                t.Kill();
        }
        m_moveTweens.Clear();

        // 슬롯 각각에 대해 "옆에서 들어오도록" 시작 위치를 잡고 슬라이드
        for (int slotIndex = 0; slotIndex < m_listSlots.Count; slotIndex++)
        {
            var slot = m_listSlots[slotIndex];
            if (slot == null) continue;

            RectTransform rt = slot.transform as RectTransform;
            if (rt == null) continue;

            // 현재 위치를 "최종 위치"로 취급
            Vector2 finalPos = rt.anchoredPosition;

            // delta > 0 이면 오른쪽에서 들어오고, < 0 이면 왼쪽에서 들어오게
            float startX = finalPos.x - delta * SlotSpacing;

            // 시작 위치 설정 (한 칸 옆)
            rt.anchoredPosition = new Vector2(startX, finalPos.y);

            // 거기서부터 최종 위치까지 슬라이드
            Tween tw = rt.DOAnchorPosX(finalPos.x, MoveDuration)
                         .SetEase(MoveEase);

            m_moveTweens.Add(tw);
        }

        // 첫 번째 트윈 기준으로 완료 처리
        if (m_moveTweens.Count > 0)
        {
            m_moveTweens[0].OnComplete(() =>
            {
                // 혹시 중간에 약간의 오차가 있을 수 있으니 한 번 더 스냅
                RefreshImmediate();

                m_isAnimating = false;
                m_moveTweens.Clear();
            });
        }
        else
        {
            m_isAnimating = false;
        }
    }
    #endregion
}