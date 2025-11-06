using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LUIZ.DataGraph;

[NodeInfo("MainNode", "~Samples/SampleMainNode (Root)")]
public class SampleMainNode : DataGraphNodeBase, IInputPortProvider, IOutputPortProvider
{
	[GraphInspectorHeader("MainNodeItems")]
	public List<SampleItemData> ListItems = new List<SampleItemData>();
	
	//---------------------------------------------------------------
	public IEnumerable<PortDefinition> GetInputPorts()
	{
		yield return new PortDefinition("Prev MainNode",1,1, typeof(SampleMainNode),0,1);
	}

	public IEnumerable<PortDefinition> GetOutputPorts()
	{
		yield return new PortDefinition("Next MainNode",1,1, typeof(SampleMainNode),0,1);
		yield return new PortDefinition("Next DynamicNode",2,2, typeof(SampleDynamicNode),0,1);
	}
}

