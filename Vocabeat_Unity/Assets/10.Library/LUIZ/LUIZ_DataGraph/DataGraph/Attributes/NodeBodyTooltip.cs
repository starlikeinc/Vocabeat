using System;
using LUIZ.DataGraph;
using UnityEngine;

namespace LUIZ.DataGraph
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NodeBodyTooltipAttribute : Attribute
    {
        public string Content { get; }

        public NodeBodyTooltipAttribute(string content)
        {
            Content = content;
        }
    }
}
