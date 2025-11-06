using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

namespace LUIZ.DataGraph.Editor
{
    [CustomEditor(typeof(DataGraph))]
    public class DataGraphAssetEditor : UnityEditor.Editor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int index)
        {
            object asset = EditorUtility.InstanceIDToObject(instanceId);
            if (asset is DataGraph graph)
            {
                DataGraphEditorWindow.Open(graph);
                return true;
            }
            return false;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Edit"))
            {
                DataGraphEditorWindow.Open((DataGraph)target);
            }
        }
    }
}
