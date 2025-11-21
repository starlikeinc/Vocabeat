using System.Collections.Generic;
using UnityEngine;

public partial class ChartEdit
{
    private float _secPerBeat;
    private float _secPerTick;

    // Timeline 세팅 (곡 로딩할 때 호출하면 좋음)
    private void SetupTiming()
    {
        float bpm = TargetSongData.BPM;
        _secPerBeat = 60f / bpm;
        _secPerTick = _secPerBeat / 240f; // Tick당 시간 (현재 시스템 기준)
    }

    private void UpdateScanlineByMusic()
    {
        if (_bgmAudioSource == null || !_bgmAudioSource.isPlaying)
            return;

        if (_visualizer == null || _scanline == null)
            return;

        float time = _bgmAudioSource.time;
        int curTick = Mathf.FloorToInt(time / _secPerTick);

        int ticksPerPage = _visualizer.TicksPerPage;

        // 🔥 재생 시작 기준 상대 Tick
        int relativeTick = curTick - _playStartPageTick;
        if (relativeTick < 0)
            relativeTick = 0;

        // 🔥 페이지 계산은 절대 Tick 기반 유지
        int newPage = Mathf.FloorToInt((float)curTick / ticksPerPage);

        if (newPage >= _pageCount)
            _pageCount = newPage + 1;

        if (newPage != _currentPageIndex)
        {
            _currentPageIndex = Mathf.Clamp(newPage, 0, _pageCount - 1);
            RefreshPageView();
        }

        // 🔥 페이지 시작 Tick을 "재생 기준 상대 좌표계"로 변환
        int startTickOfPage = (_currentPageIndex * ticksPerPage) - _playStartPageTick;
        int localTick = relativeTick - startTickOfPage;
        localTick = Mathf.Max(localTick, 0);

        float t = Mathf.Clamp01((float)localTick / ticksPerPage);

        _scanline.SetProgress(t);
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
        if (_isPlayingFromPage) return;

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
}
