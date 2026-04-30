using AI.HSM;

using AI.Adapters;

using Utilities;

[System.Serializable]
public class AbilityState : State {

    private float _timer = 0.0f;
    private bool _attackPending = false;
    private PriorityQueue<AttackAdaptor, float> _queue;
    private readonly StateMachineContext _context;

    public static State Create(StateMachineContext context, StateMachine stateMachine, State parent) {
        return new AbilityState(context, stateMachine, parent);
    }

    public AbilityState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
        _context = context;
    }

    protected override void OnEnter() {
        // Start Ability Injector Timer
        // Start Cast Animation
    }

    protected override void OnUpdate(float dt) {

        _context.AbilityInjector.OnUpdate(_context, dt);

        if (_context.AbilityInjector.CooldownFinished(_context)) {
            _context.AbilityInjector.ResetCooldown(_context);
            _context.AbilityInjector.StartCooldown(_context);

            // Start timer + enqueue attack with its normalized time
        }

        // Update timer if running
        // drain queue while peek value has higher normalized time than elapsed time / ability cooldown
        // See Attack State for example of this
    }

    protected override void OnExit() {
        // Start Locomotion animation
    }
}