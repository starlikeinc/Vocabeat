using System;
using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIFrameResult : UIFrameBase
{
    public enum ERank { S, A, B, C, F }    

    [Serializable]
    public class RankData
    {
        public ERank RankType;
        public Sprite RankSprite;
    }

    [Header("판정 횟수")]
    [SerializeField] private TMP_Text _textBlueStarCount;
    [SerializeField] private TMP_Text _textWhiteStarCount;
    [SerializeField] private TMP_Text _textYellowStarCount;
    [SerializeField] private TMP_Text _textRedStarCount;

    [Header("점수 및 랭크")]
    [SerializeField] private TMP_Text _textScoreValue;
    [SerializeField] private Image _imgRank;
    [SerializeField] private RankData[] RankDatas;

    [Header("실패 이미지")]
    [SerializeField] private GameObject _objFail;

    [Header("Thumb")]
    [SerializeField] private Image _imgThumb;

    [Header("BG")]
    [SerializeField] private Image _imgBG;

    [Header("실패 연출")]
    [SerializeField] private UnityEvent OnFailed;

    // ========================================            
    protected override void OnUIFrameHide()
    {
        base.OnUIFrameHide();
        _objFail.gameObject.SetActive(false);
    }

    // ========================================            
    public void DoFrameResultSetting()
    {
        SetJudgementCount();
        SetPointAndRank();
        SetImages();
        TryInvokeFailEvent();
    }

    // ========================================            
    private void SetJudgementCount()
    {
        int blueStarCount = ManagerRhythm.Instance.NoteJudegeSystem.GetJudgeCountByType(EJudgementType.BlueStar);
        int whiteStarCount = ManagerRhythm.Instance.NoteJudegeSystem.GetJudgeCountByType(EJudgementType.WhiteStar);
        int yellowStarCount = ManagerRhythm.Instance.NoteJudegeSystem.GetJudgeCountByType(EJudgementType.YellowStar);
        int redStarCount = ManagerRhythm.Instance.NoteJudegeSystem.GetJudgeCountByType(EJudgementType.RedStar);

        _textBlueStarCount.text = blueStarCount.ToString("D4");
        _textWhiteStarCount.text = whiteStarCount.ToString("D4");
        _textYellowStarCount.text = yellowStarCount.ToString("D4");
        _textRedStarCount.text = redStarCount.ToString("D4");
    }

    private void SetPointAndRank()
    {
        int score = ManagerRhythm.Instance.CurrentScore;

        _textScoreValue.text = score.ToString("N0");

        ERank rank = score >= GameConstant.RequirePoint_S ? ERank.S
                   : score >= GameConstant.RequirePoint_A ? ERank.A
                   : score >= GameConstant.RequirePoint_B ? ERank.B
                   : score >= GameConstant.RequirePoint_C ? ERank.C
                   : ERank.F;
        _imgRank.overrideSprite = GetRankSpriteByType(rank);
    }

    private void SetImages()
    {
        var songSO = ManagerRhythm.Instance.CurSongDataSO;

        _imgBG.overrideSprite = songSO.SongBG;
        _imgThumb.overrideSprite = songSO.SongThumb;
    }

    private void TryInvokeFailEvent()
    {
        bool isFail = ManagerRhythm.Instance.CurrentScore < GameConstant.RequirePoint_C;
        if (isFail)
            OnFailed?.Invoke();
    }

    private Sprite GetRankSpriteByType(ERank rank)
    {
        foreach(var data in RankDatas)
        {
            if (data.RankType == rank)
                return data.RankSprite;
        }

        Debug.LogError($"{rank} 에 해당하는 데이터가 없습니다.");
        return null;
    }
}
