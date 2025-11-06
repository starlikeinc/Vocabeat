using System;
using System.Collections.Generic;
using System.Linq;

namespace LUIZ.DataGraph
{
    public static class NodeTypeRenameMap
    {
        //내용은 예시임
        private static readonly (string from, string to)[] Raw = new[]
        {
            ("SampleDynamicContentNode, LUIZ.DataGraphSamples", "SampleDynamicContentNodeTest, LUIZ.DataGraphSamples"),
        };
        
        private static readonly Dictionary<string, string> Map;

        static NodeTypeRenameMap()
        {
            Map = new Dictionary<string, string>();
            foreach (var (from, to) in Raw)
            {
                var key = NormalizeAqn(from);
                if (!string.IsNullOrEmpty(key) && !Map.ContainsKey(key))
                    Map.Add(key, to);
            }
        }

        //Type, Assembly만....
        private static string NormalizeAqn(string aqn)
        {
            if (string.IsNullOrWhiteSpace(aqn)) return string.Empty;
            var parts = aqn.Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();
            var typeName = parts.Length > 0 ? parts[0] : "";
            var asmName  = parts.Length > 1 ? parts[1] : "";
            return $"{typeName},{asmName}".ToLowerInvariant();
        }

        public static Type Resolve(string oldAqn)
        {
            if (string.IsNullOrEmpty(oldAqn)) return null;
            
            if (Map.TryGetValue(NormalizeAqn(oldAqn), out var newAqn))
            {
                var t = Type.GetType(newAqn);
                if (t != null) return t;

                //NodeTypeResolver로 점검
                return NodeTypeResolver.Resolve(newAqn);
            }
            return null;
        }
    }
}