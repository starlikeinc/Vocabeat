using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.AI.BT
{
    public abstract class BTCompositeNodeBase : IBTNode
    {
        protected List<IBTNode> m_listChildNodes = null;

        //--------------------------------------------------------
        public void DoAddChild(IBTNode node)
        {
            if (m_listChildNodes == null)
                m_listChildNodes = new List<IBTNode>();

            m_listChildNodes.Add(node);
        }

        public abstract EBTNodeState DoEvaluate();
    }
}
