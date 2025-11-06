using System;
using System.Collections.Generic;

namespace LUIZ.DataGraph
{
    public static class RuntimeGraphBuilder
    {
        private static readonly Dictionary<Type, Func<DataGraphNodeBase, RuntimeGraphNodeBase>> c_dicNodeBuilders = new();

        /// <summary> T(DataGraphNodeBase상속받음)를 원하는 RuntimeGraphNodeBase로 변환하는 빌더 등록  </summary>
        public static void Register<TGraphNode, TRuntimeNode>(Func<TGraphNode, TRuntimeNode> builderFunc) where TGraphNode : DataGraphNodeBase where TRuntimeNode : RuntimeGraphNodeBase
        {
            c_dicNodeBuilders[typeof(TGraphNode)] = node => builderFunc((TGraphNode)node);
        }

        /// <summary>
        /// 실제 DataGraph의 노드들을 런타임 데이터로 변환
        /// onNodeCreation을 이용하여 타입 캐스팅 등을 통해 시작노드를 받아와 추후 Execute 하거나 커스텀 로직을 실행할 수 도 있음.
        /// </summary>
        public static void BuildRuntimeGraph(DataGraph dataGraph, ref Dictionary<ulong, RuntimeGraphNodeBase> dicRuntimeNodes, 
            Action<DataGraphNodeBase, RuntimeGraphNodeBase> onNodeCreation = null)
        {
            if (dataGraph == null)
            {
                throw new NotSupportedException($"[RuntimeGraphBuilder] dataGraph is NULL!!!!");
                return;
            }

            dicRuntimeNodes ??= new Dictionary<ulong, RuntimeGraphNodeBase>();
            dicRuntimeNodes.Clear();
            
            //노드 생성
            foreach (var nodeData in dataGraph.Nodes)
            {
                if (c_dicNodeBuilders.TryGetValue(nodeData.GetType(), out var builder))
                {
                    var runtimeNode = builder(nodeData);
                    
                    runtimeNode.SetNodeID(nodeData.NodeID);
                    dicRuntimeNodes[nodeData.NodeID] = runtimeNode;

                    onNodeCreation?.Invoke(nodeData, runtimeNode);
                }
                else
                {
                    throw new NotSupportedException($"[RuntimeGraphBuilder] No builder registered for type {nodeData.GetType()}");
                }
            }

            //엣지 연결
            foreach (var edge in dataGraph.Edges)
            {
                if (!dicRuntimeNodes.TryGetValue(edge.OutputPort.NodeID, out var fromNode)) continue;
                if (!dicRuntimeNodes.TryGetValue(edge.InputPort.NodeID,  out var toNode))   continue;
                
                Connect(fromNode, toNode, edge.OutputPort.PortID, edge.InputPort.PortID);
            }
        }
        
        private static void Connect(RuntimeGraphNodeBase from, RuntimeGraphNodeBase to, int fromPortID, int toPortID)
        {
            from.AddNext(to);
            to.AddPrev(from);

            //노드가 원하면 포트ID 기반으로 직접 바인딩
            if (to is IRuntimeInputBinder  inBinder) 
                inBinder.BindInput(toPortID, from);
            if (from is IRuntimeOutputBinder outBinder)
                outBinder.BindOutput(fromPortID, to);
        }
    }
}