using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace LUIZ.DataGraph.Editor
{
    //Edge 연결 드래그 및 드롭 처리
    public class CustomEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly INodeHostContext m_nodeContext;
        private Port m_dragStartPort;

        public CustomEdgeConnectorListener(INodeHostContext context)
        {
            m_nodeContext = context;
        }

        //-------------------------
        public void OnDropOutsidePort(Edge edge, Vector2 screenMousePosition)
        {
            if (m_dragStartPort != null)
            {
                m_nodeContext.RequestShowSearchWindowFromPort(m_dragStartPort, screenMousePosition);
            }
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            graphView.AddElement(edge); //기본 연결 처리
        }

        //----------------------------------------------------
        public void OnStartEdgeDrag(Port startPort)
        {
            if (startPort != null && m_dragStartPort != startPort)
                m_dragStartPort = startPort;
        }
    }
}