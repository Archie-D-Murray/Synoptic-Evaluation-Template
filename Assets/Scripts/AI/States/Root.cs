namespace AI.HSM {

    ///<summary>Root state is needed to act as base</summary>
    public class RootState : State {
        public RootState(StateMachine stateMachine, State parent) : base(stateMachine, parent) { }
    }
}