using LUIZ.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(LToggleAdvanced), true)]
public class LToggleAdvancedEditor : LButtonEditor
{
    //------------------------------------------------
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.LabelField("[ LToggle ]", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultVisualState"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ToggleDataAry"));

        serializedObject.ApplyModifiedProperties();
    }
}
