using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.AI.BT
{
    public enum EBTNodeState
    {
        Running,
        Success,
        Failure
    }

    public interface IBTNode
    {
        public EBTNodeState DoEvaluate();
    }
}
