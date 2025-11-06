using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.DataGraph
{
    public interface IRuntimeInputBinder
    {
        //toPortID: 수신(입력) 포트ID, upstream: 연결된 상위 런타임 노드
        public void BindInput(int toPortID, RuntimeGraphNodeBase upstream);
    }
    
    public interface IRuntimeOutputBinder
    {
        public void BindOutput(int fromPortID, RuntimeGraphNodeBase downstream);
    }
    
    public abstract class RuntimeGraphNodeBase
    {
        private readonly List<RuntimeGraphNodeBase> m_nextNodes = new(4);
        private readonly List<RuntimeGraphNodeBase> m_prevNodes = new(4);
        
        //-----------------------------------------------
        public ulong NodeID { get; private set; }
        
        protected IReadOnlyList<RuntimeGraphNodeBase> NextNodes => m_nextNodes;
        protected IReadOnlyList<RuntimeGraphNodeBase> PrevNodes => m_prevNodes;
        
        //-----------------------------------------------
        internal void SetNodeID(ulong nodeID) => NodeID = nodeID;
        internal void AddNext(RuntimeGraphNodeBase node)
        {
            if (!m_nextNodes.Contains(node))
            {
                m_nextNodes.Add(node);
                OnAddNext(node);
            }
        }
        internal void AddPrev(RuntimeGraphNodeBase node)
        {
            if (!m_prevNodes.Contains(node))
            {
                m_prevNodes.Add(node);
                OnAddPrev(node);
            }
        }

        //-----------------------------------------------
        protected virtual void OnAddNext(RuntimeGraphNodeBase node) { }
        protected virtual void OnAddPrev(RuntimeGraphNodeBase node) { }
    }
}