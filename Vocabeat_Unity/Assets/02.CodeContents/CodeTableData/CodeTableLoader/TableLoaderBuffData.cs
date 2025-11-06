using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LUIZ;
public class TableLoaderBuffData : TableDataLoaderJsonBase<TableDataBuffData>
{
    private readonly Dictionary<uint, TableDataBuffData.STableItem> m_dicTableBuffData = new Dictionary<uint, TableDataBuffData.STableItem>();

    //--------------------------------------------------------------
    protected override void OnTableDataJsonLoad(TableDataBuffData loadedData)
    {
        for (int i = 0; i < loadedData.BuffData.Count; i++)
        {
            TableDataBuffData.STableItem pItem = loadedData.BuffData[i];
            m_dicTableBuffData[(uint)pItem.Key] = pItem;
        }
    }

    //--------------------------------------------------------------
    public TableDataBuffData.STableItem GetTableBuffData(uint buffID)
    {
        m_dicTableBuffData.TryGetValue(buffID, out var buffData);
        return buffData;
    }
}