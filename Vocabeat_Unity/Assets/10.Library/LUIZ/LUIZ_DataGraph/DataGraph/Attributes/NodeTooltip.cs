using System;
using UnityEngine;

namespace LUIZ.DataGraph
{
    public class NodeTooltip : Attribute
    {
        public string Content { get; private set;}

        public NodeTooltip(string content)
        {
            Content = content;
        }
    }
}
