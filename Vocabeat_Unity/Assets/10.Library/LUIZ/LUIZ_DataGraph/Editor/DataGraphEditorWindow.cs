using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;

namespace LUIZ.DataGraph.Editor
{
    //데이터 그래프의 메인 에디터 윈도우.
    //그래프 뷰, 그래프인스펙터 뷰를 관리 함.
	public class DataGraphEditorWindow : EditorWindow
	{
		private SerializedObject m_serializedObject;
		private DataGraphViewEditor m_currentViewEditor;
		private NodeInspectorViewEditor m_nodeInspectorViewEditor;

        private Label m_labelNodeCount;
        private Label m_labelGroupCount;
        private Label m_labelEdgeCount;
        
        private bool m_isUndoScheduled;
        
		//--------------------------------------------------------------
		public DataGraph CurrentGraph { get; private set; }
		
		//--------------------------------------------------------------
		public static void Open(DataGraph target)
		{
			DataGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<DataGraphEditorWindow>();
			foreach (DataGraphEditorWindow window in windows)
			{
				if (window.CurrentGraph == target)
				{
					window.Focus();
					return;
				}
			}

			DataGraphEditorWindow newWindow = CreateWindow<DataGraphEditorWindow>(typeof(DataGraphEditorWindow), typeof(SceneView));
			newWindow.titleContent = new GUIContent($"{target.name}", EditorGUIUtility.ObjectContent(null, typeof(DataGraph)).image);
			newWindow.DrawEditorWindow(target);
		}

        //--------------------------------------------------------------
        public void NotifyNodeSelectionChanged(List<DataGraphNodeBaseEditor> node)
        {
            m_nodeInspectorViewEditor.UpdateInspector(node);
        }
        
        public void SetAssetDirty()
        {
            m_serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(CurrentGraph);
        }
        
        //--------------------------------------------------------------
		private void OnEnable()
		{
            DataGraph.GraphRestored -= OnGraphRestored;
            DataGraph.GraphRestored += OnGraphRestored;
            
            m_labelNodeCount = new Label();
            m_labelGroupCount = new Label();
            m_labelEdgeCount = new Label();
            
            Undo.undoRedoPerformed += OnUndoRedo;
            
			if (CurrentGraph != null)
				DrawEditorWindow(CurrentGraph);
            
			rootVisualElement.RegisterCallback<GeometryChangedEvent>(OnResize);
		}

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            DataGraph.GraphRestored -= OnGraphRestored;
        }
        
		private void OnGUI()
		{
			if (CurrentGraph != null)
			{
				hasUnsavedChanges = EditorUtility.IsDirty(CurrentGraph);
			}
		}
        
        //--------------------------------------------------------------
        void OnGraphRestored(DataGraph g)
        {
            if (!ReferenceEquals(g, CurrentGraph)) return;
            SafeRefresh();
        }
        
        private void SafeRefresh()
        {
            if (CurrentGraph == null) return;

            //SerializedObject를 새로 만들어 바인딩 갱신
            m_serializedObject = new SerializedObject(CurrentGraph);

            //그래프뷰/인스펙터가 캐싱한 SerializedObject/바인딩을 다시 연결
            m_currentViewEditor?.ClearGraph();
            m_currentViewEditor?.DrawDataGraph();
            m_nodeInspectorViewEditor?.UpdateInspector(null); // 필요 시 선택 갱신

            m_serializedObject.UpdateIfRequiredOrScript();
            Repaint();
        }
        
        private void DrawEditorWindow(DataGraph target)
		{
			CurrentGraph = target;
			m_serializedObject = new SerializedObject(CurrentGraph);

			rootVisualElement.Clear();

			m_currentViewEditor = new DataGraphViewEditor(m_serializedObject, this)
			{
				name = "graph-view"
			};
			m_currentViewEditor.graphViewChanged += OnGraphViewChanged;
            m_currentViewEditor.OnSelectionChanged += UpdateCountLabels;
			m_currentViewEditor.style.flexGrow = 1;

			var splitView = new TwoPaneSplitView(0, 800, TwoPaneSplitViewOrientation.Horizontal);
			splitView.Add(m_currentViewEditor);

			m_nodeInspectorViewEditor = new NodeInspectorViewEditor { name = "inspector-view" };
			m_nodeInspectorViewEditor.style.flexGrow = 1;
			splitView.Add(m_nodeInspectorViewEditor);

			rootVisualElement.Add(splitView);

			//json 세이브 버튼 추가
			var saveButton = new Button(() => SaveGraph())
			{
				text = "Save"
			};
			saveButton.style.position = Position.Absolute;
			saveButton.style.top = 10;
			saveButton.style.left = 10;
			saveButton.style.width = 50;
			saveButton.style.height = 30;
			rootVisualElement.Add(saveButton);

			//json 컨버트 버튼 추가
			var convertButton = new Button(() => ConvertGraphToJson())
			{
				text = "JSON Export"
			};
			convertButton.style.position = Position.Absolute;
			convertButton.style.top = 10;
			convertButton.style.left = 70;
			convertButton.style.width = 100;
			convertButton.style.height = 30;
			rootVisualElement.Add(convertButton);
			
            //그래프 뷰 중심으로 포커스 하는 버튼 추가
			var focusButton = new Button(() => m_currentViewEditor.RecenterGraphView())
			{
				text = "Recenter"
			};
			focusButton.style.position = Position.Absolute;
			focusButton.style.top = 10;
			focusButton.style.left = 180;
			focusButton.style.width = 60;
			focusButton.style.height = 30;
			rootVisualElement.Add(focusButton);
            
            //그래프 통계 정보
            void StyleCountLabel(Label label, float bottomOffset)
            {
                label.style.position = Position.Absolute;
                label.style.right = 10;
                label.style.bottom = bottomOffset; // 위에서부터 위치 다르게
                label.style.fontSize = 12;
                label.style.color = new Color(1f, 1f, 1f, 0.5f); // 반투명 흰색
                label.style.unityFontStyleAndWeight = FontStyle.Normal;
                label.style.marginRight = 0;
                label.style.marginLeft = 0;
            }
            StyleCountLabel(m_labelNodeCount, 80);
            StyleCountLabel(m_labelGroupCount, 65);
            StyleCountLabel(m_labelEdgeCount, 50);
            UpdateCountLabels(); //최초 초기화
            rootVisualElement.Add(m_labelNodeCount);
            rootVisualElement.Add(m_labelGroupCount);
            rootVisualElement.Add(m_labelEdgeCount);

            // 스냅 사이즈 필드 감싸는 박스
            var containerBox = new Box();
            containerBox.style.position = Position.Absolute;            
            containerBox.style.bottom = 10;
            containerBox.style.right = 10;
            containerBox.style.width = 150;
            containerBox.style.height = 30;
            containerBox.style.paddingTop = 4;
            containerBox.style.paddingBottom = 4;
            containerBox.style.paddingLeft = 3;
            containerBox.style.paddingRight = 3;
            containerBox.style.borderBottomWidth = 1;
            containerBox.style.borderTopWidth = 1;
            containerBox.style.borderLeftWidth = 1;
            containerBox.style.borderRightWidth = 1;
            containerBox.style.borderBottomColor = Color.gray;
            containerBox.style.borderTopColor = Color.gray;
            containerBox.style.borderLeftColor = Color.gray;
            containerBox.style.borderRightColor = Color.gray;
            containerBox.style.backgroundColor = new Color(0.13f, 0.13f, 0.13f);

            // 노드 이동 제어를 위한 스냅 사이즈 조절 필드
            var snapField = new IntegerField("Snap Size (px)");
            snapField.value = DataGraphNodeBaseEditor.SnapSize;
            snapField.style.flexDirection = FlexDirection.Row;
            snapField.labelElement.style.minWidth = 0;
            snapField.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            snapField.labelElement.style.marginRight = 4;

            snapField.value = DataGraphNodeBaseEditor.SnapSize;
            snapField.RegisterValueChangedCallback(evt =>
            {
                snapField.value = DataGraphNodeBaseEditor.SnapSize = Mathf.Max(1, evt.newValue);                
            });
            
            containerBox.Add(snapField);
            
            rootVisualElement.Add(containerBox);
        }
        
        private void UpdateCountLabels()
        {
            if (m_currentViewEditor == null) return;

            var selection = m_currentViewEditor.selection;

            int nodeCount = 0;
            int groupCount = 0;
            int edgeCount = 0;

            foreach (var element in selection)
            {
                if (element is DataGraphNodeBaseEditor) nodeCount++;
                else if (element is Group) groupCount++;
                else if (element is Edge) edgeCount++;
            }

            m_labelNodeCount.text = $"Nodes: {nodeCount}";
            m_labelGroupCount.text = $"Groups: {groupCount}";
            m_labelEdgeCount.text = $"Edges: {edgeCount}";
        }
        
        private void SaveGraph()
        {
            m_serializedObject.ApplyModifiedProperties();
            CurrentGraph?.NotifyBeforeSaveEditor();//저장 이벤트 호출 (ctrl+s 로 인한 저장 이벤트는 DataGraphSaveProcessor 에서 처리 중)
            EditorUtility.SetDirty(CurrentGraph);
            AssetDatabase.SaveAssets();
        }

        private void ConvertGraphToJson()
        {
            if (CurrentGraph == null)
            {
                Debug.LogError("No graph loaded to convert!");
                return;
            }

            var converter = DataGraphSettings.GetCurrentConverter();
            if (converter == null)
            {
                Debug.LogError("No converter selected in DataGraphSettings!");
                return;
            }

            converter.ExportGraphToJson(CurrentGraph);
        }
        
        //--------------------------------------------------------------
        //TODO: 뭔가 UI나 뷰가 이상하다 싶을떄 그냥 refresh 하라고 버튼 하나 뚫어 줘도 괜찮을듯
        private void OnUndoRedo()
        {
            if (CurrentGraph == null) return;
            if (m_isUndoScheduled) return;

            m_isUndoScheduled = true;

            //한 프레임만 지연
            EditorApplication.delayCall += () =>
            {
                m_isUndoScheduled = false;
                if (CurrentGraph == null) return;

                CurrentGraph.ClearCache();
                CurrentGraph.DoForceApplyJsonInEditor(); // 여기서 GraphRestored가 뜸 → SafeRefresh 1회만 수행
            };
        }
        
		private void OnResize(GeometryChangedEvent evt)
		{
			var splitView = rootVisualElement.Q<TwoPaneSplitView>();
			if (splitView != null)
			{
				float totalWidth = rootVisualElement.resolvedStyle.width;
				splitView.fixedPaneInitialDimension = totalWidth * 0.73f;
			}
		}
        
		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
		{
			EditorUtility.SetDirty(CurrentGraph);
			return graphViewChange;
		}
	}
}
