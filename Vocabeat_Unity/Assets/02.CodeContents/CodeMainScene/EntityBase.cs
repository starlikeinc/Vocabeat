using LUIZ;
using LUIZ.AI.FSM;
using UnityEngine;

public class EntityBase : MonoBase
{
    private StateMachine m_stateMachine = null;

    //------------------------------------------------
    protected override void OnUnityAwake()
    {
        m_stateMachine = new StateMachine();

        OnEntityInitialize();
    }

    //------------------------------------------------
    protected void ProtUpdateEntity()
    {
        m_stateMachine.DoUpdateFSM();
    }

    //------------------------------------------------
    protected virtual void OnEntityInitialize() { }
}
