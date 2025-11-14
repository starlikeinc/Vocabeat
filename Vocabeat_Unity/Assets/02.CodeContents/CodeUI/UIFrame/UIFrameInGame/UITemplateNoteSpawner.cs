using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;

public class UITemplateNoteSpawner : UITemplateBase
{
    [Header("Note Spawner")]
    [SerializeField] private UIItemNote _notePrefab;
    [SerializeField] private RectTransform _noteParent; // 스캔라인 위젯으로 두면 됨.
    [SerializeField] private int _appearOffsetTicks = 480; // 자신의 Tick 기준 몇 Tick 전에 등장할지 - 이건 나중에 따로 빼야됨.

    private readonly List<UIItemNote> _activeNotes = new List<UIItemNote>();
    private Queue<Note> _pendingNotes;
    private RhythmTimeline _timeline;
    private int _preSongTicks;
    private RectTransform _parentRect;

    /// <summary>
    /// 노트 시트 + 타임라인을 바인딩하고, 내부 상태 초기화
    /// </summary>
    public void Setup(NoteDataSheet sheet, RhythmTimeline timeline)
    {
        _timeline = timeline;
        _preSongTicks = (timeline != null) ? timeline.PreSongTicks : 0;
        _parentRect = _noteParent != null ? _noteParent : (RectTransform)transform;

        ClearAllNotes();

        if (sheet == null)
        {
            _pendingNotes = null;
            return;
        }
        
        var notes = new List<Note>(sheet.NoteData);
        notes.Sort((a, b) => a.Tick.CompareTo(b.Tick));

        _pendingNotes = new Queue<Note>(notes);
    }

    /// <summary>
    /// 매 프레임 Tick 기준으로 스폰/삭제 처리
    /// UIFrameInGame.Update 에서 호출해 줄 것
    /// </summary>
    public void TickUpdate()
    {
        if (_timeline == null || _pendingNotes == null)
            return;

        int timelineTick = _timeline.CurTick;
        // 곡 기준 Tick(0 = 곡 시작 시점)
        int songTick = timelineTick - _preSongTicks;

        SpawnNotes(songTick);
        DespawnNotes(songTick);
    }

    public void ResetSpawner()
    {
        ClearAllNotes();
        _pendingNotes = null;
        _timeline = null;
    }

    // -----------------------------------------------------

    private void SpawnNotes(int songTick)
    {
        if (_pendingNotes == null) return;

        // 등장해야 할 노트들 다 뽑기 
        while (_pendingNotes.Count > 0)
        {
            var next = _pendingNotes.Peek();
            int appearTick = next.Tick - _appearOffsetTicks;

            // 아직 등장할 시간이 안 됐으면 멈춤
            if (songTick < appearTick)
                break;

            // 등장 시간 지났으니 실제 생성
            _pendingNotes.Dequeue();

            var item = GetNoteItem(next);
            _activeNotes.Add(item);
        }
    }

    private void DespawnNotes(int songTick)
    {
        for (int i = _activeNotes.Count - 1; i >= 0; i--)
        {
            var item = _activeNotes[i];
            if (item == null)
            {
                _activeNotes.RemoveAt(i);
                continue;
            }

            // 자신의 Tick 이 된 순간 바로 제거
            if (songTick >= item.Data.Tick)
            {
                DoUITemplateReturn(item);
                _activeNotes.RemoveAt(i);
            }
        }
    }

    private UIItemNote GetNoteItem(Note data) // 수정 중
    {
        UIItemNote item = DoTemplateRequestItem<UIItemNote>(transform);
        item.Init(data, _parentRect);
        return item;
    }

    private void ClearAllNotes()
    {
        DoUITemplateReturnAll();
        _activeNotes.Clear();
    }
}
