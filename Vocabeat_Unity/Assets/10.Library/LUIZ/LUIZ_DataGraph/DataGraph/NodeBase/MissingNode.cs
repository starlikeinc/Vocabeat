using System;
using UnityEngine;
using System.Reflection;
using LUIZ.DataGraph;

namespace LUIZ.DataGraph
{
    [Serializable]
    [NodeInfo("Missing Node", "System/Debug", 0.6f, 0.2f, 0.2f)]
    public sealed class MissingNode : DataGraphNodeBase
    {
        [SerializeField] private string m_missingTypeAqn;
        [TextArea(3, 20)] [SerializeField] private string m_rawJson;

        //DataGraph.OnAfterDeserialize에서 넘겨준 rec를 reflection으로 읽어 생성
        public static MissingNode CreateFrom(object rec)
        {
            if (rec == null) return new MissingNode();

            var t = rec.GetType(); //internal NodeRecord라 reflection으로 접근

            //필드
            var position       = (Rect)   t.GetField("position",       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(rec);
            var json           = (string) t.GetField("json",           BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(rec);
            var legacyTypeName = (string) t.GetField("typeNameLegacy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(rec);
            var displayName    = (string) t.GetField("name",           BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(rec);
            var description    = (string) t.GetField("description",    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(rec);

            var n = new MissingNode();
            ((IDataGraphNode)n).SetPosition(position);
            n.Name        = string.IsNullOrEmpty(displayName) ? "[Missing]" : displayName;
            n.Description = string.IsNullOrEmpty(description) ? $"Type not found:\n{legacyTypeName}" : description;
            n.m_missingTypeAqn = legacyTypeName;
            n.m_rawJson        = json;
            return n;
        }

        //나중에 타입 수동으로 선택해 리매핑 할 때
        public bool TryRemapTo(Type newType, out DataGraphNodeBase remapped)
        {
            remapped = null;
            try
            {
                if (newType == null || !typeof(DataGraphNodeBase).IsAssignableFrom(newType))
                    return false;

                var inst = (DataGraphNodeBase)Activator.CreateInstance(newType);
                JsonUtility.FromJsonOverwrite(m_rawJson, inst);

                ((IDataGraphNode)inst).SetPosition(this.Position);
                inst.Name        = this.Name;
                inst.Description = this.Description;

                remapped = inst;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}