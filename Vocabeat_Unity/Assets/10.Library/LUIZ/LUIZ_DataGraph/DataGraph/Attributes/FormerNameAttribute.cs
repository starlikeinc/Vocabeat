using System;

namespace LUIZ.DataGraph
{
    //노드 이름 바꿧을때 이용하는 어트리뷰트
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class FormerNameAttribute : Attribute
    {
        public string AssemblyQualifiedName { get; }
        public FormerNameAttribute(string aqn) => AssemblyQualifiedName = aqn;
    }
}