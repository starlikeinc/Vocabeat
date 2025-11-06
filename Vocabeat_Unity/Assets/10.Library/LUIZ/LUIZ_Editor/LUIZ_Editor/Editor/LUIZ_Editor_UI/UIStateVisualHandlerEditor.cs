using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LUIZ.UI;
using UnityEditor.SceneManagement;


[CustomEditor(typeof(UIStateVisualHandler))]
public class UIStateVisualHandlerEditor : Editor
{
    private List<string> m_listStateNames = new List<string>();
    private int m_selectedStateIndex = 0;

    //-------------------------------------------------------------------------
    public void OnEnable()
    {
        PrivRefreshStateNames();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("====== UIStateVisualHandler ver1.1 ======    MadeBy : LUIZ", EditorStyles.boldLabel);

        EditorGUILayout.Space(10);
        GUILayout.Label("UI 작업자와 프로그래머의 협업을 원할하기 위해 제작된 핸들러", EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(10);
        GUILayout.Label("[ 작업 개요 ]", EditorStyles.boldLabel);
        GUILayout.Label("1. UI작업자는 해당 스크립트를 제작 중인 프리팹이나 오브젝트의 최상단에 추가한다.", EditorStyles.wordWrappedLabel);
        GUILayout.Label("2. UI작업자는 해당 스크립트의 인스펙터를 통해 State를 재량껏 추가하며, 각 State에 맞는 유아이 상태를 세팅한다.", EditorStyles.wordWrappedLabel);
        GUILayout.Label("3. UI작업자는 작업이 완료 된 후 프로그래머에게 보고한다. 이후 프로그래머는 해당 State들의 이름이나 인덱스를 통해 UI상태를 제어한다.", EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(10);
        GUILayout.Label("[ 주의! ]", EditorStyles.boldLabel);
        GUILayout.Label("프로그래머에게 보고한 이후 State의 이름이나 순서가 바뀐다면 반드시 해당 프로그래머에게 보고 할 것.", EditorStyles.wordWrappedLabel);

        EditorGUILayout.Space(10);
        Rect horRect = EditorGUILayout.BeginHorizontal();

        m_selectedStateIndex = EditorGUI.Popup(new Rect(20, horRect.y, 100, 22), m_selectedStateIndex, m_listStateNames.ToArray());

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Test State",GUILayout.Width(250), GUILayout.Height(20)))
        {
            PrivPreviewState();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);


        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if(EditorGUI.EndChangeCheck() == true)
        {
            PrivRefreshStateNames();
        }
    }

    //-------------------------------------------------------------------------
    private void PrivPreviewState()
    {
        string stateName = m_listStateNames[m_selectedStateIndex];

        //UnityEvent가 에디터에서 작동하도록 세팅
        SerializedProperty property = serializedObject.FindProperty("StateDataAry");
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty propertyData = property.GetArrayElementAtIndex(i);
            string name = propertyData.FindPropertyRelative("StateName").stringValue;

            if (name == stateName)
            {
                SerializedProperty persistentCalls = propertyData.FindPropertyRelative("Events.m_PersistentCalls.m_Calls");

                for (int k = 0; k < persistentCalls.arraySize; k++)
                    persistentCalls.GetArrayElementAtIndex(k).FindPropertyRelative("m_CallState").intValue = (int)UnityEngine.Events.UnityEventCallState.EditorAndRuntime;

                serializedObject.ApplyModifiedProperties();

                break;
            }
        }

        UIStateVisualHandler handler = (UIStateVisualHandler)target;
        if (handler.DoTryInvokeState(stateName) == true)
        {
            EditorSceneManager.MarkSceneDirty(handler.gameObject.scene);
        }
    }

    private void PrivRefreshStateNames()
    {
        m_listStateNames.Clear();

        PrivAddPropertyStateName(serializedObject.FindProperty("StateDataAry"));
    }

    private void PrivAddPropertyStateName(SerializedProperty property)
    {
        for (int i = 0; i < property.arraySize; i++)
        {
            string name = property.GetArrayElementAtIndex(i).FindPropertyRelative("StateName").stringValue;
            if (m_listStateNames.Contains(name) == false)
            {
                m_listStateNames.Add(name);
            }
        }
    }
}
