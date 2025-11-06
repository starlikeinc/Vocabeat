using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace LUIZ.DataGraph.Editor
{
    //노드와 GraphView 간 통신을 위한 인터페이스
    public interface INodeHostContext
    {
        void RequestAddDynamicPort(Direction direction, DataGraphNodeBase node);
        void RequestRemovePort(Port port);
        void RequestRefreshGraph(); //Rebuild all or part of the graph
        void RequestShowSearchWindowFromPort(Port port, Vector2 screenMousePos);
        void SetAssetDirty();
    }
}
