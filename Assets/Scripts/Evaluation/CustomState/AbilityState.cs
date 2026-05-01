using AI.HSM;

using AI.Adapters;

using Utilities;

[System.Serializable]
public class AbilityState : State {

    private float _elapsedAttackTime = 0.0f;
    private bool _attackPending = false;
    private PriorityQueue<AttackAdaptor, float> _queue = new PriorityQueue<AttackAdaptor, float>(1);
    private readonly StateMachineContext _context;

    public static State Create(StateMachineContext context, StateMachine stateMachine, State parent) {
        return new AbilityState(context, stateMachine, parent);
    }

    public AbilityState(StateMachineContext context, StateMachine stateMachine, State parent) : base(stateMachine, parent) {
        _context = context;
    }

    protected override void OnEnter() {
        // Start Cast Animation + propagate OnEnter to injector
    }

    protected override void OnUpdate(float dt) {
        _context.AttackContext.Origin = _context.Position.Offset(y: 1.5f);
        _context.AttackContext.Direction = (_context.Detector.TargetPosition - _context.Position).normalized;

        _context.AbilityInjector.OnUpdate(_context, dt);

        if (_context.AbilityInjector.CooldownFinished(_context)) {
            _context.AbilityInjector.ResetCooldown(_context);
            _context.AbilityInjector.StartCooldown(_context);

            _context.AttackContext.Clip = _context.Animator.GetCurrentClip();
            // Start incrementing _elapsedAttackTime + flag _attackPending + enqueue attack with its normalized time

        }
 
        // Update timer if running
        // drain queue while peek value has higher normalized time than elapsed time / ability cooldown
        // See Attack State for example of this
    }

    protected override void OnExit() {
        // Start Locomotion animation
        // Reset attack time and propagate OnExit to the injector
    }
}