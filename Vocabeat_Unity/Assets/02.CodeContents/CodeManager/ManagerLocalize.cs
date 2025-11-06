using LUIZ.Localization;
using System.Collections.Generic;
using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine;

//여러 군데서 반복적으로 필요한 이넘들의 표기명을 받아올 때 이용
//일반적인 텍스트는 각 텍스트 컴포넌트의 식별코드를 이용함
public class ManagerLocalize : ManagerLocalizeBase
{
    public new static ManagerLocalize Instance => ManagerLocalizeBase.Instance as ManagerLocalize;

    //-------------------------------------------------------------------------
    public enum ELocalizeLocaleType
    {
        None,

        //어드레서블 이름과 동일해야한다.
        Locale_English,
        Locale_Japanese,
        Locale_Korean,

        Locale_ChineseSimplified,
        Locale_ChineseTraditional
    }

    public enum ELocalizeLoadResult
    {
        None, //성공

        LocaleNotSet,
        TableLoadFail
    }

    /// <summary> 현재 Locale의 모든 로컬라이징 Table들이 로드 완료되었을 때 호출 된다.
    /// <para> [주의!!!] </para>
    /// <para> <see cref="StringTable"/>의 경우 동기 로드지만 <see cref="AssetTable"/>은 비동기 로드되기 때문에 사용자는 유의가 필요</para>
    /// </summary>
    public static event Action<ELocalizeLocaleType> OnLocaleTableLoaded;

    //-------------------------------------------------------------
    //TODO 우선 2개만 선언해놨는데 여러개가 되면 추후에 List등으로 묶는게 나을수도 있음
    private readonly string m_tableStringName = "Localization_TableString";
    private readonly string m_tableAssetName = "Localization_TableAsset";
    private StringTable m_tableString = null;
    private AssetTable m_tableAsset = null;

    private ELocalizeLocaleType m_curTableLoadedType = ELocalizeLocaleType.None;

    //-------------------------------------------------------------
    public ELocalizeLocaleType CurrentLocaleType { get; private set; } = ELocalizeLocaleType.None;
    public bool IsTableLoaded => m_curTableLoadedType == CurrentLocaleType && m_curTableLoadedType != ELocalizeLocaleType.None;

    //-------------------------------------------------------------
    /// <summary>Locale을 변경한다. 
    /// <para> autoLoadAllTables = true일 경우 TableLoad까지 자동으로 해줌 </para>
    /// <para> autoLoadAllTables = false일 경우 DoMgrLocalizeLoadAllCurrentLocaleTables 수동 호출할 것</para>
    /// </summary>
    public async Awaitable<ELocalizeLoadResult> DoMgrLocalizeSetLocale(ELocalizeLocaleType localeType, bool autoLoadAllTables = true)
    {
        string localeName = localeType.ToString();

        Locale localeSet = await ProtMgrLocalizeTrySetLocale(localeName);
        if (localeSet == null)
        {
            CurrentLocaleType = ELocalizeLocaleType.None;
            return ELocalizeLoadResult.LocaleNotSet;
        }
        else
        {
            CurrentLocaleType = localeType;
        }

        if (autoLoadAllTables)
            return await DoMgrLocalizeLoadAllCurrentLocaleTables();
        else
            return ELocalizeLoadResult.None;
    }

    public async Awaitable<ELocalizeLoadResult> DoMgrLocalizeLoadAllCurrentLocaleTables()
    {
        if (CurrentLocaleType == ELocalizeLocaleType.None)
        {
            return ELocalizeLoadResult.LocaleNotSet;
        }

        return await PrivTryLoadAllTables(CurrentLocaleType);
    }

    //---------------------------------------------------------------
    public string DoMgrLocalizeGetString(string key)
    {
        return ProtMgrLocalizeGetTableDataString(m_tableString, key);
    }

    public void DoMgrLocalizeGetAudio(string key, Action<AudioClip> delFinish)
    {
        ProtMgrLocalizeGetTableDataAsset<AudioClip>(m_tableAsset, key, delFinish);
    }

    //-------------------------------------------------------------
    private async Awaitable<ELocalizeLoadResult> PrivTryLoadAllTables(ELocalizeLocaleType localeType)
    {
        if (localeType == m_curTableLoadedType)//이미 현재 Locale과 이전에 로드된 Table이 동일하면
            return ELocalizeLoadResult.None;

        m_tableString = await ProtMgrLocalizeLoadTable<StringTable>(m_tableStringName);
        m_tableAsset = await ProtMgrLocalizeLoadTable<AssetTable>(m_tableAssetName);

        ELocalizeLoadResult result = ELocalizeLoadResult.None;
        if (m_tableString != null && m_tableAsset != null)
        {
            Debug.Log($"Initialized All Localization Tables \n Locale : {localeType.ToString()}");
            m_curTableLoadedType = localeType;
            OnLocaleTableLoaded?.Invoke(m_curTableLoadedType);
        }
        else
        {
            Debug.LogWarning($"Initialize Localization Tables FAILED");
            result = ELocalizeLoadResult.TableLoadFail;
            m_curTableLoadedType = ELocalizeLocaleType.None;
        }

        return result;
    }
}
