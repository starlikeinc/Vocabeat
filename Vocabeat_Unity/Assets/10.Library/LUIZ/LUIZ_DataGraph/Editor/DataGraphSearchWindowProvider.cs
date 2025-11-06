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
	public struct SearchContextElement
	{
		public object target { get; private set; }
		public string title { get; private set; }

		public SearchContextElement(object target, string title)
		{
			this.target = target;
			this.title = title;
		}
	}

    public class DataGraphSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private static readonly List<SearchContextElement> c_listElements = new List<SearchContextElement>();

        public DataGraphViewEditor GraphViewEditor { get; set; }
        public Port StartPort { get; set; }

        //---------------------------------------------------------
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            c_listElements.Clear();

            if (!TryCollectNodeElements(out var errorTree))
                return errorTree;

            c_listElements.Sort(CompareSearchContextElements);

            return BuildSearchTreeFromElements();
        }

        //---------------------------------------------------------
        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            var windowMousePosition = GraphViewEditor.ChangeCoordinatesTo(GraphViewEditor,
                context.screenMousePosition - GraphViewEditor.GraphEditorWindow.position.position);
            var graphMousePosition = GraphViewEditor.contentViewContainer.WorldToLocal(windowMousePosition);

            var element = (SearchContextElement)SearchTreeEntry.userData;
            var newNode = (DataGraphNodeBase)element.target;

            ((IDataGraphNode)newNode).SetPosition(new Rect(graphMousePosition, Vector2.zero));
            newNode.OnEditorNodeCreate();
            GraphViewEditor.ClearSelection();
            GraphViewEditor.CreateNode(newNode, true);
            
            // 자동 연결 처리
            if (StartPort != null)
            {
                var newNodeEditor = GraphViewEditor.GetNodeEditorByID(newNode.NodeID);
                if (newNodeEditor != null)
                {
                    var candidatePorts = newNodeEditor.Ports;

                    foreach (var targetPort in candidatePorts)
                    {
                        if (targetPort.direction == StartPort.direction)
                            continue;

                        if (GraphViewEditor.ArePortsCompatible(StartPort, targetPort))
                        {
                            // 연결 제한 체크
                            if (!GraphViewEditor.IsPortConnectionLimitExceeded(StartPort) &&
                                !GraphViewEditor.IsPortConnectionLimitExceeded(targetPort))
                            {
                                var edge = StartPort.direction == Direction.Output
                                    ? StartPort.ConnectTo(targetPort)
                                    : targetPort.ConnectTo(StartPort);

                                GraphViewEditor.AddElement(edge);
                                GraphViewEditor.NotifyEdgeCreated(edge); // CreateEdge 호출용

                                GraphViewEditor.UpdatePortVisualStatesForNode(newNodeEditor);
                                GraphViewEditor.UpdatePortVisualStatesForNode(StartPort.node as DataGraphNodeBaseEditor);
                                break;
                            }
                        }
                    }
                }
            }
            StartPort = null; //StartPort 리셋
            return true;
        }

        //---------------------------------------------------------
        private bool TryCollectNodeElements(out List<SearchTreeEntry> errorTree)
        {
            errorTree = null;

            var assemblyNames = DataGraphSettings.Instance?.NodeAssemblyNames ?? new List<string>();
            if (assemblyNames.Count == 0)
            {
                Debug.LogWarning("[DataGraphSearchWindowProvider] No assembly names set in DataGraphSettings.");
                errorTree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent("No Assembly Set")) };
                return false;
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => assemblyNames.Contains(a.GetName().Name));

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract)
                        continue;
                    
                    var nodeInfo = type.GetCustomAttribute<NodeInfoAttribute>();
                    if (nodeInfo == null || string.IsNullOrEmpty(nodeInfo.MenuItem))
                        continue;

                    var node = Activator.CreateInstance(type);
                    if (node is IDataGraphNode idAssignable)
                        idAssignable.RegenerateNodeID();

                    if (StartPort != null && !IsCompatibleWithStartPort(type, node))
                        continue;

                    c_listElements.Add(new SearchContextElement(node, nodeInfo.MenuItem));
                }
            }

            return true;
        }

        private bool IsCompatibleWithStartPort(Type curNodeType, object node)
        {
            //메타가 없으면 필터 안 함
            if (StartPort?.userData is not PortMetaData startMeta)
                return true;

            var startNodeType    = (StartPort.node as DataGraphNodeBaseEditor)?.Node?.GetType();
            var startChannelID   = startMeta.ChannelID;
            var requiredNodeType = startMeta.AcceptedType; //null일 수 있음
            
            if (StartPort.direction == Direction.Output && node is IInputPortProvider inProv)
            {
                //시작 포트가 요구하는 타입이 있으면 노드 타입 1차 필터
                if (requiredNodeType != null && !requiredNodeType.IsAssignableFrom(node.GetType()))
                    return false;

                foreach (var def in inProv.GetInputPorts())
                {
                    //채널은 항상 강제
                    if (def.ChannelID != startChannelID) continue;
                    
                    if (def.AcceptedType != null && startNodeType != null &&
                        !def.AcceptedType.IsAssignableFrom(startNodeType))
                        continue;

                    return true; // 하나라도 맞으면 후보
                }
                return false;
            }
            else if (StartPort.direction == Direction.Input && node is IOutputPortProvider outProv)
            {
                if (requiredNodeType != null && !requiredNodeType.IsAssignableFrom(node.GetType()))
                    return false;

                foreach (var def in outProv.GetOutputPorts())
                {
                    if (def.ChannelID != startChannelID) continue;

                    if (def.AcceptedType != null && startNodeType != null &&
                        !def.AcceptedType.IsAssignableFrom(startNodeType))
                        continue;

                    return true;
                }
                return false;
            }

            return false;
        }

        //---------------------------------------------------------
        private List<SearchTreeEntry> BuildSearchTreeFromElements()
        {
            var tree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent("Nodes"), 0) };
            var listGroups = new HashSet<string>();

            foreach (var element in c_listElements)
            {
                var splits = element.title.Split('/');
                var groupPath = "";

                for (int i = 0; i < splits.Length - 1; i++)
                {
                    groupPath += splits[i];
                    if (!listGroups.Contains(groupPath))
                    {
                        tree.Add(new SearchTreeGroupEntry(new GUIContent(splits[i]), i + 1));
                        listGroups.Add(groupPath);
                    }

                    groupPath += "/";
                }

                var entry = new SearchTreeEntry(
                    new GUIContent(splits.Last(), EditorGUIUtility.IconContent("ScriptableObject Icon").image))
                {
                    level = splits.Length,
                    userData = element
                };
                tree.Add(entry);
            }
            
            return tree;
        }

        private int CompareSearchContextElements(SearchContextElement a, SearchContextElement b)
        {
            var aSplits = a.title.Split('/');
            var bSplits = b.title.Split('/');

            for (int i = 0; i < aSplits.Length; i++)
            {
                if (i >= bSplits.Length) return 1;
                var cmp = aSplits[i].CompareTo(bSplits[i]);
                if (cmp != 0)
                {
                    if (aSplits.Length != bSplits.Length &&
                        (i == aSplits.Length - 1 || i == bSplits.Length - 1))
                    {
                        return aSplits.Length < bSplits.Length ? 1 : -1;
                    }

                    return cmp;
                }
            }

            return 0;
        }
    }
}