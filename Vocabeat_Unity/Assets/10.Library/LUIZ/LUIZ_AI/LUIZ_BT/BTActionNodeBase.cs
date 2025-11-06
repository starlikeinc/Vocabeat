using UnityEngine;

namespace LUIZ.AI.BT
{
    public abstract class BTActionNodeBase : IBTNode
    {
        public EBTNodeState DoEvaluate()
        {
            return DoAction();
        }

        //--------------------------------------------------
        public abstract EBTNodeState DoAction();
    }
}
