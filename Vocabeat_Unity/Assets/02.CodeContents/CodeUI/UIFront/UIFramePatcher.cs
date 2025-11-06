using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LUIZ.UI;
using LUIZ.Patcher;
using UnityEngine.UI;

public class UIFramePatcher : UIFrameBase
{
    [Header("DownloadSize")]
    [SerializeField] private Text Description;

    [Header("DownloadProgress")]
    [SerializeField] private Image ProgressBar;
    [SerializeField] private Text ProgressPercent;

    [Header("DownloadConfirmPivot")]
    [SerializeField] private GameObject DownloadConfirmPivot;

    [Header("MoveScenePivot")]
    [SerializeField] private GameObject MoveScenePivot;

    private IPatcherHandle m_patcherHandle = null;

    private string m_totalDownloadSizeMB;

    //------------------------------------------------------------
    protected override void OnUIFrameInitialize()
    {
        base.OnUIFrameInitialize();
        //UnityEngine.Caching.ClearCache();
    }

    protected override void OnUIFrameShow()
    {
        base.OnUIFrameShow();
        m_patcherHandle = ManagerPatcher.Instance.DoPatcherInitialize();
        m_patcherHandle.PatchInitComplete += OnPatcherInitComplete;
        m_patcherHandle.PatchError += OnPatcherError;
    }

    //------------------------------------------------------------
    private void PrivMoveToMaster()
    {
        ManagerLoaderScene.Instance.DoLoadSceneMaster(() =>
        {
            AsyncOperation handle = SceneManager.UnloadSceneAsync("FrontPatcher");
        });
    }

    private void PrivPatcherShowDownloadSize(long size)
    {
        m_totalDownloadSizeMB = PrivByteToMB(size).ToString();

        if (size <= 0)//다운로드 받을 번들이 없을 시
        {
            Description.text = "다운받을 내용 없음";

            m_patcherHandle.PatchFinish += PrivShowGoToMasterButton;
            ManagerPatcher.Instance.DoPatcherStart(ManagerPatcher.ELabelType.Main);
        }
        else
        {
            Description.text = $"( Download : {m_totalDownloadSizeMB} MB )";

            m_patcherHandle.PatchProgress += OnPatcherProgress;
            m_patcherHandle.PatchFinish += OnPatcherDownloadEnd;
            //m_patcherHandle.PatchFinish += PrivShowGoToMasterButton;

            DownloadConfirmPivot.SetActive(true);
        }

        Debug.LogFormat("<color=red>PrivPatcherShowDownloadSize : {0} MB </color>", m_totalDownloadSizeMB);
    }

    private void PrivShowGoToMasterButton()
    {
        MoveScenePivot.SetActive(true);
    }

    private long PrivByteToMB(long size)
    {
        return (size / 1048576);
        // 1MB = 1048576 Byte
    }

    private string PrivPatcherGetErrorMessage(EPatchErrorType errorType, string message)
    {
        string errorMessage = $"{errorType}\n";

        switch (errorType)
        {
            case EPatchErrorType.NotInitialized:
                errorMessage += "Patcher 초기화 실패 오류";
                break;
            case EPatchErrorType.AlreadyPatchProcess:
                errorMessage += "패치가 이미 진행중입니다";
                break;
            case EPatchErrorType.NotEnoughDiskSpace:
                errorMessage += "저장공간이 부족합니다";
                break;
            case EPatchErrorType.NetworkDisable:
                errorMessage += "인터넷이 연결되어 있지 않습니다";
                break;
            case EPatchErrorType.CatalogUpdateFail:
                errorMessage += "카탈로그 업데이트 실패";
                break;
            case EPatchErrorType.PatchFail:
                errorMessage += "패치가 실패하였습니다";
                break;
            case EPatchErrorType.HTTPError:
                errorMessage += "프로토콜 에러";
                break;
            case EPatchErrorType.WebRequestError:
                errorMessage += "WebRequest 에러";
                break;
        }

        if (message != null) errorMessage += $"\n{message}";

        Debug.LogFormat("<color=red>UIFramePatcher : PrivPatcherGetErrorMessage {0} </color>", errorMessage);

        return errorMessage;
    }

    //------------------------------------------------------------
    public void OnBtnMoveScene()
    {
        PrivMoveToMaster();
    }

    public void OnBtnDownloadConfirm()
    {
        DownloadConfirmPivot.SetActive(false);
        ManagerPatcher.Instance.DoPatcherStart(ManagerPatcher.ELabelType.Main);
    }

    public void OnPatcherInitComplete()
    {
        ManagerPatcher.Instance.DoPatcherTotalDownloadSize(ManagerPatcher.ELabelType.Main, PrivPatcherShowDownloadSize);
    }

    public void OnPatcherProgress(string name, long downloadedByte, long totalByte, float progress, int loadCurrent, int loadMax)
    {
        Debug.Log($"{downloadedByte} / {totalByte} Prgress : {progress}");
        ProgressBar.fillAmount = progress;
        ProgressPercent.text = $"( {PrivByteToMB(downloadedByte).ToString(),4} / {m_totalDownloadSizeMB,4} )MB";
    }

    private void OnPatcherDownloadEnd()
    {
        ProgressBar.fillAmount = 1;
        ProgressPercent.text = $"( {m_totalDownloadSizeMB,4} / {m_totalDownloadSizeMB,4} )MB";

        Debug.LogFormat("<color=red>UIFramePatcher : OnPatcherEnd {0} </color>", m_totalDownloadSizeMB);

        PrivShowGoToMasterButton();
    }

    private void OnPatcherError(EPatchErrorType errorType, string message)
    {
        Description.text = PrivPatcherGetErrorMessage(errorType, message);
    }
}
