using UnityEditor;
using UnityEngine;
using System;

namespace LUIZ.DataGraph
{
	public interface IDataGraphNode
    {
        void RegenerateNodeID();
        void SetPosition(Rect position);
    }

	[System.Serializable]
    public abstract class DataGraphNodeBase : IDataGraphNode
    {
        [SerializeField] private ulong m_nodeID;
        [SerializeField] private Rect m_position;
        
        [HideInGraphInspector] public ulong NodeID => m_nodeID;
        [HideInGraphInspector] public Rect Position => m_position;
        
        //---------------------------------------------------------
        [GraphInspectorHeader("Node Info")] 
        public string Name;
        public string Description;

        //---------------------------------------------------------
        void IDataGraphNode.RegenerateNodeID()
        {
            m_nodeID = IDGenerator.NewID();
        }

        void IDataGraphNode.SetPosition(Rect position)
        {
            m_position = position;
        }

        //---------------------------------------------------------
        public virtual DataGraphNodeBase Clone() //그래프 뷰에서 노드를 복사 붙여넣기 할때 이용한다.
        {
            var copy = this.MemberwiseClone() as IDataGraphNode;

            // 새로운 GUID
            copy.RegenerateNodeID();

            // 살짝 위치 이동
            var newPos = this.Position;
            newPos.position += new Vector2(30, 30);
            copy.SetPosition(newPos);

            return copy as DataGraphNodeBase;
        }
#if UNITY_EDITOR
        public event Action<DataGraphNodeBase> OnEditorNodeValueChanged;
        public void NotifyNodeValueChangedEvent()
        {
            OnEditorNodeValueChanged?.Invoke(this);
        }
        
        //-------------------------------------------------
        public virtual void OnEditorNodeCreate() { }
        public virtual void OnEditorNodeDestroy() { }
#endif
    }
}