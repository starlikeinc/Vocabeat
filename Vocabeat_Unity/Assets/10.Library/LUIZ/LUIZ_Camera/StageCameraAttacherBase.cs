using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LUIZ
{
    public abstract class StageCameraAttacherBase : MonoBase
    {
        [SerializeField] private ManagerCameraBase.ECameraType CameraType;  public ManagerCameraBase.ECameraType GetCameraType() { return CameraType; }
        [SerializeField] private bool ShowOnStart = true;                   public bool IsShowOnStart() { return ShowOnStart; }
        [SerializeField] private bool IsExclusive = true;                   public bool IsCameraExclusive() { return IsExclusive; }

        protected Camera m_camera = null;

        private UniversalAdditionalCameraData m_universalCameraData = null;
        private int m_cameraCullingMask = 0;

        private bool m_isInitialized = false;

        //--------------------------------------------------------
        protected override void OnUnityStart()
        {
            base.OnUnityStart();
            PrivStageCameraRegister();
        }

        protected override void OnUnityDestroy()
        {
            base.OnUnityDestroy();
            PrivStageCameraUnregister();
        }

        //--------------------------------------------------------
        public Camera GetCamera()
        {
            if (m_camera == null)
            {
                m_camera = GetComponent<Camera>();
            }
            return m_camera;
        }

        public int GetCameraID()
        {
            return GetCamera().GetInstanceID();
        }

        public void ShowCamera()
        {
            ManagerCameraBase.Instance.InterManagerCameraShow(this);
        }

        public void HideCamera()
        {
            ManagerCameraBase.Instance.InterManagerCameraHide(this);
        }

        //--------------------------------------------------------
        internal void InterCameraInitialize()
        {
            if (m_isInitialized) return;

            m_isInitialized = true;
            m_camera = GetComponent<Camera>();
            m_cameraCullingMask = m_camera.cullingMask;
            m_universalCameraData = GetComponent<UniversalAdditionalCameraData>();

            if (CameraType == ManagerCameraBase.ECameraType.Stage)
            {
                m_universalCameraData.renderType = CameraRenderType.Base;
                this.gameObject.tag = "MainCamera";
            }
            else
            {
                m_universalCameraData.renderType = CameraRenderType.Overlay;
            }

            OnCameraInitialize();
        }

        internal void InterCameraHide()
        {
            this.gameObject.SetActive(false);
            OnCameraHide();
        }

        internal void InterCameraShow()
        {
            this.gameObject.SetActive(true);
            OnCameraShow();
        }

        internal void InterCameraRemove()
        {
            PrivClearOverlayStack();
            OnCameraRemove();
        }

        internal void InterCameraAddOverlay(Camera overlayCamera)
        {
            if (overlayCamera == null)
                return;

            if (m_universalCameraData.cameraStack.Contains(overlayCamera) == true)
                return;

            m_universalCameraData.cameraStack.Add(overlayCamera);
        }

        //--------------------------------------------------------
        private void PrivStageCameraRegister()
        {
            if (ManagerCameraBase.Instance != null)
            {
                ManagerCameraBase.Instance.InterManagerCameraRegister(this);

                if (ShowOnStart)
                {
                    ManagerCameraBase.Instance.InterManagerCameraShow(this);
                }
                else
                {
                    ManagerCameraBase.Instance.InterManagerCameraHide(this);
                }

                OnCameraRegist();
            }
        }

        private void PrivStageCameraUnregister()
        {
            ManagerCameraBase.Instance?.InterManagerCameraUnregister(this);
            OnCameraUnregist();
        }

        private void PrivClearOverlayStack()
        {
            if (m_universalCameraData.renderType == CameraRenderType.Base)
                m_universalCameraData.cameraStack.Clear();
        }

        //--------------------------------------------------------
        protected virtual void OnCameraInitialize() { }

        protected virtual void OnCameraRegist() { }
        protected virtual void OnCameraUnregist() { }

        protected virtual void OnCameraShow() { }
        protected virtual void OnCameraHide() { }

        protected virtual void OnCameraRemove() { }

        protected virtual void OnCameraOverlayStack(Camera overlayCamera) { }
    }
}
