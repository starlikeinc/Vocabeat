using System;
using System.Linq;

namespace LUIZ.DataGraph
{
    public static class DataGraphTypeNames
    {
        //"Namespace.Type, AssemblyName" (짧은 AQN)
        public static string ToShort(Type t)
        {
            if (t == null) return string.Empty;
            var asmName = t.Assembly.GetName().Name;
            return $"{t.FullName}, {asmName}";
        }

        //문자열 AQN을 Type으로 해석 (짧은/긴 AQN 모두 지원)
        public static Type Resolve(string aqn)
        {
            if (string.IsNullOrEmpty(aqn)) return null;

            //Type.GetType (풀네임?아무튼 전체 AQN이면 바로 성공 가능)
            var t = Type.GetType(aqn);
            if (t != null) return t;

            //정규화 + FormerName + RenameMap
            return NodeTypeResolver.Resolve(aqn);
        }
    }
}