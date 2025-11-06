using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LUIZ.DataGraph.Editor
{
    public class DataGraphViewEditor : GraphView, INodeHostContext
    {
        private DataGraph m_dataGraph;
        private SerializedObject m_serializedObject;
        private DataGraphEditorWindow m_graphEditorWindow;

        private Dictionary<ulong, DataGraphNodeBaseEditor> m_dicNodeEditors;
        private Dictionary<ulong, Group> m_dicNodeGroups;
        private Dictionary<Edge, DataGraphEdge> m_dicEdges;

        //우클릭 했을때 그룹에서 제거가능한 노드가 있으면 캐싱해두고 실제 제거 할때 인덱싱 안하게 캐싱해둠.
        private Dictionary<Group, List<DataGraphNodeBaseEditor>> m_dicRemovableFromGroupsCache = new();

        private DataGraphSearchWindowProvider m_searchProvider;
        private List<Port> m_listCompatiblePortsCache = new();

        private Vector2 m_lastRightClickMousePos;
        private bool m_isRestoring = false;
        //-------------------------------------------------------------------------
        public DataGraphEditorWindow GraphEditorWindow => m_graphEditorWindow;
        public event Action OnSelectionChanged;

        //--------------------------------------------------------------------------
        public DataGraphViewEditor(SerializedObject serializedObject, DataGraphEditorWindow dataGraphEditorWindow)
        {
            SetupClipboardHandlers();
            SetupFields(serializedObject, dataGraphEditorWindow);
            SetupGraphViewStyle();
            SetupManipulators();
            SetupCallbacks();
            DrawDataGraph();
        }

        //------------------------------------------------------------
        #region ======== SET UP ========

        private void SetupClipboardHandlers()
        {
            //복붙 로직 콜백 연결
            serializeGraphElements = SerializeGraphElementsImpl;
            canPasteSerializedData = CanPasteSerializedDataImpl;
            unserializeAndPaste = UnserializeAndPasteImpl;
        }

        private void SetupFields(SerializedObject serializedObject, DataGraphEditorWindow dataGraphEditorWindow)
        {
            m_serializedObject = serializedObject;
            m_dataGraph = serializedObject.targetObject as DataGraph;
            m_graphEditorWindow = dataGraphEditorWindow;

            m_dicNodeEditors = new Dictionary<ulong, DataGraphNodeBaseEditor>();
            m_dicEdges = new Dictionary<Edge, DataGraphEdge>();
            m_dicNodeGroups = new Dictionary<ulong, Group>();

            m_searchProvider = ScriptableObject.CreateInstance<DataGraphSearchWindowProvider>();
            m_searchProvider.GraphViewEditor = this;
            nodeCreationRequest = ShowSearchWindow;
        }

        private void SetupGraphViewStyle()
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(DataGraphSettings.EditorStyleSheetPath);
            styleSheets.Add(styleSheet);

            var background = new GridBackground { name = "Grid" };
            Add(background);
            background.SendToBack();

            //눈아파서 최대 값 늘렸음..
            //SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            SetupZoom(0.15f, 2.0f);
        }

        private void SetupManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            this.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                //그룹 추가 버튼
                evt.menu.AppendAction("Add Group", _ =>
                {
                    Debug.Log($"[AddGroup] using cached pos = {m_lastRightClickMousePos}");
                    AddGroupAt(m_lastRightClickMousePos);
                });
                //선택된 노드들을 본인들 그룹에서 제거하는 버튼
                evt.menu.AppendAction("Remove From Group", _ => { RemoveSelectedNodesFromGroups(); },
                    GetRemoveFromGroupMenuStatus());
            }));
        }

        private void SetupCallbacks()
        {
            graphViewChanged += OnGraphViewChanged;

            RegisterCallback<PointerDownEvent>(evt =>
            {
                //ContextualMenuManipulator의 evt 에서 마우스 클릭 위치를 받아올 경우 그래프 뷰 좌표와 안맞음.
                //우클릭 때 위치를 캐싱 해두는 식으로 해결...
                if (evt.button == 1) //오른쪽 클릭
                {
                    Vector2 worldMouse = evt.position;
                    Vector2 graphMouse = contentViewContainer.WorldToLocal(worldMouse);
                    m_lastRightClickMousePos = graphMouse;
                }
            });

            RegisterCallback<MouseUpEvent>(evt =>
            {
                var selectedNodes = new List<DataGraphNodeBaseEditor>();
                foreach (var selectedItem in selection)
                {
                    if (selectedItem is DataGraphNodeBaseEditor editorNode)
                    {
                        selectedNodes.Add(editorNode);
                    }
                }

                m_graphEditorWindow.NotifyNodeSelectionChanged(selectedNodes);
                OnSelectionChanged?.Invoke();
            });
        }

        #endregion

        //------------------------------------------------------------
        #region ======== INodeHostContext ========

        public void RequestAddDynamicPort(Direction direction, DataGraphNodeBase node)
        {
            if (direction == Direction.Input && node is IDynamicInputPortProvider dynIn)
                dynIn.AddDynamicInputPort();
            else if (direction == Direction.Output && node is IDynamicOutputPortProvider dynOut)
                dynOut.AddDynamicOutputPort();

            RequestRefreshGraph(); //TODO 전체를 다 그리지 말고 본인과 연결된 애들만 최신화 하기
        }

        public void RequestRemovePort(Port port)
        {
            RemoveEdgesConnectedToPort(port);
            if (port.userData is not PortMetaData meta) return;

            var node = (port.node as DataGraphNodeBaseEditor)?.Node;
            if (node == null) return;

            if (port.direction == Direction.Input && node is IDynamicInputPortProvider dynIn)
                dynIn.RemoveDynamicInputPortByID(meta.PortID);
            else if (port.direction == Direction.Output && node is IDynamicOutputPortProvider dynOut)
                dynOut.RemoveDynamicOutputPortByID(meta.PortID);

            RequestRefreshGraph(); //TODO 전체를 다 그리지 말고 본인과 연결된 애들만 최신화 하기
        }

        public void RequestRefreshGraph()
        {
            ClearGraph();
            DrawDataGraph();
            SetAssetDirty();
        }

        public void RequestShowSearchWindowFromPort(Port port, Vector2 screenMousePos)
        {
            ShowSearchWindowFromPort(port, screenMousePos);
        }

        public void SetAssetDirty()
        {
            GraphEditorWindow?.SetAssetDirty();
        }

        #endregion

        //------------------------------------------------------------
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            m_listCompatiblePortsCache.Clear();

            if (IsPortConnectionLimitExceeded(startPort)) return m_listCompatiblePortsCache;
            if (startPort.userData is not PortMetaData startMeta) return m_listCompatiblePortsCache;

            foreach (var (_, nodeEditor) in m_dicNodeEditors)
            {
                foreach (var port in nodeEditor.Ports)
                {
                    if (port == startPort) continue;
                    if (ArePortsCompatible(startPort, port))
                    {
                        if (!IsPortConnectionLimitExceeded(port))
                            m_listCompatiblePortsCache.Add(port);
                    }
                }
            }

            return m_listCompatiblePortsCache;
        }

        public void NotifyEdgeCreated(Edge edge)
        {
            CreateEdge(edge); //내부에서 m_dataGraph.AddEdgeEditor 등 처리
        }

        //--------------------------------------------------------------------------
        public DataGraphNodeBaseEditor GetNodeEditorByID(ulong nodeID)
        {
            return m_dicNodeEditors.TryGetValue(nodeID, out var editor) ? editor : null;
        }

        public bool ArePortsCompatible(Port a, Port b)
        {
            if (a == null || b == null) return false;
            if (a.direction == b.direction) return false;

            var fromPort = a.direction == Direction.Output ? a : b;
            var toPort = a.direction == Direction.Output ? b : a;

            if (fromPort.userData is not PortMetaData fromMeta || toPort.userData is not PortMetaData toMeta)
                return false;

            var fromNodeType = (fromPort.node as DataGraphNodeBaseEditor)?.Node?.GetType();
            var acceptedType = toMeta.AcceptedType;
            if (fromNodeType == null || acceptedType == null) return false;
            if (!acceptedType.IsAssignableFrom(fromNodeType)) return false;

            /*
            //정적 포트끼리는 ID까지 맞아야 함
            if (!fromMeta.IsDynamic && !toMeta.IsDynamic && fromMeta.PortID != toMeta.PortID)
                return false;
                */
            
            if (fromMeta.ChannelID != toMeta.ChannelID)
                return false;
            
            return true;
        }

        //포트 최대 연결 제한 확인
        public bool IsPortConnectionLimitExceeded(Port port)
        {
            if (port.userData is PortMetaData meta)
            {
                if (meta.MaxConnection < 0) //-1인 경우 연결 가능이 무한으로 취급함
                    return false;

                int currentCount = port.connections.Count();
                return currentCount >= meta.MaxConnection;
            }

            return false;
        }

        //실제 그래프를 그린다.
        public void DrawDataGraph()
        {
            //TODO : 지금 전부 그래프를 새로 그리고 있는데 버전 관리를 통해 변경된 데이터만 새로 그리도록 로직을 보완하도록 한다.
            //TODO : 그래야 Undo 같은 곳에서 그릴때 불필요한 부하를 피할 수 있다...
            if (m_dataGraph == null) return;
            m_isRestoring = true;

            //그룹 먼저
            if (m_dataGraph.Groups != null)
                foreach (var g in m_dataGraph.Groups)
                    AddNodeGroupToGraphView(g, attachNodes: false);

            //노드
            if (m_dataGraph.Nodes != null)
                foreach (var node in m_dataGraph.Nodes)
                    AddNodeToGraphView(node);

            //그룹에 노드 붙이기
            if (m_dataGraph.Groups != null)
                foreach (var g in m_dataGraph.Groups)
                    AttachNodesToGroup(g);
            
            if (m_dataGraph.Edges != null)
                foreach (var edge in m_dataGraph.Edges)
                    AddEdgeToGraphView(edge);

            UpdateAllPortVisualStates();
            EditorApplication.delayCall += () =>
            {
                //한 프레임 더 미룬 뒤 복원 종료 플래그 내리기
                m_isRestoring = false;
            };
        }

        //노드 뷰의 현재 위치를 초기화 ( 뷰에서 길 잃을 때 이용 ), F키 눌러서도 가능함..
        public void RecenterGraphView()
        {
            ClearSelection();

            foreach (var node in m_dicNodeEditors.Values)
                AddToSelection(node); // 노드를 선택

            FrameSelection(); // 선택된 노드를 기준으로 뷰 포커스
            OnSelectionChanged?.Invoke();
        }

        public void ClearGraph()
        {
            foreach (var element in graphElements.ToList())
            {
                if (element is Node or Edge or Group)
                    RemoveElement(element);
            }

            m_dicNodeEditors.Clear();
            m_dicEdges.Clear();
            m_dicNodeGroups.Clear();
        }

        //--------------------------------------------------------------------------
        #region =====복붙 로직 구현=====
        private string SerializeGraphElementsImpl(IEnumerable<GraphElement> elements)
        {
            var data = new NodeClipboardContainer
            {
                nodes = new List<NodeClipboardSerializableData>(),
                edges = new List<EdgeClipboardSerializableData>()
            };

            //선택된 노드 수집
            var selectedEditors = elements.OfType<DataGraphNodeBaseEditor>().ToList();
            if (selectedEditors.Count == 0)
                return JsonUtility.ToJson(data);

            var selectedIds = new HashSet<ulong>(selectedEditors.Select(ne => ne.Node.NodeID));

            //노드 직렬화 (JSON with PPtr-clean + objectRefs + oldNodeId + position)
            foreach (var editorNode in selectedEditors)
            {
                var node = editorNode.Node;
                var entry = new NodeClipboardSerializableData
                {
                    nodeType  = node.GetType().AssemblyQualifiedName,
                    json      = ToJsonWithInstanceIdZero(node),
                    objectRefs = SnapshotObjectRefs(node),
                    oldNodeId = node.NodeID,
                    position  = node.Position
                };
                data.nodes.Add(entry);
            }

            //간선 스냅샷 (선택된 노드들 사이의 연결만)
            foreach (var kv in m_dicEdges)
            {
                var e = kv.Value;
                if (selectedIds.Contains(e.OutputPort.NodeID) && selectedIds.Contains(e.InputPort.NodeID))
                {
                    data.edges.Add(new EdgeClipboardSerializableData
                    {
                        fromNodeId = e.OutputPort.NodeID,
                        fromPortId = e.OutputPort.PortID,
                        toNodeId   = e.InputPort.NodeID,
                        toPortId   = e.InputPort.PortID
                    });
                }
            }
            return JsonUtility.ToJson(data);
        }

        private bool CanPasteSerializedDataImpl(string data)
        {
            return !string.IsNullOrEmpty(data) && data.StartsWith("{"); // 대충 Json인지 정도만 검사
        }

        private void UnserializeAndPasteImpl(string operationName, string data)
        {
            var container = JsonUtility.FromJson<NodeClipboardContainer>(data);
            if (container?.nodes == null || container.nodes.Count == 0) return;

            //Undo 하나로 묶기 + 더티 마킹 최소화
            Undo.RecordObject(m_dataGraph, "Paste Nodes");
            ClearSelection();

            var mapOldToNew = new Dictionary<ulong, ulong>();

            //노드 생성
            foreach (var entry in container.nodes)
            {
                var type = Type.GetType(entry.nodeType);
                if (type == null) continue;

                var node = Activator.CreateInstance(type) as DataGraphNodeBase;
                if (node == null) continue;
                
                JsonUtility.FromJsonOverwrite(entry.json, node);
                RestoreObjectRefs(node, entry.objectRefs);

                //살짝 위치 옮김
                var pos = entry.position;
                pos.position += new Vector2(30, 30);
                (node as IDataGraphNode)?.SetPosition(pos);

                //새 NodeID
                (node as IDataGraphNode)?.RegenerateNodeID();
                mapOldToNew[entry.oldNodeId] = node.NodeID;
                
                CreateNode(node, addToSelection: true);
            }

            //간선 복구 (모든 노드가 생성된 뒤)
            if (container.edges != null && container.edges.Count > 0)
            {
                foreach (var e in container.edges)
                {
                    if (!mapOldToNew.TryGetValue(e.fromNodeId, out var newFrom)) continue;
                    if (!mapOldToNew.TryGetValue(e.toNodeId, out var newTo)) continue;

                    var outPort = GetPortFromEdge(newFrom, e.fromPortId, isInput: false);
                    var inPort = GetPortFromEdge(newTo, e.toPortId, isInput: true);
                    if (outPort == null || inPort == null) continue;
                    if (IsPortConnectionLimitExceeded(outPort) || IsPortConnectionLimitExceeded(inPort)) continue;

                    var edge = outPort.ConnectTo(inPort);
                    AddElement(edge);

                    // DataGraph 내부 데이터 반영(Edge→DataGraphEdge 등록)
                    NotifyEdgeCreated(edge);
                }
            }
            
            UpdateAllPortVisualStates();
            m_graphEditorWindow.SetAssetDirty();
            FrameSelection();
            OnSelectionChanged?.Invoke();
        }

        #endregion

        #region =======배경 그룹 생성 로직========

        private void AddGroupAt(Vector2 localMousePos)
        {
            Undo.RecordObject(m_dataGraph, "Add Group");

            //TODO : 그룹 안에 있는 노드를 우클릭 하면 그룹에서 Remove하는 옵션을 추가하자...
            var group = new Group
            {
                title = "New Node Group"
            };

            group.SetPosition(new Rect(localMousePos, Vector2.zero));
            group.pickingMode = PickingMode.Position;
            //유니크 ID 부여
            var groupID = IDGenerator.NewID();
            group.userData = groupID;
            //배경색 기본 세팅 (TODO : 이거도 스타일 시트로 뺴야함!!!)
            var color = new Color(0.2f, 0.4f, 0.9f, 0.22f);
            group.style.backgroundColor = color;

            //저장
            var groupData = new DataGraphNodeGroup()
            {
                GroupID = groupID,
                Title = group.title,
                Position = group.GetPosition(),
                Color = color,
            };

            m_dataGraph.AddNodeGroupEditor(groupData);
            m_dicNodeGroups.Add(groupData.GroupID, group);

            AddElement(group);
            RegisterGroupEvents(group, groupData);
            m_graphEditorWindow.SetAssetDirty();
        }

        private void RegisterGroupEvents(Group group, DataGraphNodeGroup groupData)
        {
            group.RegisterCallback<FocusOutEvent>(evt => { groupData.Title = group.title; });
            group.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (m_isRestoring) return;
                
                var newPos = RoundRectToInt(group.GetPosition());//정수 스냅
                var oldPos = RoundRectToInt(groupData.Position);   //비교도 같은 기준
                
                if (!ApproximatelyRect(oldPos, newPos, 0.2f))//미세 변화 무시
                {
                    Undo.RecordObject(m_dataGraph, "Move Group");
                    groupData.Position = newPos;
                    group.schedule.Execute(() => UpdateGroupContainedIDs(group, groupData));
                    m_graphEditorWindow.SetAssetDirty();
                }
                
            });
            group.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Set Color...", _ =>
                {
                    var localMousePos = evt.mousePosition;
                    var screenMousePos = GUIUtility.GUIToScreenPoint(localMousePos);
                    DateGraphColorPickerEditorWindow.Show(this, groupData.Color, newColor =>
                    {
                        Undo.RecordObject(m_dataGraph, "Changed Group Color");
                        group.style.backgroundColor = newColor;
                        groupData.Color = newColor;
                    });
                });
            }));
        }
        
        private bool ApproximatelyRect(Rect a, Rect b, float eps = 0.5f)
        {
            return Mathf.Abs(a.x - b.x) < eps &&
                   Mathf.Abs(a.y - b.y) < eps &&
                   Mathf.Abs(a.width - b.width) < eps &&
                   Mathf.Abs(a.height - b.height) < eps;
        }

        private Rect RoundRectToInt(Rect r)
        {
            r.x      = Mathf.Round(r.x);
            r.y      = Mathf.Round(r.y);
            r.width  = Mathf.Round(r.width);
            r.height = Mathf.Round(r.height);
            return r;
        }
        
        private void UpdateGroupContainedIDs(Group group, DataGraphNodeGroup groupData)
        {
            groupData.ContainedNodeIDs.Clear();

            foreach (var element in group.containedElements)
            {
                if (element is DataGraphNodeBaseEditor editor)
                    groupData.ContainedNodeIDs.Add(editor.Node.NodeID);
            }

            m_graphEditorWindow.SetAssetDirty();
        }

        #endregion

        #region =======배경 그룹에서 노드 제거 로직===========

        private DropdownMenuAction.Status GetRemoveFromGroupMenuStatus()
        {
            m_dicRemovableFromGroupsCache.Clear();

            foreach (var selectedElement in selection)
            {
                if (selectedElement is DataGraphNodeBaseEditor nodeEditor)
                {
                    foreach (var group in m_dicNodeGroups.Values) //TODO 추후 데이터 그래프가 비대해지면 인덱싱 덜 돌게 캐싱하는식으로 보완
                    {
                        if (group.containedElements.Contains(nodeEditor))
                        {
                            if (!m_dicRemovableFromGroupsCache.TryGetValue(group, out var nodeList))
                            {
                                nodeList = new List<DataGraphNodeBaseEditor>();
                                m_dicRemovableFromGroupsCache[group] = nodeList;
                            }

                            nodeList.Add(nodeEditor);
                        }
                    }
                }
            }

            return m_dicRemovableFromGroupsCache.Count > 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
        }

        private void RemoveSelectedNodesFromGroups()
        {
            if (m_dicRemovableFromGroupsCache.Count == 0) return;

            Undo.RecordObject(m_dataGraph, "Remove Nodes From Group");

            foreach (var kvp in m_dicRemovableFromGroupsCache)
            {
                var group = kvp.Key;
                var groupID = (ulong)group.userData;
                var groupData = m_dataGraph.Groups.FirstOrDefault(g => g.GroupID == groupID);

                foreach (var nodeEditor in kvp.Value)
                {
                    group.RemoveElement(nodeEditor);
                    groupData?.ContainedNodeIDs.Remove(nodeEditor.Node.NodeID);
                }
            }

            m_graphEditorWindow.SetAssetDirty();
            m_dicRemovableFromGroupsCache.Clear(); //캐시 정리
        }

        #endregion

        //제공 받은 포트에서 연결 가능한 노드들만 표기하는 SearchWindow를 열어준다.
        private void ShowSearchWindowFromPort(Port startPort, Vector2 screenMousePos)
        {
            // GraphView의 VisualElement root 기준으로 변환
            Vector2 screenPos = GUIUtility.GUIToScreenPoint(screenMousePos);

            m_searchProvider.StartPort = startPort;
            SearchWindow.Open(new SearchWindowContext(screenPos), m_searchProvider);
        }

        private void RemoveEdgesConnectedToPort(Port targetPort)
        {
            var connectedEdges = targetPort.connections.ToList(); //여러 개일 수 있음
            foreach (var edge in connectedEdges)
            {
                RemoveEdge(edge);
            }
        }

        //--------------------------------------------------------------------------

        #region ========EVENTS======

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (m_isRestoring)// 복원 중엔 아무것도 하지 않음
                return graphViewChange;
            
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    switch (element)
                    {
                        case DataGraphNodeBaseEditor node: RemoveNode(node); break;
                        case Edge edge: RemoveEdge(edge); break;
                        case Group group: RemoveNodeGroup(group); break;
                    }
                }
            }

            if (graphViewChange.movedElements != null)
            {
                var nodesToMove = graphViewChange.movedElements.OfType<DataGraphNodeBaseEditor>().ToList();
                if (nodesToMove.Count > 0)
                {
                    Undo.RecordObject(m_dataGraph, "Moved Node");
                    foreach (var editorNode in nodesToMove)
                        editorNode.DoSavePosition();

                    m_graphEditorWindow.SetAssetDirty();
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                    CreateEdge(edge);
            }

            if (graphViewChange.edgesToCreate != null || graphViewChange.elementsToRemove != null)
            {
                //사실 여기서 모든 Port를 검사할 필요 없다. 변경된 변경된 노드, 포트만 받아와서 돌려도된다...
                //최적화할때 이 곳부터 수정 할 것....
                schedule.Execute(() => UpdateAllPortVisualStates()).ExecuteLater(1); //즉시 업데이트 하면 반영이 안되어 딜레이를 넣음
            }


            return graphViewChange;
        }

        private void OnEditorNodeValueChanged(DataGraphNodeBase node)
        {
            m_graphEditorWindow.SetAssetDirty();

            DataGraphNodeBaseEditor nodeEditor = m_dicNodeEditors[node.NodeID];
            nodeEditor.UpdateDescription(node.Name, node.Description);
        }

        #endregion

        #region ======REMOVE=======

        private void RemoveEdge(Edge edge)
        {
            if (m_dicEdges.TryGetValue(edge, out DataGraphEdge edgeToRemove))
            {
                Undo.RecordObject(m_dataGraph, "Removed Edge");

                m_dataGraph.RemoveEdgeEditor(edgeToRemove);
                m_dicEdges.Remove(edge);

                m_graphEditorWindow.SetAssetDirty();
            }
        }

        private void RemoveNodeGroup(Group group)
        {
            Undo.RecordObject(m_dataGraph, "Removed Group");

            if (group.userData is not ulong groupID)
                return;

            m_dicNodeGroups.Remove(groupID); //캐시에서 제거

            var groupToRemove = m_dataGraph.Groups.FirstOrDefault(g => g.GroupID == groupID);
            if (groupToRemove != null)
                m_dataGraph.RemoveNodeGroupEditor(groupToRemove);

            m_graphEditorWindow.SetAssetDirty();
        }

        private void RemoveNode(DataGraphNodeBaseEditor nodeEditor)
        {
            Undo.RecordObject(m_dataGraph, "Removed Node");

            foreach (Port port in nodeEditor.Ports)
            {
                foreach (Edge edge in port.connections)
                    RemoveEdge(edge);
            }

            nodeEditor.Node.OnEditorNodeDestroy();

            m_dataGraph.RemoveNodeEditor(nodeEditor.Node);
            m_dicNodeEditors.Remove(nodeEditor.Node.NodeID);

            m_graphEditorWindow.SetAssetDirty();
        }

        #endregion

        #region ======CREATE=======

        public void CreateNode(DataGraphNodeBase node, bool addToSelection = false)
        {
            Undo.RecordObject(m_dataGraph, "Created Node");

            m_dataGraph.AddNodeEditor(node);
            AddNodeToGraphView(node, addToSelection);

            m_graphEditorWindow.SetAssetDirty();
        }

        private void CreateEdge(Edge edge)
        {
            Undo.RecordObject(m_dataGraph, "Created Edge");

            DataGraphNodeBaseEditor outputNode = (DataGraphNodeBaseEditor)(edge.output.node);
            DataGraphNodeBaseEditor inputNode = (DataGraphNodeBaseEditor)(edge.input.node);

            // PortMetaData에서 PortID 가져오기
            if (!(edge.output.userData is PortMetaData outputMeta) ||
                !(edge.input.userData is PortMetaData inputMeta))
            {
                Debug.LogError("CreateEdge: 연결된 포트에 PortMetaData가 없습니다.");
                return;
            }

            var dataGraphEdge = new DataGraphEdge(
                new DataGraphEdgePort(outputNode.Node.NodeID, outputMeta.PortID),
                new DataGraphEdgePort(inputNode.Node.NodeID, inputMeta.PortID)
            );

            m_dataGraph.AddEdgeEditor(dataGraphEdge);
            m_dicEdges.Add(edge, dataGraphEdge);

            m_graphEditorWindow.SetAssetDirty();
        }

        #endregion

        #region ======ADD_TO_GRAPH======

        private void AddEdgeToGraphView(DataGraphEdge dataGraphEdge)
        {
            Port outputPort = GetPortFromEdge(dataGraphEdge.OutputPort.NodeID, dataGraphEdge.OutputPort.PortID, isInput: false);
            Port inputPort = GetPortFromEdge(dataGraphEdge.InputPort.NodeID, dataGraphEdge.InputPort.PortID, isInput: true);

            if (outputPort == null || inputPort == null)
            {
                //Debug.LogWarning($"[EdgeRestore] 포트를 찾을 수 없어 연결 복원 실패: {dataGraphEdge.OutputPort.NodeID}:{dataGraphEdge.OutputPort.PortID} → {dataGraphEdge.InputPort.NodeID}:{dataGraphEdge.InputPort.PortID}");
                return;
            }

            if (outputPort.direction != Direction.Output || inputPort.direction != Direction.Input)
            {
                Debug.LogError($"잘못된 포트 방향 연결 시도: {outputPort.direction} → {inputPort.direction}");
                return;
            }

            //타입 호환성 검사
            if (outputPort.userData is not PortMetaData outMeta || inputPort.userData is not PortMetaData inMeta)
                return;

            var outputNodeType = (outputPort.node as DataGraphNodeBaseEditor)?.Node?.GetType();
            var inputAcceptType = inMeta.AcceptedType;
            if (outputNodeType == null || inputAcceptType == null || !inputAcceptType.IsAssignableFrom(outputNodeType))
            {
                Debug.LogWarning($"[EdgeRestore] 타입 불일치로 연결 복원 생략: {outputNodeType?.Name} → {inputAcceptType?.Name}");
                return;
            }

            //연결 수 검사, 복원 중엔 MaxConnection 체크 스킵
            if (!m_isRestoring)
            {
                if (IsPortConnectionLimitExceeded(outputPort) || IsPortConnectionLimitExceeded(inputPort))
                    return;
            }

            Edge edge = outputPort.ConnectTo(inputPort);
            AddElement(edge);
            m_dicEdges.Add(edge, dataGraphEdge);
        }

        private void AddNodeGroupToGraphView(DataGraphNodeGroup nodeGroup, bool attachNodes)
        {
            var newGroup = new Group { title = nodeGroup.Title };
            newGroup.userData = nodeGroup.GroupID;
            newGroup.SetPosition(nodeGroup.Position);
            newGroup.style.backgroundColor = nodeGroup.Color;
            newGroup.pickingMode = PickingMode.Position;

            RegisterGroupEvents(newGroup, nodeGroup);
            AddElement(newGroup);
            m_dicNodeGroups.Add(nodeGroup.GroupID, newGroup);

            if (attachNodes) // 기본 false
            {
                foreach (var nodeID in nodeGroup.ContainedNodeIDs)
                    if (m_dicNodeEditors.TryGetValue(nodeID, out var nodeEditor))
                        newGroup.AddElement(nodeEditor);
            }
        }

        //노드 붙이기는 별도 함수로
        private void AttachNodesToGroup(DataGraphNodeGroup nodeGroup)
        {
            if (!m_dicNodeGroups.TryGetValue(nodeGroup.GroupID, out var group)) return;
            foreach (var nodeID in nodeGroup.ContainedNodeIDs)
                if (m_dicNodeEditors.TryGetValue(nodeID, out var nodeEditor))
                    group.AddElement(nodeEditor);
        }

        private void AddNodeToGraphView(DataGraphNodeBase node, bool addToSelection = false)
        {
            node.OnEditorNodeValueChanged += OnEditorNodeValueChanged;

            DataGraphNodeBaseEditor nodeEditor = new DataGraphNodeBaseEditor(node, this);
            nodeEditor.SetPosition(node.Position);
            nodeEditor.UpdateDescription(node.Name, node.Description);
            m_dicNodeEditors.Add(node.NodeID, nodeEditor);

            if (addToSelection)
                AddToSelection(nodeEditor);

            UpdatePortVisualStatesForNode(nodeEditor);

            AddElement(nodeEditor);
        }

        #endregion

        private Port GetPortFromEdge(ulong nodeId, int portID, bool isInput)
        {
            if (!m_dicNodeEditors.TryGetValue(nodeId, out var nodeEditor))
                return null;

            foreach (var port in nodeEditor.Ports)
            {
                if (port.userData is PortMetaData meta &&
                    meta.PortID == portID &&
                    port.direction == (isInput ? Direction.Input : Direction.Output))
                {
                    return port;
                }
            }

            return null;
        }

        private void ShowSearchWindow(NodeCreationContext context)
        {
            m_searchProvider.StartPort = null;
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_searchProvider);
        }

        private void UpdateAllPortVisualStates()
        {
            foreach (var node in m_dicNodeEditors.Values)
            {
                UpdatePortVisualStatesForNode(node);
            }
        }

        public void UpdatePortVisualStatesForNode(DataGraphNodeBaseEditor node)
        {
            foreach (var port in node.Ports)
            {
                if (port.userData is PortMetaData meta)
                {
                    int count = port.connections.Count();

                    bool isBelowMin = meta.MinConnection > 0 && count < meta.MinConnection;

                    if (isBelowMin)
                        port.AddToClassList("connector-underflow");
                    else
                        port.RemoveFromClassList("connector-underflow");

                    // 연결 수 표시 업데이트
                    string maxDisplay = meta.MaxConnection < 0 ? DataGraphSettings.c_Infinity : meta.MaxConnection.ToString();
                    port.portName = $"{meta.DisplayName} ({count}/{maxDisplay})";
                }
            }
        }

        //--------복붙 관련 클래스
        [Serializable]
        private class NodeObjectRefEntry
        {
            public string fieldPath; // DeclaringType.FullName|FieldName
            public string guid;
            public long localId;
            public string typeName; // 짧은 AQN (선택)
        }

        [Serializable]
        private class NodeClipboardSerializableData
        {
            public string nodeType; // AQN
            public string json; // 오브젝트 필드 null 처리 후 JSON
            public List<NodeObjectRefEntry> objectRefs; // 복원용
            public ulong oldNodeId; // 간선 매핑용
            public Rect position; // 좌표(붙여넣기 오프셋용)
        }

        [Serializable]
        private class EdgeClipboardSerializableData
        {
            public ulong fromNodeId;
            public int fromPortId;
            public ulong toNodeId;
            public int toPortId;
        }

        [Serializable]
        private class NodeClipboardContainer
        {
            public List<NodeClipboardSerializableData> nodes;
            public List<EdgeClipboardSerializableData> edges;
        }

        // ===================== Clipboard Helpers =====================
        #region ======== ClipboardHelpers ========
        private const BindingFlags BF_INSTANCE_ALL = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private static IEnumerable<FieldInfo> GetUnityObjFields(Type t)
        {
            while (t != null && t != typeof(object))
            {
                foreach (var fi in t.GetFields(BF_INSTANCE_ALL))
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(fi.FieldType))
                    {
                        bool isUnitySerializable = fi.IsPublic || Attribute.IsDefined(fi, typeof(SerializeField), inherit: false);
                        if (isUnitySerializable) yield return fi;
                    }
                }

                t = t.BaseType;
            }
        }
        private static string FieldPath(FieldInfo fi) => $"{fi.DeclaringType.FullName}|{fi.Name}";

        //오브젝트 필드 임시 null -> JSON 생성 -> 원상복구
        private static string ToJsonWithInstanceIdZero(DataGraphNodeBase node)
        {
            var backups = new List<(FieldInfo fi, UnityEngine.Object val)>();
            foreach (var fi in GetUnityObjFields(node.GetType()))
            {
                var val = fi.GetValue(node) as UnityEngine.Object;
                if (val)
                {
                    backups.Add((fi, val));
                    fi.SetValue(node, null);
                }
            }

            try
            {
                return JsonUtility.ToJson(node);
            }
            finally
            {
                for (int i = backups.Count - 1; i >= 0; --i)
                    backups[i].fi.SetValue(node, backups[i].val);
            }
        }

        private static List<NodeObjectRefEntry> SnapshotObjectRefs(DataGraphNodeBase node)
        {
            var list = new List<NodeObjectRefEntry>();
            foreach (var fi in GetUnityObjFields(node.GetType()))
            {
                var obj = fi.GetValue(node) as UnityEngine.Object;
                if (!obj) continue;
                
                var entry = new NodeObjectRefEntry { fieldPath = FieldPath(fi), typeName = fi.FieldType.AssemblyQualifiedName };
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId))
                {
                    entry.guid = guid;
                    entry.localId = localId;
                }
                list.Add(entry);
            }

            return list;
        }

        private static FieldInfo FindFieldByPath(Type t, string fieldPath)
        {
            int sep = fieldPath.IndexOf('|');
            if (sep < 0) return null;

            string decl = fieldPath.Substring(0, sep);
            string name = fieldPath.Substring(sep + 1);

            while (t != null && t != typeof(object))
            {
                if (t.FullName == decl)
                    return t.GetField(name, BF_INSTANCE_ALL);
                t = t.BaseType;
            }

            return null;
        }

        private static UnityEngine.Object LoadByGuidLocalId(string guid, long localId, Type tHint)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            if (localId != 0)
            {
                var all = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int i = 0; i < all.Length; ++i)
                {
                    var a = all[i];
                    if (!a) continue;
                    if (!tHint.IsAssignableFrom(a.GetType())) continue;

                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out _, out long lid) && lid == localId)
                        return a;
                }

                return null;
            }
            return AssetDatabase.LoadAssetAtPath(path, tHint);
        }
        private static void RestoreObjectRefs(DataGraphNodeBase node, List<NodeObjectRefEntry> refs)
        {
            if (refs == null) return;
            
            foreach (var r in refs)
            {
                if (string.IsNullOrEmpty(r.fieldPath))
                    continue;
                
                var fi = FindFieldByPath(node.GetType(), r.fieldPath);
                
                if (fi == null) continue;
                
                UnityEngine.Object val = null;
                
                if (!string.IsNullOrEmpty(r.guid))
                    val = LoadByGuidLocalId(r.guid, r.localId, fi.FieldType);
                
                if (val) fi.SetValue(node, val);
            }
        }
        #endregion
    }
}