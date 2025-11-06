using System;
using UnityEngine;

namespace LUIZ.DataGraph
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
	public class GraphInspectorHeaderAttribute : PropertyAttribute
	{
		public string Header { get; }
		public GraphInspectorHeaderAttribute(string header)
		{
			Header = header;
		}
	}
}