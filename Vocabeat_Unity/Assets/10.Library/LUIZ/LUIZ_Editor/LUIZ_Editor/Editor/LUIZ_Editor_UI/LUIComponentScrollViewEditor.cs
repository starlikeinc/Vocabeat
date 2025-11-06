using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LUIZ.UI.Editor
{
    public partial class LUIComponentEditor
    {
        [MenuItem("GameObject/UI_LUIZ/LScrollView", priority = 8)]
        private static LScrollRect CreateLScrollView()
        {
            GameObject rootObj = PrivCreateNewUIObject("LScrollView");
            RectTransform rootRect = rootObj.transform as RectTransform;
            rootRect.sizeDelta = new Vector2(200,200);
            LImage image = rootObj.AddComponent<LImage>();
            LScrollRect scrollRect = rootObj.AddComponent<LScrollRect>();
            image.color = new Color(1, 1, 1, 0.5f);

            GameObject viewPort = PrivCreateEmptyUIObject("Viewport", rootObj.transform);
            viewPort.AddComponent<LImageEmpty>();
            viewPort.AddComponent<RectMask2D>();
            RectTransform viewPortRect = viewPort.transform as RectTransform;
            viewPortRect.anchorMin = new Vector2(0, 0);
            viewPortRect.anchorMax = new Vector2(1, 1);
            viewPortRect.pivot = new Vector2(0, 1);

            GameObject content = PrivCreateEmptyUIObject("Content", viewPort.transform);
            RectTransform contentRect = content.transform as RectTransform;
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, 300);

            //가로 스크롤 바-----------------
            GameObject horScrollBarObj = PrivCreateEmptyUIObject("ScrollbarHorizontal", rootObj.transform);
            RectTransform horScrollBarRect = horScrollBarObj.transform as RectTransform;
            LImage horScrollBarImg = horScrollBarObj.AddComponent<LImage>();
            horScrollBarImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            horScrollBarImg.type = Image.Type.Sliced;
            LScrollBar horScrollBar = horScrollBarObj.AddComponent<LScrollBar>();
            horScrollBarRect.anchorMin = new Vector2(0, 0);
            horScrollBarRect.anchorMax = new Vector2(1, 0);
            horScrollBarRect.pivot = new Vector2(0, 0);
            horScrollBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);

            GameObject horScrollSlideAreaObj = PrivCreateEmptyUIObject("SlidingArea", horScrollBarObj.transform);
            RectTransform horSlideAreaRect = horScrollSlideAreaObj.transform as RectTransform;
            horSlideAreaRect.anchorMin = new Vector2(0, 0);
            horSlideAreaRect.anchorMax = new Vector2(1, 1);
            horSlideAreaRect.offsetMin = new Vector2(10, 10);
            horSlideAreaRect.offsetMax = new Vector2(-10, -10);

            GameObject horScrollHandle = PrivCreateEmptyUIObject("Handle", horScrollSlideAreaObj.transform);
            RectTransform horScrollHandleRect = horScrollHandle.transform as RectTransform;
            horScrollHandleRect.offsetMin = new Vector2(-10, -10);
            horScrollHandleRect.offsetMax = new Vector2(10, 10);
            LImage horHandleImg = horScrollHandle.AddComponent<LImage>();
            horHandleImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            horHandleImg.type = Image.Type.Sliced;

            horScrollBar.targetGraphic = horHandleImg;
            horScrollBar.handleRect = horScrollHandleRect;
            horScrollBar.direction = Scrollbar.Direction.LeftToRight;

            //세로 스크롤 바-------------------
            GameObject vertScrollBarObj = PrivCreateEmptyUIObject("ScrollbarVertical", rootObj.transform);
            RectTransform vertScrollBarRect = vertScrollBarObj.transform as RectTransform;
            LImage vertScrollBarImg = vertScrollBarObj.AddComponent<LImage>();
            vertScrollBarImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            vertScrollBarImg.type = Image.Type.Sliced;
            LScrollBar vertScrollBar = vertScrollBarObj.AddComponent<LScrollBar>();
            vertScrollBarRect.anchorMin = new Vector2(1, 0);
            vertScrollBarRect.anchorMax = new Vector2(1, 1);
            vertScrollBarRect.pivot = new Vector2(1, 1);
            vertScrollBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 20);

            GameObject vertScrollSlideAreaObj = PrivCreateEmptyUIObject("SlidingArea", vertScrollBarObj.transform);
            RectTransform vertSlideAreaRect = vertScrollSlideAreaObj.transform as RectTransform;
            vertSlideAreaRect.anchorMin = new Vector2(0, 0);
            vertSlideAreaRect.anchorMax = new Vector2(1, 1);
            vertSlideAreaRect.offsetMin = new Vector2(10, 10);
            vertSlideAreaRect.offsetMax = new Vector2(-10, -10);

            GameObject vertScrollHandle = PrivCreateEmptyUIObject("Handle", vertScrollSlideAreaObj.transform);
            RectTransform vertScrollHandleRect = vertScrollHandle.transform as RectTransform;
            vertScrollHandleRect.offsetMin = new Vector2(-10, -10);
            vertScrollHandleRect.offsetMax = new Vector2(10, 10);
            LImage vertHandleImg = vertScrollHandle.AddComponent<LImage>();
            vertHandleImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            vertHandleImg.type = Image.Type.Sliced;

            vertScrollBar.targetGraphic = vertHandleImg;
            vertScrollBar.handleRect = vertScrollHandleRect;
            vertScrollBar.direction = Scrollbar.Direction.BottomToTop;

            //스크롤-----------------------------------
            scrollRect.content = contentRect;
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 1;
            scrollRect.viewport = viewPortRect;
            scrollRect.horizontalScrollbar = horScrollBar as Scrollbar;
            scrollRect.horizontalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3;
            scrollRect.verticalScrollbar = vertScrollBar as Scrollbar;
            scrollRect.verticalScrollbarVisibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarSpacing = -3;

            return scrollRect;
        }
    }
}
