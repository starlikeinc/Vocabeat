using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SongDB_", menuName = "SongDB/New SongDB")]
public class SongDatabaseSO : ScriptableObject
{
    [SerializeField] private List<SongDataSO> _songs = new(); // Inspector에 할당할 리스트
    private Dictionary<string, SongDataSO> _songDictByName;   // 런타임 빠른 접근 용

    public IReadOnlyList<SongDataSO> Songs => _songs;

    /// <summary>런타임 접근용 캐시. 없으면 빌드.</summary>
    public Dictionary<string, SongDataSO> SongDictByName
    {
        get
        {
            if (_songDictByName == null)
                BuildSongDict();
            return _songDictByName;
        }
    }

    // ========================================
    public void BuildSongDict()
    {
        _songDictByName = new();

        foreach (var song in _songs)
        {
            if (song == null) continue;
            if (!_songDictByName.ContainsKey(song.SongName))
                _songDictByName.Add(song.SongName, song);
        }
    }

    // ========================================
    /// 노래 이름으로 곡 찾기
    public SongDataSO GetSong(string songName)
    {
        if (SongDictByName.TryGetValue(songName, out var data))
            return data;

        Debug.LogWarning($"[SongDB] Song '{songName}'을 찾을 수 없음.");
        return null;
    }

    /// 인덱스 기반 접근 (UI용)
    public SongDataSO GetSong(int index)
    {
        if (index < 0 || index >= _songs.Count)
            return null;
        return _songs[index];
    }
}