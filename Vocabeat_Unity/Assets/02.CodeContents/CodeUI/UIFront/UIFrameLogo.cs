using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using LUIZ.UI;

public class UIFrameLogo : UIFrameBase
{
    [System.Serializable]
    private class FadeInOutData
    {
        [Tooltip("FadeIn, FadeOut시 키고 꺼줘야 하는 부모 오브젝트")]
        public GameObject PageParent;

        [Tooltip("해당 슬라이드에서 FadeIn, FadeOut할 그래픽들")]
        public List<Graphic> FadeInOutGraphics = new List<Graphic>();

        [Tooltip("FadeIn 시간")]
        public float FadeInTime;

        [Tooltip("FadeOut 시간")]
        public float FadeOutTime;

        [Tooltip("FadeIn 완료 후 FadeOut 시작 전까지 대기 시간")]
        public float ShowTime;
    }

    [SerializeField] private List<FadeInOutData> FadeInOutDataList;

    [Tooltip("모든 슬라이드쇼 완료 후 다음 씬으로 넘어가기 전까지 대기 시간")]
    [SerializeField] private float NextSceneMoveTime = 1;

    //-----------------------------------------------------
    protected override void OnUIFrameInitialize()
    {
        base.OnUIFrameInitialize();
        UIChannel.UIShow<UIFrameLogo>();
    }

    protected override void OnUIFrameShow()
    {
        base.OnUIFrameShow();
        StartCoroutine(CoroutineSceneStepFadeInGraphics());
    }

    //-----------------------------------------------------
    private IEnumerator CoroutineSceneStepFadeInGraphics()
    {
        for (int i = 0; i < FadeInOutDataList.Count; i++)
        {
            FadeInOutData fadeInOutData = FadeInOutDataList[i];

            yield return StartCoroutine(CoroutineSceneStepFadeInOut(fadeInOutData, true));
            yield return StartCoroutine(CoroutineSceneStepFadeInOut(fadeInOutData, false));
        }

        Invoke("PrivMoveToPatcher", NextSceneMoveTime);
    }

    private IEnumerator CoroutineSceneStepFadeInOut(FadeInOutData fadeInOutData, bool bFadeIn)
    {
        if (bFadeIn) fadeInOutData.PageParent.SetActive(true);

        float progressTime = 0;
        float endTime = bFadeIn ? fadeInOutData.FadeInTime : fadeInOutData.FadeOutTime;

        while (progressTime <= endTime)
        {
            float progress = bFadeIn ? (progressTime / endTime) : 1 - (progressTime / endTime);

            for (int k = 0; k < fadeInOutData.FadeInOutGraphics.Count; k++)
            {
                Color pGraphicColor = fadeInOutData.FadeInOutGraphics[k].color;
                fadeInOutData.FadeInOutGraphics[k].color = new Color(pGraphicColor.r, pGraphicColor.g, pGraphicColor.b, progress);
            }

            progressTime += Time.deltaTime;

            yield return null;
        }

        if (bFadeIn) yield return new WaitForSeconds(fadeInOutData.ShowTime);
        if (!bFadeIn) fadeInOutData.PageParent.SetActive(false);
    }

    private void PrivMoveToPatcher()
    {
        //Network.Instance.DoNetworkSessionStart(Network.ENetSessionType.Virtual);
        //SceneManager.LoadScene("FrontPatcher");

        ManagerLoaderScene.Instance.DoLoadSceneMaster(() =>
        {
            SceneManager.UnloadSceneAsync("FrontLogo");            
        });
    }
}
