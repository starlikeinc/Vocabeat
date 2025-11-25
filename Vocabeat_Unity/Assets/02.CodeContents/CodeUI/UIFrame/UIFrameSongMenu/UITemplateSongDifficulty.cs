using LUIZ.UI;
using System;
using UnityEngine;

public class UITemplateSongDifficulty : UITemplateBase
{
    [Serializable]
    public class SongDifficultyInfoData
    {
        public EDifficulty Difficulty;
        public Sprite DiffIcon;        
    }

    [SerializeField] private RectTransform _content;

    [SerializeField] private SongDifficultyInfoData[] _infoDatas;

    private bool _isInit = false;

    public void DoTemplateSongDiffSetting(SongDataSO songData)
    {
        DoUITemplateReturnAll();

        foreach (var data in songData.DifficultyValueByDiff)
        {            
            var uiItem = DoTemplateRequestItem<UIItemSongDifficulty>(_content);
            uiItem.DoSongDifficultySetting(GetSongDifficultyInfoData(data.Key), data.Value, data.Key);

            if (!_isInit)
            {
                _isInit = true;
                uiItem.OnSelectThisDifficulty();
            }
        }        
    }

    private Sprite GetSongDifficultyInfoData(EDifficulty diff)
    {
        foreach(var data in _infoDatas)
        {
            if (data.Difficulty == diff)
                return data.DiffIcon;
        }
        return null;
    }
}
