using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LUIZ.UI;

namespace LUIZ
{
    public abstract class ManagerCameraBase : SingletonBase<ManagerCameraBase>, IManagerInstance
    {
        public enum ECameraType
        {
            Stage,      //StageCamera는 반드시 한 개만 존재한다. 만일 새로운 StageCamera가 활성화된다면 이전 Stage카메라는 비활성화 된다.
            Overlay,    //StageCamera 위에 오버레이로 올라가는 카메라. 특정 오브젝트의 컬링 등에 사용
        }

        private List<StageCameraAttacherBase> m_listStageCamera = new List<StageCameraAttacherBase>();

        private Dictionary<int, StageCameraAttacherBase> m_dicOverlayCamera = new Dictionary<int, StageCameraAttacherBase>();

        private HashSet<StageCameraAttacherBase> m_hashsetAllCamera = new HashSet<StageCameraAttacherBase>();

        private StageCameraAttacherBase m_currentStageCamera = null;

        //---------------------------------------------------------
        public bool IsInitialized()
        {
            return Instance != null;
        }

        //---------------------------------------------------------
        internal void InterManagerCameraRegister(StageCameraAttacherBase camera)
        {
            PrivStageCameraRegist(camera);
        }

        internal void InterManagerCameraUnregister(StageCameraAttacherBase camera)
        {
            PrivStageCameraUnRegist(camera);
        }

        internal void InterManagerCameraShow(StageCameraAttacherBase camera)
        {
            PrivStageCameraShow(camera);
        }

        internal void InterManagerCameraHide(StageCameraAttacherBase camera)
        {
            PrivStageCameraHide(camera);
        }

        //---------------------------------------------------------
        private void PrivStageCameraRegist(StageCameraAttacherBase camera)
        {
            int cameraID = camera.GetCameraID();
            bool isSuccess = false;

            switch (camera.GetCameraType())
            {
                case ECameraType.Stage:
                    isSuccess = PrivStageCameraTryAddStageCamera(camera);
                    break;
                case ECameraType.Overlay:
                    isSuccess = m_dicOverlayCamera.TryAdd(cameraID, camera);
                    break;
            }

            if (isSuccess == true)
            {
                m_hashsetAllCamera.Add(camera);
                camera.InterCameraInitialize();
                PrivStageCameraOverlayStack(camera);
            }
        }

        private void PrivStageCameraUnRegist(StageCameraAttacherBase camera)
        {
            int cameraID = camera.GetCameraID();
            bool isSuccess = false;

            switch (camera.GetCameraType())
            {
                case ECameraType.Stage:
                    isSuccess = m_listStageCamera.Remove(camera);
                    if (m_listStageCamera.Count > 0) //다른 스테이지 카메라가 존재하면 대체
                        PrivStageCameraShow(m_listStageCamera.Last());
                    break;
                case ECameraType.Overlay:
                    isSuccess = m_dicOverlayCamera.Remove(cameraID);
                    break;
            }

            if (isSuccess == true)
            {
                m_hashsetAllCamera.Remove(camera);
                camera.InterCameraRemove();
            }
        }

        private void PrivStageCameraShow(StageCameraAttacherBase camera)
        {
            if (m_hashsetAllCamera.Contains(camera) == false)
            {
                Debug.LogError($"[ManagerCamera] {camera.name} must be registerd before use!!");
                return;
            }

            if (camera.GetCameraType() == ECameraType.Stage)
            {
                m_currentStageCamera = camera;
            }

            camera.InterCameraShow();
            PrivStageCameraExclusive(camera);
        }

        private void PrivStageCameraHide(StageCameraAttacherBase camera)
        {
            if (m_hashsetAllCamera.Contains(camera) == false)
            {
                Debug.LogError($"[ManagerCamera] {camera.name} must be registerd before use!!");
                return;
            }

            camera.InterCameraHide();
        }

        private void PrivStageCameraExclusive(StageCameraAttacherBase showCamera)
        {
            bool isExclusive = showCamera.IsCameraExclusive();
            ECameraType cameraType = showCamera.GetCameraType();

            if (cameraType == ECameraType.Stage)
            {
                foreach (StageCameraAttacherBase cameraBase in m_listStageCamera)
                {
                    if (cameraBase.GetCameraID() != showCamera.GetCameraID())
                        cameraBase.InterCameraHide();
                }
                return;
            }

            if (isExclusive == true)
            {
                if (cameraType == ECameraType.Overlay)
                {
                    foreach (StageCameraAttacherBase cameraBase in m_dicOverlayCamera.Values)
                    {
                        if (cameraBase.GetCameraID() != showCamera.GetCameraID())
                            cameraBase.InterCameraHide();
                    }
                }
            }
        }

        private void PrivStageCameraOverlayStack(StageCameraAttacherBase camera)
        {
            //해당 카메라가 Stage 면 지금 존재하는 오버레이 카메라들을 전부 추가해준다
            //해당 카메라가 오버레이면 지금 존재하는 Stage 카메라에 추가해준다

            if (camera.GetCameraType() == ECameraType.Stage)
            {
                foreach (StageCameraAttacherBase overlayCamera in m_dicOverlayCamera.Values)
                {
                    camera.InterCameraAddOverlay(overlayCamera.GetCamera());
                }

                camera.InterCameraAddOverlay(ManagerUIBase.Instance.InterMgrUIGetUICamera());
            }
            else
            {
                if (m_currentStageCamera != null)
                    m_currentStageCamera.InterCameraAddOverlay(camera.GetCamera());
            }
        }

        private bool PrivStageCameraTryAddStageCamera(StageCameraAttacherBase camera)
        {
            bool isSuccess = false;

            if (camera.GetCameraType() == ECameraType.Stage)
            {
                if (m_listStageCamera.Contains(camera) == false)
                {
                    m_listStageCamera.Add(camera);
                    isSuccess = true;
                }
            }

            return isSuccess;
        }
    }
}
