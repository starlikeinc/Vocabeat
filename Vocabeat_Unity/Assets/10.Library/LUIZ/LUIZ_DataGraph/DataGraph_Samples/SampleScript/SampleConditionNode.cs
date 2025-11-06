using System.Collections.Generic;
using LUIZ.DataGraph;
using UnityEngine;

[NodeInfo("ConditionNode", "~Samples/SampleStep/SampleStepCondition", 0.5f, 0.5f, 0.3f)]
public class SampleConditionNode : DataGraphNodeBase, IOutputPortProvider
{
    [NodeBody("Item")]
    public ESampleItemType RequiredItem;
    [NodeBody("Amount")]
    public int RequiredAmount;
    
    //----------------------------------------------------
    public IEnumerable<PortDefinition> GetOutputPorts()
    {
        yield return new PortDefinition("SampleStep", 2,2, typeof(SampleStepNode));
    }
}
