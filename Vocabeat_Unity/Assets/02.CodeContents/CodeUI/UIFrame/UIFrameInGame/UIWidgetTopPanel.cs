using LUIZ.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetTopPanel : UIWidgetCanvasBase
{
    [SerializeField] private Image _imgSongThumb;
    [SerializeField] private TMP_Text _textSongName;
    [SerializeField] private TMP_Text _textSongComposer;
    [SerializeField] private UITemplateGameScore _templateGameScore;

    public void DoWidgetTopPanelSetting(SongDataSO songDataSO)
    {
        _imgSongThumb.overrideSprite = songDataSO.SongThumb;
        _textSongName.text = songDataSO.SongName;
        _textSongComposer.text = songDataSO.SongComposer;
    }
}
