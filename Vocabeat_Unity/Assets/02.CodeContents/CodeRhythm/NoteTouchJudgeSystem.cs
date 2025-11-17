using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteTouchJudgeSystem : MonoBehaviour
{
    public event Action<INote, EJudgementType> OnJudgeResult;

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

    private bool _isInit = false;

    // ========================================
    private void Update()
    {
        if (_isInit)
            AutoMissUpdate();

#if UNITY_EDITOR
        PCInput();
#elif UNITY_ANDROID || UNITY_IOS
        MobileInput();
#endif
    }

    private void PCInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryTouchNote(Input.mousePosition);
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
            if (touch.phase == TouchPhase.Began)
                TryTouchNote(touch.position);
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
    }

    // ========================================
    private void TryTouchNote(Vector2 screenPos)
    {
        if (_context == null || !_context.IsPlaying)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_touchArea, screenPos, _uiCam, out Vector2 localPos);

        if (_listNotes == null || _listNotes.Count == 0)
            return;

        INote bestNote = null;
        float bestDist = float.MaxValue;

        foreach (var note in _listNotes)
        {
            float dist = Vector2.Distance(localPos, note.RectTrs.anchoredPosition);

            if (dist < bestDist)
            {
                bestDist = dist;
                bestNote = note;
            }
        }

        if (bestNote != null && bestDist < _touchRadius)
            JudgeNote(bestNote);
    }

    private void JudgeNote(INote note) // 기획서 보니까 RedStar == Miss임 해당 부분 수정예정
    {
        Debug.Log($"노트 터치됨: Tick={note.NoteData.Tick}");

        if (_judgedNoteIds.Contains(note.NoteData.ID))
            return;

        if (_context == null || _context.RTimeline == null)
            return;

        var timeline = _context.RTimeline;
        
        int songTick = timeline.CurTick - timeline.PreSongTicks;        

        // 곡이 아직 시작 안 했으면 (프리롤 구간) 판정 무시
        if (songTick < 0)
            return;

        int noteTick = note.NoteData.Tick;
        int diff = Mathf.Abs(noteTick - songTick);

        EJudgementType judgeType;

        if (diff <= _blueStarRange)
            judgeType = EJudgementType.BlueStar;
        else if (diff <= _whiteStarRange)
            judgeType = EJudgementType.WhiteStar;
        else if (diff <= _yellowStarRange)
            judgeType = EJudgementType.YellowStar;
        else 
            judgeType = EJudgementType.RedStar;
        
        ApplyJudge(note, EJudgementType.RedStar);

        FinalizeJudge(note, judgeType);        
    }

    private void AutoMissUpdate()
    {
        if (!_context.IsPlaying)
            return;        

        if(_listNotes == null || _listNotes.Count == 0) 
            return;

        _tempMissCandidates.Clear();

        var timeline = _context.RTimeline;
        int songTick = timeline.CurTick - timeline.PreSongTicks;

        for (int i = 0; i < _listNotes.Count; i++)
        {
            var note = _listNotes[i];
            if (note == null)
                continue;

            if (_judgedNoteIds.Contains(note.NoteData.ID))
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

    private void ApplyJudge(INote note, EJudgementType type)
    {
        if (_dictNoteJudgementCounts.TryGetValue(type, out int count))
            _dictNoteJudgementCounts[type] = count + 1;
        else
            _dictNoteJudgementCounts[type] = 1;

        PlayJudgeSFX(type);

        FinalizeJudge(note, type);
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
