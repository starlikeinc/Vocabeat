using UnityEngine;

namespace LUIZ.AI.BT
{
    public class BTSequenceNode : BTCompositeNodeBase
    {
        public override EBTNodeState DoEvaluate()
        {
            if (m_listChildNodes == null || m_listChildNodes.Count <= 0)
                return EBTNodeState.Failure;

            foreach (IBTNode childNode in m_listChildNodes)
            {
                switch (childNode.DoEvaluate())
                {
                    case EBTNodeState.Success://다음 노드 재생
                        continue;
                    case EBTNodeState.Running:
                        return EBTNodeState.Running;//다음 Evaluate 시에도 Running을 유지해야함
                    case EBTNodeState.Failure:
                        return EBTNodeState.Failure;
                }
            }

            return EBTNodeState.Success;
        }
    }
}
