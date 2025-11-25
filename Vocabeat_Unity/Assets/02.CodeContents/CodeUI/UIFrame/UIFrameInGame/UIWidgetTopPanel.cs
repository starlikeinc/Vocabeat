using DG.Tweening;
using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetTopPanel : UIWidgetCanvasBase
{
    [SerializeField] private Image _imgSongThumb;
    [SerializeField] private TMP_Text _textSongName;
    [SerializeField] private TMP_Text _textSongComposer;

    [SerializeField] private TMP_Text _textScore; // 일단 텍스트로 하고 나중에 Sprite로 교체...(하게 된다면?)

    [SerializeField] private UITemplateGameScore _templateGameScore;

    private int _lastScore;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        ManagerRhythm.Instance.OnScoreChanged -= HandleScoreChange;
        ManagerRhythm.Instance.OnScoreChanged += HandleScoreChange;
    }

    public void DoWidgetTopPanelSetting(SongDataSO songDataSO)
    {
        _imgSongThumb.overrideSprite = songDataSO.SongThumb;
        _textSongName.text = songDataSO.SongName;
        _textSongComposer.text = songDataSO.SongComposer;
        _textScore.text = "0";
    }

    private void HandleScoreChange(int score)
    {
        if (_lastScore == score)
            return;

        _lastScore = score;
        _textScore.text = score.ToString("N0");
        ScoreTextEffect();
    }

    private void ScoreTextEffect()
    {
        _textScore.transform.DOKill();
        _textScore.transform.localScale = Vector3.one;

        _textScore.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _textScore.transform.DOScale(1f, 0.1f).SetEase(Ease.InQuad);
            });
    }
}
