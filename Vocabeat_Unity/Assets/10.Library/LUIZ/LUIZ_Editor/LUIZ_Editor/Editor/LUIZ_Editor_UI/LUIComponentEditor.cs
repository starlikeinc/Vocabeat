using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using LUIZ.UI;

namespace LUIZ.UI.Editor
{
    public partial class LUIComponentEditor
    {
        [MenuItem("GameObject/UI_LUIZ/LButton", priority = 8)]
        private static LButton CreateLButton()
        {
            GameObject gameObj = PrivCreateNewUIObject("LButton");
            gameObj.AddComponent<LImage>();

            LButton button = gameObj.AddComponent<LButton>();
            return button;
        }

        [MenuItem("GameObject/UI_LUIZ/LButtonEmpty", priority = 8)]
        private static LButton CreateLButtonEmpty()
        {
            GameObject gameObj = PrivCreateNewUIObject("LButtonEmpty");
            gameObj.AddComponent<LImageEmpty>();

            LButton buttonEmpty = gameObj.AddComponent<LButton>();
            return buttonEmpty;
        }

        [MenuItem("GameObject/UI_LUIZ/LButtonLongPress", priority = 8)]
        private static LButtonLongPress CreateLButtonLongPress()
        {
            GameObject gameObj = PrivCreateNewUIObject("LButtonLongPress");
            gameObj.AddComponent<LImage>();

            LButtonLongPress button = gameObj.AddComponent<LButtonLongPress>();
            return button;
        }

        //------------------------------------------------------------------------
        [MenuItem("GameObject/UI_LUIZ/LToggle", priority = 8)]
        private static LToggle CreateLToggle()
        {
            GameObject gameObj = PrivCreateNewUIObject("LToggle");
            LToggle toggle = gameObj.AddComponent<LToggle>();

            GameObject pivotOn = PrivCreateEmptyUIObject("UIPivotToggleOn", gameObj.transform);
            LImage imageOn = CreateLImage();
            imageOn.transform.SetParent(pivotOn.transform);

            GameObject pivotOff = PrivCreateEmptyUIObject("UIPivotToggleOff", gameObj.transform);
            LImage imageOff = CreateLImage();
            imageOff.transform.SetParent(pivotOff.transform);

            Selection.activeGameObject = toggle.gameObject;

            return toggle;
        }

        [MenuItem("GameObject/UI_LUIZ/LToggleAdvanced", priority = 8)]
        private static LToggleAdvanced CreateLToggleAdvanced()
        {
            GameObject gameObj = PrivCreateNewUIObject("LToggleAdvanced");
            LToggleAdvanced toggle = gameObj.AddComponent<LToggleAdvanced>();

            GameObject pivot0 = PrivCreateEmptyUIObject("UIPivotToggleState0", gameObj.transform);
            LImage image0 = CreateLImage();
            image0.transform.SetParent(pivot0.transform);

            GameObject pivot1 = PrivCreateEmptyUIObject("UIPivotToggleState1", gameObj.transform);
            LImage image1 = CreateLImage();
            image1.transform.SetParent(pivot1.transform);

            Selection.activeGameObject = toggle.gameObject;

            return toggle;
        }

        [MenuItem("GameObject/UI_LUIZ/LToggleRadio", priority = 8)]
        private static LToggleRadio CreateLRadio()
        {
            GameObject gameObject = PrivCreateNewUIObject("LToggleRadio");
            LToggleRadio radio = gameObject.AddComponent<LToggleRadio>();

            GameObject pivotOn = PrivCreateEmptyUIObject("UIPivotRadioOn", gameObject.transform);
            LImage imageOn = CreateLImage();
            imageOn.transform.SetParent(pivotOn.transform);

            GameObject pivotOff = PrivCreateEmptyUIObject("UIPivotRadioOff", gameObject.transform);
            LImage imageOff = CreateLImage();
            imageOff.transform.SetParent(pivotOff.transform);

            Selection.activeGameObject = radio.gameObject;

            return radio;
        }

        //------------------------------------------------------------------------

        [MenuItem("GameObject/UI_LUIZ/LImage", priority = 8)]
        private static LImage CreateLImage()
        {
            GameObject gameObj = PrivCreateNewUIObject("LImage");

            LImage image = gameObj.AddComponent<LImage>();
            return image;
        }

        [MenuItem("GameObject/UI_LUIZ/LImageEmpty", priority = 8)]
        private static LImageEmpty CreateLImageEmpty()
        {
            GameObject gameObj = PrivCreateNewUIObject("LImageEmpty");

            LImageEmpty image = gameObj.AddComponent<LImageEmpty>();
            return image;
        }

        /*    [MenuItem("GameObject/UI Core/CRawImage", priority = 8)]
            private static CRawImage CreateCRawImage()
            {
                GameObject pCObject = CreateNewUIObject("CRawImage");

                CRawImage pCRawImage = pCObject.AddComponent<CRawImage>();
                return pCRawImage;
            }*/

        [MenuItem("GameObject/UI_LUIZ/LText", priority = 8)]
        private static LText CreateLText()
        {
            GameObject gameObj = PrivCreateNewUIObject("LText");

            LText text = gameObj.AddComponent<LText>();
            return text;
        }

        [MenuItem("GameObject/UI_LUIZ/LText_TMP", priority = 8)]
        private static LText_TMP CreateLTextTMP()
        {
            GameObject gameObj = PrivCreateNewUIObject("LText(TMP)");

            LText_TMP text = gameObj.AddComponent<LText_TMP>();
            return text;
        }

        //TODO : 슬라이더 추가

        //-----------------------------------------------------------
        private static GameObject PrivCreateNewUIObject(string strName)
        {
            GameObject currentSelected = Selection.activeGameObject;
            PrefabStage currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            GameObject newObject = null;

            if (currentSelected != null)
            {
                if (currentSelected.GetComponentInParent<Canvas>())
                {
                    newObject = PrivCreateEmptyUIObject(strName, currentSelected.transform);
                }
                else
                {
                    GameObject newCanvas = PrivCreateEmptyCanvas(currentSelected.transform);
                    newObject = PrivCreateEmptyUIObject(strName, newCanvas.transform);
                }
            }
            else
            {
                if (currentPrefabStage != null) //프리팹 뷰 일 경우
                {
                    GameObject prefabRoot = currentPrefabStage.prefabContentsRoot;
                    if (prefabRoot.GetComponent<Canvas>())
                    {
                        newObject = PrivCreateEmptyUIObject(strName, prefabRoot.transform);
                    }
                    else
                    {
                        GameObject newCanvas = PrivCreateEmptyCanvas(prefabRoot.transform);
                        newObject = PrivCreateEmptyUIObject(strName, newCanvas.transform);
                    }
                }
                else
                {
                    GameObject newCanvas = PrivCreateEmptyCanvas(null);
                    newObject = PrivCreateEmptyUIObject(strName, newCanvas.transform);
                }
            }

            newObject.layer = LayerMask.NameToLayer("UI");

            return newObject;
        }

        private static GameObject PrivCreateEmptyUIObject(string objName, Transform parent)
        {
            GameObject newObject = new GameObject();
            newObject.transform.SetParent(parent);
            newObject.name = objName;
            PrivPlaceUIObject(newObject);

            newObject.transform.localScale = Vector3.one;
            newObject.AddComponent<RectTransform>();

            return newObject;
        }

        private static GameObject PrivCreateEmptyCanvas(Transform parent)
        {
            GameObject newCanvas = new GameObject();
            newCanvas.AddComponent<Canvas>();
            newCanvas.name = "Canvas";
            PrivPlaceUIObject(newCanvas);

            newCanvas.transform.localScale = Vector3.one;
            return newCanvas;
        }

        private static void PrivPlaceUIObject(GameObject gameObject)
        {
            SceneView lastView = SceneView.lastActiveSceneView;
            gameObject.transform.position = lastView ? new Vector3(lastView.pivot.x, lastView.pivot.y, 0) : Vector3.zero;

            StageUtility.PlaceGameObjectInCurrentStage(gameObject);
            GameObjectUtility.EnsureUniqueNameForSibling(gameObject);

            Undo.RegisterCreatedObjectUndo(gameObject, $"Create Object: {gameObject.name}");
            Selection.activeGameObject = gameObject;
            gameObject.layer = LayerMask.NameToLayer("UI");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
