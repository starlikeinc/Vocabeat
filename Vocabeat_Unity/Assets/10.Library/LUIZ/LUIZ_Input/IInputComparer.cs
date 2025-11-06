using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LUIZ.InputSystem
{
    public interface IInputComparer<T> where T : struct
    {
        bool ShouldInvoke(T prev, T current);
    }
    
    public class Vector2MoveComparer : IInputComparer<Vector2>
    {
        private readonly float m_thresholdSqr;

        public Vector2MoveComparer(float threshold = 0.1f)
        {
            m_thresholdSqr = threshold * threshold;
        }

        //TODO : 데드존, 감도를 그냥 m_thershold로 통합해서 쓰는중...나중에 fps 같은 예민한 겜이면 데드존도 따로
        //건드릴 수 있게 클래스 수정하거나 사용처에서 직접 IInputComparer 전달해서 바꿀 것
        public bool ShouldInvoke(Vector2 prev, Vector2 current)
        {
            bool prevActive = prev.sqrMagnitude > m_thresholdSqr;
            bool currActive = current.sqrMagnitude > m_thresholdSqr;

            //상태 전환 시 무조건 invoke
            if (prevActive != currActive)
                return true;

            //누르고 있는 상태면 계속 invoke
            if (currActive)
                return true;

            //0,0 중에는 invoke 안함
            return false;
        }
    }
    
    public class FloatButtonComparer : IInputComparer<float>
    {
        private readonly float m_threshold;

        public FloatButtonComparer(float threshold = 0.01f)
        {
            m_threshold = threshold;
        }

        public bool ShouldInvoke(float prev, float current)
        {
            return Mathf.Abs(prev - current) > m_threshold;
        }
    }
}

