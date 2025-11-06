using System.Collections.Generic;

[System.Serializable]
public class TableDataBuffData
{
    public enum BuffIconAttribute
    {
        None = 0,
        Percentage = 1,
    }

    [System.Serializable]
    public class STableItem
    {
        public int Key;
        public string BuffName;
        public string BuffIconName;
        public BuffIconAttribute BuffIconAttribute;
        public string BuffDesc;
        public List<SDescArgument> DescArgument = new List<SDescArgument>();
    }

    [System.Serializable]
    public class SDescArgument
    {
        public int Level;
        public int Value;
    }

    public List<STableItem> BuffData = new List<STableItem>();
}
