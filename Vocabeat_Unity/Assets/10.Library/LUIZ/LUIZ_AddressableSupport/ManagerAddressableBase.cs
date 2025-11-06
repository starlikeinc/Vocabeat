using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LUIZ.AddressableSupport
{
    public abstract class ManagerAddressableBase<TProvider, TType> : SingletonBase<ManagerAddressableBase<TProvider, TType>>, IManagerInstance where TProvider : AddressableProviderBase<TProvider, TType>, new()
    {
        /// <summary> 동시 다운로드 </summary>
        public int ConcurrentCount
        {
            get => m_conCurrentCount;
            set => m_conCurrentCount = Math.Max(1, value);
        }
        private int m_conCurrentCount = 999;

        private readonly LinkedList<TProvider>   m_listCurrentProvider   = new LinkedList<TProvider>();
        private readonly Queue<TProvider>        m_queStandByProvider    = new Queue<TProvider>();

        //------------------------------------------------------------
        private void Update()//TODO 업데이트 말고 코루틴 같은거로 대체할것
        {
            if (m_listCurrentProvider.Count > 0)
            {
                foreach (TProvider provider in m_listCurrentProvider)
                {
                    provider.UpdateLoadWork();
                }
            }

            PrivNextProvider();
        }

        //------------------------------------------------------------
        public bool IsAddressableLoading()
        {
            bool isWorking = !(m_listCurrentProvider.Count == 0 && m_queStandByProvider.Count == 0);
            return isWorking;
        }
        public bool IsInitialized()
        {
            return Instance != null;
        }

        //-----------------------------------------------------------------------
        protected void ProtRequestLoad(string strAddressableName, Action<string, float, DownloadStatus> delProgress, Action<AddressableProviderBase<TProvider, TType>.ProviderLoadResult> delFinish)
        {
            PrivAddressableEnque(strAddressableName, delProgress, delFinish);
        }

        //-----------------------------------------------------------------------
        private void PrivNextProvider()
        {
            int EmptyCount = m_conCurrentCount - m_listCurrentProvider.Count;

            for (int i = 0; i < EmptyCount; i++)
            {
                if (m_queStandByProvider.Count == 0)
                    return;
                
                TProvider provider = m_queStandByProvider.Dequeue();
                m_listCurrentProvider.AddLast(provider);
                provider.DoProviderLoadStart();
            }
        }

        private void PrivDeleteProvider(TProvider provider)
        {
            AddressableProviderBase<TProvider, TType>.InstancePoolReturn(provider);
            m_listCurrentProvider.Remove(provider);
        }

        private void PrivAddressableEnque(string addressableName, Action<string, float, DownloadStatus> delProgress, Action<AddressableProviderBase<TProvider, TType>.ProviderLoadResult> delFinish)
        {
            TProvider provider = AddressableProviderBase<TProvider, TType>.InstancePoolGet<TProvider>();

            provider.SetProviderLoadPrepare(addressableName, delProgress,
                (AddressableProviderBase<TProvider, TType>.ProviderLoadResult result) =>
                {
                    PrivDeleteProvider(provider);
                    delFinish?.Invoke(result);
                }, HandleLoadError);

            m_queStandByProvider.Enqueue(provider);
        }

        //-----------------------------------------------------------------------
        private void HandleLoadError(TProvider provider, string addressableName, string error)
        {
            PrivDeleteProvider(provider);

            OnAddressableLoadError(addressableName, error);
        }

        //-----------------------------------------------------------------------
        protected virtual void OnAddressableLoadError(string addressableName, string errorMessage) { }
    }
}
