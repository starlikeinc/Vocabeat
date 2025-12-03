using LUIZ.UI;
using TMPro;
using UnityEngine;

public class UIWidgetSongInfo : UIWidgetBase
{
    [SerializeField] private UITemplateSongDifficulty _templateDifficulty;

    [SerializeField] private TMP_Text _textSongName;
    [SerializeField] private TMP_Text _textSongBPM;
    [SerializeField] private TMP_Text _textSongComposer;

    public void WidgetSongInfoSetting(int songIndex)
    {
        var songDataSO = ManagerRhythm.Instance.SongDB.GetSong(songIndex);

        _textSongName.text = songDataSO.SongName;
        _textSongBPM.text = $"BPM {songDataSO.BPM}";
        _textSongComposer.text = songDataSO.SongComposer;
        SongDifficultySetting(songDataSO);
    }

    private void SongDifficultySetting(SongDataSO songData)
    {
        _templateDifficulty.DoTemplateSongDiffSetting(songData);
    }
}
