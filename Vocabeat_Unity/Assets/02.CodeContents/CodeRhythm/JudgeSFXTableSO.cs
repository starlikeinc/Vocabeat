using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JudgeSfxTable", menuName = "AudioChannel/Judge SFX Table")]
public class JudgeSFXTableSO : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public EJudgementType judgeType;
        public AudioCueSO cue;
    }

    [SerializeField] private List<Entry> _entries = new();

    private Dictionary<EJudgementType, AudioCueSO> _dict;

    private void BuildDict()
    {
        _dict = new Dictionary<EJudgementType, AudioCueSO>();
        foreach (var e in _entries)
        {
            if (!_dict.ContainsKey(e.judgeType))
                _dict.Add(e.judgeType, e.cue);
            else
                _dict[e.judgeType] = e.cue; // 중복 있으면 마지막 값으로 덮어쓰기
        }
    }

    public AudioCueSO GetCue(EJudgementType type)
    {
        if (_dict == null)
            BuildDict();

        _dict.TryGetValue(type, out var cue);
        return cue;
    }
}
