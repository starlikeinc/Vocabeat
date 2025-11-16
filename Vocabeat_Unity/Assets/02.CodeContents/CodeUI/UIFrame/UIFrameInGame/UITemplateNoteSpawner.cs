using System.Collections.Generic;
using LUIZ.UI;
using UnityEngine;

public class UITemplateNoteSpawner : UITemplateBase
{
    [Header("노트가 보여질 RectTrs")] // WidgetScanline으로 두면 됨.
    [SerializeField] private RectTransform _spawnRectTrs;

    [Header("Note Spawner")]    
    [SerializeField] private int _appearOffsetTicks = 480;  // 자신의 Tick 기준 몇 Tick 전에 등장할지 - 이건 나중에 따로 빼야됨.    
    [SerializeField] private int _disappearOffsetTick = 60; // 자신의 Tick 기준 몇 Tick 이상 지날 동안 터치 없으면 비활성화 할 건지. (Miss처리는 JudgeSystem에서 함)

    private readonly List<UIItemNote> _activeNotes = new List<UIItemNote>();
    private Queue<Note> _pendingNotes;
    private RhythmTimeline _timeline;
    private int _preSongTicks;    

    public IReadOnlyList<UIItemNote> ActiveNotes => _activeNotes;

    // ========================================
    /// <summary>
    /// 노트 시트 + 타임라인을 바인딩하고, 내부 상태 초기화
    /// </summary>
    public void Setup(List<Note> listNote, RhythmTimeline timeline)
    {
        _timeline = timeline;
        _preSongTicks = (timeline != null) ? timeline.PreSongTicks : 0;        

        ClearAllNotes();

        if (listNote == null)
        {
            _pendingNotes = null;
            return;
        }
        
        var notes = new List<Note>(listNote);
        notes.Sort((a, b) => a.Tick.CompareTo(b.Tick));
        
        _pendingNotes = new Queue<Note>(notes);
    }

    /// <summary>
    /// 매 프레임 Tick 기준으로 스폰/삭제 처리    
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

    // ========================================
    private void SpawnNotes(int songTick)
    {
        if (_pendingNotes == null) return;

        // 등장해야 할 노트들 다 뽑기 
        while (_pendingNotes.Count > 0)
        {
            var next = _pendingNotes.Peek();
            int appearTick = next.Tick - _appearOffsetTicks;
            
            if (appearTick < 0)
                appearTick = 0;

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
            
            if (songTick >= item.NoteData.Tick + _disappearOffsetTick)
            {
                DoUITemplateReturn(item);
                _activeNotes.RemoveAt(i);
            }
        }
    }

    private UIItemNote GetNoteItem(Note data)
    {
        UIItemNote item = DoTemplateRequestItem<UIItemNote>(transform);
        item.DoUINoteVisualSetting(data, _spawnRectTrs);
        return item;
    }

    private void ClearAllNotes()
    {
        DoUITemplateReturnAll();
        _activeNotes.Clear();
    }
}
