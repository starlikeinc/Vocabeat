using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using LUIZ.AddressableSupport;

public interface ISceneLoadingWork
{
    public int WorkID { get; }
    public string WorkDescription { get; }
    
    //LoadAtLastStep = true 일 경우 해당 값이 false인 ISceneLoadingWork들이 종료 된 이후 따로 로딩함
    public bool LoadAtLastStep { get; }

    public Task DoTaskLoadingWork();
}

public class SceneLoadingWork_MinTime : ISceneLoadingWork
{
    public SceneLoadingWork_MinTime(float minTime)
    {
        m_minTime = minTime;
    }
    
    private float m_minTime = 1.0f;
    
    //--------------------------------------------------------
    public int WorkID => 9999999;
    public string WorkDescription => "Scene 최소 로딩 시간 보장";
    public bool LoadAtLastStep => false;
    
    //--------------------------------------------------------
    public async Task DoTaskLoadingWork()
    {
        await Task.Delay((int)(m_minTime * 1000f));
    }
}

//프로젝트에서 필요로 하는 씬 이동 로직은 이곳에 작성
public class ManagerLoaderScene : ManagerAddressableSceneBase
{
    public new static ManagerLoaderScene Instance => ManagerAddressableSceneBase.Instance as ManagerLoaderScene;

    public bool IsAdditionalWorkRunning { get; private set; }
    
    public static event Action<bool> OnAdditionalWorkRunningChanged;

    //------------------------------------------------------------------------
    private const string c_SceneMasterName    = "SceneMaster";
    private const string c_SceneUIMainName    = "SceneUIContainerMain";

    private const string c_SceneHomeName      = "SceneHome";
    private const string c_SceneBattleName    = "SceneBattleStage";

    private const string c_SceneLoadingName   = "SceneLoading";

    //------------------------------------------------------------------------
    private float   m_currentSceneLoadProgress = 0.0f;        public float GetCurrentSceneLoadProgress() { return m_currentSceneLoadProgress; }
    private string  m_currentSubSceneName      = string.Empty;
    
    private bool    m_isMasterSceneLoaded      = false;

    private readonly List<Task> m_listAdditionLoadWork = new();
    private readonly List<Func<Task>> m_listAdditionLoadWorkLast = new();
    
    //------------------------------------------------------------------------
    /// <summary>
    /// additionalLoadingWorks는 비동기로 작동하기 때문에 완료 순서를 보장하지 않음에 유의!!!!
    /// </summary>
    public void DoMoveToSceneHome(Action delFinish, IEnumerable<ISceneLoadingWork> additionalLoadingWorks = null)
    {
        PrivMoveToSubSceneWithLoading(c_SceneHomeName, () =>
        {
            delFinish?.Invoke();
        }, additionalLoadingWorks);
    }
    
    /// <summary>
    /// additionalLoadingWorks는 비동기로 작동하기 때문에 완료 순서를 보장하지 않음에 유의!!!!
    /// </summary>
    public void DoMoveToSceneBattle(Action delFinish, IEnumerable<ISceneLoadingWork> additionalLoadingWorks = null)
    {
        PrivMoveToSubSceneWithLoading(c_SceneBattleName ,() =>
        {
            delFinish?.Invoke();
        }, additionalLoadingWorks);
    }

    public void DoLoadSceneMaster(Action delFinish)
    {
        ProtLoadSceneMain(c_SceneMasterName, null, (string sceneName, SceneInstance sceneInstance) =>
        {
            m_isMasterSceneLoaded = true;
            delFinish?.Invoke();
        });
    }

    public void DoLoadSceneUIMain(Action delFinish)
    {
        PrivLoadUIScene(c_SceneUIMainName, () =>
        {
            delFinish?.Invoke();
        });
    }

    //------------------------------------------------------------------------
    private async Task PrivTaskWaitForAdditionalLoadWorks(IEnumerable<ISceneLoadingWork> additionalSceneWorks, Action delFinish)
    {
        if (additionalSceneWorks == null)
        {
            delFinish?.Invoke();
            return;
        }

        m_listAdditionLoadWork.Clear();
        m_listAdditionLoadWorkLast.Clear();

        SetAdditionalWorkRunning(true);

        foreach (var additionalWork in additionalSceneWorks)
        {
            if (!additionalWork.LoadAtLastStep)
            {
                //동시에 진행 가능한 작업 한번에 진행
                Debug.Log(additionalWork.WorkDescription);
                m_listAdditionLoadWork.Add(additionalWork.DoTaskLoadingWork());
            }
            else
            {
                //따로 작업해야하는 것들은 Action으로 나중에 로드하게 대기
                m_listAdditionLoadWorkLast.Add(async() =>
                {
                    Debug.Log($"[Last Work]{additionalWork.WorkDescription}");
                    await additionalWork.DoTaskLoadingWork();
                });
            }
        }
        await Task.WhenAll(m_listAdditionLoadWork);
        Debug.Log("<color=green>병렬 작업 모두 완료됨</color>");

        SetAdditionalWorkRunning(false);

        foreach (var lastWork in m_listAdditionLoadWorkLast)
        {
            if(lastWork != null)
                await lastWork();
        }
        delFinish?.Invoke();
    }

    private void SetAdditionalWorkRunning(bool isRunning)
    {
        IsAdditionalWorkRunning = isRunning;
        OnAdditionalWorkRunningChanged?.Invoke(isRunning);
    }

    private void PrivUnloadSceneLoading(Action delFinish)
    {
        ProtUnloadScene(c_SceneLoadingName, (string unloadName) => { delFinish?.Invoke(); });
    }

    private void PrivMoveToSubSceneWithLoading(string nextSceneName, Action delFinish, IEnumerable<ISceneLoadingWork> loadingSceneWorks = null)
    {
        PrivMoveToSubScene(c_SceneLoadingName, () =>
        {
            Resources.UnloadUnusedAssets().completed += (AsyncOperation operation) =>
            {
                System.GC.Collect();

                PrivMoveToSubScene(nextSceneName, () =>
                {
                    if(loadingSceneWorks != null)
                    {
                        _ = PrivTaskWaitForAdditionalLoadWorks(loadingSceneWorks, () => 
                        {
                            PrivUnloadSceneLoading(delFinish);
                        });
                    }
                    else
                    {
                        PrivUnloadSceneLoading(delFinish);
                    }
                },false);
            };
        }, true);
    }

    private void PrivMoveToSubScene(string subSceneName, Action delFinish, bool isUnloadPrevScene)
    {
        if(isUnloadPrevScene == true && m_currentSubSceneName != string.Empty)
        {
            ProtUnloadScene(m_currentSubSceneName, (string unloadSceneName) =>
            {
                PrivLoadSubScene(subSceneName, delFinish);
            });
        }
        else
        {
            if(m_isMasterSceneLoaded == true)
            {
                PrivLoadSubScene(subSceneName, delFinish);
            }
            else
            {
                DoLoadSceneMaster(() => 
                {
                    PrivLoadSubScene(subSceneName, delFinish);
                });
            }
        }
    }

    private void PrivLoadUIScene(string uiSceneName, Action delFinish)
    {
        ProtLoadSceneSub(uiSceneName, null,
            (string sceneName, SceneInstance sceneInstance) =>
            {
                delFinish?.Invoke();
            });
    }

    private void PrivLoadSubScene(string subSceneName, Action delFinish)
    {
        m_currentSceneLoadProgress = 0f;

        ProtLoadSceneSub(
            subSceneName,
            (string sceneName, float progressTotal, DownloadStatus downloadStat) => 
            {
                m_currentSceneLoadProgress = progressTotal;
            }, 
            (string sceneName, SceneInstance sceneInstance) =>
            {
                m_currentSubSceneName = subSceneName;
                delFinish?.Invoke();
            });
    }
}
