using LUIZ.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(LButtonLongPress), true)]
public class LButtonLongPressEditor : LButtonEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.LabelField("[ LButtonLongPress ]", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("LongPressStartOffset"));
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLongPressEvent"));

        serializedObject.ApplyModifiedProperties();
    }
}
