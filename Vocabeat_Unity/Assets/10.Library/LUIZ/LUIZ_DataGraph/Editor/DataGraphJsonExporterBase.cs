using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LUIZ.DataGraph.Editor
{
    public abstract class DataGraphJsonExporterBase : ScriptableObject
    {
        public abstract void ExportGraphToJson(DataGraph graph);
        
        //-------------------------------------------------------------------
        protected List<T> GetIncomingNodesOfType<T>(DataGraph graph, DataGraphNodeBase targetNode) where T : DataGraphNodeBase
        {
            return graph.GetIncomingNodes(targetNode)
                .OfType<T>()
                .ToList();
        }

        protected List<T> GetOutgoingNodesOfType<T>(DataGraph graph, DataGraphNodeBase targetNode) where T : DataGraphNodeBase
        {
            return graph.GetOutgoingNodes(targetNode)
                .OfType<T>()
                .ToList();
        }
    }
}