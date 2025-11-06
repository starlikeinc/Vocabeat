using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using LUIZ.UI;

[CanEditMultipleObjects, CustomEditor(typeof(LToggleRadio), true)]
public class LToggleRadioEditor : LToggleEditor
{
    private bool m_isShowRadio = false;

    //------------------------------------------------
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        m_isShowRadio = EditorGUILayout.Foldout(m_isShowRadio, "[ LToggleRadio ]");

        if (m_isShowRadio == true)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GroupName"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
