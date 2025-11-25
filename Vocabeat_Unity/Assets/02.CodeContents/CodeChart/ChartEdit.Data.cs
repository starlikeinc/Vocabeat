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

    public void OnSaveNoteData(EDifficulty diff, int level)
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

        FixBoundaryTicksForList(listNoteData);

        TargetSongData.SaveNoteDatas(diff, listNoteData, level);

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

    private void SortAndReindexNotes(List<Note> list)
    {
        if (list == null || list.Count <= 0)
            return;

        // 1) Tick → Y 정렬
        list.Sort((a, b) =>
        {
            int cmp = a.Tick.CompareTo(b.Tick);
            if (cmp != 0) return cmp;
            return a.Y.CompareTo(b.Y);
        });

        // 2) 정렬된 순서대로 ID 재할당
        for (int i = 0; i < list.Count; i++)
            list[i].ID = i;
    }

    /// <summary>
    /// 저장 전에, 페이지 오른쪽 끝에 있다고 봐야 하는데
    /// Tick이 다음 페이지 시작(960, 1920...)에 걸려있는 노트들을 -1 Tick 보정.
    /// </summary>
    private void FixBoundaryTicksForList(List<Note> list)
    {
        if (list == null || list.Count == 0)
            return;

        int ticksPerPage = _visualizer != null ? _visualizer.TicksPerPage : 960;

        foreach (var n in list)
        {
            if (n == null)
                continue;

            // 1) 시작 Tick 보정 (모든 노트 공통)
            if (n.Tick > 0)
            {
                int pageFromTick = n.Tick / ticksPerPage;
                int localTick = n.Tick % ticksPerPage;

                // 예: Tick = 960, PageIndex = 0 같은 케이스만 보정
                if (localTick == 0 && pageFromTick > 0 && n.PageIndex == pageFromTick - 1)
                {
                    n.Tick -= 1;
                }
            }

            // 2) FlowHold 끝 지점 보정 (FlowHold 전용)
            if (n.NoteType == ENoteType.FlowHold && n.HoldTick > 0)
            {
                int endTick = n.Tick + n.HoldTick;
                if (endTick <= 0)
                    continue;

                int endPage = endTick / ticksPerPage;
                int localEndTick = endTick % ticksPerPage;

                // 예: start=460, hold=500 → endTick=960
                // endPage = 1, localEndTick = 0, PageIndex = 0 이면
                // "0페이지 오른쪽 끝"으로 본다
                if (localEndTick == 0 && endPage > 0 && n.PageIndex == endPage - 1)
                {
                    // endTick을 1 줄이고 싶으니 HoldTick을 1 줄인다.
                    n.HoldTick -= 1;
                }
            }
        }
    }

    // Save 버튼에서 쓸 래핑용
    public void SaveCurrentDifficulty()
    {
        OnSaveNoteData(_currentDifficulty, _currentDifficultyLevel);
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
            _visualizer.SetGhostNoteType(EditState);
    }
}
