using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace LUIZ.DataGraph
{
public static class NodeTypeResolver
{
        private static bool m_isScanned;
        private static Dictionary<string, Type> m_dicFormerToType; // key: normalized "Type,Assembly" lower

        //Type, Assembly만 남기고 나머지 토큰(Version/Culture/PKT) 버렸음
        //공백/대소문자 통일!!!!
        private static string NormalizeAqn(string aqn)
        {
            if (string.IsNullOrWhiteSpace(aqn)) return string.Empty;
            var parts = aqn.Split(',')
                           .Select(p => p.Trim())
                           .Where(p => !string.IsNullOrEmpty(p))
                           .ToArray();

            if (parts.Length == 0) return string.Empty;

            //타입명과 어셈블리명만 사용
            var typeName = parts[0];
            var asmName  = parts.Length > 1 ? parts[1] : string.Empty;

            return $"{typeName},{asmName}".ToLowerInvariant();
        }

        private static void EnsureScanned()
        {
            if (m_isScanned) return;
            m_isScanned = true;
            m_dicFormerToType = new();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (t == null) continue;
                    if (!typeof(DataGraphNodeBase).IsAssignableFrom(t) || t.IsAbstract) continue;

                    var attrs = t.GetCustomAttributes(typeof(FormerNameAttribute), false).Cast<FormerNameAttribute>();
                    foreach (var a in attrs)
                    {
                        var key = NormalizeAqn(a.AssemblyQualifiedName);
                        if (string.IsNullOrEmpty(key)) continue;
                        m_dicFormerToType.TryAdd(key, t);
                    }
                }
            }
        }

        public static Type Resolve(string aqn)
        {
            //매핑 테이블 먼저
            var t = NodeTypeRenameMap.Resolve(aqn);
            if (t != null) return t;

            //FormerNameAttribute 매핑
            EnsureScanned();
            var key = NormalizeAqn(aqn);
            return m_dicFormerToType.GetValueOrDefault(key);
        }
    }
}