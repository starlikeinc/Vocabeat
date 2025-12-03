using DG.Tweening;
using LUIZ.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UIFrameResult;

public class UIFrameResult : UIFrameBase
{
    public enum ERank { S, A, B, C, F }

    [Serializable]
    public class DifficultyIconData
    {
        public EDifficulty Diff;
        public Sprite DiffIcon;
    }

    [Serializable]
    public class RankData
    {
        public ERank RankType;
        public string RankText;
        public Sprite RankSprite;
    }

    private const int c_AddKeyValue = 1;

    [Header("BGM")]
    [SerializeField] private BGMEventChannelSO _eventChannel;
    [SerializeField] private AudioCueSO _audioCue;

    [Header("판정 횟수")]
    [SerializeField] private TMP_Text _textBlueStarCount;
    [SerializeField] private TMP_Text _textWhiteStarCount;
    [SerializeField] private TMP_Text _textYellowStarCount;
    [SerializeField] private TMP_Text _textRedStarCount;

    [Header("점수 및 랭크")]
    [SerializeField] private TMP_Text _textScoreValue;
    [SerializeField] private TMP_Text _textRank;
    [SerializeField] private Image _imgRank;
    [SerializeField] private RankData[] RankDatas;

    [Header("난이도 정보")]
    [SerializeField] private TMP_Text _textDiff;
    [SerializeField] private TMP_Text _textDiffValue;
    [SerializeField] private Image _imgDiffIcon;
    [SerializeField] private DifficultyIconData[] DifficultyIconDataDatas;

    [Header("실패 이미지")]
    [SerializeField] private GameObject _objFail;

    [Header("Thumb")]
    [SerializeField] private Image _imgThumb;

    [Header("BG")]
    [SerializeField] private Image _imgBG;

    [Header("키 획득 연출")]
    [SerializeField] private GameObject _pivotKey;
    [SerializeField] private DOTweenAnimation _tweenKeyAcquire;

    [Header("실패 연출")]
    [SerializeField] private UnityEvent OnFailed;        

    // ========================================            
    public void DoFrameResultSetting()
    {
        _objFail.gameObject.SetActive(false);

        _eventChannel.Raise(_audioCue);
        _eventChannel.PlayScheduled(0);

        SetJudgementCount();
        SetPointAndRank();
        SetResultInfos();
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
        _textRank.text = GetRankTextByType(rank);
        _imgRank.overrideSprite = GetRankSpriteByType(rank);
    }

    private void SetResultInfos()
    {
        var songSO = ManagerRhythm.Instance.CurSongDataSO;

        _imgBG.overrideSprite = songSO.SongBG;
        _imgThumb.overrideSprite = songSO.SongThumb;

        EDifficulty curDiff = ManagerRhythm.Instance.CurDiff;
        _textDiff.text = curDiff.ToString();
        _textDiffValue.text = $"{songSO.DifficultyValueByDiff[curDiff]}";
        _imgDiffIcon.overrideSprite = GetDiffIconByType(curDiff);
    }

    private void TryInvokeFailEvent()
    {
        bool isFail = ManagerRhythm.Instance.CurrentScore < GameConstant.RequirePoint_C;
        _pivotKey.SetActive(!isFail);

        if (isFail)
            OnFailed?.Invoke();
        else
        {
            _tweenKeyAcquire.RecreateTweenAndPlay();
            ManagerRhythm.Instance.AddMusicKey(c_AddKeyValue);
        }
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

    private Sprite GetDiffIconByType(EDifficulty diff)
    {
        foreach (var data in DifficultyIconDataDatas)
        {
            if (data.Diff == diff)
                return data.DiffIcon;
        }

        Debug.LogError($"{diff} 에 해당하는 데이터가 없습니다.");
        return null;
    }

    private string GetRankTextByType(ERank rank)
    {
        foreach (var data in RankDatas)
        {
            if (data.RankType == rank)
                return data.RankText;
        }

        Debug.LogError($"{rank} 에 해당하는 데이터가 없습니다.");
        return null;
    }

    // ========================================            
    public void OnRetry()
    {
        UIChannel.UIHide<UIFrameResult>();
        ManagerRhythm.Instance.RetrySong();
    }

    public void OnResume()
    {        
        UIChannel.UIShow<UIFrameBlinder>().BlindWithNextStep(() =>
        {
            _eventChannel.StopAudio();
            UIChannel.UIHide<UIFrameResult>();
            UIChannel.UIShow<UIFrameSongMenu>().DoFrameSongMenuSetting(false);
        });        
    }
}
