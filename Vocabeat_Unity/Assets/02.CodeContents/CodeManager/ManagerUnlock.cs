using LUIZ;
using System.Collections.Generic;
using UnityEngine;

public class ManagerUnlock : SingletonBase<ManagerUnlock>, IManagerInstance
{
    private const string PREF_KEY_SONG_UNLOCKS = "SongUnlocks";

    private readonly HashSet<int> _unlockedSongIds = new();
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        Load();
    }

    public bool IsInitialized()
    {        
        return Instance != null;
    }
    
    // ---------------------------------------
    // Public API
    // ---------------------------------------

    public bool IsUnlocked(SongDataSO song)
    {
        // 1) 조건상 항상 오픈이라면 true
        if (song.UnlockCondition == null ||
            song.UnlockCondition.UnlockType == ESongUnlockType.AlwaysOpen ||
            song.UnlockCondition.UnlockType == ESongUnlockType.ByDefault)
        {
            return true;
        }

        // 2) 세이브 데이터상 언락되어 있으면 true
        int songId = GetSongId(song);
        return _unlockedSongIds.Contains(songId);
    }

    public void Unlock(SongDataSO song, bool saveImmediately = true)
    {
        int songId = GetSongId(song);

        if (_unlockedSongIds.Add(songId))
        {
            if (saveImmediately)
                Save();
        }
    }

    // 필요하면 직접 ID로도 체크/언락 가능하게
    public bool IsUnlocked(int songId) => _unlockedSongIds.Contains(songId);

    public void Unlock(int songId, bool saveImmediately = true)
    {
        if (_unlockedSongIds.Add(songId) && saveImmediately)
            Save();
    }

    public void UnlockAllSongs(bool saveImmediately = true)
    {
        // SongDatabase에서 모든 SongDataSO 리스트 가져온다고 가정
        var allSongs = ManagerRhythm.Instance.SongDB.Songs;

        foreach (var song in allSongs)
        {
            int songId = GetSongId(song);
            _unlockedSongIds.Add(songId);
        }

        if (saveImmediately)
            Save();

#if UNITY_EDITOR
        Debug.Log($"[ManagerUnlock] All songs unlocked. Total: {_unlockedSongIds.Count}");
#endif
    }

    // ---------------------------------------
    // 내부: SongId 추출
    // ---------------------------------------

    private int GetSongId(SongDataSO song) => song.SongId;

    // ---------------------------------------
    // 저장 / 로드
    // ---------------------------------------

    private void Save()
    {
        // 아무 곡도 언락 안 되어 있으면 빈 문자열로 저장
        if (_unlockedSongIds.Count == 0)
        {
            SecurePrefs.SetString(PREF_KEY_SONG_UNLOCKS, string.Empty);
        }
        else
        {
            // HashSet이라 순서는 상관없음
            // "1,3,5,10" 같은 형식으로 저장
            var list = new List<int>(_unlockedSongIds);
            string payload = string.Join(",", list);
            SecurePrefs.SetString(PREF_KEY_SONG_UNLOCKS, payload);
        }

        SecurePrefs.Save();
#if UNITY_EDITOR
        Debug.Log($"[ManagerUnlock] Saved unlocks: {_unlockedSongIds.Count} songs");
#endif
    }

    public void Load()
    {
        _unlockedSongIds.Clear();

        // defaultValue를 null로 주면
        // - 키 없음
        // - 복호화 실패 / 데이터 깨짐
        // 둘 다 null or ""로 떨어지게 할 수 있음
        string payload = SecurePrefs.GetString(PREF_KEY_SONG_UNLOCKS, null);

        if (string.IsNullOrEmpty(payload))
        {
#if UNITY_EDITOR
            Debug.Log("[ManagerUnlock] No unlock data found. Starting fresh.");
#endif
            return;
        }

        string[] tokens = payload.Split(',');
        int loadedCount = 0;

        foreach (string token in tokens)
        {
            if (int.TryParse(token, out int songId))
            {
                _unlockedSongIds.Add(songId);
                loadedCount++;
            }
        }

#if UNITY_EDITOR
        Debug.Log($"[ManagerUnlock] Loaded unlocks: {loadedCount} songs");
#endif
    }

    // (옵션) 디버그용 전체 초기화
    [ContextMenu("Debug: Clear All Unlocks")]
    private void DebugClearAll()
    {
        _unlockedSongIds.Clear();
        SecurePrefs.SetString(PREF_KEY_SONG_UNLOCKS, string.Empty);
        SecurePrefs.Save();
        Debug.Log("[ManagerUnlock] All unlocks cleared.");
    }
}
