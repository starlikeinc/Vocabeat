using System.Collections;
using System.Collections.Generic;
using LUIZ;

public class TableLoaderCharacterData : TableDataLoaderCSVBase<TableDataCharacterData>
{
    private readonly Dictionary<uint, TableDataCharacterData> m_dicTableCharacterData = new Dictionary<uint, TableDataCharacterData>();

    //--------------------------------------------------------------
    protected override void OnTableDataCSVLoadComplete()
    {
        base.OnTableDataCSVLoadComplete();

        List<TableDataCharacterData> listCharacterData = ProtTableDataLoadCSV();

        for(int i = 0; i < listCharacterData.Count; i++)
        {
            m_dicTableCharacterData[(uint)listCharacterData[i].Key] = listCharacterData[i];
        }
    }

    //--------------------------------------------------------------
    public TableDataCharacterData GetTableCharacterData(uint characterID)
    {
        TableDataCharacterData characterData = null;
        m_dicTableCharacterData.TryGetValue(characterID, out characterData);

        return characterData;
    }
}