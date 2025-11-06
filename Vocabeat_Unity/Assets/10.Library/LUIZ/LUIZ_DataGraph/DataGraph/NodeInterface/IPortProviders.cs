using System;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.DataGraph
{
    public interface IInputPortProvider
    {
        IEnumerable<PortDefinition> GetInputPorts();
    }
    public interface IDynamicInputPortProvider : IInputPortProvider
    {
        int DynamicInputPortCount { get; }
        void AddDynamicInputPort();
        void RemoveDynamicInputPortByID(int portID);
    }
    
    public interface IOutputPortProvider
    {
        IEnumerable<PortDefinition> GetOutputPorts();
    }
    public interface IDynamicOutputPortProvider : IOutputPortProvider
    {
        int DynamicOutputPortCount { get; }
        void AddDynamicOutputPort();
        void RemoveDynamicOutputPortByID(int portID);
    }
    
    [System.Serializable]
    public struct PortDefinition
    {
        [SerializeField] private string m_name;
        [SerializeField] private int m_portID;
        [SerializeField] private string m_acceptedTypeName;
        [SerializeField] private int m_minConnectionCount;
        [SerializeField] private int m_maxConnectionCount;
        [SerializeField] private bool m_isDynamic;
        [SerializeField] private int m_channelID; //채널 키(같아야 연결 가능) 미지정 시 PortID와 동일하게 저장해 역호환
        
        public string Name => m_name;
        public int PortID => m_portID;
        public int ChannelID => (m_channelID == 0) ? m_portID : m_channelID;
        public int MinConnectionCount => m_minConnectionCount;
        public int MaxConnectionCount => m_maxConnectionCount;
        public bool IsDynamic => m_isDynamic;
        
        //Type은 직렬화가 안되므로 요청 시 변환해서 반환
        public System.Type AcceptedType => string.IsNullOrEmpty(m_acceptedTypeName)
            ? null
            : System.Type.GetType(m_acceptedTypeName) ?? NodeTypeResolver.Resolve(m_acceptedTypeName);

        /// <summary>
        /// 포트의 유효성 검사 순서 => 1. acceptType으로 연결 가능한 노드를 검사. 2.이후 해당 노드의 포트들 중 portID가 동일한 포트끼리만 연결이 가능.
        /// minConnectionCount = 해당 포트에서 연결되어야하는 최소 노드 갯수
        /// maxConnectionCount = 해당 포트에서 연결 가능한 최대 노드 갯수. -1 은 무한을 의미함
        /// </summary>
        /// <summary>
        /// 유효성: 1) AcceptedType 호환  2) ChannelID 동일  3) 연결수 제한
        /// </summary>
        public PortDefinition(string name, int channelID, int portID, Type acceptedType, int minConnectionCount = 0, int maxConnectionCount = -1, bool isDynamic = false)
        {
            m_name = name;
            m_portID = portID;
            m_acceptedTypeName = acceptedType != null ? DataGraphTypeNames.ToShort(acceptedType) : string.Empty;
            m_minConnectionCount = minConnectionCount;
            m_maxConnectionCount = maxConnectionCount;
            m_isDynamic = isDynamic;
            m_channelID = channelID;
        }
    }
}
