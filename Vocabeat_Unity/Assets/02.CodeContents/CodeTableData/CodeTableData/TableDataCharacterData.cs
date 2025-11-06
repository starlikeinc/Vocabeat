using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableDataCharacterData
{
    public enum RarityType
    {
        Epic,
        Rare,
        Normal
    }

    public int Key;

    public string CharacterName;
    public string CharacterSecondName;

    public RarityType CharacterRarity;

    public string StandingImgName;
    public string PortraitImgName;
}
