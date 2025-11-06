using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LUIZ.UI
{
    public class UIStateVisualHandler : MonoBase
    {
        [System.Serializable]
        private class StateData
        {
            public string StateName;

            public PositionChangeData[] PositionChanges;

            public ImageSpriteSwapData[] ImgSpriteSwaps;
            public ImageColorSwapData[] ImgColorSwaps;

            public TextContentSwapData[] TxtContentSwaps;
            public TextColorSwapData[] TxtColorSwaps;

            public AnimationData[] Animations;

            public UnityEvent Events;
        }

        [System.Serializable]
        private struct PositionChangeData
        {
            public RectTransform Rect;
            public Vector3 Position;
        }

        [System.Serializable]
        private struct ImageSpriteSwapData
        {
            public Image Image;
            public Sprite Sprite;
        }

        [System.Serializable]
        private struct ImageColorSwapData
        {
            public Image Image;
            public Color Color;
        }

        [System.Serializable]
        private struct TextContentSwapData
        {
            public TextMeshProUGUI Text;
            [TextArea(2, 10)]
            public string Content;
        }

        [System.Serializable]
        private struct TextColorSwapData
        {
            public TextMeshProUGUI Text;
            public Color Color;
        }

        [System.Serializable]
        private struct AnimationData
        {
            public Animator Animator;
            public string AnimationName;
        }

        //--------------------------------------------------------------
        [SerializeField] private StateData[] StateDataAry;

        //--------------------------------------------------------------
        public void GetAllStateNames(List<string> listNames)
        {
            listNames.Clear();
            foreach (StateData stateData in StateDataAry)
            {
                listNames.Add(stateData.StateName);
            }
        }

        public bool DoTryInvokeState(string stateName)
        {
            StateData stateData = PrivFindState(stateName);

            if (stateData != null)
            {
                PrivInvokeStateData(stateData);
                return true;
            }

            return false;
        }

        public bool DoTryInvokeState(int stateIndex)
        {
            StateData stateData = PrivFindState(stateIndex);

            if (stateData != null)
            {
                PrivInvokeStateData(stateData);
                return true;
            }

            return false;
        }
        public void DoInvokeState(string stateName)
        {
            StateData stateData = PrivFindState(stateName);

            if (stateData != null)
            {
                PrivInvokeStateData(stateData);
            }
        }

        public void DoInvokeState(int stateIndex)
        {
            StateData stateData = PrivFindState(stateIndex);

            if (stateData != null)
            {
                PrivInvokeStateData(stateData);
            }
        }
        //--------------------------------------------------------------
        private StateData PrivFindState(string stateName)
        {
            StateData findData = null;

            for (int i = 0; i < StateDataAry.Length; i++)
            {
                if (StateDataAry[i].StateName == stateName)
                {
                    findData = StateDataAry[i];
                    break;
                }
            }

            if (findData == null)
            {
                Debug.LogError($"[UIStateVisualHandler] State : {stateName} Not found!!!");
            }

            return findData;
        }

        private StateData PrivFindState(int stateIndex)
        {
            StateData findData = null;

            if (stateIndex >= 0 && stateIndex < StateDataAry.Length)
            {
                findData = StateDataAry[stateIndex];
            }

            if (findData == null)
            {
                Debug.LogError($"[UIStateVisualHandler] State Index : {stateIndex} Not found!!!");
            }

            return findData;
        }

        //------------------------------------------------------------------
        private void PrivInvokeStateData(StateData stateData)
        {
            PrivPositionChange(stateData);

            PrivImageSpriteSwap(stateData);
            PrivImageColorSwap(stateData);

            PrivTextContentSwap(stateData);
            PrivTextColorSwap(stateData);

            PrivAnimationPlay(stateData);

            PrivEventInvoke(stateData);
        }

        //------------------------------------------------------------------
        private void PrivPositionChange(StateData stateData)
        {
            for (int i = 0; i < stateData.PositionChanges.Length; i++)
            {
                PositionChangeData positionChangeData = stateData.PositionChanges[i];

                if (positionChangeData.Rect == null)
                    Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / PositionChange Rect is null");
                else
                {
                    positionChangeData.Rect.anchoredPosition = positionChangeData.Position;
                }
            }
        }

        private void PrivImageSpriteSwap(StateData stateData)
        {
            for (int i = 0; i < stateData.ImgSpriteSwaps.Length; i++)
            {
                ImageSpriteSwapData spriteSwapData = stateData.ImgSpriteSwaps[i];

                if (spriteSwapData.Image == null)
                    Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / SpriteSwap Image is null");
                else if (spriteSwapData.Sprite == null)
                    Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / Sprite is null");
                else
                {
                    spriteSwapData.Image.sprite = spriteSwapData.Sprite;
                }
            }
        }

        private void PrivImageColorSwap(StateData stateData)
        {
            for (int i = 0; i < stateData.ImgColorSwaps.Length; i++)
            {
                ImageColorSwapData colorSwapData = stateData.ImgColorSwaps[i];

                if (colorSwapData.Image == null)
                    Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / Image is null");
                else
                {
                    colorSwapData.Image.color = colorSwapData.Color;
                }
            }
        }

        //------------------------------------------------------------------
        private void PrivTextContentSwap(StateData stateData)
        {
            for (int i = 0; i < stateData.TxtContentSwaps.Length; i++)
            {
                TextContentSwapData contentSwapData = stateData.TxtContentSwaps[i];

                if (contentSwapData.Text == null)
                    Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / Text is null");
                else
                {
                    contentSwapData.Text.text = contentSwapData.Content;
                }
            }
        }

        private void PrivTextColorSwap(StateData stateData)
        {
            for (int i = 0; i < stateData.TxtColorSwaps.Length; i++)
            {
                TextColorSwapData colorSwapData = stateData.TxtColorSwaps[i];

                if (colorSwapData.Text == null)
                    Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / Text is null");
                else
                {
                    colorSwapData.Text.color = colorSwapData.Color;
                }
            }
        }

        //------------------------------------------------------------------
        private void PrivAnimationPlay(StateData stateData)
        {
            for (int i = 0; i < stateData.Animations.Length; i++)
            {
                AnimationData animationData = stateData.Animations[i];

                if (animationData.Animator == null)
                    Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / Animator is null");
                else
                {
                    //TODO : °ËÁõ
                    animationData.Animator.Play(animationData.AnimationName);
                }
            }
        }

        //------------------------------------------------------------------
        private void PrivEventInvoke(StateData stateData)
        {
            if (stateData.Events == null)
                Debug.LogWarning($"[UIStateVisualHandler] State : {stateData.StateName} / Event is null");
            else
            {
                stateData.Events.Invoke();
            }
        }
    }
}
