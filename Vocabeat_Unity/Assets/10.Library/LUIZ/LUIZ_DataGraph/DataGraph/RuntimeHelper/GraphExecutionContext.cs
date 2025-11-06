using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.DataGraph
{
    //런타임 노드들끼리 서로 데이터를 전달 하기 위한 공용 컨텍스트.
    
    //===사용 예시===========
    
    //이런식으로 프로젝트 단위에서 사용할 키들을 미리 등록해둔다.
    // public static class Keys
    //{
    //    public static readonly ContextKey<string> CurrentQuestName = new("CurrentQuestName");
    //}
    
    //런타임 노드의 Execute는 아래 처럼 이용
    //public override void Execute(GraphExecutionContext context)
    //{
    //    int value = context.Get(Keys.CurrentQuestName);
    //    if (value == "MAINQUEST1")
    //        하위 노드 A에 context 전달
    //    else
    //        하위 노드 B에 context 전달
    //}
    
    //주의 ! ContextKey는 박싱 방지를 위해 class 타입만 지원함. 정말정말 만에 하나 값타입을 원하면 따로 레퍼를 만들 것
    
    public class GraphExecutionContext
    {
        private readonly Dictionary<ContextKeyBase, object> m_contextData = new();

        //-------------------------------------------------
        public void Set<T>(ContextKey<T> key, T value) where T : class
        {
            m_contextData[key] = value;
        }

        public T Get<T>(ContextKey<T> key) where T : class
        {
            if (m_contextData.TryGetValue(key, out var value) && value is T typed)
                return typed;
            return default;
        }

        public bool TryGet<T>(ContextKey<T> key, out T value) where T : class
        {
            if (m_contextData.TryGetValue(key, out var obj) && obj is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }
    }
}
