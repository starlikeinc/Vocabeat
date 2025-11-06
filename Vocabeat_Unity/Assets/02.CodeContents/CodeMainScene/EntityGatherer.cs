using LUIZ;
using LUIZ.AI.FSM;
using UnityEngine;

public class EntityGatherer : EntityBase
{
    private GatherableResource m_targetResource = null;

    //--------------------------------------------------------
    protected override void OnEntityInitialize()
    {
        base.OnEntityInitialize();


    }

    //--------------------------------------------------------
    public void SetTargetResource(GatherableResource targetResource)
    {
        m_targetResource = targetResource;
    }
}
