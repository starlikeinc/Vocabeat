using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Pool;

namespace LUIZ.DataGraph
{
    [Serializable]
    public class NodeRecord
    {
        public ulong nodeId;
        public Rect position;
        public string name;
        public string description;

        public string typeNameLegacy;   //AssemblyQualifiedName
        public string json;             //노드 전체 JSON

        [Serializable]
        public class ObjectRefEntry
        {
            public string fieldPath;
            public UnityEngine.Object obj; //런타임에서도 직렬화되는 직접 참조
#if UNITY_EDITOR
            public string guid;
            public long localId;
            public string typeName; //짧은 AQN
#endif
        }

        public List<ObjectRefEntry> objectRefs = new(); //빌드에도 포함
    }

    public abstract class DataGraph : ScriptableObject, ISerializationCallbackReceiver
    {
        private static readonly IReadOnlyList<DataGraphNodeBase> c_listEmpty = new List<DataGraphNodeBase>();

        [NonSerialized] private Dictionary<ulong, string> m_dicPendingJsonByNodeID = null;

        //DataGraphGuidPostprocessor에서 m_graphID 필드 접근중, 이름 변경 시 참고
        [SerializeField, HideInInspector] private ulong m_graphID;

        //리스트들 필드들을 DataGraphGuidPostprocessor에서 리플렉션으로 접근중, 이름 변경 시 참고
        [NonSerialized, HideInInspector] private List<DataGraphNodeBase> m_listNodes;
        [SerializeField, HideInInspector] private List<DataGraphEdge> m_listEdges;
        [SerializeField, HideInInspector] private List<DataGraphNodeGroup> m_listNodeGroups;

        //노드 직렬화 저장용 래퍼
        [SerializeField, HideInInspector] private List<NodeRecord> m_serializedNodes = new();

        //런타임 or JsonExport 등에서 빠른 검색을 위해 사용하는 캐시
        [NonSerialized] private Dictionary<ulong, DataGraphNodeBase> m_dicNodesByID = null;
        [NonSerialized] private Dictionary<ulong, List<DataGraphNodeBase>> m_dicOutgoingCache = null;
        [NonSerialized] private Dictionary<ulong, List<DataGraphNodeBase>> m_dicIncomingCache = null;

        [NonSerialized] private bool m_isCacheBuilt = false;

        //-----------------------------------------------------------
        protected DataGraph()
        {
            m_graphID = IDGenerator.NewID();

            m_listNodes = new List<DataGraphNodeBase>();
            m_listEdges = new List<DataGraphEdge>();
            m_listNodeGroups = new List<DataGraphNodeGroup>();
        }

        //-----------------------------------------------------------
        public ulong GraphID => m_graphID;
        public IReadOnlyList<DataGraphNodeBase> Nodes => m_listNodes;
        public IReadOnlyList<DataGraphEdge> Edges => m_listEdges;
        public IReadOnlyList<DataGraphNodeGroup> Groups => m_listNodeGroups;

        public bool TryGetNode(ulong nodeID, out DataGraphNodeBase node)
        {
            EnsureCacheBuilt();
            return m_dicNodesByID.TryGetValue(nodeID, out node);
        }

        public IReadOnlyList<DataGraphNodeBase> GetOutgoingNodes(DataGraphNodeBase node)
        {
            EnsureCacheBuilt();
            return m_dicOutgoingCache.TryGetValue(node.NodeID, out var list) ? list : c_listEmpty;
        }

        public IReadOnlyList<DataGraphNodeBase> GetIncomingNodes(DataGraphNodeBase node)
        {
            EnsureCacheBuilt();
            return m_dicIncomingCache.TryGetValue(node.NodeID, out var list) ? list : c_listEmpty;
        }

        public void ClearCache()
        {
            if (m_dicOutgoingCache != null)
            {
                foreach (var list in m_dicOutgoingCache.Values)
                    list.Clear();
                m_dicOutgoingCache.Clear();
            }

            if (m_dicIncomingCache != null)
            {
                foreach (var list in m_dicIncomingCache.Values)
                    list.Clear();
                m_dicIncomingCache.Clear();
            }

            m_dicNodesByID?.Clear();
            m_isCacheBuilt = false;
        }
        
        //-----------------------------------------------------------
        private void OnEnable()
        {
            //역직렬화가 끝난 이후라 PPtr 해제가 허용됨
            if (m_dicPendingJsonByNodeID == null || m_listNodes == null) return;

            //중요!!
            //Resources.UnloadUnusedAssets() 이런거 ApplyPendingJson전에 하면 문제 생길수도 있음 캐시 날아가서...꼭 알고 있을 것
#if UNITY_EDITOR
            ApplyPendingJson();
            //혹시몰라 다음 프레임에 한 번 더(혹시 인스펙터/윈도우가 더 늦게 초기화된 경우)
            UnityEditor.EditorApplication.delayCall += () =>
            {
                ApplyPendingJson();// m_isJsonApplied 가드 때문에 2중 적용 안 됨
                RaiseGraphRestoredEvent();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();//강제 리페인트
            };
#else
            ApplyPendingJson();
#endif
        }
        
        //-----------------------------------------------------------
#if UNITY_EDITOR
        //------에디터 전용 항목!!!!!!! 런타임에 사용 불가!!!!!!!------
        public void AddNodeEditor(DataGraphNodeBase node) => m_listNodes.Add(node);
        public void RemoveNodeEditor(DataGraphNodeBase node) => m_listNodes.Remove(node);

        public void AddEdgeEditor(DataGraphEdge edge) => m_listEdges.Add(edge);
        public void RemoveEdgeEditor(DataGraphEdge edge) => m_listEdges.Remove(edge);

        public void AddNodeGroupEditor(DataGraphNodeGroup group) => m_listNodeGroups.Add(group);
        public void RemoveNodeGroupEditor(DataGraphNodeGroup group) => m_listNodeGroups.Remove(group);

        /// <summary> 저장 직전에 호출되는 이벤트 메소드 </summary>
        public void NotifyBeforeSaveEditor() => OnBeforeAssetSave();
#endif
        //--------------------------------------------------------------
        private void EnsureCacheBuilt()
        {
            if (!m_isCacheBuilt)
                BuildCache();
        }

        // 내부 전용 TryGet
        private bool TryGetNodePrivate(ulong nodeID, out DataGraphNodeBase node) =>
            m_dicNodesByID.TryGetValue(nodeID, out node);

        private void BuildCache()
        {
            if (m_dicOutgoingCache == null)
                m_dicOutgoingCache = new();
            else
                m_dicOutgoingCache.Clear();

            if (m_dicIncomingCache == null)
                m_dicIncomingCache = new();
            else
                m_dicIncomingCache.Clear();

            if (m_dicNodesByID == null)
                m_dicNodesByID = new();
            else
                m_dicNodesByID.Clear();

            foreach (var node in m_listNodes)
            {
                m_dicNodesByID[node.NodeID] = node;
                m_dicOutgoingCache[node.NodeID] = new List<DataGraphNodeBase>();
                m_dicIncomingCache[node.NodeID] = new List<DataGraphNodeBase>();
            }

            foreach (var edge in m_listEdges)
            {
                if (TryGetNodePrivate(edge.OutputPort.NodeID, out var outputNode) &&
                    TryGetNodePrivate(edge.InputPort.NodeID, out var inputNode))
                {
                    m_dicOutgoingCache[outputNode.NodeID].Add(inputNode);
                    m_dicIncomingCache[inputNode.NodeID].Add(outputNode);
                }
            }

            m_isCacheBuilt = true;
        }

        //-------------------------------------
        ///<summary>
        ///저장 직전에 커스텀 그래프에서 오버라이드할 수 있는 이벤트. 정렬하거나 뭐...데이터 초기화? 할때 이용 가능
        ///</summary>
        protected virtual void OnBeforeAssetSave()
        {
        }

        //-------------------------------------
        [NonSerialized] bool m_isJsonApplied;
        private void ApplyPendingJson()
        {
            if (m_isJsonApplied) return;
            m_isJsonApplied = true;
            
            if (m_dicPendingJsonByNodeID == null) return;

            //nodeId -> NodeRecord 맵 만들기
            var recMap = new Dictionary<ulong, NodeRecord>(m_serializedNodes?.Count ?? 0);

            if (m_serializedNodes != null)
                foreach (var r in m_serializedNodes)
                    recMap[r.nodeId] = r;

            foreach (var node in m_listNodes)
            {
                if (m_dicPendingJsonByNodeID.TryGetValue(node.NodeID, out var json) && !string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var keepId = node.NodeID;//nodeID 오염 방지 (GUidProcessor 등에서 오버라이드 될때 옛날 값으로 돌아오는거 방지용임
                        JsonUtility.FromJsonOverwrite(json, node);
                        NodeIDField.SetValue(node, keepId);
                        
                        //오브젝트 참조 복원
                        if (recMap.TryGetValue(node.NodeID, out var rec))
                            RestoreObjectRefsPerNode(node, rec);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DataGraph] Payload overwrite failed on {node.GetType().Name} : {e}");
                    }
                }
            }

            m_dicPendingJsonByNodeID.Clear();
            m_dicPendingJsonByNodeID = null;

            ClearCache();
        }


        #region ======== ISerializationCallbackReceiver ========
        //=========================
        public void OnBeforeSerialize()
        {
            var prevSnapshot = m_serializedNodes;
            m_serializedNodes = new List<NodeRecord>();

            if (m_listNodes == null) return;

            Dictionary<ulong, NodeRecord> prevMap = null;
            if (prevSnapshot != null && prevSnapshot.Count > 0)
            {
                prevMap = new Dictionary<ulong, NodeRecord>(prevSnapshot.Count);
                foreach (var p in prevSnapshot) prevMap[p.nodeId] = p;
            }

            foreach (var node in m_listNodes)
            {
                var rec = new NodeRecord
                {
                    nodeId = node.NodeID,
                    position = node.Position,
                    name = node.Name,
                    description = node.Description,
                    typeNameLegacy = DataGraphTypeNames.ToShort(node.GetType()),
                    json = ToJsonWithInstanceIDZero(node),
                };

                //이전 스냅샷 있으면 그대로 복사
                if (prevMap != null && prevMap.TryGetValue(node.NodeID, out var prev) && prev.objectRefs != null && prev.objectRefs.Count > 0)
                {
                    rec.objectRefs = new List<NodeRecord.ObjectRefEntry>(prev.objectRefs.Count);
                    rec.objectRefs.AddRange(prev.objectRefs);
                    
                    //레거시 정리용. 같은 fieldPath가 여러 개면 마지막 꺼만 남김
                    NormalizeObjectRefs(rec.objectRefs);
                }
                else
                {
                    rec.objectRefs = new List<NodeRecord.ObjectRefEntry>(4); //대충 4개로 잡음. 현실적으로 이이상 선언 자체를 안할거라 생각하기는 함
                }

                //라이브 필드로 덮어쓰기 or 새로 추가
                var fields = GetUnityObjFieldsCached(node.GetType());
                for (int f = 0; f < fields.Length; ++f)
                {
                    var fi = fields[f];
                    var obj = fi.GetValue(node) as UnityEngine.Object;
                    if (!obj) continue;

                    var key = $"{fi.DeclaringType.FullName}|{fi.Name}";

                    //존재하면 덮어쓰고 없으면 추가
                    int idx = -1;
                    for (int i = 0; i < rec.objectRefs.Count; ++i)
                    {
                        if (rec.objectRefs[i].fieldPath == key)
                        {
                            idx = i;
                            break;
                        }
                    }

                    NodeRecord.ObjectRefEntry entry;
                    if (idx >= 0)
                    {
                        entry = rec.objectRefs[idx];
                    }
                    else
                    {
                        entry = new NodeRecord.ObjectRefEntry { fieldPath = key };
                        rec.objectRefs.Add(entry);
                    }

                    entry.obj = obj;
#if UNITY_EDITOR
                    if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId))
                    {
                        entry.guid = guid;
                        entry.localId = localId;
                        entry.typeName = DataGraphTypeNames.ToShort(fi.FieldType);
                    }
                    else
                    {
                        //guid 못 얻어온 경우가 있으면 최소 필드명만이라도 유지..
                    }
#endif
                }
                m_serializedNodes.Add(rec);
            }
        }

        public void OnAfterDeserialize()
        {
            //래퍼에서 노드 복원
            m_listNodes ??= new List<DataGraphNodeBase>();
            m_listNodes.Clear();

            if (m_serializedNodes == null) return;

            m_dicPendingJsonByNodeID = new Dictionary<ulong, string>();

            foreach (var rec in m_serializedNodes)
            {
                DataGraphNodeBase instance = null;
                try
                {
                    var type = Type.GetType(rec.typeNameLegacy) ?? NodeTypeResolver.Resolve(rec.typeNameLegacy);

                    if (type != null && typeof(DataGraphNodeBase).IsAssignableFrom(type))
                    {
                        //IL2CPP 에서는 리플렉션 이슈가 생길수도 있을거같은데......
                        //MONO빌드면 나중에 빌드하고 검증해봐야함
                        instance = (DataGraphNodeBase)Activator.CreateInstance(type);

                        //여기서 FromJsonOverwrite 하면 안 됨!
                        //JsonUtility.FromJsonOverwrite(rec.json, instance);
                        //필수 메타(포지션/이름/설명)만 우선 셋업
                        ((IDataGraphNode)instance).SetPosition(rec.position);
                        instance.Name = rec.name;
                        instance.Description = rec.description;

                        //나머지는 나중에 OnEnable에서 오버라이트
                        m_dicPendingJsonByNodeID[rec.nodeId] = rec.json;
                    }
                    else
                    {
                        Debug.LogWarning($"[DataGraph] Type not found: {rec.typeNameLegacy}");
                        instance = MissingNode.CreateFrom(rec);
                    }
                }
                catch
                {
                    Debug.LogWarning($"[DataGraph] Type not found: {rec.typeNameLegacy}");
                    instance = MissingNode.CreateFrom(rec);
                }

                // NodeID 유지

                NodeIDField.SetValue(instance, rec.nodeId);
                m_listNodes.Add(instance);
            }

            ClearCache();
            
            m_isJsonApplied = false;
        }
        #endregion
        
#if UNITY_EDITOR
        public static event Action<DataGraph> GraphRestored;
        private void RaiseGraphRestoredEvent()
        {
            GraphRestored?.Invoke(this);
        }
#endif
        
        //노드에 오브젝트 필드 복원하는 함수임
        private void RestoreObjectRefsPerNode(DataGraphNodeBase node, NodeRecord rec)
        {
            if (rec.objectRefs == null || rec.objectRefs.Count == 0) return;

            foreach (var entry in rec.objectRefs)
            {
                var objField = FindFieldByPathCached(node.GetType(), entry.fieldPath);
                if (objField == null) continue;

                UnityEngine.Object value = null;

#if UNITY_EDITOR //에디터면 guid로 복원시도
                if (!string.IsNullOrEmpty(entry.guid))
                {
                    value = LoadByGuidAndLocalId(entry.guid, entry.localId, objField.FieldType);
                }
                if (!value) value = entry.obj;
#else
        value = entry.obj;
#endif
                if (value) objField.SetValue(node, value);
            }
        }

        private static void NormalizeObjectRefs(List<NodeRecord.ObjectRefEntry> list)
        {
            if (list == null || list.Count <= 1) return;

            //뒤에서부터 훑어서 같은 fieldPath가 또 나오면 앞에 있는거 제거해서 마지막 값을 보존 하게함
            var seen = new HashSet<string>();
            for (int i = list.Count - 1; i >= 0; --i)
            {
                var entry = list[i];
                var key = entry?.fieldPath;

                if (string.IsNullOrEmpty(key))
                {
                    //fieldPath가 비어있던 레거시 항목은 제거
                    list.RemoveAt(i);
                    continue;
                }

                if (seen.Contains(key))
                {
                    list.RemoveAt(i);
                    continue;
                }

                seen.Add(key);
            }
        }
        
        private static string ToJsonWithInstanceIDZero(DataGraphNodeBase node)
        {
            //json 저장시 pptr 값인 instanceID가 같이 저장돼서 git에 불필요한 diff 들어오는거 방지 + 오브젝트 필드 참조 오류 예방
            var backups = new List<(FieldInfo fi, UnityEngine.Object val)>();
            try
            {
                foreach (var objField in GetUnityObjFieldsCached(node.GetType()))
                {
                    var val = objField.GetValue(node) as UnityEngine.Object;
                    if (val)
                    {
                        backups.Add((objField, val));
                        objField.SetValue(node, null);
                    }
                }
                return JsonUtility.ToJson(node);
            }
            finally
            {
                for (int i = backups.Count - 1; i >= 0; --i)
                    backups[i].fi.SetValue(node, backups[i].val);
            }
        }
        
#if UNITY_EDITOR
        private static UnityEngine.Object LoadByGuidAndLocalId(string guid, long localId, Type tHint)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;

            if (localId != 0)
            {
                var all = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                for (int i = 0; i < all.Length; ++i)
                {
                    var a = all[i];
                    if (!a) continue;
                    if (!tHint.IsAssignableFrom(a.GetType())) continue;

                    if (UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out _, out long lid))
                    {
                        if (lid == localId) return a;
                    }
                }
                return null; //로컬ID 못 찾음
            }

            //서브에셋이 아닐 때는 그냥 한번에 로드
            return UnityEditor.AssetDatabase.LoadAssetAtPath(path, tHint);
        }
#endif
        
        //------------캐시용-----------
        private static readonly Dictionary<Type, FieldInfo[]> s_unityObjFieldsCache = new();
        private static readonly Dictionary<(Type, string), FieldInfo> s_fieldPathCache = new();

        private static readonly BindingFlags BF_INSTANCE_ALL = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static FieldInfo s_nodeIDField; //m_nodeID 캐시 ( 이름 바꾸면 안됨!!!!!!!!!!!!!!!)
        private static FieldInfo NodeIDField => s_nodeIDField ??= typeof(DataGraphNodeBase).GetField("m_nodeID",BindingFlags.Instance | BindingFlags.NonPublic);

        //타입별 UnityEngine.Object 필드 배열 캐시
        private static FieldInfo[] GetUnityObjFieldsCached(Type t)
        {
            if (s_unityObjFieldsCache.TryGetValue(t, out var arr)) return arr;

            List<FieldInfo> list = new();
            var cur = t;
            while (cur != null && cur != typeof(object))
            {
                var fields = cur.GetFields(BF_INSTANCE_ALL | BindingFlags.DeclaredOnly);
                foreach (var fi in fields)
                {
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(fi.FieldType)) continue;
                    bool isUnitySerializable = fi.IsPublic || Attribute.IsDefined(fi, typeof(SerializeField), inherit: false);
                    if (isUnitySerializable) list.Add(fi);
                }

                cur = cur.BaseType;
            }

            var result = list.ToArray();
            s_unityObjFieldsCache[t] = result;
            return result;
        }

        // DeclaringType.FullName|FieldName
        // 또는... FieldName → FieldInfo 캐시
        private static FieldInfo FindFieldByPathCached(Type t, string fieldPath)
        {
            var key = (t, fieldPath);
            if (s_fieldPathCache.TryGetValue(key, out var fi)) return fi;

            FieldInfo found = null;

            int sep = fieldPath.IndexOf('|');
            if (sep >= 0)
            {
                string declName = fieldPath[..sep];
                string fname = fieldPath[(sep + 1)..];

                var cur = t;
                while (cur != null && cur != typeof(object))
                {
                    if (cur.FullName == declName)
                    {
                        found = cur.GetField(fname, BF_INSTANCE_ALL | BindingFlags.DeclaredOnly);
                        break;
                    }

                    cur = cur.BaseType;
                }
            }
            else
            {
                var cur = t;
                while (cur != null && cur != typeof(object))
                {
                    found = cur.GetField(fieldPath, BF_INSTANCE_ALL | BindingFlags.DeclaredOnly);
                    if (found != null) break;
                    cur = cur.BaseType;
                }
            }

            s_fieldPathCache[key] = found;
            return found;
        }
        
        //에디터에서 ctrl+z 후에 json 바로 적용 위해서 사용
#if UNITY_EDITOR
        public void DoForceApplyJsonInEditor()
        {
            m_isJsonApplied = false;
            ApplyPendingJson();
            
            RaiseGraphRestoredEvent();
        }
#endif
    }
}

