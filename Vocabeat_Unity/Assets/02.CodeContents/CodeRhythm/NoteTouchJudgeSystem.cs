using System;
using System.Collections.Generic;
using UnityEngine;

public class NoteTouchJudgeSystem : MonoBehaviour
{
    public event Action<INote, EJudgementType> OnJudgeResult;

    [Header("Judge Option")]
    [SerializeField] private float _touchRadius = 80f;
    [SerializeField] private int _blueStarRange = 30;
    [SerializeField] private int _whiteStarRange = 80;
    [SerializeField] private int _YellowStarRange = 120;

    private ManagerRhythm _context;

    private Dictionary<EJudgementType, int> _dictNoteJudgementCounts = new();

    private RectTransform _touchArea;
    private Camera _uiCam;
    private IReadOnlyList<INote> _listNotes;

    // ========================================
    private void Update()
    {
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
    public void InitJudgementSystem(ManagerRhythm ctx, RectTransform touchArea, Camera uiCam, IReadOnlyList<INote> listNotes)
    {
        _context = ctx;

        _touchArea = touchArea;
        _uiCam = uiCam;
        _listNotes = listNotes;
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

    private void JudgeNote(INote note)
    {
        Debug.Log($"노트 터치됨: Tick={note.NoteData.Tick}");
    }
}
