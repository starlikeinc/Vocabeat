using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectNameChanger : EditorWindow
{
    private string[] m_aryTabNames = { "Append", "Replace" };
    private int m_currentTabIndex = 0;

    private GameObject m_rootObject = null;

    private string m_textPrefix = string.Empty;
    private string m_textSuffix = string.Empty;

    private string m_textFrom = string.Empty;
    private string m_textTo = string.Empty;

    //--------------------------------------------------------------

    [MenuItem("LUIZ/ObjectNameChanger")]
    static void Open()
    {
        EditorWindow.GetWindow<ObjectNameChanger>();
    }

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            m_currentTabIndex = GUILayout.Toolbar(m_currentTabIndex, m_aryTabNames, new GUIStyle(EditorStyles.toolbarButton), GUI.ToolbarButtonSize.FitToContents);
        }

        if (m_currentTabIndex == 0)
        {
            GUILayout.Space(20);
            GUILayout.Label("Append string front or back", EditorStyles.boldLabel);
            GUILayout.Space(20);

            EditorGUILayout.LabelField("RootObject");
            m_rootObject = EditorGUILayout.ObjectField(m_rootObject, typeof(GameObject), true) as GameObject;

            GUILayout.Space(20);

            m_textPrefix = EditorGUILayout.TextField("Prefix Text", m_textPrefix);
            m_textSuffix = EditorGUILayout.TextField("Suffix Text", m_textSuffix);

            GUILayout.Space(20);

            if (GUILayout.Button("Append"))
            {
                OnBtnAppendText();
            }
        }
        else if (m_currentTabIndex == 1)
        {
            GUILayout.Space(20);
            GUILayout.Label("Replace string", EditorStyles.boldLabel);
            GUILayout.Space(20);

            EditorGUILayout.LabelField("RootObject");
            m_rootObject = EditorGUILayout.ObjectField(m_rootObject, typeof(GameObject), true) as GameObject;

            GUILayout.Space(20);

            m_textFrom = EditorGUILayout.TextField("From", m_textFrom);
            m_textTo = EditorGUILayout.TextField("To", m_textTo);

            GUILayout.Space(20);

            if (GUILayout.Button("Replace"))
            {
                OnBtnReplaceText();
            }
        }
    }

    void OnBtnAppendText()
    {
        if (m_rootObject != null)
        {
            if (m_textPrefix.Length > 0 || m_textSuffix.Length > 0)
            {
                string unDoName = string.Format("{0}{1}{2}", m_textPrefix, m_rootObject.name, m_textSuffix);
                Undo.RegisterFullObjectHierarchyUndo(m_rootObject, unDoName);

                Transform[] aryAllChild = m_rootObject.transform.GetComponentsInChildren<Transform>();
                foreach (var child in aryAllChild)
                {
                    child.name = string.Format("{0}{1}{2}", m_textPrefix, child.name, m_textSuffix);
                }

                m_rootObject = EditorGUILayout.ObjectField(null, typeof(GameObject), true) as GameObject;
                m_textPrefix = EditorGUILayout.TextField("Prefix Text", "");
                m_textSuffix = EditorGUILayout.TextField("Suffix Text", "");
            }
        }
    }

    void OnBtnReplaceText()
    {
        if (m_rootObject != null)
        {
            if (m_textFrom.Length > 0)
            {
                string unDoName = string.Format("{0}", m_rootObject.name);
                Undo.RegisterFullObjectHierarchyUndo(m_rootObject, unDoName);

                Transform[] aryAllChild = m_rootObject.transform.GetComponentsInChildren<Transform>();
                foreach (Transform child in aryAllChild)
                {
                    child.name = child.name.Replace(m_textFrom, m_textTo);
                }

                m_rootObject = EditorGUILayout.ObjectField(null, typeof(GameObject), true) as GameObject;
                m_textFrom = EditorGUILayout.TextField("From", "");
                m_textTo = EditorGUILayout.TextField("To", "");
            }
        }
    }
}
