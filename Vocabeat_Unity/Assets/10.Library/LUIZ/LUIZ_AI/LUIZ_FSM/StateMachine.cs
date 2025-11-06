using System;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.AI.FSM
{
    public class StateMachine
    {   
        private static List<Transition> c_EmptyTransitions = new List<Transition>(0);

        //------------------------------------------------
        private IFSMState m_currentState;

        private Dictionary<Type, List<Transition>> m_dicTransitions = new Dictionary<Type, List<Transition>>();
        private List<Transition> m_listAnyTransitions = new List<Transition>();

        private List<Transition> m_listCurrentTransitions = new List<Transition>();

        //-------------------------------------------------------------
        //유니티 update에서 돌리거나 while, 코루틴 등에서 지속 호출하여 상태 검사
        public void DoUpdateFSM()
        {
            Transition transition = PrivGetNextTransition();

            if (transition != null)//다음 Transition이 있다면 m_currentState를 변경한다.
                DoSetState(transition.To);

            m_currentState?.Update();
        }

        public void DoAddTransition(IFSMState stateFrom, IFSMState stateTo, Func<bool> predicate)
        {
            if(m_dicTransitions.TryGetValue(stateFrom.GetType(), out List<Transition> transitions) == false)
            {
                transitions = new List<Transition>();
                m_dicTransitions[stateFrom.GetType()] = transitions;
            }

            //해당 stateFrom 에서 갈 수 있는 Transition을 추가 등록
            transitions.Add(new Transition(stateTo, predicate));
        } 

        public void DoAddAnyTransition(IFSMState stateTo, Func<bool> predicate)
        {
            m_listAnyTransitions.Add(new Transition(stateTo, predicate));
        }

        //State를 강제로 바꾸거나 최초 State 실행용
        public void DoSetState(IFSMState state)
        {
            if (state == m_currentState)
            {
                Debug.LogWarning("[StateMachine] Same State is already active");
                return;
            }

            m_currentState?.OnExit();
            m_currentState = state;

            if(m_dicTransitions.TryGetValue(m_currentState.GetType(), out m_listCurrentTransitions) == false)
            {
                m_listCurrentTransitions = c_EmptyTransitions;
            }

            m_currentState.OnEnter();
        }

        //------------------------------------------------
        private Transition PrivGetNextTransition()
        {
            //추후 transition간 우선순위 시스템이 필요할 수 있음. 현재 등록 순으로 작동
            foreach (var transition in m_listAnyTransitions)
            {
                if (transition.Condition() == true)
                    return transition;
            }

            foreach(var transition in m_listCurrentTransitions)
            {
                if (transition.Condition() == true)
                    return transition;
            }

            return null;
        }

        private class Transition
        {
            public Func<bool> Condition { get; }

            public IFSMState To {  get; }

            public Transition(IFSMState stateTo, Func<bool> delCondition)
            {
                this.To = stateTo;
                this.Condition = delCondition;
            }
        }       
     }
}
