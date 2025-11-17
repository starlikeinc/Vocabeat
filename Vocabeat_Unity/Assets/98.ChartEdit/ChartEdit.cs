using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ChartEdit : MonoBehaviour
{
    [Header("Target SO")]
    [SerializeField] private SongDataSO TargetSongData;

    private readonly Dictionary<EDifficulty, List<Note>> EditNotesDict = new();

    // ========================================
    public void InitFromSO()
    {
        if (TargetSongData == null)
        {
            Debug.LogError($"<color=red>타겟 노래 없음.</color>");
            return;
        }

        foreach (var kvp in TargetSongData.NoteDatasByDiff)
        {
            List<Note> listEditNoteDatas = new();
            EditNotesDict[kvp.Key] = listEditNoteDatas;

            if (TargetSongData.NoteDatasByDiff.TryGetValue(kvp.Key, out var src))
            {
                foreach(var note in src)
                {
                    listEditNoteDatas.Add(new()
                    {
                        ID = note.ID,
                        PageIndex = note.PageIndex,
                        NoteType = note.NoteType,
                        Tick = note.Tick,
                        Y = note.Y,                        
                        HasSibling = true,
                        HoldTick = note.HoldTick,
                        NextID = note.NextID,
                    });
                }
            }
        }
    }

    public void OnSaveNoteData(EDifficulty diff)
    {
        if (TargetSongData == null)
        {
            Debug.LogError($"<color=red>타겟 노래 없음</color>");
            return;
        }

        if (!EditNotesDict.TryGetValue(diff, out var listNoteData))
        {
            Debug.LogWarning($"<color=red>편집용 버퍼 없음. 빈 리스트로 저장</color>");
            listNoteData = new List<Note>();
            EditNotesDict[diff] = listNoteData;
        }

        TargetSongData.SaveNoteDatas(diff, listNoteData);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(TargetSongData);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"<color=green>{diff} 난이도 노트 {listNoteData.Count}개 SO 저장 완료</color>");
    }
}
