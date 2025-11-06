namespace LUIZ.AI.FSM
{
    public interface IFSMState
    {
        public void Update();
        public void OnEnter();
        public void OnExit();
    }
}