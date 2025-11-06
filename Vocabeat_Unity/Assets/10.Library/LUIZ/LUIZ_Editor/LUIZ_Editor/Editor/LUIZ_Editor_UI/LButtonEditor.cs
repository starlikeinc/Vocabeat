using LUIZ.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(LButton), true)]
public class LButtonEditor : ButtonEditor
{
    //------------------------------------------------
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}
