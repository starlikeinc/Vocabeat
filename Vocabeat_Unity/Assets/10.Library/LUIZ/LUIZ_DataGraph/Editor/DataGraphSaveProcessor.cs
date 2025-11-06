using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LUIZ.DataGraph.Editor
{
    public class DataGraphSaveProcessor : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            //저장 대상 GUID 목록
            var targetGuids = paths
                .Select(path => AssetDatabase.AssetPathToGUID(path))
                .Where(guid => !string.IsNullOrEmpty(guid))
                .ToHashSet();

            //현재 열려 있는 DataGraphEditorWindow들 검사
            var windows = Resources.FindObjectsOfTypeAll<DataGraphEditorWindow>();

            foreach (var graphWindow in windows)
            {
                var dataGraph = graphWindow.CurrentGraph;
                if (dataGraph == null)
                    continue;

                string graphPath = AssetDatabase.GetAssetPath(dataGraph);
                if (string.IsNullOrEmpty(graphPath))
                    continue;

                string graphGUID = AssetDatabase.AssetPathToGUID(graphPath);
                if (string.IsNullOrEmpty(graphGUID))
                    continue;

                if (targetGuids.Contains(graphGUID))
                {
                    dataGraph.NotifyBeforeSaveEditor();
                }
            }

            return paths;
        }
    }
}