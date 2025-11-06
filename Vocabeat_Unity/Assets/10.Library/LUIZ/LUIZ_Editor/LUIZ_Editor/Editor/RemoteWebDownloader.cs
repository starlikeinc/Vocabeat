using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

public class RemoteWebDownloader : EditorWindow
{
    [System.Serializable]
    private class FileListWrapper
    {
        public string[] files;
    }

    private class FileDownloadStatus
    {
        public string fileName;
        public string status;
        public float progress;

        public FileDownloadStatus(string name)
        {
            fileName = name;
            status = "Pending";
            progress = 0f;
        }
    }

    private const int c_maxLogCount = 100;
    private const string c_jsonFileListName = "0.file_list.json";

    private DefaultAsset m_DownloadFolder;
    private string m_DownloadURL;
    private TextAsset m_FileListJson;

    private Vector2 m_vecScrollPos;
    private Vector2 m_vecLogScrollPos;
    private bool m_isSyncing = false;
    private float m_totalProgress = 0f;

    private List<FileDownloadStatus> m_listfileStatus = new List<FileDownloadStatus>();
    private List<string> m_listLogs = new List<string>();

    [MenuItem("LUIZ/RemoteWebDownloader")]
    public static void OpenWindow()
    {
        GetWindow<RemoteWebDownloader>("RemoteWebDownloader");
    }
    private void OnGUI()
    {
        #region ========개요========
        GUIStyle wrappedLabelStyle = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true
        };

        GUILayout.Label("======== 로컬 다운로드 에디터 ver1.1 ========         MadeBy : LUIZ", EditorStyles.boldLabel);
        EditorGUILayout.Space(20);
        
        GUILayout.Label("[개요]", EditorStyles.boldLabel);
        GUILayout.Label(
            "Git 사용 중 용량이 너무 큰 파일들은 굳이 Git으로 올리지 않고 " +
            "그냥 ignore 처리하고 직접 드라이브나 이런데서 다운받는 게 나을 수 있음",
            wrappedLabelStyle
        );

        EditorGUILayout.Space(20);
        GUILayout.Label("프로젝트 내부 폴더, 웹 주소, Json(다운로드 파일 목록)을 통해 웹 다운로드", wrappedLabelStyle);
        
        EditorGUILayout.Space(20);
        GUILayout.Label("Json이 NULL일 경우 : 웹 주소가 파일 주소를 직접 참조중이라 간주하고 해당 주소만 다운로드", wrappedLabelStyle);
        GUILayout.Label("Json NULL이 아닌 경우 : 웹 주소를 폴더 주소로 간주해 json 안의 파일들과 주소와 조합 후 전부 다운로드", wrappedLabelStyle);
        
        EditorGUILayout.Space(20);
        GUILayout.Label("[주의!!!]", EditorStyles.boldLabel);
        GUILayout.Label("이미 있는 파일의 경우 무조건 다운 받지 않고 업데이트 된건지 확인을 용량으로 함. 진짜 우연의 일치로 파일 내용이 바뀌었는데 용량이"
            +"완전히 동일하면 다운로드를 안 받을 수도 있다..."
            , wrappedLabelStyle);
        EditorGUILayout.Space(20);
        #endregion
        
        m_DownloadFolder =
            (DefaultAsset)EditorGUILayout.ObjectField("DownloadFolder", m_DownloadFolder, typeof(DefaultAsset), false);
        m_DownloadURL = EditorGUILayout.TextField("Download URL", m_DownloadURL);
        m_FileListJson =
            (TextAsset)EditorGUILayout.ObjectField("File List JSON (Optional)", m_FileListJson, typeof(TextAsset), false);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (!m_isSyncing && GUILayout.Button("Start Download"))
        {
            PrivStartDownload();
        }
        if (!m_isSyncing && GUILayout.Button("Add GitIgnore In Folder"))
        {
            PrivMakeGitIgnore();
        }
        if (!m_isSyncing && GUILayout.Button("Add Current File List Json"))
        {
            MakeJsonFileList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (m_isSyncing)
        {
            EditorGUILayout.LabelField("Syncing...", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Progress: {m_totalProgress * 100f:0.0}%");

            m_vecScrollPos = EditorGUILayout.BeginScrollView(m_vecScrollPos, GUILayout.Height(200));
            foreach (var item in m_listfileStatus)
            {
                EditorGUILayout.LabelField($"{item.fileName} - {item.status} - {item.progress * 100f:0.0}%");
            }
            EditorGUILayout.EndScrollView();

            //TODO : 다운로드 중간에 취소하기
            /*if (GUILayout.Button("Cancel"))
            {
                PrivAddLog("Cancel not implemented yet.");
            }*/
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Logs:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Logs", GUILayout.Width(100)))
        {
            m_listLogs.Clear();
        }
        EditorGUILayout.EndHorizontal();

        m_vecLogScrollPos = EditorGUILayout.BeginScrollView(m_vecLogScrollPos, GUILayout.Height(400));
        GUIStyle helpBoxRich = new GUIStyle(EditorStyles.helpBox)
        {
            richText = true,
            wordWrap = true,
            padding = new RectOffset(10, 10, 8, 8)
        };
        int startIdx = Mathf.Max(0, m_listLogs.Count - c_maxLogCount);
        for (int i = startIdx; i < m_listLogs.Count; i++)
        {
            GUILayout.Label(m_listLogs[i], helpBoxRich);
        }
        EditorGUILayout.EndScrollView();
    }

    private void PrivAddLog(string msg)
    {
        int index = m_listLogs.Count;
        m_listLogs.Add($"{index} : {msg}");
        Repaint();
    }

    private bool PrivIsValidDownloadFolder(out string folderPath)
    {
        folderPath = null;

        if (m_DownloadFolder == null)
        {
            PrivAddLog("<color=red>DownloadFolder is not assigned!</color>");
            return false;
        }

        folderPath = AssetDatabase.GetAssetPath(m_DownloadFolder);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            PrivAddLog($"<color=red>Selected asset is not a folder: {folderPath}</color>");
            return false;
        }

        return true;
    }

    private async void PrivStartDownload()
    {
        PrivAddLog("Start Download...");
        m_listfileStatus.Clear();
        m_totalProgress = 0f;

        if (!PrivIsValidDownloadFolder(out string folderPath)) return;
        if (string.IsNullOrEmpty(m_DownloadURL))
        {
            PrivAddLog("<color=red>Web URL is empty.</color>");
            return;
        }

        m_isSyncing = true;
        Repaint();

        try
        {
            if (m_FileListJson == null)
            {
                string fileName = Path.GetFileName(m_DownloadURL);
                m_listfileStatus.Add(new FileDownloadStatus(fileName));
                await PrivTaskDownloadFileWithSizeCheck(m_DownloadURL, Path.Combine(folderPath, fileName), m_listfileStatus[0]);
            }
            else
            {
                string jsonText = m_FileListJson.text;
                FileListWrapper wrapper;
                try { wrapper = JsonUtility.FromJson<FileListWrapper>(jsonText); }
                catch (Exception e)
                {
                    PrivAddLog($"<color=red>Failed to parse JSON: {e.Message}</color>");
                    m_isSyncing = false; Repaint(); return;
                }

                if (wrapper == null || wrapper.files == null || wrapper.files.Length == 0)
                {
                    PrivAddLog("<color=red>JSON file list is empty or invalid format.</color>");
                    m_isSyncing = false; Repaint(); return;
                }

                m_listfileStatus.Clear();
                foreach (var f in wrapper.files)
                {
                    if (f == c_jsonFileListName) continue;
                    m_listfileStatus.Add(new FileDownloadStatus(f));
                }

                string baseUrl = m_DownloadURL.TrimEnd('/');
                int completedCount = 0;
                int totalCount = m_listfileStatus.Count;

                for (int i = 0; i < totalCount; i++)
                {
                    var item = m_listfileStatus[i];
                    string fileUrl = $"{baseUrl}/{Uri.EscapeUriString(item.fileName)}";
                    string localPath = Path.Combine(folderPath, item.fileName);
                    await PrivTaskDownloadFileWithSizeCheck(fileUrl, localPath, item);

                    completedCount++;
                    m_totalProgress = (float)completedCount / totalCount;
                    Repaint();
                }
            }

            PrivAddLog("<b><color=green>Download complete!</color></b>");
        }
        catch (Exception e)
        {
            PrivAddLog($"<color=red>Error during Download: {e.Message}</color>");
        }
        finally
        {
            m_isSyncing = false;
            AssetDatabase.Refresh();
            Repaint();
        }
    }

    private async Task PrivTaskDownloadFileWithSizeCheck(string url, string savePath, FileDownloadStatus status)
    {
        status.status = "Checking size";
        status.progress = 0f;
        Repaint();

        long serverFileSize = -1;
        using (UnityWebRequest headRequest = UnityWebRequest.Head(url))
        {
            var op = headRequest.SendWebRequest();
            while (!op.isDone) await Task.Delay(100);
            if (headRequest.result != UnityWebRequest.Result.Success)
            {
                PrivAddLog($"<color=red>Failed to get HEAD info for {url}: {headRequest.error}</color>");
                status.status = "HEAD request error";
                return;
            }
            string contentLengthStr = headRequest.GetResponseHeader("Content-Length");
            if (!long.TryParse(contentLengthStr, out serverFileSize))
            {
                PrivAddLog($"<color=red>Content-Length header missing or invalid for {url}</color>");
                status.status = "No Content-Length";
                return;
            }
        }

        long localFileSize = File.Exists(savePath) ? new FileInfo(savePath).Length : -1;
        if (localFileSize == serverFileSize)
        {
            status.status = "File size match, skipping";
            status.progress = 1f;
            PrivAddLog($"Skipped (size match): {status.fileName}");
            Repaint();
            return;
        }

        status.status = "Downloading";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                status.progress = www.downloadProgress;
                Repaint();
                await Task.Delay(100);
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                status.status = $"Error: {www.error}";
                PrivAddLog($"<color=red>Failed to download {url}: {www.error}</color>");
                return;
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                File.WriteAllBytes(savePath, www.downloadHandler.data);
                status.status = "Completed";
                status.progress = 1f;
                PrivAddLog($"Downloaded: {status.fileName}");
            }
            catch (Exception e)
            {
                status.status = $"Write Error: {e.Message}";
                PrivAddLog($"<color=red>Failed to save file {savePath}: {e.Message}</color>");
            }
            Repaint();
        }
    }

    private void PrivMakeGitIgnore()
    {
        if (!PrivIsValidDownloadFolder(out string folderPath)) return;

        string gitIgnorePath = Path.Combine(folderPath, ".gitignore");
        if (File.Exists(gitIgnorePath))
        {
            PrivAddLog(".gitignore already exists.");
            return;
        }

        try
        {
            string content =
                "# Auto-generated\n" +
                "*\n" +
                "!.gitignore\n" +
                "!" + c_jsonFileListName + "\n";  // 상수 사용

            File.WriteAllText(gitIgnorePath, content);
            PrivAddLog(".gitignore created.");
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            PrivAddLog($"<color=red>Failed to create .gitignore: {e.Message}</color>");
        }
    }

    private void MakeJsonFileList()
    {
        if (!PrivIsValidDownloadFolder(out string folderPath)) return;

        try
        {
            string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            List<string> relativeFiles = new List<string>();
            foreach (var f in files)
            {                
                if(File.Exists(f) == false) continue;
                if(f.EndsWith(".meta")) continue;
                
                if (Path.GetFileName(f) != c_jsonFileListName && Path.GetFileName(f) != ".gitignore")
                {
                    string relativePath = f.Substring(folderPath.Length).TrimStart('\\', '/');
                    relativeFiles.Add(relativePath.Replace("\\", "/"));
                }
            }

            FileListWrapper wrapper = new FileListWrapper { files = relativeFiles.ToArray() };
            string json = JsonUtility.ToJson(wrapper, true);
            string jsonPath = Path.Combine(folderPath, c_jsonFileListName);

            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
                PrivAddLog($"Existing {c_jsonFileListName} deleted.");
            }

            File.WriteAllText(jsonPath, json);
            PrivAddLog("JSON file list created: " + jsonPath);
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            PrivAddLog($"<color=red>Failed to create JSON file list: {e.Message}</color>");
        }
    }
}