using System.Collections.Generic;
using UnityEngine;

public partial class ChartEdit
{
    public void InitFromSO()
    {
        EditNotesDict.Clear();
        _undoStack.Clear();

        if (TargetSongData == null)
        {
            Debug.LogError("<color=red>타겟 노래 없음.</color>");
            return;
        }

        foreach (var kvp in TargetSongData.NoteDatasByDiff)
        {
            List<Note> listEditNoteDatas = new();
            EditNotesDict[kvp.Key] = listEditNoteDatas;

            if (TargetSongData.NoteDatasByDiff.TryGetValue(kvp.Key, out var src))
            {
                foreach (var note in src)
                {
                    listEditNoteDatas.Add(new Note
                    {
                        ID = note.ID,
                        PageIndex = note.PageIndex,
                        NoteType = note.NoteType,
                        Tick = note.Tick,
                        Y = note.Y,
                        HasSibling = note.HasSibling,
                        HoldTick = note.HoldTick,
                        NextID = note.NextID,
                        FlowLongMeta = note.FlowLongMeta, // 필요시 복사 방식 나중에 조정
                    });
                }
            }
        }
    }

    public void OnSaveNoteData(EDifficulty diff)
    {
        if (TargetSongData == null)
        {
            Debug.LogError("<color=red>타겟 노래 없음</color>");
            return;
        }

        if (!EditNotesDict.TryGetValue(diff, out var listNoteData))
        {
            Debug.LogWarning("<color=red>편집용 버퍼 없음. 빈 리스트로 저장</color>");
            listNoteData = new List<Note>();
            EditNotesDict[diff] = listNoteData;
        }

        TargetSongData.SaveNoteDatas(diff, listNoteData);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(TargetSongData);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"<color=green>{diff} 난이도 노트 {listNoteData.Count}개 SO 저장 완료</color>");

        // 저장 후 페이지 정보 갱신
        if (diff == _currentDifficulty)
        {
            RecalculatePageCount();
            RefreshPageView();
        }
    }

    // Save 버튼에서 쓸 래핑용
    public void SaveCurrentDifficulty()
    {
        OnSaveNoteData(_currentDifficulty);
    }

    // ========================================
    // 난이도 변경
    public void SetDifficulty(int diffIndex)
    {
        _currentDifficulty = (EDifficulty)diffIndex;
        _currentPageIndex = 0;

        _undoStack.Clear();   // 다른 난이도 Undo 섞이지 않게

        RecalculatePageCount();
        RefreshPageView();

        if (_visualizer != null)
            _visualizer.SetGhostNoteType(_currentNoteType);
    }
}
