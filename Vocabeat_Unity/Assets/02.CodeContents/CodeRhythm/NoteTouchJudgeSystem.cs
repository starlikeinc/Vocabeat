using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NoteTouchJudgeSystem : MonoBehaviour
{
    public event Action<Note, EJudgementType> OnJudgeResult;
    // 롱노트 시작/종료 (isEnd=true면 종료)
    public event Action<Note, EJudgementType, bool> OnHoldJudgeResult;

    [Header("Judge SFX")]
    [SerializeField] private SFXEventChannelSO _sfxEventChannel;
    [SerializeField] private JudgeSFXTableSO _judgeSFXTable;

    [Header("Judge Option")]
    [SerializeField] private float _touchRadius = 80f;
    [SerializeField] private int _blueStarRange = 30;
    [SerializeField] private int _whiteStarRange = 80;
    [SerializeField] private int _yellowStarRange = 120;
    [SerializeField] private int _redStarRange = 150;

    private ManagerRhythm _context;

    private Dictionary<EJudgementType, int> _dictNoteJudgementCounts = new();
    private HashSet<int> _judgedNoteIds = new();

    private RectTransform _touchArea;
    private Camera _uiCam;

    private IReadOnlyList<Note> _listNotes;

    private readonly List<Note> _tempMissCandidates = new();
    private readonly List<int> _tempFinalizeIds = new();

    private bool _isInit = false;

    // 롱노트 상태 -------------------------------------------------
    private class HoldState
    {
        public Note Note;
        public int StartTick;
        public int EndTick;
        public int PointerId; // 모바일 fingerId, PC는 -1
        public bool Started;
        public bool Completed;
        public bool ReleasedEarly;
        public EJudgementType StartJudgeType;
    }

    private readonly Dictionary<int, HoldState> _activeHoldStates = new(); // NoteID -> HoldState
    private readonly Dictionary<int, int> _pointerToNoteId = new();        // PointerId -> NoteID

    // ========================================
    private void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        PCInput();
#endif
#if UNITY_ANDROID || UNITY_IOS
        MobileInput();
#endif

        if (_isInit)
        {
            UpdateHoldStates();
            AutoMissUpdate();
        }
    }

    private void PCInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryTouchNoteFromPointer(Input.mousePosition, -1);
        }
        if (Input.GetMouseButtonUp(0))
        {
            HandlePointerRelease(-1);
        }
    }

    private void MobileInput()
    {
        int touchCount = Input.touchCount;
        if (touchCount == 0)
            return;

        for (int i = 0; i < touchCount; i++)
        {
            var touch = Input.GetTouch(i);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    TryTouchNoteFromPointer(touch.position, touch.fingerId);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    HandlePointerRelease(touch.fingerId);
                    break;
            }
        }
    }

    // ========================================
    public void InitJudgementSystem(ManagerRhythm ctx, RectTransform touchArea, Camera uiCam)
    {
        if (_isInit)
            return;

        _context = ctx;
        _touchArea = touchArea;
        _uiCam = uiCam;

        _isInit = true;
    }

    public void BindJudgementNoteDatas(IReadOnlyList<Note> listNotes)
    {
        _listNotes = listNotes;

        _judgedNoteIds.Clear();
        _dictNoteJudgementCounts.Clear();
        _activeHoldStates.Clear();
        _pointerToNoteId.Clear();
    }

    public void ResetForNewSong()
    {
        _judgedNoteIds.Clear();
        _dictNoteJudgementCounts.Clear();
        _activeHoldStates.Clear();
        _pointerToNoteId.Clear();
        _tempMissCandidates.Clear();
    }

    public int GetJudgeCountByType(EJudgementType judgeType)
    {
        if (!_dictNoteJudgementCounts.TryGetValue(judgeType, out int count))
            Debug.LogError($"{judgeType} 에 해당하는 판정 횟수 없음");
        return count;
    }

    // ========================================
    private void TryTouchNoteFromPointer(Vector2 screenPos, int pointerId)
    {
        if (_context == null || !_context.IsPlaying)
            return;

        if (_touchArea == null || _uiCam == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_touchArea, screenPos, _uiCam, out Vector2 localPos);

        if (_listNotes == null || _listNotes.Count == 0)
            return;

        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;

        Note bestNote = null;
        int bestDiff = int.MaxValue;
        float bestDist = float.MaxValue;

        foreach (var note in _listNotes)
        {
            if (note == null) continue;

            int id = note.ID;

            if (_judgedNoteIds.Contains(id))
                continue;
            if (_activeHoldStates.ContainsKey(id))
                continue;

            int diffTick = Mathf.Abs(note.Tick - songTick);
            if (diffTick > _redStarRange)
                continue;

            Vector2 idealLocal = GetExpectedLocalPositionForTap(note, songTick);
            float dist = Vector2.Distance(localPos, idealLocal);

            if (diffTick < bestDiff || (diffTick == bestDiff && dist < bestDist))
            {
                bestDiff = diffTick;
                bestDist = dist;
                bestNote = note;
            }
        }

        if (bestNote != null && bestDist < _touchRadius)
        {
            JudgeOrStartHold(bestNote, pointerId);
        }
    }

    private void JudgeOrStartHold(Note note, int pointerId)
    {
        if (_context == null || _context.RTimeline == null)
            return;

        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;
        if (songTick < 0)
            return;

        int noteTick = note.Tick;
        int diff = Mathf.Abs(noteTick - songTick);

        EJudgementType judgeType =
            diff <= _blueStarRange ? EJudgementType.BlueStar :
            diff <= _whiteStarRange ? EJudgementType.WhiteStar :
            diff <= _yellowStarRange ? EJudgementType.YellowStar :
            EJudgementType.RedStar;

        // 롱노트 여부 판단
        bool isHold =
            note.NoteType == ENoteType.FlowHold ||
            note.NoteType == ENoteType.LongHold ||
            note.HoldTick > 0;

        if (!isHold)
        {
            ApplyJudge(note, judgeType);
            return;
        }

        int endTick = noteTick + Mathf.Max(0, note.HoldTick);

        int id = note.ID;
        if (_activeHoldStates.ContainsKey(id))
            return;

        var holdState = new HoldState
        {
            Note = note,
            StartTick = noteTick,
            EndTick = endTick,
            PointerId = pointerId,
            Started = true,
            Completed = false,
            ReleasedEarly = false,
            StartJudgeType = judgeType
        };

        _activeHoldStates.Add(id, holdState);
        if (!_pointerToNoteId.ContainsKey(pointerId))
            _pointerToNoteId.Add(pointerId, id);

        PlayJudgeSFX(judgeType);
        OnHoldJudgeResult?.Invoke(note, judgeType, false);
    }

    private EJudgementType GetEndJudgeType(int endTick, int songTick)
    {
        if (songTick >= endTick)
            return EJudgementType.BlueStar;

        int diffEnd = Mathf.Abs(endTick - songTick);

        return
            diffEnd <= _blueStarRange ? EJudgementType.BlueStar :
            diffEnd <= _whiteStarRange ? EJudgementType.WhiteStar :
            diffEnd <= _yellowStarRange ? EJudgementType.YellowStar :
            EJudgementType.RedStar;
    }

    private void FinalizeHoldByEnd(Note note, HoldState hs, int songTick)
    {
        int endTick = hs.EndTick;
        EJudgementType endJudge = GetEndJudgeType(endTick, songTick);

        hs.Completed = true;

        Debug.Log($"<color=yellow>[{note.ID}]롱 노트 최종 판정: [{endJudge}]</color>");

        PlayJudgeSFX(endJudge);
        OnHoldJudgeResult?.Invoke(note, endJudge, true);
        ApplyJudgeFinal(note, endJudge);
    }

    private void UpdateHoldStates()
    {
        if (_activeHoldStates.Count == 0 || _context?.RTimeline == null)
            return;

        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;

        _tempFinalizeIds.Clear();

        foreach (var kv in _activeHoldStates)
        {
            var hs = kv.Value;
            if (hs.Completed) continue;

            var note = hs.Note;
            int id = note.ID;

            int startTick = hs.StartTick;
            int endTick = hs.EndTick;

            if (songTick < startTick)
                continue;

            bool pointerActive = IsPointerStillActive(hs.PointerId);
            if (!pointerActive)
            {
                hs.ReleasedEarly = songTick < endTick;
                FinalizeHoldByEnd(note, hs, songTick);
                _tempFinalizeIds.Add(id);
                continue;
            }

            if (note.NoteType == ENoteType.FlowHold && note.HoldTick > 0)
            {
                if (TryGetPointerLocalPosition(hs.PointerId, out Vector2 pointerLocal))
                {
                    Vector2 idealLocal = GetExpectedLocalPositionForHold(note, songTick);
                    float dist = Vector2.Distance(pointerLocal, idealLocal);
                    if (dist > _touchRadius)
                    {
                        hs.ReleasedEarly = songTick < endTick;
                        FinalizeHoldByEnd(note, hs, songTick);
                        _tempFinalizeIds.Add(id);
                        continue;
                    }
                }
            }

            if (songTick < endTick)
                continue;

            hs.ReleasedEarly = false;
            FinalizeHoldByEnd(note, hs, songTick);
            _tempFinalizeIds.Add(id);
        }

        for (int i = 0; i < _tempFinalizeIds.Count; i++)
        {
            RemoveHoldState(_tempFinalizeIds[i]);
        }
    }

    private bool IsPointerStillActive(int pointerId)
    {
#if UNITY_EDITOR
        if (pointerId == -1)
            return Input.GetMouseButton(0);
#endif
#if UNITY_ANDROID || UNITY_IOS
        if (pointerId >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.fingerId == pointerId &&
                    (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Began))
                    return true;
            }
            return false;
        }
#endif
        if (pointerId == -1)
            return Input.GetMouseButton(0);

        return false;
    }

    private bool TryGetPointerLocalPosition(int pointerId, out Vector2 localPos)
    {
#if UNITY_EDITOR
        if (pointerId == -1)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_touchArea, Input.mousePosition, _uiCam, out localPos);
            return true;
        }
#endif
#if UNITY_ANDROID || UNITY_IOS
        if (pointerId >= 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.fingerId == pointerId)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_touchArea, t.position, _uiCam, out localPos);
                    return true;
                }
            }
        }
#endif
        localPos = Vector2.zero;
        return false;
    }

    private void HandlePointerRelease(int pointerId)
    {
        if (!_pointerToNoteId.TryGetValue(pointerId, out int noteId))
            return;

        if (_activeHoldStates.TryGetValue(noteId, out var hs))
        {
            if (!hs.Completed && _context != null && _context.RTimeline != null)
            {
                int songTick = _context.RTimeline.CurTick - _context.RTimeline.PreSongTicks;
                int endTick = hs.EndTick;

                hs.ReleasedEarly = songTick < endTick;
                
                FinalizeHoldByEnd(hs.Note, hs, songTick);

                RemoveHoldState(noteId);
            }
        }
    }

    private void RemoveHoldState(int noteId)
    {
        _activeHoldStates.Remove(noteId);

        int removePointer = int.MinValue;
        foreach (var p in _pointerToNoteId)
        {
            if (p.Value == noteId)
            {
                removePointer = p.Key;
                break;
            }
        }
        if (removePointer != int.MinValue)
            _pointerToNoteId.Remove(removePointer);
    }

    // ======================================== AutoMiss
    private void AutoMissUpdate()
    {
        if (_context == null || !_context.IsPlaying)
            return;

        if (_listNotes == null || _listNotes.Count == 0)
            return;

        _tempMissCandidates.Clear();

        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;

        for (int i = 0; i < _listNotes.Count; i++)
        {
            var note = _listNotes[i];
            if (note == null)
                continue;

            int id = note.ID;

            if (_judgedNoteIds.Contains(id))
                continue;

            if (_activeHoldStates.ContainsKey(id))
                continue;

            if (songTick > note.Tick + _redStarRange)
            {
                _tempMissCandidates.Add(note);
            }
        }

        for (int i = 0; i < _tempMissCandidates.Count; i++)
        {
            Note note = _tempMissCandidates[i];
            ApplyJudge(note, EJudgementType.RedStar);
        }
    }

    // ======================================== Judge 적용 & SFX
    private void ApplyJudgeFinal(Note note, EJudgementType type)
    {
        if (_dictNoteJudgementCounts.TryGetValue(type, out int count))
            _dictNoteJudgementCounts[type] = count + 1;
        else
            _dictNoteJudgementCounts[type] = 1;

        FinalizeJudge(note, type);
    }

    private void ApplyJudge(Note note, EJudgementType type)
    {
        if (_judgedNoteIds.Contains(note.ID))
            return;

        PlayJudgeSFX(type);
        ApplyJudgeFinal(note, type);
    }

    private void FinalizeJudge(Note note, EJudgementType type)
    {
        Debug.Log($"[{note.ID}]노트 판정: [{type}]");
        _judgedNoteIds.Add(note.ID);
        _context.SetScoreValueByJudgeType(type);
        OnJudgeResult?.Invoke(note, type);
    }

    private void PlayJudgeSFX(EJudgementType type)
    {
        if (_judgeSFXTable == null || _sfxEventChannel == null)
            return;

        var cue = _judgeSFXTable.GetCue(type);
        if (cue != null)
        {
            _sfxEventChannel.Raise(cue);
        }
    }

    // ======================================== 기대 좌표 계산부 (핵심)

    /// <summary>
    /// 첫 터치 시, 이 노트를 어느 위치로 간주하고 거리 비교할지
    /// </summary>
    private Vector2 GetExpectedLocalPositionForTap(Note note, int songTick)
    {
        // Normal: 고정 위치
        if (note.NoteType == ENoteType.Normal || note.HoldTick <= 0 || note.FlowLongMeta == null)
        {
            return NoteUtility.GetNotePosition(_touchArea, note.Tick, note.Y);
        }

        // FlowHold: 시작 쪽에 가깝게
        return GetExpectedLocalPositionForHold(note, songTick);
    }

    /// <summary>
    /// FlowHold 진행 중, 현재 Tick에서 기대되는 위치
    /// </summary>
    private Vector2 GetExpectedLocalPositionForHold(Note note, int songTick)
    {
        int startTick = note.Tick;
        int endTick = note.Tick + Mathf.Max(0, note.HoldTick);

        int clampedTick = Mathf.Clamp(songTick, startTick, endTick);
        float t = Mathf.InverseLerp(startTick, endTick, songTick);
        t = Mathf.Clamp01(t);

        float y01;
        if (note.FlowLongMeta != null && note.FlowLongMeta.CurvePoints != null && note.FlowLongMeta.CurvePoints.Count > 0)
        {
            y01 = NoteUtility.EvaluateY01(note.FlowLongMeta, t);
        }
        else
        {
            y01 = note.Y;
        }

        // X는 Tick 기반, Y는 커브 기반
        return NoteUtility.GetNotePosition(_touchArea, clampedTick, y01);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (_touchArea == null || _context == null || _context.RTimeline == null)
            return;

        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;

        Vector3 forward = Vector3.forward;
        float radius = _touchRadius * _touchArea.lossyScale.x;

        // ================================
        // NORMAL 노트 판정 유효 범위 표시
        // ================================
        Handles.color = new Color(0.2f, 0.7f, 1f, 0.6f); // 파란색

        if (_listNotes != null)
        {
            foreach (var note in _listNotes)
            {
                if (note == null) continue;
                if (note.NoteType != ENoteType.Normal) continue;
                if (_judgedNoteIds.Contains(note.ID)) continue;
                if (_activeHoldStates.ContainsKey(note.ID)) continue;

                // Normal은 GetExpectedLocalPositionForTap()
                Vector2 idealLocal = GetExpectedLocalPositionForTap(note, songTick);
                Vector3 world = _touchArea.TransformPoint(idealLocal);

                Handles.DrawWireDisc(world, forward, radius);
            }
        }

        // ================================
        // FLOW HOLD 현재 진행 중 판정 범위
        // ================================
        Handles.color = new Color(1f, 0.2f, 0.2f, 0.6f); // 빨간색

        foreach (var kv in _activeHoldStates)
        {
            HoldState hs = kv.Value;
            if (hs.Completed) continue;

            Note note = hs.Note;

            // FlowHold 진행 중 기대 위치
            Vector2 idealLocal = GetExpectedLocalPositionForHold(note, songTick);
            Vector3 world = _touchArea.TransformPoint(idealLocal);

            Handles.DrawWireDisc(world, forward, radius);
        }
    }
#endif

}
