using UnityEngine;
using LUIZ.DataGraph;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DataGraph", menuName = "LUIZ/DataGraph/SampleGraph")]
public class SampleGraph : DataGraph
{
    [SerializeField] private int m_firstNodeID = 0;
    
    
    protected override void OnBeforeAssetSave()
    {
        base.OnBeforeAssetSave();
        //커스텀 데이터 조작이 가능
        //모든 노드들의 첫번째 ID를 가져온다던가, 스킬관련 정보만 따로 가져온다던가 등...
        
    }
}
