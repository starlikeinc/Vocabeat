using UnityEngine;

public enum ESampleItemType
{
    Guitar,
    Bass,
    Piano
}

[System.Serializable]
public struct SampleItemData
{
    public ESampleItemType Item;
    public int Amount;

    public SampleItemData(ESampleItemType item, int amount)
    {
        this.Item = item;
        this.Amount = amount;
    }
}