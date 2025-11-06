using LUIZ.UI;
using UnityEditor;
using UnityEditor.EventSystems;
using UnityEngine;


[CanEditMultipleObjects, CustomEditor(typeof(UIEventHandler), true)]
public class UIEventHandlerEditor : EventTriggerEditor
{
    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
