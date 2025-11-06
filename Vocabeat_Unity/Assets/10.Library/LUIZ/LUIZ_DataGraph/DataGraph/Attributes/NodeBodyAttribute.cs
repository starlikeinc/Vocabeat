using System;
using UnityEngine;

namespace LUIZ.DataGraph
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NodeBodyAttribute : Attribute
    {
        public string Label { get; }

        public NodeBodyAttribute(string label = null)
        {
            Label = label;
        }
    }
}
