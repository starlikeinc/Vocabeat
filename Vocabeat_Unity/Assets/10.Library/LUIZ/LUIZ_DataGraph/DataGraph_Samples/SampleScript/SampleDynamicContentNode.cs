using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LUIZ.DataGraph;

[FormerName("SampleDynamicContentNode33, LUIZ.DataGraphSamples")]
[FormerName("SampleDynamicContentNode2, LUIZ.DataGraphSamples")]
[FormerName("SampleDynamicContentNodeTest, LUIZ.DataGraphSamples")]
[NodeInfo("DynamicContentNode", "~Samples/SampleDynamicContentNode")]
public class SampleDynamicContentNode : DataGraphNodeBase, IInputPortProvider
{
    public IEnumerable<PortDefinition> GetInputPorts()
    {
        yield return new PortDefinition("Prev DynamicNode", 5, 1, typeof(SampleDynamicNode), 1, 1);
    }
}
