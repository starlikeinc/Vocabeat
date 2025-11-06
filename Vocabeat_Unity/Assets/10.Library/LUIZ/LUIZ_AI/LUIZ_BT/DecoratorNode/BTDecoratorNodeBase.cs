using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.AI.BT
{
    public abstract class BTDecoratorNodeBase : IBTNode
    {
        protected IBTNode m_ChildNode = null;

        //--------------------------------------------------------
        public void DoAddChild(IBTNode childNode)
        {
            if (m_ChildNode != null)
            {
                Debug.LogWarning($"[BTDecoratorNodeBase] Decorator node can only have 1 child. " +
                    $"Replaced child {m_ChildNode.GetType().Name} => {childNode.GetType().Name}");
            }

            m_ChildNode = childNode;
        }

        public abstract EBTNodeState DoEvaluate();
    }
}
