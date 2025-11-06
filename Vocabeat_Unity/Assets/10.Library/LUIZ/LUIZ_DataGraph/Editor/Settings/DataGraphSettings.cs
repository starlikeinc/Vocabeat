using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LUIZ.DataGraph.Editor
{
    [CreateAssetMenu(fileName = "DataGraphSettings", menuName = "LUIZ/DataGraph/Settings/DataGraphSettings")]
    public class DataGraphSettings : ScriptableObject
    {
        private static DataGraphSettings m_instance;
        public static DataGraphSettings Instance
        {
            get
            {
                if (m_instance == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:DataGraphSettings");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        m_instance = AssetDatabase.LoadAssetAtPath<DataGraphSettings>(path);
                    }
                }
                return m_instance;
            }
        }

        //------------------------------------------------------
        [Header("Node Type Assembly Names")]
        public List<string> NodeAssemblyNames = new List<string>();
        
        [Header("Editor Style Sheet")]
        public StyleSheet EditorStyleSheet;
        
        [Header("DataGraph Json Exporter")] [SerializeReference]
        private DataGraphJsonExporterBase JsonExporter;
        
        //------------------------------------------------------
        public const string c_Infinity = "∞";
        
        public static DataGraphJsonExporterBase GetCurrentConverter() => Instance?.JsonExporter;
        public static string EditorStyleSheetPath => AssetDatabase.GetAssetPath(Instance?.EditorStyleSheet);
    }
}
