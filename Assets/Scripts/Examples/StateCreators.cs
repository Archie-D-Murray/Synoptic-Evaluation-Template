using AI.HSM;

namespace AI.Examples {
    public static class StateCreators {
        public static State CreateRanged(StateMachineContext context, StateMachine stateMachine, State parent) {
            return new RangedState(context, stateMachine, parent);
        }
    }
}
