using LUIZ.UI;
using UnityEngine;

public class UITemplateNoteDecisionEffect : UITemplateBase
{
    [SerializeField] private RectTransform _effectRoot;

    private NoteTouchJudgeSystem _judgeSystem;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);

        _judgeSystem = ManagerRhythm.Instance.NoteJudegeSystem;
        _judgeSystem.OnJudgeResult += HandleNormalJudge;
        _judgeSystem.OnHoldJudgeResult += HandleHoldJudge;
    }

    private void OnDestroy()
    {
        if (_judgeSystem != null)
        {
            _judgeSystem.OnJudgeResult -= HandleNormalJudge;
            _judgeSystem.OnHoldJudgeResult -= HandleHoldJudge;
        }
    }

    private void HandleNormalJudge(Note note, EJudgementType judgeType)
    {
        // 노트 위치 → anchoredPosition 계산
        Vector2 localPos = NoteUtility.GetNotePosition(_effectRoot, note.Tick, note.Y);        
        var item = DoTemplateRequestItem<UIItemNoteDecisionEffect>();
        item.Play(judgeType, _effectRoot, localPos);
    }

    private void HandleHoldJudge(Note note, EJudgementType judgeType, bool isEnd, Vector2 effectLocalPos)
    {
        // 아까 만든 "끝날 때 커서 위치" 그대로 쓰면 됨
        var item = DoTemplateRequestItem<UIItemNoteDecisionEffect>();
        item.Play(judgeType, _effectRoot, effectLocalPos);
    }
}
