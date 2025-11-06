using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.Patcher
{
    internal class PatcherBase
    {
        private class PatcherEvent : IPatcherHandle
        {
            public event Action PatchInitComplete;
            public event Action<string, long, long, float, int, int> PatchProgress;
            public event Action<EPatchErrorType, string> PatchError;
            public event Action PatchFinish;
            public event Action<string> PatchLabelStart;

            //---------------------------------------------
            public void DoInitComplete() { PatchInitComplete?.Invoke(); }
            public void DoPatchFinish() { PatchFinish?.Invoke(); }
            public void DoProgress(string Name, long _downloadedByte, long _totalByte, float Progress, int _loadCurrent, int _loadMax) { PatchProgress?.Invoke(Name, _downloadedByte, _totalByte, Progress, _loadCurrent, _loadMax); }
            public void DoError(EPatchErrorType _errorType, string _message) { PatchError?.Invoke(_errorType, _message); }
            public void DoLabelStart(string Name) { PatchLabelStart?.Invoke(Name); }

            public void DoReset()
            {
                PatchInitComplete = null;
                PatchProgress = null;
                PatchError = null;
                PatchFinish = null;
                PatchLabelStart = null;
            }
        }

        //---------------------------------------------------------------------
        private PatcherEvent m_PatchEvent = new PatcherEvent();

        //---------------------------------------------------------------------
        internal IPatcherHandle InterPatcherInitialize(string downloadURL, string downloadSavePath, bool resetHandler)
        {
            if (resetHandler)
            {
                m_PatchEvent.DoReset();
            }

            OnPatcherInitialize(downloadURL, downloadSavePath);

            return m_PatchEvent;
        }

        internal void InterPatcherUpdateEvent(float deltaTime)
        {
            OnPatcherUpdateEvent(deltaTime);
        }

        //---------------------------------------------------------------------
        protected void ProtPatchInitComplete()
        {
            m_PatchEvent.DoInitComplete();
        }

        protected void ProtPatchProgress(string _patchName, long _downloadedByte, long _totalByte, float _percent, int _loadCurrent, int _loadMax)
        {
            m_PatchEvent.DoProgress(_patchName, _downloadedByte, _totalByte, _percent, _loadCurrent, _loadMax);
        }

        protected void ProtPatchError(EPatchErrorType _errorType, string _message = null)
        {
            m_PatchEvent.DoError(_errorType, _message);
        }

        protected void ProtPatchFinish()
        {
            m_PatchEvent.DoPatchFinish();
        }

        protected void ProtPatchLabelStart(string _labelName)
        {
            m_PatchEvent.DoLabelStart(_labelName);
        }

        //---------------------------------------------------------------------
        protected virtual void OnPatcherInitialize(string strDownloadURL, string strDownloadSavePath) { }
        protected virtual void OnPatcherUpdateEvent(float DeltaTime) { }
    }
}
