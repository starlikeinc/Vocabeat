using LUIZ;

public class ManagerTableData : ManagerTableDataBase
{
    public new static ManagerTableData Instance => ManagerTableDataBase.Instance as ManagerTableData;

    //----------------------------------------------------------------------------
    public TableLoaderBuffData              BuffData { get; private set; } = null;
    public TableLoaderCharacterData         CharacterData { get; private set; } = null;

    //----------------------------------------------------------------------------

    //----------------------------------------------------------------------------
    protected override void OnMgrTableDataInit(IDataLoader tableLoader)
    {
        switch (tableLoader)
        {
            case TableLoaderBuffData                buffData                : BuffData      = buffData;         break;
            case TableLoaderCharacterData           characterData           : CharacterData = characterData;    break;
        }

        //받아온 TableDataBase를 캐스팅 한 후 캐싱 해둔다.
    }
}
