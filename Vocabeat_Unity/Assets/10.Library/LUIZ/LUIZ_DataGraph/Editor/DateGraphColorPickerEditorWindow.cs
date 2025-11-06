using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace LUIZ.DataGraph.Editor
{
    public class DateGraphColorPickerEditorWindow : EditorWindow
    {
        private System.Action<Color> m_onColorPicked;
        private Color m_initialColor;

        private Action<Color> m_onColorSelected;
        private Color m_currentColor;

        public static void Show(GraphView graphView, Color currentColor, Action<Color> onColorSelected)
        {
            var window = CreateInstance<DateGraphColorPickerEditorWindow>();
            window.m_currentColor = currentColor;
            window.m_onColorSelected = onColorSelected;
            window.titleContent = new GUIContent("Select Group Color");

            Vector2 size = new Vector2(220, 100);

            // 그래프 뷰의 세계좌표 기준에서 왼쪽 아래
            Rect layoutRect = graphView.layout;

            // 그래프 뷰 내 좌측 하단 기준 좌표
            Vector2 bottomLeftInGraph = new Vector2(10, layoutRect.height - size.y - 10);

            // 로컬 좌표 -> 스크린 좌표
            Vector2 screenPos = graphView.LocalToWorld(bottomLeftInGraph);
            screenPos = GUIUtility.GUIToScreenPoint(screenPos);

            window.position = new Rect(screenPos, size);
            window.ShowPopup();
        }

        private void OnGUI()
        {
            m_currentColor = EditorGUILayout.ColorField("Color", m_currentColor);
            if (GUILayout.Button("Apply"))
            {
                m_onColorSelected?.Invoke(m_currentColor);
                Close();
            }
        }
    }
}