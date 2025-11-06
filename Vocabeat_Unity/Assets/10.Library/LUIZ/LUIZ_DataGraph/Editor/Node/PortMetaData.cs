using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

namespace LUIZ.DataGraph.Editor
{
    //port에 제공하는 userData 용 클래스
    public struct PortMetaData
    {
        public string DisplayName { get; }
        public int PortID { get; }
        public int ChannelID { get; }
        public Type AcceptedType{ get; }
        public int MinConnection{ get; }
        public int MaxConnection{ get; }
        public bool IsDynamic { get; }

        public PortMetaData(string displayName, int id, int channelID, Type acceptedType, int minConnection, int maxConnection, bool isDynamic)
        {
            DisplayName = displayName;
            PortID = id;
            ChannelID = channelID;
            AcceptedType = acceptedType;
            MinConnection = minConnection;
            MaxConnection = maxConnection;
            IsDynamic = isDynamic;
        }
    }
}