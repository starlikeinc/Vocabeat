using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LUIZ.DataGraph;

[NodeInfo("DynamicNode", "~Samples/SampleDynamicNode")]
public class SampleDynamicNode : DataGraphNodeBase, IInputPortProvider, IDynamicOutputPortProvider
{
    [SerializeField, HideInGraphInspector]
    private DynamicPortHandler m_dynamicPortHandler = new("Next DynamicContent", typeof(SampleDynamicContentNode),5, 5);
    
    //-------------------------------------------------------------------------------------
    public IEnumerable<PortDefinition> GetInputPorts()
    {
        yield return new PortDefinition("Prev MainNode", 2,2, typeof(SampleMainNode), 0, 1);
    }
    
    public IEnumerable<PortDefinition> GetOutputPorts()
    {
        yield return new PortDefinition("Next MainNode",2,2, typeof(SampleMainNode), 0,1);
        foreach (var port in m_dynamicPortHandler.ListDynamicPorts)
            yield return port;
    }
    public void AddDynamicOutputPort() => m_dynamicPortHandler.AddPort();
    public void RemoveDynamicOutputPortByID(int id) => m_dynamicPortHandler.RemovePortByID(id);
    public int DynamicOutputPortCount => m_dynamicPortHandler.ListDynamicPorts.Count;
}
