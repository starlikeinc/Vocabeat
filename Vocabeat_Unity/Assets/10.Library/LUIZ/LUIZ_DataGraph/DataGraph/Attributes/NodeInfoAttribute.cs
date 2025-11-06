using System;
using UnityEngine;

namespace LUIZ.DataGraph
{
    public class NodeInfoAttribute : Attribute
    {
        //노드 배경 색상 값
        private readonly float m_r;
        private readonly float m_g;
        private readonly float m_b;
        
        //---------------------------------------------
        public string Title { get; private set;}
        public string MenuItem { get; private set;}

        public UnityEngine.Color DefaultColor => new UnityEngine.Color(m_r, m_g, m_b);

        public NodeInfoAttribute(
            string nodeTitle, string menuItem = "",
            float r = 0.2f, float g = 0.2f, float b = 0.2f)
        {
            Title = nodeTitle;
            MenuItem = menuItem;
            m_r = r;
            m_g = g;
            m_b = b;
        }
    }
}