using System;
using System.Collections.Generic;
using LUIZ.Patcher;
using UnityEngine;

namespace LUIZ.AddressableSupport
{
    //AddressableSetting에 MaxConcurrentWebRequest를 500에서 1~10정도로 변경한 후 에셋번들 빌드를 해야한다. 한꺼번에 리퀘스트가 몰리면 딜레이가 상당히 걸리게 된다.
    //동적으로 Remote 다운로드 경로를 설정하고 싶으면 어드레서블 profile의 경로에 {ManagerAddressablePatcherBase.DownloadURL} 로 삽입할 것.

    public class ManagerAddressablePatcherBase : SingletonBase<ManagerAddressablePatcherBase>
    {
        public static string DownloadURL = string.Empty;

        private readonly PatcherAddressable m_patcherAddressable = new PatcherAddressable();

        //-----------------------------------------------------------------
        protected virtual void Update() //TODO : 업데이트 말고 코루틴이나 await등으로 수정할 것
        {
            m_patcherAddressable.InterPatcherUpdateEvent(0);
        }

        //-----------------------------------------------------------------
        protected IPatcherHandle ProtPatcherInitialize()
        {
            IPatcherHandle handle = m_patcherAddressable.InterPatcherInitialize("", string.Empty, true);

            handle.PatchInitComplete  += OnPatcherInitComplete;
            handle.PatchFinish        += OnPatcherFinish;
            handle.PatchProgress      += OnPatcherProgress;
            handle.PatchError         += OnPatcherError;
            handle.PatchLabelStart    += OnPatcherStartLabel;

            return handle;
        }

        protected void ProtPatcherStart(List<string> listBundleLabelNames)
        {
            m_patcherAddressable.DoPatcherAddressableStart(listBundleLabelNames);
        }

        protected void ProtPatcherTotalDowloadSize(List<string> listBundleLabelNames, Action<long> delFinish)
        {
            m_patcherAddressable.DoPatcherAddressableDowloadSize(listBundleLabelNames, delFinish);
        }

        protected void ProtPatcherCleanExpiredBundles(Action delFinish)
        {
            m_patcherAddressable.DoPatcherAddressableCleanExpiredBundles(delFinish);
        }

        //---------------------------------------------------------------------------
        protected virtual void OnPatcherInitComplete() { }
        protected virtual void OnPatcherProgress(string labelhName, long downloadedByte, long totalByte, float percent, int loadCurrent, int loadMax) { }
        protected virtual void OnPatcherFinish() { }
        protected virtual void OnPatcherStartLabel(string strLabelName) { }
        protected virtual void OnPatcherError(EPatchErrorType errorType, string smessage) { }
    }
}
