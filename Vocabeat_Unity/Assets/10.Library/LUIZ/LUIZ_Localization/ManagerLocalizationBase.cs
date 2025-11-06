using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

namespace LUIZ.Localization
{
    /// [주의!]
    /// 1. 프로젝트 폴더의 Localization Settings에서 PreloadBehavior를 No Preloading 으로 할 것.
    /// 2. 각 로컬라이징 Table들에서 Preload를 false로 할 것.
    /// (ManagerLocalizeBase의 함수로 항상 수동으로 로드 하도록 하기 위함)
    public class ManagerLocalizeBase : SingletonBase<ManagerLocalizeBase>
    {
        private static readonly CustomLocalizationTableProvider m_customStringTableProvider = new CustomLocalizationTableProvider();

        private ILocalesProvider m_localeProviders = null;

        // LocalizationSettings.AvailableLocales는 기본적으로 자동으로 할당되는데
        // Locale 설정 유무를 직접 컨트롤 하기 위해 따로 선언하여 이용
        private Locale m_curLocale = null;

        //--------------------------------------------------
        /// <summary> LocalizationSettings.SelectedLocale을 세팅해주는 함수
        /// <para> LocalizationSettings의 AvailableLocales에서 검색 후 세팅하기 때문에 잘못된 값 제공시 반환 값이 Null 일수 있음 </para></summary>
        protected async Awaitable<Locale> ProtMgrLocalizeTrySetLocale(string localeName)
        {
            await LocalizationSettings.InitializationOperation.Task;

            if (m_localeProviders == null)
                m_localeProviders = LocalizationSettings.AvailableLocales;

            if (m_curLocale != null && m_curLocale.LocaleName == localeName)//이미 로드된 Locale임
                return m_curLocale;

            if (TryGetLocale(localeName, out Locale localeFound) == false)
                Debug.LogError("[ManagerLocalize] locale not found!!!");
            else
            {
                m_curLocale = localeFound;

                //SelectedLocale이 변경되면 이전 Locale에서 사용한 asset들은 자동으로 Release 된다고함 (아래 링크 ReleaseAssets() 부분 참고)
                //참고 : https://docs.unity3d.com/Packages/com.unity.localization@1.0/api/UnityEngine.Localization.Tables.AssetTable.html
                LocalizationSettings.SelectedLocale = localeFound;
                Debug.Log("Locale Set :" + localeFound.LocaleName);
            }

            return localeFound;
        }

        /// <summary> 현재 Locale에 기반한 유니티 LocaLocalization Tables 에서 번들로 세팅된 테이블 로드.</summary>
        protected async Awaitable<TTable> ProtMgrLocalizeLoadTable<TTable>(string tableName) where TTable : LocalizationTable
        {
            if (m_curLocale == null)
            {
                Debug.LogError("[ManagerLocalizeBase] Error! set locale first.");
                return null;
            }

            //------------------------------
            return await LoadTable<TTable>(tableName, m_curLocale, null);
        }

        /// <summary> <see cref="ICustomLocaleData"/>를 제공하면 테이블을 런타임에 생성한 후 반환한다. 이미 생성한 테이블의 경우 캐싱된 값 반환
        /// <para> 런타임에 생성한거라 게임 재실행 하면 다시 생성해야함. 싫다면 번들로 만들어서 <see cref="ProtMgrLocalizeLoadTable"/>이용할 것</para></summary>
        protected async Awaitable<TTable> ProtMgrLocalizeCustomTable<TTable>(string tableName, ICustomLocaleData customLocaleData) where TTable : LocalizationTable
        {
            if (m_curLocale == null)
            {
                Debug.LogError("[ManagerLocalizeBase] Error! set locale first.");
                return null;
            }

            m_customStringTableProvider.SetCustomTableProviderData<TTable>(m_curLocale, customLocaleData);

            return await LoadTable<TTable>(tableName, m_curLocale, m_customStringTableProvider);
        }

        /// <summary> 이미 존재하는 <see cref="LocalizationTable"/> <see cref="ICustomLocaleData"/>를 데이터 엔트리들을 추가해주는 함수
        /// <para> <see cref="StringTable"/>일 경우 entry value를 필요한 string 값으로, <see cref="AssetTable"/>일 경우 asset guid string값을 제공할 것</para>
        /// <para> 런타임에 생성한거라 게임 재실행 하면 다시 추가해야함. 싫다면 번들로 만들어서 <see cref="ProtMgrLocalizeLoadTable"/>이용할 것</para></summary>
        protected void ProtMgrLocalizeAddCustomData<TTable>(TTable table, ICustomLocaleData customLocaleData) where TTable : LocalizationTable
        {
            if (table is StringTable stringTable)
            {
                foreach (var entry in customLocaleData.Entries)
                    stringTable.AddEntry(entry.EntryKey, entry.EntryValue);
            }
            else if (table is AssetTable assetTable)
            {
                //TODO : AssetTable 도 추가하도록 추가
                foreach (var entry in customLocaleData.Entries)
                    assetTable.AddEntry(entry.EntryKey, entry.EntryValue);
            }
        }

        protected string ProtMgrLocalizeGetTableDataString(StringTable stringTable, string key)
        {
            if (stringTable == null)
                return string.Empty;

            var entry = stringTable.GetEntry(key);
            if (entry == null)
                return string.Empty;
            else
                return entry?.GetLocalizedString();
        }

        protected void ProtMgrLocalizeGetTableDataAsset<TAsset>(AssetTable assetTable, string key, Action<TAsset> delFinish) where TAsset : UnityEngine.Object
        {
            if (assetTable == null)
            {
                delFinish?.Invoke(null);
                return;
            }

            assetTable.GetAssetAsync<TAsset>(key).Completed += (handle) =>
            {
                delFinish?.Invoke(handle.Result);
            };
        }

        //-------------------------------------------------------------
        private async Awaitable<TTable> LoadTable<TTable>(string tableName, Locale locale, ITableProvider tableProvider) where TTable : LocalizationTable
        {
            if(typeof(TTable) == typeof(StringTable))
                LocalizationSettings.StringDatabase.TableProvider = tableProvider;
            else if (typeof(TTable) == typeof(AssetTable))
                LocalizationSettings.AssetDatabase.TableProvider = tableProvider;

            AsyncOperationHandle? tableOp = FindTableHandle<TTable>(tableName, locale);
            if (tableOp == null)
                return null;

            //------------------------------
            TTable loadedtable = null;

            await tableOp.Value.Task;

            if (tableOp.Value.Status == AsyncOperationStatus.Succeeded)
                loadedtable = tableOp.Value.Result as TTable;
            else
                Debug.Log("Could not load Table\n" + tableOp.Value.OperationException.ToString());

            return loadedtable;
        }

        private AsyncOperationHandle? FindTableHandle<TTable>(string tableName, Locale locale) where TTable : LocalizationTable
        {
            AsyncOperationHandle? tableOp = null;

            if (typeof(TTable) == typeof(StringTable))
                tableOp = LocalizationSettings.StringDatabase.GetTableAsync(tableName, locale);
            else if (typeof(TTable) == typeof(AssetTable))
                tableOp = LocalizationSettings.AssetDatabase.GetTableAsync(tableName, locale);
            else
                tableOp = null;

            return tableOp;
        }

        private bool TryGetLocale(string localeName, out Locale localeFound)
        {
            localeFound = null;
            foreach (var locale in m_localeProviders.Locales)
            {
                if (locale.LocaleName == localeName)
                {
                    localeFound = locale;
                    return true;
                }
            }
            return false;
        }
    }
}