using System.Collections.Generic;
using UnityEngine;

public enum EChartEditType
{
    Normal,
    FlowPlace,
    FlowCurve,
}

public partial class ChartEdit
{
    private Note _flowStartNote;
    private FlowLongMeta _currentFlowMeta;

    private int _flowLastStartTick;
    private int _flowLastEndTick;

    public bool IsNoStartNote { get; private set; } = true;
    public bool IsNoEndNote { get; private set; } = true;

    // ========================================    
    // FlowHold Curve 편집 진입 가능 여부
    public bool HasCurveNotePair()
    {
        // Start/End가 모두 결정된 상태에서만 Curve 편집 허용
        if (_currentFlowMeta == null)
            return false;

        if (_flowLastEndTick <= _flowLastStartTick)
            return false;

        // t=0, t=1 포인트가 모두 있는지 확인
        var points = _currentFlowMeta.CurvePoints;
        if (points == null || points.Count < 2)
            return false;

        bool hasStart = false;
        bool hasEnd = false;

        foreach (var p in points)
        {
            if (!hasStart && Mathf.Approximately(p.t, 0f))
                hasStart = true;
            if (!hasEnd && Mathf.Approximately(p.t, 1f))
                hasEnd = true;
        }

        return hasStart && hasEnd;
    }

    // ========================================    
    public void OnGhostChangeNormal() // 일반 노트 모드
    {
        if (EditState == EEditState.Long_Curve)
        {
            Debug.LogWarning("Curve 모드 풀고 바꾸세요.");
            return;
        }            

        ChangeState(new NoteEditStateNormal(this));
        _currentNoteType = ENoteType.Normal;
        _visualizer.SetGhostNoteType(EditState);
    }

    public void OnGhostChangeFollowPlace() // 롱 노트(따라가기) 모드 - 세부 내용은 NoteEditStateFlowHold 에 서술
    {
        ChangeState(new NoteEditStateFlowHold(this));
        _currentNoteType = ENoteType.FlowHold;
        _visualizer.SetGhostNoteType(EditState);
    }

    // ========================================    
    public void RecordUndoSnapshot()
    {
        if (!EditNotesDict.TryGetValue(_currentDifficulty, out var list) || list == null)
            return;

        var copy = new List<Note>(list.Count);
        foreach (var n in list)
        {
            copy.Add(CloneNote(n));
        }

        _undoStack.Push(copy);
    }

    private static Note CloneNote(Note src)
    {
        if (src == null) return null;

        return new Note
        {
            ID = src.ID,
            PageIndex = src.PageIndex,
            NoteType = src.NoteType,
            Tick = src.Tick,
            Y = src.Y,
            HasSibling = src.HasSibling,
            HoldTick = src.HoldTick,
            NextID = src.NextID,
            FlowLongMeta = src.FlowLongMeta, // 필요 시 깊은 복사로 변경
        };
    }

    // Undo 버튼에 연결
    public void Undo()
    {
        if (!Application.isPlaying)
            return;

        if (_undoStack.Count == 0)
            return;

        var prev = _undoStack.Pop();
        SortAndReindexNotes(prev);
        EditNotesDict[_currentDifficulty] = prev;

        RecalculatePageCount();
        RefreshPageView();
    }

    // ========================================    
    public void OnRequestAddOrUpdateNote(int tick, float yNorm, int pageIndex, EChartEditType chartEditType)
    {
        if (!Application.isPlaying)
            return;

        // 공통: 리스트 확보 + Undo
        if (!EditNotesDict.TryGetValue(_currentDifficulty, out var list))
        {
            list = new List<Note>();
            EditNotesDict[_currentDifficulty] = list;
        }

        RecordUndoSnapshot();

        switch (chartEditType)
        {
            case EChartEditType.Normal:
                HandleNormalNote(list, tick, yNorm, pageIndex);
                break;

            case EChartEditType.FlowPlace:
                HandleFlowHoldPlace(list, tick, yNorm, pageIndex);
                break;

            case EChartEditType.FlowCurve:
                HandleFlowHoldCurvePoint(list, tick, yNorm, pageIndex);
                break;
        }

        SortAndReindexNotes(list);

        RecalculatePageCount();
        RefreshPageView();
    }

    public void OnRequestRemoveNote(int tick, float yNorm, int pageIndex)
    {
        if (!Application.isPlaying)
            return;

        if (!EditNotesDict.TryGetValue(_currentDifficulty, out var list) || list == null || list.Count == 0)
            return;

        Note target = FindNoteAt(list, tick, yNorm);
        if (target == null)
            return;

        RecordUndoSnapshot();
        list.Remove(target);

        SortAndReindexNotes(list);

        RecalculatePageCount();
        RefreshPageView();
    }

    #region 편집 모드 별 편집요청
    // 일반 노트 찍기
    private void HandleNormalNote(List<Note> list, int tick, float yNorm, int pageIndex)
    {
        if (pageIndex < 0) pageIndex = 0;

        Note target = FindNoteAt(list, tick, yNorm);
        if (target != null)
            return;

        int newId = GenerateNextNoteId(list);

        var newNote = new Note
        {
            ID = newId,
            Tick = tick,
            PageIndex = pageIndex,
            Y = yNorm,
            NoteType = ENoteType.Normal,
            HasSibling = false,
            HoldTick = 0,
            NextID = -1,
        };

        list.Add(newNote);

        list.Sort((a, b) =>
        {
            int cmp = a.Tick.CompareTo(b.Tick);
            if (cmp != 0) return cmp;
            return a.Y.CompareTo(b.Y);
        });
    }

    // 롱 노트(따라가기) 첫, 끝 점 찍기
    private void HandleFlowHoldPlace(List<Note> list, int tick, float yNorm, int pageIndex)
    {
        Note target = FindNoteAt(list, tick, yNorm);
        if (target != null)
            return;

        // Start가 없으면 → Start 생성
        if (_flowStartNote == null)
        {
            int newId = GenerateNextNoteId(list);

            _currentFlowMeta = new FlowLongMeta();
            _currentFlowMeta.CurvePoints.Add(new FlowCurvePoint { t = 0f, y01 = yNorm });

            Debug.Log($"[FlowHold] Start Tick::[{tick}] Y Normal::[{yNorm}] Page Index::[{pageIndex}] 찍음");
            IsNoStartNote = false;
            IsNoEndNote = true;

            _flowStartNote = new Note
            {
                ID = newId,
                Tick = tick,
                PageIndex = pageIndex,
                Y = yNorm,
                NoteType = ENoteType.FlowHold,
                HoldTick = 0,
                FlowLongMeta = _currentFlowMeta,
            };

            _flowLastStartTick = tick;
            _flowLastEndTick = tick;

            list.Add(_flowStartNote);
        }
        else
        {
            if (tick <= _flowLastStartTick)
            {                
                Debug.LogWarning($"[FlowHoldPlace] End Tick({tick}) 이 Start Tick({_flowLastStartTick}) 보다 작거나 같아서 무시됨.");
                return;
            }

            // 이미 Start가 있다면 Start에 해당하는 Note의 t = 1인 CurvePoint 추가.
            _currentFlowMeta.CurvePoints.Add(new FlowCurvePoint { t = 1f, y01 = yNorm });
            Debug.Log($"[FlowHoldPlace] End Tick::[{tick}] Y Normal::[{yNorm}] Page Index::[{pageIndex}] 찍음");
            IsNoStartNote = true;
            IsNoEndNote = false;

            _flowLastEndTick = tick;

            _flowStartNote.HoldTick = _flowLastEndTick - _flowLastStartTick;
            Debug.Log($"[FlowHoldPlace] Hold Tick::[{_flowStartNote.HoldTick}]");

            _flowStartNote = null;            
        }

        list.Sort((a, b) => a.Tick.CompareTo(b.Tick));
    }

    private void HandleFlowHoldCurvePoint(List<Note> list, int tick, float yNorm, int pageIndex)
    {
        // 마지막 FlowHold 메타가 뭔지 알고 있다는 전제 필요
        if (_currentFlowMeta == null)
        {
            Debug.LogError($"[FlowHoldCurve] 현재 편집중인 Long Hold Note 데이터가 없거나 End 지점 찍지 않음.");
            return;
        }            

        // Start/End Tick 기준으로 t 계산
        int startTick = _flowLastStartTick;
        int endTick = _flowLastEndTick;

        if (endTick <= startTick)
            return;

        if (tick <= startTick || tick >= endTick)
            return;

        float t = Mathf.InverseLerp(startTick, endTick, tick);

        _currentFlowMeta.CurvePoints.Add(new FlowCurvePoint
        {
            t = t,
            y01 = yNorm
        });

        Debug.Log($"[FlowHoldCurve] Tick::[{tick}] Y Normal::[{yNorm}] Page Index::[{pageIndex}] 찍음");

        _currentFlowMeta.CurvePoints.Sort((a, b) => a.t.CompareTo(b.t));
    }
    #endregion
    private Note FindNoteAt(List<Note> list, int tick, float yNorm)
    {
        if (list == null)
            return null;

        const int tickTolerance = 0;      // 스냅 된 Tick은 정확히 동일하다고 가정
        const float yTolerance = 0.03f;   // Y는 약간의 여유

        Note best = null;
        foreach (var n in list)
        {
            int dt = Mathf.Abs(n.Tick - tick);
            if (dt > tickTolerance)
                continue;

            float dy = Mathf.Abs(n.Y - yNorm);
            if (dy > yTolerance)
                continue;

            best = n;
            break;
        }

        return best;
    }

    private int GenerateNextNoteId(List<Note> list)
    {
        int maxId = 0;
        if (list != null)
        {
            foreach (var n in list)
            {
                if (n != null && n.ID > maxId)
                    maxId = n.ID;
            }
        }
        return maxId + 1;
    }
}
