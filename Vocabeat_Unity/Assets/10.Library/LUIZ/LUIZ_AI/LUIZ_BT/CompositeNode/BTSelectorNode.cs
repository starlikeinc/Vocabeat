using UnityEngine;

namespace LUIZ.AI.BT
{
    public class BTSelectorNode : BTCompositeNodeBase
    {
        //자식 노드 중에서 처음으로 Success 나 Running 상태를 가진 노드가 발생하면 그 노드까지 진행하고 멈춘다.
        public override EBTNodeState DoEvaluate()
        {
            if (m_listChildNodes == null)
                return EBTNodeState.Failure;

            foreach(IBTNode childNode in m_listChildNodes)
            {
                switch (childNode.DoEvaluate())
                {
                    case EBTNodeState.Failure:
                        continue;
                    case EBTNodeState.Running:
                        return EBTNodeState.Running;
                    case EBTNodeState.Success:
                        return EBTNodeState.Success;
                }
            }

            return EBTNodeState.Failure;
        }
    }
}
