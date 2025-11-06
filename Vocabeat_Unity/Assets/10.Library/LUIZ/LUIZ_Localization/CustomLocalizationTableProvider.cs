using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LUIZ.Localization
{
    public interface ICustomLocaleData
    {
        public string TableName { get; }
        public string LocaleName { get; }
        public IEnumerable<ICustomLocaleEntry> Entries { get; }
    }

    public interface ICustomLocaleEntry
    {
        public string EntryKey { get; }
        public string EntryValue {  get; }
    }

    public class CustomLocalizationTableProvider : ITableProvider
    {
        private Dictionary<Locale, List<LocalizationTable>> m_dicCustomTables = new Dictionary<Locale, List<LocalizationTable>>();

        //shardTable은 여러 Locale의 같은 테이블들이 공유하는 정보이다. Entry 이름, Table 이름 등 ( Locale 마다 동일한 타입의 Table 이 있기 때문)
        private Dictionary<string, SharedTableData> m_dicSharedTableData = new Dictionary<string, SharedTableData>();

        private ICustomLocaleData m_customLocaleData = null;

        public void RemoveTableProviderData(Locale locale)
        {
            //TODO

            //locale 바꾸면 기존 locale로 캐싱된 table 데이터를 레퍼런스를 제거한다. 커스텀 provider라 따로 메모리 헨들링을 해야함

            //[주의]
            //원본 데이터를 내린 후에 여기서 레퍼런스를 지워야 의미 있지 레퍼런스만 지워봤자 의미 없음
        }

        /// <summary> <see cref="StringTable"/>일 경우 entry value를 필요한 string 값으로, <see cref="AssetTable"/>일 경우 asset guid string값을 제공할 것</summary>
        public void SetCustomTableProviderData<TTable>(Locale locale, ICustomLocaleData customData)
        {
            if (customData.LocaleName != locale.LocaleName)
            {
                //Locale 불일치
                m_customLocaleData = null;
                return;
            }

            m_customLocaleData = customData;

            if (m_dicSharedTableData.ContainsKey(customData.TableName) == false)
            {
                SharedTableData sharedTableData = ScriptableObject.CreateInstance<SharedTableData>();
                sharedTableData.TableCollectionName = customData.TableName;
                m_dicSharedTableData.Add(customData.TableName, sharedTableData);
            }

            LocalizationTable cachedTable = null;
            if (m_dicCustomTables.TryGetValue(locale, out List<LocalizationTable> customTables))
            {
                foreach (var table in customTables)
                {
                    if (table.TableCollectionName == customData.LocaleName)
                    {
                        cachedTable = table;
                        break;
                    }
                }
            }
            else
            {
                m_dicCustomTables.Add(locale, new List<LocalizationTable>());
            }

            if(cachedTable == null)
            {
                if (typeof(TTable) == typeof(StringTable))
                {
                    StringTable newStringTable = ScriptableObject.CreateInstance<StringTable>();
                    newStringTable.SharedData = m_dicSharedTableData[customData.TableName];

                    foreach (var entry in m_customLocaleData.Entries)
                        newStringTable.AddEntry(entry.EntryKey, entry.EntryValue);

                    m_dicCustomTables[locale].Add(newStringTable);
                }
                else if(typeof(TTable) == typeof(AssetTable))
                {
                    AssetTable newAssetTable = ScriptableObject.CreateInstance<AssetTable>();
                    newAssetTable.SharedData = m_dicSharedTableData[customData.TableName];

                    foreach (var entry in m_customLocaleData.Entries)
                        newAssetTable.AddEntry(entry.EntryKey, entry.EntryValue);

                    m_dicCustomTables[locale].Add(newAssetTable);
                }
            }
        }

        public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(string tableCollectionName, Locale locale) where TTable : LocalizationTable
        {
            if(m_customLocaleData == null)
            {
                //ERROR
                return default;
            }

            LocalizationTable cachedTable = null;
            if (m_dicCustomTables.TryGetValue(locale, out List<LocalizationTable> customTables))
            {
                foreach (var table in customTables)
                {
                    if (table.TableCollectionName == tableCollectionName)
                    {
                        cachedTable = table;
                        break;
                    }
                }
            }

            if (cachedTable != null)
            {
                return Addressables.ResourceManager.CreateCompletedOperation(cachedTable as TTable, "");
            }
            else
            {
                return default;
            }
        }
    }
}
