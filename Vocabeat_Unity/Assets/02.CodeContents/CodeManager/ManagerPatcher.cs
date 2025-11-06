using System;
using System.Collections.Generic;
using UnityEngine;
using LUIZ.AddressableSupport;
using LUIZ.Patcher;

public class ManagerPatcher : ManagerAddressablePatcherBase
{
    public static new ManagerPatcher Instance { get { return ManagerAddressablePatcherBase.Instance as ManagerPatcher; } }

    //-------------------------------------------------------------------------
    public enum ELabelType
    {
        Main,
        DLC1,
    }

    [SerializeField] private string MainLabelName = "Main";

    private List<string> m_listAssetBundleLabels = new List<string>();
    
    //-------------------------------------------------------------------------
    public void SetDownloadURL(string downloadURL)
    {
        DownloadURL = downloadURL;
    }

    public IPatcherHandle DoPatcherInitialize()
    {
        return ProtPatcherInitialize();
    }

    public void DoPatcherStart(ELabelType labelType)
    {
        PrivListLabelNamesByType(labelType);

        ProtPatcherStart(m_listAssetBundleLabels);
    }

    public void DoPatcherTotalDownloadSize(ELabelType labelType, Action<long> delFinish)
    {
        PrivListLabelNamesByType(labelType);

        ProtPatcherTotalDowloadSize(m_listAssetBundleLabels, delFinish);
    }

    //-------------------------------------------------------------------------

    private void PrivListLabelNamesByType(ELabelType eLabelType)
    {
        m_listAssetBundleLabels.Clear();

        if (eLabelType == ELabelType.Main)
        {
            m_listAssetBundleLabels.Add(MainLabelName);
        }
    }
}
