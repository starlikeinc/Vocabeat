using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace LUIZ
{
    public class ManagerApplication : SingletonBase<ManagerApplication>, IManagerInstance
    {
        [SerializeField] private int TargetFrameRate = 60;

        [SerializeField] private bool ShowFPS = true;


        //Logo씬부터 진입하여 플레이 중인 플레이 과정일 시. false일 경우 develop씬이나 기타 어태쳐로 플레이 중임
        private bool m_isRegularAppStart = true;                        public bool IsRegularAppPlay() { return m_isRegularAppStart; }

        private float m_deltaTime = 0f;
        private GUIStyle m_GUIStyle = new GUIStyle();

        //------------------------------------------------------------
        protected override void OnUnityAwake()
        {
            base.OnUnityAwake();

            if (SceneManager.GetActiveScene().name != SceneManager.GetSceneByBuildIndex(0).name)
            {
                m_isRegularAppStart = false;
            }

            Application.targetFrameRate = TargetFrameRate;
            Application.runInBackground = true;
            Input.simulateMouseWithTouches = true;
            Input.multiTouchEnabled = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;

            PrivApplicationGUIFont();
        }

        protected override void OnUnityStart()
        {
            PrivApplicationPrintDebug();
        }

        //--------------------------------------------------------
        public bool IsInitialized()
        {
            return Instance != null;
        }

        //--------------------------------------------------------
        private void Update()
        {
            if (ShowFPS)
            {
                m_deltaTime += (Time.deltaTime - m_deltaTime) * 0.1f;
            }
        }

        private void OnGUI()
        {
            if (ShowFPS)
            {
                float msec = m_deltaTime * 1000.0f;
                float fps = 1.0f / m_deltaTime;
                string text = string.Format("{0:0.0}ms, FPS: {1:0.}", msec, fps);

                GUI.Label(new Rect(0, 0, 200, 100), text, m_GUIStyle);
                GUI.Label(new Rect(0, 0, 200, 100), text, m_GUIStyle);
            }
        }

        private void PrivApplicationGUIFont()
        {
            m_GUIStyle.fontSize = 20;
            m_GUIStyle.normal.textColor = Color.green;
        }

        private void PrivApplicationPrintDebug()
        {
            Debug.LogFormat("[Screen Info] Screen With {0} / Screen Height {1}", Screen.width, Screen.height);
        }
    }
}
