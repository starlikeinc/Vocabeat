using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteTouchJudgeSystem : MonoBehaviour
{
    public event Action<INote, EJudgementType> OnJudgeResult;
    // 필요 시 롱노트 시작/종료 구분 이벤트(옵션)
    public event Action<INote, EJudgementType, bool> OnHoldJudgeResult; // isEnd=true면 종료

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

    private IReadOnlyList<INote> _listNotes;

    private readonly List<INote> _tempMissCandidates = new();
    private readonly List<int> _tempFinalizeIds = new();

    private bool _isInit = false;

    // 롱노트 상태 -------------------------------------------------
    private class HoldState
    {
        public INote Note;
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
        // 1) 입력 먼저
#if UNITY_STANDALONE || UNITY_EDITOR
        PCInput();
#endif
#if UNITY_ANDROID || UNITY_IOS
        MobileInput();
#endif

        // 2) Hold 상태 업데이트
        if (_isInit)
        {
            UpdateHoldStates();
            // 3) 마지막에 AutoMiss
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

    public void BindJudgementNoteDatas(IReadOnlyList<INote> listNotes)
    {
        _listNotes = listNotes;

        _judgedNoteIds.Clear();
        _dictNoteJudgementCounts.Clear();
        _activeHoldStates.Clear();
        _pointerToNoteId.Clear();
    }

    // 재생/중지 시 외부에서 호출(ManagerRhythm에서 호출)
    public void ResetForNewSong()
    {
        _judgedNoteIds.Clear();
        _dictNoteJudgementCounts.Clear();
        _activeHoldStates.Clear();
        _pointerToNoteId.Clear();
        _tempMissCandidates.Clear();
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

        // 시간 우선 필터링
        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;

        INote bestNote = null;
        int bestDiff = int.MaxValue;
        float bestDist = float.MaxValue;

        foreach (var note in _listNotes)
        {
            if (note == null) continue;

            int id = note.NoteData.ID;

            if (_judgedNoteIds.Contains(id))
                continue;
            if (_activeHoldStates.ContainsKey(id))
                continue;

            int diffTick = Mathf.Abs(note.NoteData.Tick - songTick);
            if (diffTick > _redStarRange) // 시간 창 밖이면 스킵
                continue;

            float dist = Vector2.Distance(localPos, note.RectTrs.anchoredPosition);

            // 1차: 시간 diff 작은 것, 2차: 거리 작은 것
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

    private void JudgeOrStartHold(INote note, int pointerId)
    {
        if (_context == null || _context.RTimeline == null)
            return;

        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;
        if (songTick < 0)
            return;

        int noteTick = note.NoteData.Tick;
        int diff = Mathf.Abs(noteTick - songTick);

        EJudgementType judgeType =
            diff <= _blueStarRange ? EJudgementType.BlueStar :
            diff <= _whiteStarRange ? EJudgementType.WhiteStar :
            diff <= _yellowStarRange ? EJudgementType.YellowStar :
            EJudgementType.RedStar;

        // 롱노트 판정 기준: IFlowHoldNote 우선, 없으면 HoldTick
        bool isHold =
            (note is IFlowHoldNote) ||
            note.NoteData.HoldTick > 0 ||
            note.NoteData.NoteType == ENoteType.FlowHold ||
            note.NoteData.NoteType == ENoteType.LongHold;

        if (!isHold)
        {
            ApplyJudge(note, judgeType);
            return;
        }

        int endTick;
        if (note is IFlowHoldNote flow)
        {
            endTick = flow.EndTick; // 데이터에서 보장되는 종료 Tick 사용
        }
        else
        {
            endTick = noteTick + Mathf.Max(0, note.NoteData.HoldTick);
        }

        int id = note.NoteData.ID;
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
        // EndTick을 이미 지난 상태에서 끝났다면 무조건 Perfect
        if (songTick >= endTick)
            return EJudgementType.BlueStar;

        int diffEnd = Mathf.Abs(endTick - songTick);

        return
            diffEnd <= _blueStarRange ? EJudgementType.BlueStar :
            diffEnd <= _whiteStarRange ? EJudgementType.WhiteStar :
            diffEnd <= _yellowStarRange ? EJudgementType.YellowStar :
            EJudgementType.RedStar;
    }

    private void FinalizeHoldByEnd(INote note, HoldState hs, int songTick)
    {
        int endTick = hs.EndTick;
        EJudgementType endJudge = GetEndJudgeType(endTick, songTick);

        hs.Completed = true;

        PlayJudgeSFX(endJudge);
        OnHoldJudgeResult?.Invoke(note, endJudge, true); // isEnd = true
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

            int id = hs.Note.NoteData.ID;
            var note = hs.Note;

            int startTick = hs.StartTick;
            int endTick = hs.EndTick;

            // 아직 시작 전이면 스킵(안전용)
            if (songTick < startTick)
                continue;

            // 1) 포인터가 살아있는지 체크
            bool pointerActive = IsPointerStillActive(hs.PointerId);
            if (!pointerActive)
            {
                // OS가 터치를 끊어버렸거나, 입력이 갑자기 사라진 경우
                hs.ReleasedEarly = songTick < endTick;
                FinalizeHoldByEnd(note, hs, songTick);
                _tempFinalizeIds.Add(id);
                continue;
            }

            // 2) FlowLong이면 곡선 안에 있는지 체크
            if (note is IFlowHoldNote flowNote)
            {
                if (TryGetPointerLocalPosition(hs.PointerId, out Vector2 pointerLocal))
                {
                    Vector2 idealLocal = flowNote.GetLocalPositionAtTick(songTick);
                    float dist = Vector2.Distance(pointerLocal, idealLocal);
                    if (dist > _touchRadius)
                    {
                        // 라인에서 벗어난 순간을 "이 시점에 손을 뗀 것"처럼 취급
                        hs.ReleasedEarly = songTick < endTick;
                        FinalizeHoldByEnd(note, hs, songTick);
                        _tempFinalizeIds.Add(id);
                        continue;
                    }
                }
            }

            // 3) 아직 끝 Tick 전이면 계속 유지
            if (songTick < endTick)
                continue;

            // 4) 끝 Tick을 지날 때까지 잘 버텼다면 → 무조건 Perfect
            hs.ReleasedEarly = false;
            FinalizeHoldByEnd(note, hs, songTick);
            _tempFinalizeIds.Add(id);
        }

        // 끝난 롱노트 정리
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

                // EndTick 전에 뗐는지 여부 기록
                hs.ReleasedEarly = songTick < endTick;

                // 여기서도 동일하게 EndTick 기준 판정
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

    // 아직 시작하지 않은 노트만 AutoMiss 처리 (롱노트 진행 중 제외)
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

            int id = note.NoteData.ID;

            if (_judgedNoteIds.Contains(id))
                continue;

            if (_activeHoldStates.ContainsKey(id))
                continue;

            if (songTick > note.NoteData.Tick + _redStarRange)
            {
                _tempMissCandidates.Add(note);
            }
        }

        for (int i = 0; i < _tempMissCandidates.Count; i++)
        {
            INote note = _tempMissCandidates[i];
            ApplyJudge(note, EJudgementType.RedStar);
        }
    }

    // 단일 노트 / 롱노트 종료 최종 통계용
    private void ApplyJudgeFinal(INote note, EJudgementType type)
    {
        if (_dictNoteJudgementCounts.TryGetValue(type, out int count))
            _dictNoteJudgementCounts[type] = count + 1;
        else
            _dictNoteJudgementCounts[type] = 1;

        FinalizeJudge(note, type);
    }

    // 단일 노트용(최종)
    private void ApplyJudge(INote note, EJudgementType type)
    {
        if (_judgedNoteIds.Contains(note.NoteData.ID))
            return;

        PlayJudgeSFX(type);
        ApplyJudgeFinal(note, type);
    }

    private void FinalizeJudge(INote note, EJudgementType type)
    {
        Debug.Log($"[{note.NoteData.ID}]노트 판정: [{type}]");
        _judgedNoteIds.Add(note.NoteData.ID);
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
}
