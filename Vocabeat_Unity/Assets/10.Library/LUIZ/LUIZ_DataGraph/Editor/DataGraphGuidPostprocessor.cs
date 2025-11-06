using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using LUIZ.DataGraph;

namespace LUIZ.DataGraph.Editor
{
    //Unity 에디터에서 DataGraph에셋을 복제(Ctrl+D) 시, 중복 or 빈 GraphID 및 NodeID를 자동으로 감지하고 새로 재할당한 뒤 자동 저장까지 수행
    public class DataGraphGuidPostprocessor : AssetPostprocessor
    {
        //-------------------------------------------------
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var listOpenedGraphs = new List<DataGraph>(Resources.FindObjectsOfTypeAll<DataGraph>());
            foreach (string path in importedAssets)
            {
                if (!path.EndsWith(".asset")) continue;

                var graph = AssetDatabase.LoadAssetAtPath<DataGraph>(path);
                if (graph == null) continue;

                //다음 프레임에서 중복 검사 및 저장을 수행 (지연 처리)
                //코드 수정시 c_dicGraphIDToPaths은 static 이며 실제 이용 함수들은 delayCall이라는거에 유의
                var capturedGraph = graph;
                var capturedPath = path;
                EditorApplication.delayCall += () => { HandleGraphImportWithDelay(capturedGraph, capturedPath, listOpenedGraphs); };
            }
        }

        private static void HandleGraphImportWithDelay(DataGraph graph, string path, List<DataGraph> listOpenedGraphs)
        {
            if (graph == null) return;

            var so = new SerializedObject(graph);
            var graphIDProp = so.FindProperty("m_graphID");
            if (graphIDProp == null)
            {
                Debug.LogWarning("[DataGraphGuidPostprocessor] m_graphID 검색 불가! 필드 이름이 바뀌었는지 확인 필요.");
                return;
            }
            
            var latestMap = new Dictionary<ulong, List<string>>();
            BuildGraphIDDic(AssetDatabase.FindAssets("t:DataGraph"), latestMap);
            
            ulong myGraphID = graphIDProp.ulongValue;
            if (myGraphID == 0)//그래프ID가 비어 있으면 새로 할당...(일반적으로 DataGraph 생성자에서 지정됨)
            {
                graphIDProp.ulongValue = IDGenerator.NewID();
                so.ApplyModifiedProperties();
                graph.NotifyBeforeSaveEditor();
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
                Debug.Log($"[DataGraphGuidPostprocessor] 그래프 ID가 비어있는 그래프에 ID 할당 및 저장 완료: {path}");
                return;
            }

            //복제본인지 판별
            bool isDuplicateOfOpenedGraph = false;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(graph, out string thisGuid, out _);
            DateTime myWriteTime = System.IO.File.GetLastWriteTime(path);
            foreach (var opened in listOpenedGraphs)
            {
                if (opened == graph) continue;
                if (opened.GraphID == 0) continue;
                if (opened.GraphID != myGraphID) continue;

                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(opened, out string openedGuid, out _);
                if (thisGuid == openedGuid) continue;

                string openedPath = AssetDatabase.GetAssetPath(opened);
                if (string.IsNullOrEmpty(openedPath)) continue;

                DateTime openedWriteTime = System.IO.File.GetLastWriteTime(openedPath);
                if (myWriteTime > openedWriteTime)
                {
                    isDuplicateOfOpenedGraph = true;
                    break;
                }
            }
            if (!isDuplicateOfOpenedGraph)
                return;

            //디스크 상에서도 동일한 ID를 가진 에셋이 존재하면 복제된 것으로 판단
            if (!HasDuplicateGraphID(graph, myGraphID, path, latestMap))
                return;

            //복제본임이 확인된 경우 ID 및 구성 요소 ID 재할당
            graphIDProp.ulongValue = IDGenerator.NewID();
            so.ApplyModifiedProperties();

            var BF = BindingFlags.NonPublic | BindingFlags.Instance;
            
            //노드 데이터 재할당
            var fSerializedNodes = typeof(DataGraph).GetField("m_serializedNodes", BF);
            var serNodes = (List<NodeRecord>)fSerializedNodes.GetValue(graph);
            var nodeMap = new Dictionary<ulong, ulong>(serNodes.Count);
            for (int i = 0; i < serNodes.Count; ++i) {
                var rec = serNodes[i];
                ulong oldId = rec.nodeId;
                ulong newId = IDGenerator.NewID();
                nodeMap[oldId] = newId;
                rec.nodeId = newId; // JSON/objRefs는 건드리지 않음
            }
            
            //메모리 올라온거도 다 교체
            var fListNodes = typeof(DataGraph).GetField("m_listNodes", BF);
            var runtimeNodes = (List<DataGraphNodeBase>)fListNodes.GetValue(graph);
            if (runtimeNodes != null) {
                var fNodeID = typeof(DataGraphNodeBase).GetField("m_nodeID", BF);
                for (int i = 0; i < runtimeNodes.Count; ++i) {
                    var n = runtimeNodes[i];
                    ulong oldId = (ulong)fNodeID.GetValue(n);
                    if (nodeMap.TryGetValue(oldId, out var newId))
                        fNodeID.SetValue(n, newId);
                }
            }
            
            //간선 정보 갱신
            var updatedEdges = new List<DataGraphEdge>();
            foreach (var e in graph.Edges) {
                updatedEdges.Add(new DataGraphEdge(
                    new DataGraphEdgePort(nodeMap.TryGetValue(e.OutputPort.NodeID, out var nOut) ? nOut : e.OutputPort.NodeID, e.OutputPort.PortID),
                    new DataGraphEdgePort(nodeMap.TryGetValue(e.InputPort.NodeID,  out var nIn)  ? nIn  : e.InputPort.NodeID,  e.InputPort.PortID)
                ));
            }
            typeof(DataGraph).GetField("m_listEdges", BF)?.SetValue(graph, updatedEdges);

            //그룹 정보 갱신
            var updatedGroups = new List<DataGraphNodeGroup>();
            foreach (var g in graph.Groups) {
                var ng = new DataGraphNodeGroup {
                    GroupID = g.GroupID,
                    Title = g.Title,
                    Color = g.Color,
                    Position = g.Position,
                    ContainedNodeIDs = new List<ulong>(g.ContainedNodeIDs.Count)
                };
                foreach (var old in g.ContainedNodeIDs)
                    ng.ContainedNodeIDs.Add(nodeMap.TryGetValue(old, out var nw) ? nw : old);
                updatedGroups.Add(ng);
            }
            typeof(DataGraph).GetField("m_listNodeGroups", BF)?.SetValue(graph, updatedGroups);
            
            
            var pendingFI = typeof(DataGraph).GetField("m_dicPendingJsonByNodeID", BF);
            var newPending = new Dictionary<ulong, string>(serNodes.Count);
            foreach (var rec in serNodes)
                newPending[rec.nodeId] = rec.json;
            pendingFI.SetValue(graph, newPending);
            
            typeof(DataGraph).GetField("m_isJsonApplied", BF)?.SetValue(graph, false);
            typeof(DataGraph).GetMethod("ApplyPendingJson", BF)?.Invoke(graph, null);
            
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();

            EditorApplication.delayCall += () =>
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                //UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            };
            Debug.Log($"[DataGraphGuidPostprocessor] 복제로 인한 GraphID 중복 감지 → 복제본의 ID 및 NodeID 재할당 완료!\npath : {path}");
        }

        private static bool HasDuplicateGraphID(DataGraph currentGraph, ulong myGraphID, string myPath, Dictionary<ulong, List<string>> dicGraphIDToPaths)
        {
            if (!dicGraphIDToPaths.TryGetValue(myGraphID, out var paths))
                return false;

            //동일 경로를 제외한 동일 ID 존재시 중복으로 판단
            foreach (var path in paths)
            {
                if (path != myPath)
                    return true;
            }

            return false;
        }
        
        private static void BuildGraphIDDic(string[] graphGuids, Dictionary<ulong, List<string>> dic)
        {
            dic.Clear();
            
            foreach (var guid in graphGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var graph = AssetDatabase.LoadAssetAtPath<DataGraph>(path);
                if (graph == null) continue;

                var so = new SerializedObject(graph);
                var prop = so.FindProperty("m_graphID");
                if (prop == null || prop.ulongValue == 0) continue;

                if (!dic.TryGetValue(prop.ulongValue, out var pathList))
                {
                    pathList = new List<string>();
                    dic[prop.ulongValue] = pathList;
                }

                pathList.Add(path);
            }
        }
    }
}