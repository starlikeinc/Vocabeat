using System.Collections.Generic;
using UnityEngine;

public class ChartEdit : MonoBehaviour
{
    [Header("Target SO")]
    [SerializeField] private SongDataSO TargetSongData;

    [Header("Visualizer")]
    [SerializeField] private ChartVisualizer _visualizer;

    [Header("Audio")]
    [SerializeField] private AudioSource _bgmAudioSource;

    [Header("Edit State")]
    [SerializeField] private EDifficulty _currentDifficulty = EDifficulty.Easy;
    [SerializeField] private int _currentPageIndex = 0;

    // 이 값은 "존재 가능한 페이지 수"
    // 0 ~ (_pageCount-1) 까지 이동 가능
    [SerializeField] private int _pageCount = 1;

    [Header("Note Edit")]
    [SerializeField] private ENoteType _currentNoteType = ENoteType.Normal;

    private readonly Dictionary<EDifficulty, List<Note>> EditNotesDict = new();

    // Undo 스택 (현재 난이도용)
    private readonly Stack<List<Note>> _undoStack = new();

    private bool _isFlowHoldMode = false;
    private bool _isFlowHoldWaitingSecondPoint = false;

    private Note _flowHoldStartNote = null;
    private Note _flowHoldEndNote = null;
    private FlowLongMeta _currentFlowMeta = null;

    public ENoteType CurrentNoteType => _currentNoteType;

    // ========================================
    private void Start()
    {
        if (!Application.isPlaying)
            return;

        InitFromSO();

        if (_visualizer != null)
        {
            _visualizer.Initialize(this);

            if (TargetSongData != null)
                _visualizer.VisualizerSetting(TargetSongData);

            // 초기 고스트 노트 타입 반영
            _visualizer.SetGhostNoteType(_currentNoteType);
        }

        RecalculatePageCount();
        RefreshPageView();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        // 마우스 휠로 페이지 이동
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0.1f)
        {
            ChangePage(-1);
        }
        else if (scroll < -0.1f)
        {
            ChangePage(1);
        }
    }

    // ========================================
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

    private void RecalculatePageCount()
    {
        if (!EditNotesDict.TryGetValue(_currentDifficulty, out var list)
            || list == null || list.Count == 0)
        {
            _pageCount = 1;
            return;
        }

        int maxPageIndex = 0;
        foreach (var n in list)
        {
            if (n.PageIndex > maxPageIndex)
                maxPageIndex = n.PageIndex;
        }

        // 존재 가능한 페이지 수 = 최대 PageIndex + 1
        _pageCount = maxPageIndex + 1;
        if (_pageCount < 1)
            _pageCount = 1;

        // 현재 페이지가 범위를 벗어났다면 보정
        _currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, _pageCount - 1);
    }

    private int GetLastPageIndexWithNote()
    {
        if (!EditNotesDict.TryGetValue(_currentDifficulty, out var list)
            || list == null || list.Count == 0)
        {
            return 0;
        }

        int maxPageIndex = 0;
        foreach (var n in list)
        {
            if (n.PageIndex > maxPageIndex)
                maxPageIndex = n.PageIndex;
        }
        return maxPageIndex;
    }

    private void RefreshPageView()
    {
        if (_visualizer == null)
            return;

        EditNotesDict.TryGetValue(_currentDifficulty, out var list);

        int lastPageWithNote = GetLastPageIndexWithNote();

        _visualizer.RefreshPageView(_currentDifficulty, _currentPageIndex, lastPageWithNote, list);
    }

    // ========================================    
    public void ChangePage(int delta)
    {
        int newPage = Mathf.Clamp(_currentPageIndex + delta, 0, Mathf.Max(_pageCount - 1, 0));
        if (newPage == _currentPageIndex)
            return;

        _currentPageIndex = newPage;
        RefreshPageView();
    }
    
    public void AddPage()
    {
        _pageCount++;
        _currentPageIndex = _pageCount - 1; // 새 페이지로 바로 이동
        RefreshPageView();
    }

    // ========================================    
    public void SelectNoteType(int noteTypeIndex)
    {
        int maxIndex = System.Enum.GetValues(typeof(ENoteType)).Length - 1;
        noteTypeIndex = Mathf.Clamp(noteTypeIndex, 0, maxIndex);

        _currentNoteType = (ENoteType)noteTypeIndex;

        _isFlowHoldMode = (_currentNoteType == ENoteType.FlowHold);

        if (!_isFlowHoldMode)
        {
            _isFlowHoldWaitingSecondPoint = false;
            _flowHoldStartNote = null;
            _flowHoldEndNote = null;
            _currentFlowMeta = null;
        }

        if (_visualizer != null)
            _visualizer.SetGhostNoteType(_currentNoteType);
    }

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
        EditNotesDict[_currentDifficulty] = prev;

        RecalculatePageCount();
        RefreshPageView();
    }

    // ========================================
    // ChartVisualizer → 클릭 이벤트 콜백
    public void OnRequestAddOrUpdateNote(int tick, float yNorm, int pageIndex, ENoteType noteType)
    {
        if (!Application.isPlaying)
            return;

        if (!EditNotesDict.TryGetValue(_currentDifficulty, out var list) || list == null)
        {
            list = new List<Note>();
            EditNotesDict[_currentDifficulty] = list;
        }

        RecordUndoSnapshot();

        if (pageIndex < 0) pageIndex = 0;

        Note target = FindNoteAt(list, tick, yNorm);

        if (target != null)
        {
            // 동일 위치에 이미 노트가 있으면 타입만 변경
            target.Tick = tick;
            target.PageIndex = pageIndex;
            target.Y = yNorm;
            target.NoteType = noteType;
        }
        else
        {
            int newId = GenerateNextNoteId(list);

            var newNote = new Note
            {
                ID = newId,
                Tick = tick,
                PageIndex = pageIndex,
                Y = yNorm,
                NoteType = noteType,
                HasSibling = false,
                HoldTick = 0,
                NextID = -1,
            };

            list.Add(newNote);
        }

        list.Sort((a, b) =>
        {
            int cmp = a.Tick.CompareTo(b.Tick);
            if (cmp != 0) return cmp;
            return a.Y.CompareTo(b.Y);
        });

        RecalculatePageCount();
        RefreshPageView();
    }

    public void OnFlowHoldLeftClick(int tick, float yNorm, int pageIndex)
    {
        if (!_isFlowHoldMode)
            return;

        EditNotesDict.TryGetValue(_currentDifficulty, out var list);
        if (list == null)
        {
            list = new();
            EditNotesDict[_currentDifficulty] = list;
        }

        RecordUndoSnapshot();

        // === 1회차 클릭: Start 생성 ===
        if (!_isFlowHoldWaitingSecondPoint)
        {
            int newId = GenerateNextNoteId(list);

            _flowHoldStartNote = new Note
            {
                ID = newId,
                Tick = tick,
                PageIndex = pageIndex,
                Y = yNorm,
                NoteType = ENoteType.FlowHold,
                FlowLongMeta = new FlowLongMeta()
            };

            _flowHoldStartNote.FlowLongMeta.CurvePoints.Add(new FlowCurvePoint
            {
                t = 0f,
                y01 = yNorm
            });

            list.Add(_flowHoldStartNote);

            _currentFlowMeta = _flowHoldStartNote.FlowLongMeta;
            _isFlowHoldWaitingSecondPoint = true;
            return;
        }

        // === 2회차 클릭: End 생성 ===
        {
            int newId = GenerateNextNoteId(list);

            _flowHoldEndNote = new Note
            {
                ID = newId,
                Tick = tick,
                PageIndex = pageIndex,
                Y = yNorm,
                NoteType = ENoteType.FlowHold,
                FlowLongMeta = _currentFlowMeta
            };

            _flowHoldEndNote.FlowLongMeta.CurvePoints.Add(new FlowCurvePoint
            {
                t = 1f,
                y01 = yNorm
            });

            list.Add(_flowHoldEndNote);

            // Tick 기반 정렬
            list.Sort((a, b) => a.Tick.CompareTo(b.Tick));

            _isFlowHoldWaitingSecondPoint = false;

            // 이제부터는 우클릭으로 중간 커브 입력
            return;
        }
    }

    public void OnFlowHoldRightClickAddCurvePoint(int tick, float yNorm)
    {
        if (_currentFlowMeta == null || _flowHoldStartNote == null || _flowHoldEndNote == null)
            return;

        int startTick = _flowHoldStartNote.Tick;
        int endTick = _flowHoldEndNote.Tick;

        if (tick <= startTick || tick >= endTick)
            return; // 범위 밖이면 무시

        float t = Mathf.InverseLerp(startTick, endTick, tick);

        RecordUndoSnapshot();

        _currentFlowMeta.CurvePoints.Add(new FlowCurvePoint
        {
            t = t,
            y01 = yNorm
        });

        // 정렬 유지
        _currentFlowMeta.CurvePoints.Sort((a, b) => a.t.CompareTo(b.t));

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

        RecalculatePageCount();
        RefreshPageView();
    }

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

    // ========================================    
    // 재생 버튼
    public void PlayBGM()
    {
        if (!Application.isPlaying)
            return;

        if (_bgmAudioSource == null)
            return;

        // 이미 재생중이면 무시
        if (_bgmAudioSource.isPlaying)
            return;

        if (_bgmAudioSource.clip == null)
            _bgmAudioSource.clip = TargetSongData.BGMCue.GetRandomClip();

        if (_bgmAudioSource.time > 0f)
        {
            // Pause 상태에서 재개
            _bgmAudioSource.UnPause();
        }
        else
        {
            // 처음부터 재생
            _bgmAudioSource.Play();
        }
    }

    // 일시정지 버튼
    public void PauseBGM()
    {
        if (!Application.isPlaying)
            return;

        if (_bgmAudioSource == null)
            return;

        if (_bgmAudioSource.isPlaying)
            _bgmAudioSource.Pause();
    }

    // 멈춤(정지 + 처음으로)
    public void StopBGM()
    {
        if (!Application.isPlaying)
            return;

        if (_bgmAudioSource == null)
            return;

        _bgmAudioSource.Stop();
        _bgmAudioSource.time = 0f;

        // 정지할 때 1페이지로 돌아가고 싶으면 유지
        _currentPageIndex = 0;
        RefreshPageView();
    }
}
