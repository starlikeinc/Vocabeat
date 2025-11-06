using LUIZ.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(LScrollBar), true)]
public class LScrollBarEditor : ScrollbarEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        base.serializedObject.Update();

        EditorGUILayout.LabelField("[ LUIZ_UI ]");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UseFixedSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("FixedSize"));
        serializedObject.ApplyModifiedProperties();
    }
}
