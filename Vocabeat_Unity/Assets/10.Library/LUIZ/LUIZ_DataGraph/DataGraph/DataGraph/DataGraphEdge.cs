using UnityEngine;

namespace LUIZ.DataGraph
{
    [System.Serializable]
    public struct DataGraphEdge
    {
        public DataGraphEdgePort OutputPort; 
        public DataGraphEdgePort InputPort;

        public DataGraphEdge(DataGraphEdgePort outputPort, DataGraphEdgePort inputPort)
        {
            this.OutputPort = outputPort;
            this.InputPort = inputPort;
        }
        
        //-----------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (obj is DataGraphEdge other)
                return OutputPort.Equals(other.OutputPort) && InputPort.Equals(other.InputPort);
            return false;
        }
        
        public override int GetHashCode()
        {
            return OutputPort.GetHashCode() ^ InputPort.GetHashCode();
        }
    }

    [System.Serializable]
    public struct DataGraphEdgePort
    {
        [SerializeField] public ulong NodeID;
        [SerializeField] public int PortID;

        public DataGraphEdgePort(ulong nodeId, int portID)
        {
            this.NodeID = nodeId;
            this.PortID = portID;
        }
    }
}