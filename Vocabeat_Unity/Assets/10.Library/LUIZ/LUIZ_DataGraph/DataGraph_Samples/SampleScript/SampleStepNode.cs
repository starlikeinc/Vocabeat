using UnityEngine;
using LUIZ.DataGraph;
using System.Collections.Generic;

[NodeInfo("StepNode", "~Samples/SampleStep/SampleStep"), System.Serializable]
public class SampleStepNode : DataGraphNodeBase, IInputPortProvider, IOutputPortProvider
{
	//-----------------------------------------------------------------
	public IEnumerable<PortDefinition> GetInputPorts()
	{
		yield return new PortDefinition("Prev Step",1,1, typeof(SampleStepNode));
		yield return new PortDefinition("Conditions",2,2, typeof(SampleConditionNode),1);
	}

	public IEnumerable<PortDefinition> GetOutputPorts()
	{
		yield return new PortDefinition("Next Step",1,1, typeof(SampleStepNode),1);
	}
}
