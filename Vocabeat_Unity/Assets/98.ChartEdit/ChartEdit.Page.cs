using System.Collections.Generic;
using UnityEngine;

public partial class ChartEdit
{
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
}
