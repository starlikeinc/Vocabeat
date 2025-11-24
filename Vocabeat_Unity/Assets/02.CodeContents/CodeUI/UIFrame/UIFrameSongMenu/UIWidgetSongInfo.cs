using LUIZ.UI;
using TMPro;
using UnityEngine;

public class UIWidgetSongInfo : UIWidgetBase
{
    [SerializeField] private UITemplateSongDifficulty _templateDifficulty;

    [SerializeField] private TMP_Text _textSongName;
    [SerializeField] private TMP_Text _textSongBPM;

    public void WidgetSongInfoSetting(int songIndex)
    {
        var songDataSO = ManagerRhythm.Instance.SongDB.GetSong(songIndex);

        _textSongName.text = songDataSO.SongName;
        _textSongBPM.text = songDataSO.BPM.ToString();
        SongDifficultySetting(songDataSO);
    }

    private void SongDifficultySetting(SongDataSO songData)
    {
        _templateDifficulty.DoTemplateSongDiffSetting(songData);
    }
}
