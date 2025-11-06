using System;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.DataGraph
{
    [System.Serializable]
    public class DynamicPortHandler
    {
        [SerializeField] private List<PortDefinition> m_listDynamicPorts = new();
        [SerializeField] private int m_lastRemovedID = -1;

        //-------------------------------------------------------------
        [NonSerialized] private int m_maxPortCount;
        [NonSerialized] private string m_portName;
        [NonSerialized] private Type m_portType;

        [NonSerialized] private int m_startID;
        [NonSerialized] private int m_channelID;
        //-------------------------------------------------------------
        public IReadOnlyList<PortDefinition> ListDynamicPorts => m_listDynamicPorts;

        //-------------------------------------------------------------
        public DynamicPortHandler(string portName, Type portType, int channelID, int startID, int maxPortCount = 8)
        {
            m_portName = portName;
            m_portType = portType;
            m_maxPortCount = maxPortCount;
            m_startID = startID;
            m_channelID = channelID;
            //기본 1개 생성
            AddPort();
        }

        //-------------------------------------------------------------
        public void AddPort()
        {
            if (m_listDynamicPorts.Count >= m_maxPortCount)
            {
                Debug.LogWarning($"[DynamicPortHandler] 최대 {m_maxPortCount}개까지 생성 가능합니다.");
                return;
            }

            int newID = GetNextAvailableID();
            if (newID == -1)
            {
                Debug.LogError("[DynamicPortHandler] 사용 가능한 ID가 없습니다.");
                return;
            }

            m_listDynamicPorts.Add(new PortDefinition(m_portName, m_channelID, newID, m_portType, 0, 1, true));//모든 동적 포트는 채널을 9로 했음
        }

        public void RemovePortByID(int id)
        {
            int idx = m_listDynamicPorts.FindIndex(p => p.PortID == id);
            if (idx >= 0)
            {
                m_listDynamicPorts.RemoveAt(idx);
                m_lastRemovedID = id;
            }
            else
            {
                Debug.LogError($"[DynamicPortHandler] ID {id} 삭제 실패: 없음");
            }
        }

        private int GetNextAvailableID()
        {
            const int c_maxBits = 512; //최대 ID 수 == 최대 비트 수
            const int c_bitsPerBlock = 64; //ulong으로 선언해서 한칸은64bit

            int bitArrayLength = (c_maxBits + c_bitsPerBlock - 1) / c_bitsPerBlock;
            Span<ulong> usedBits = stackalloc ulong[bitArrayLength];

            foreach (var port in m_listDynamicPorts)
            {
                int id = port.PortID;
                if (id >= m_startID && id < c_maxBits) //사용중인 id 에 플래그표시
                {
                    int index = id / c_bitsPerBlock;
                    int bit = id % c_bitsPerBlock;
                    usedBits[index] |= 1UL << bit;
                }
            }

            for (int id = m_startID; id < c_maxBits; id++)
            {
                if (id == m_lastRemovedID)
                    continue;

                int index = id / c_bitsPerBlock;
                int bit = id % c_bitsPerBlock;
                if ((usedBits[index] & (1UL << bit)) == 0)
                    return id;
            }

            //최후의 수단으로.. last removed ID 재사용 허용
            if (m_lastRemovedID >= m_startID && m_lastRemovedID < c_maxBits)
            {
                int index = m_lastRemovedID / c_bitsPerBlock;
                int bit = m_lastRemovedID % c_bitsPerBlock;
                if ((usedBits[index] & (1UL << bit)) == 0)
                    return m_lastRemovedID;
            }

            return -1;
        }
    }
}