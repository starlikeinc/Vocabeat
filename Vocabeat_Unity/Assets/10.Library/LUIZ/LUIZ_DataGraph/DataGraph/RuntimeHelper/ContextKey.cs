using UnityEngine;

namespace LUIZ.DataGraph
{
    //사용 방법 GraphExecutionContext.cs 참조
    public abstract class ContextKeyBase { }

    public sealed class ContextKey<T> : ContextKeyBase where T : class
    {
        public string Name { get; }

        public ContextKey(string name)
        {
            Name = name;
        }

        //-------------------------------------------------
        public override string ToString() => $"ContextKey<{typeof(T).Name}>({Name})";
        
        //키에 목적에 맞게 이름으로 구분하기 위해 구현
        public override bool Equals(object obj) => obj is ContextKey<T> other && Name == other.Name;
        public override int GetHashCode() => Name.GetHashCode();
    }
}
