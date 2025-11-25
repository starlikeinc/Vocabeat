using DG.Tweening;
using LUIZ.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSongDifficulty : UITemplateItemBase
{
    [SerializeField] private Image _imgDiffIcon;
    [SerializeField] private TMP_Text _textDiffValue;

    private EDifficulty _diff;

    private UIFrameSongMenu _frameSongMenu;

    private static event Action<UIItemSongDifficulty> _onSongDifficultyChanged;

    protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
    {
        base.OnUIWidgetInitialize(parentFrame);
        _frameSongMenu = parentFrame as UIFrameSongMenu;

        _onSongDifficultyChanged -= HandleSelectScale;
        _onSongDifficultyChanged += HandleSelectScale;
    }

    public void DoSongDifficultySetting(Sprite icon, int level, EDifficulty diff)
    {
        _imgDiffIcon.overrideSprite = icon;
        _textDiffValue.text = level.ToString();

        _diff = diff;
    }    

    public void OnSelectThisDifficulty()
    {
        _onSongDifficultyChanged?.Invoke(this);
        _frameSongMenu.SetCurrentSongDifficulty(_diff);
    }

    private void HandleSelectScale(UIItemSongDifficulty itemDifficulty)
    {
        if (!gameObject.activeSelf)
            return;

        transform.DOKill();

        if (itemDifficulty == this)
        {
            transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutQuad);
        }
        else
        {
            transform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }        
    }
}
