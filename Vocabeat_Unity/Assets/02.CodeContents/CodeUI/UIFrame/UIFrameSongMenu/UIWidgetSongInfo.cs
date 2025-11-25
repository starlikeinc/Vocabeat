using LUIZ.UI;
using TMPro;
using UnityEngine;

public class UIWidgetSongInfo : UIWidgetBase
{
    [SerializeField] private UITemplateSongDifficulty _templateDifficulty;

    [SerializeField] private TMP_Text _textSongName;
    [SerializeField] private TMP_Text _textSongBPM;
    [SerializeField] private TMP_Text _textSongDifficulty;

    public void WidgetSongInfoSetting(int songIndex)
    {
        var songDataSO = ManagerRhythm.Instance.SongDB.GetSong(songIndex);

        _textSongName.text = songDataSO.SongName;
        _textSongBPM.text = $"BPM {songDataSO.BPM}";
        SongDifficultySetting(songDataSO);
    }

    public void SetDifficulty(EDifficulty difficulty)
    {
        _textSongDifficulty.text = difficulty.ToString();
    }

    private void SongDifficultySetting(SongDataSO songData)
    {
        _templateDifficulty.DoTemplateSongDiffSetting(songData);
    }
}
