using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

//      ObjectComponentReplacer   MadeBy : LUIZ
//      ver 1.1
//      luiz04041207@gmail.com
namespace LUIZ.Editor
{
    public class ObjectComponentReplacer : EditorWindow
    {
        private GameObject m_targetPrefab = null;

        private MonoScript m_componentBefore = null; private Type m_typeBefore = null;
        private MonoScript m_componentAfter = null; private Type m_typeAfter = null;

        private MonoScript m_replacerHelper = null; private IComponentReplacerHelper m_helperInstance = null;

        private List<string> m_listChangedObjNames = new List<string>();

        private bool m_isShowResult = false;

        private Vector2 m_scrollPos;

        //-------------------------------------------------------------------
        [MenuItem("LUIZ/ObjectComponentReplacer")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ObjectComponentReplacer));
        }

        private void OnGUI()
        {
            m_scrollPos = GUILayout.BeginScrollView(m_scrollPos, false, true, GUILayout.ExpandHeight(true));

            #region ========개요========
            GUILayout.Label("======== 컴포넌트 교체기 ver1.1 ========         MadeBy : LUIZ", EditorStyles.boldLabel);
            EditorGUILayout.Space(20);

            GUILayout.Label("[개요]", EditorStyles.boldLabel);
            GUILayout.Label("오브젝트의 모든 자식 오브젝트를 검사하여");
            GUILayout.Label("Before에 해당하는 타입의 컴포넌트들을 After 컴포넌트로 전부 교체한다.");
            EditorGUILayout.Space(20);

            GUILayout.Label("After 가 null일 경우 Before에 해당하는 컴포넌트를 제거한다.");
            EditorGUILayout.Space(20);

            GUILayout.Label("%% 주의 !!%%", EditorStyles.boldLabel);
            GUILayout.Label("씬뷰에서 컴포넌트를 변경할 경우 해당 오브젝트가 특정 프리펩의 일부일 경우", EditorStyles.wordWrappedLabel);
            GUILayout.Label("해당 오브젝트가 속한 프리펩의 최상위 루트부터 컴포넌트를 전부 교체한다.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("이를 원치 않는다면 프리팹 뷰에 들어가서 변경을 하게 된다면 상위 객체는 교체하지 않는다.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space(20);

            GUILayout.Label("또한 오브젝트 검사 시 프리펩 객체를 만나게 되면 반드시 '원본 프리펩'의 컴포넌트만 교체하기 떄문에", EditorStyles.wordWrappedLabel);
            GUILayout.Label("오버라이드 된 오브젝트는 개별로 등록하여 따로 교체한다.");
            EditorGUILayout.Space(20);
            #endregion

            #region========Fields========
            m_targetPrefab = (GameObject)EditorGUILayout.ObjectField("Target Object", m_targetPrefab, typeof(GameObject), true);
            EditorGUILayout.Space(20);

            m_componentBefore = EditorGUILayout.ObjectField("Component BEFORE", m_componentBefore, typeof(MonoScript), false) as MonoScript;
            m_componentAfter = EditorGUILayout.ObjectField("Component AFTER", m_componentAfter, typeof(MonoScript), false) as MonoScript;
            EditorGUILayout.Space(20);

            GUILayout.Label("[Replacer Helper]", EditorStyles.boldLabel);
            GUILayout.Label("컴포넌트 교체시 서로 특정 값을 복사하거나 초기값을 세팅 해야할때 사용하는 헬퍼 스크립트.", EditorStyles.wordWrappedLabel);
            GUILayout.Label("개별 스크립트로 작성해야하며 IComponentReplacerHelper를 상속 받아 가능을 구현한 후 연결한다.", EditorStyles.wordWrappedLabel);

            m_replacerHelper = (MonoScript)EditorGUILayout.ObjectField("Replacer Helper", m_replacerHelper, typeof(MonoScript), false);
            EditorGUILayout.Space(20);
            #endregion

            #region ========Buttons========
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("SwapBeforeAfter", GUILayout.Width(120), GUILayout.Height(30)))
            {
                PrivSwapBeforeAfter();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("REPLACE", GUILayout.Width(120), GUILayout.Height(30)))
            {
                PrivCompoentReplacerBegin();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            #endregion

            PrivShowResult();

            EditorGUILayout.EndScrollView();
        }

        //-------------------------------------------------------------------
        private void PrivCompoentReplacerBegin()
        {
            PrivReplacerReset();

            if (m_targetPrefab == null)
            {
                Debug.LogError("Object 할당안됨");
                return;
            }

            if (m_componentBefore == null)
            {
                Debug.LogError("Before는 null 일 수 없습니다");
                return;
            }

            m_typeBefore = m_componentBefore.GetClass();
            if (m_componentAfter != null)
            {
                m_typeAfter = m_componentAfter.GetClass();
            }

            if (m_replacerHelper != null)
            {
                Type helperType = m_replacerHelper.GetClass();

                m_helperInstance = Activator.CreateInstance(helperType) as IComponentReplacerHelper;

                if (m_helperInstance == null)
                {
                    Debug.LogError("ERROR : 'Replacer Helper' must derive from 'IComponentReplacerHelper'");
                    return;
                }
                else
                {
                    bool isValid = m_helperInstance.IsReplacerValid(m_typeBefore, m_typeAfter);
                    if (isValid == false)
                    {
                        Debug.LogError("ERROR : 'Replacer Helper' Validity is false!! Check Replacer Helper Code");
                        return;
                    }
                }
            }

            PrivReplaceRecursive(m_targetPrefab);

            PrivSavePrefabStage();

            m_isShowResult = true;
        }
        private void PrivShowResult()
        {
            if (m_isShowResult == true)
            {
                EditorGUILayout.Space(20);
                GUILayout.Label("[Result]", EditorStyles.boldLabel);
                GUILayout.Label($"Total : {m_listChangedObjNames.Count} components replaced");

                if (m_typeBefore == null)
                    return;

                for (int i = 0; i < m_listChangedObjNames.Count; i++)
                {
                    string m_typeAfterName = m_typeAfter != null ? m_typeAfter.Name : "Null";
                    GUILayout.Label($"{i} : {m_listChangedObjNames[i]}      /       {m_typeBefore.Name} => {m_typeAfterName}");
                }
            }
        }

        private void PrivReplaceRecursive(GameObject targetObj)
        {
            PrefabAssetType currentAssetType = PrefabUtility.GetPrefabAssetType(targetObj);

            if (currentAssetType == PrefabAssetType.NotAPrefab)
            {
                PrivReplaceComponent(targetObj);
                for (int i = 0; i < targetObj.transform.childCount; i++)
                {
                    GameObject childObj = targetObj.transform.GetChild(i).gameObject;
                    PrivReplaceRecursive(childObj);
                }
            }
            else
            {
                GameObject targetObjPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(targetObj);
                string prefabPath = AssetDatabase.GetAssetPath(targetObjPrefab);

                using (EditPrefabAssetScope editScope = new EditPrefabAssetScope(prefabPath))
                {
                    PrivReplaceComponent(editScope.PrefabRoot);
                    for (int i = 0; i < editScope.PrefabRoot.transform.childCount; i++)
                    {
                        GameObject childObj = editScope.PrefabRoot.transform.GetChild(i).gameObject;
                        PrivReplaceRecursive(childObj);
                    }
                }
            }
        }

        private void PrivReplaceComponent(GameObject gameObject)
        {
            Component component = gameObject.GetComponent(m_typeBefore);
            if (component != null)
            {
                m_helperInstance.OnDestroyComponentBefore(component);
                DestroyImmediate(component);

                if (m_typeAfter != null)
                {
                    Component addComponent = gameObject.AddComponent(m_typeAfter);
                    m_helperInstance.OnAddComponentAfter(addComponent);
                }

                m_listChangedObjNames.Add(gameObject.name);
            }
        }

        private void PrivSavePrefabStage()
        {
            PrefabStage pStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (pStage != null)
            {
                PrefabUtility.SaveAsPrefabAsset(pStage.prefabContentsRoot, pStage.assetPath);
            }
        }

        private void PrivSwapBeforeAfter()
        {
            MonoScript temp = m_componentBefore;
            m_componentBefore = m_componentAfter;
            m_componentAfter = temp;
        }

        private void PrivReplacerReset()
        {
            m_listChangedObjNames.Clear();

            m_helperInstance = null;
            m_typeAfter = null;
            m_typeBefore = null;

            m_isShowResult = false;
        }

        //----------------------------------------------------------
        public class EditPrefabAssetScope : IDisposable
        {
            public readonly string AssetPath;
            public readonly GameObject PrefabRoot;

            public EditPrefabAssetScope(string assetPath)
            {
                this.AssetPath = assetPath;
                PrefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
            }

            public void Dispose()
            {
                PrefabUtility.SaveAsPrefabAsset(PrefabRoot, AssetPath);
                PrefabUtility.UnloadPrefabContents(PrefabRoot);
            }
        }
    }
}
