using System;
using System.Collections.Generic;
using LUIZ.UI;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class UIItemNoteDecisionEffect : UITemplateItemBase
{
    [Serializable]
    private class SNoteDecisionInfo
    {
        public EJudgementType NoteDecision = EJudgementType.RedStar;
        public SkeletonGraphic SpineGraphic = null;
    }

    [SerializeField]
    private List<SNoteDecisionInfo> NoteDecisionList = new List<SNoteDecisionInfo>();

    /// <summary>애니 끝났을 때 콜백 (필요 없으면 안 써도 됨)</summary>
    private UnityAction<UIItemNoteDecisionEffect> _onFinished;

    // ------------------------------------------------------
    // 초기화: Spine 이벤트 바인딩 + 전부 비활성
    // ------------------------------------------------------
    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);

        for (int i = 0; i < NoteDecisionList.Count; i++)
        {
            var info = NoteDecisionList[i];
            if (info.SpineGraphic == null)
                continue;

            info.SpineGraphic.gameObject.SetActive(false);
            info.SpineGraphic.AnimationState.Complete += HandleSpineNoteDecisionFinish;
        }

        gameObject.SetActive(false);
    }

    // ------------------------------------------------------
    // 외부에서 호출할 메인 API
    //  - decision    : 판정 타입
    //  - parent      : 붙일 RectTransform (보통 판정 UI 루트)
    //  - anchoredPos : parent 기준 anchoredPosition
    //  - onFinished  : 애니 끝났을 때 콜백(선택)
    // ------------------------------------------------------
    public void Play(
        EJudgementType decision,
        RectTransform parent,
        Vector2 anchoredPos,
        UnityAction<UIItemNoteDecisionEffect> onFinished = null)
    {
        _onFinished = onFinished;

        // 부모 / 위치 세팅
        var rt = (RectTransform)transform;
        transform.SetParent(parent, false);
        rt.anchoredPosition = anchoredPos;

        // 해당 판정 타입 SpineGraphic 활성 + 애니 시작
        var info = FindAndArrangeEffectNoteDecision(decision);
        if (info != null && info.SpineGraphic != null)
        {
            PlaySpineAnimation(info.SpineGraphic);
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 이미 parent가 고정(예: 판정 이펙트 전용 루트)이라면
    /// parent를 안 받는 오버로드도 쓸 수 있음.
    /// </summary>
    public void Play(
        EJudgementType decision,
        Vector2 anchoredPos,
        UnityAction<UIItemNoteDecisionEffect> onFinished = null)
    {
        Play(decision, (RectTransform)transform.parent, anchoredPos, onFinished);
    }

    // ------------------------------------------------------
    // SSEffectSpineNoteDecision.FindAndArrangeEffectNoteDecision 대응
    // ------------------------------------------------------
    private SNoteDecisionInfo FindAndArrangeEffectNoteDecision(EJudgementType decision)
    {
        SNoteDecisionInfo found = null;

        for (int i = 0; i < NoteDecisionList.Count; i++)
        {
            var info = NoteDecisionList[i];
            if (info.SpineGraphic == null)
                continue;

            if (info.NoteDecision == decision)
            {
                found = info;
                info.SpineGraphic.gameObject.SetActive(true);
            }
            else
            {
                info.SpineGraphic.gameObject.SetActive(false);
            }
        }

        return found;
    }

    // ------------------------------------------------------
    // 실제 Spine 애니 시작 로직
    // ------------------------------------------------------
    private void PlaySpineAnimation(SkeletonGraphic graphic)
    {
        // startingAnimation 에 애니 이름이 설정돼있다고 가정
        string animName = graphic.startingAnimation;
        if (string.IsNullOrEmpty(animName))
        {
            // 혹시 비어있으면 현재 트랙의 애니 이름이라도 사용
            animName = graphic.AnimationState?.GetCurrent(0)?.Animation?.Name;
        }

        if (!string.IsNullOrEmpty(animName))
        {
            graphic.AnimationState.SetAnimation(0, animName, false);
        }
    }

    // ------------------------------------------------------
    // Spine 애니메이션 완료 콜백
    // ------------------------------------------------------
    private void HandleSpineNoteDecisionFinish(TrackEntry trackEntry)
    {
        // Spine에서 Complete가 여러 번 호출될 일은 거의 없겠지만,
        // 혹시 모르니 여기서 안전하게 한 번만 처리하는 패턴으로 써도 됨.
        OnEffectFinished();
    }

    // ------------------------------------------------------
    // 이펙트 종료 처리
    //  - 콜백 호출
    //  - 비활성화
    //  - 템플릿 풀로 반환
    // ------------------------------------------------------
    private void OnEffectFinished()
    {
        _onFinished?.Invoke(this);
        _onFinished = null;

        // Spine 그래픽들 전부 끄고
        for (int i = 0; i < NoteDecisionList.Count; i++)
        {
            var info = NoteDecisionList[i];
            if (info.SpineGraphic != null)
                info.SpineGraphic.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);

        // UITemplateItemBase 쪽 풀로 반환
        DoTemplateItemReturn();
    }
}
