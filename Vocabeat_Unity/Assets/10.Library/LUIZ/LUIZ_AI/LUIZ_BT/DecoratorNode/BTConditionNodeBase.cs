using UnityEngine;

namespace LUIZ.AI.BT
{
    public abstract class BTConditionNodeBase : BTDecoratorNodeBase
    {
        public override sealed EBTNodeState DoEvaluate()
        {
            if(OnConditionEvaluate() == true)
            {
                return m_ChildNode.DoEvaluate();
            }
            else
            {
                return EBTNodeState.Failure;
            }
        }

        //-----------------------------------------------
        protected abstract bool OnConditionEvaluate();
    }
}
