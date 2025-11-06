using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CanEditMultipleObjects, CustomEditor(typeof(LImageEmpty), false)]
public class LImageEmptyEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        base.serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(base.m_Script, new GUILayoutOption[0]);
        EditorGUI.EndDisabledGroup();

        //base.AppearanceControlsGUI(); //해당 함수 스킵(Empty 이미지이기 때문에 기존 레이아웃 필요 없음)

        EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastPass"));

        //base.RaycastControlsGUI();
        base.serializedObject.ApplyModifiedProperties();
    }
}