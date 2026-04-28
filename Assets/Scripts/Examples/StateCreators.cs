using AI.HSM;

namespace AI.Examples {

    ///<summary>Static creator functions to allow state factory to construct custom state types</summary>
    public static class StateCreators {

        ///<summary>Ranged state creator</summary>
        ///<param name="context">State machine context</param>
        ///<param name="stateMachine">State machine reference</param>
        ///<param name="parent">Parent state</param>
        ///<returns>Created RangedState object</returns>
        public static State CreateRanged(StateMachineContext context, StateMachine stateMachine, State parent) {
            return new RangedState(context, stateMachine, parent);
        }
    }
}