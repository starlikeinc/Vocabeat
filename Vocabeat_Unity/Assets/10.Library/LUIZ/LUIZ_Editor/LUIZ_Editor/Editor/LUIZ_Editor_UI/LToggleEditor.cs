using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using LUIZ.UI;

[CanEditMultipleObjects, CustomEditor(typeof(LToggle), true)]
public class LToggleEditor : LButtonEditor
{
    private static bool c_isShowToggle = false;

    //------------------------------------------------
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        c_isShowToggle = EditorGUILayout.Foldout(c_isShowToggle, "[ LToggle ]");

        if (c_isShowToggle == true)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultOn"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("PivotOn"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PivotOff"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ToggleOnEvent"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ToggleOffEvent"));
        }
        EditorGUILayout.Space(5);
        serializedObject.ApplyModifiedProperties();
    }
   
}
