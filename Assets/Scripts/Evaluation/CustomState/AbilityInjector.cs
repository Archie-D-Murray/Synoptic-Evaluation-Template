using AI.HSM;
using AI.Injectors;

using UnityEngine;

public interface IAbilityInjector : IStateInjector {

    ///<summary>Returns attack adaptor as ability for player to 
    public HealAdaptor GetAbility(StateMachineContext context);

    ///<summary>Cooldown finished</summary>
    public bool CooldownFinished(StateMachineContext context);

    ///<summary>Restart cooldown timer</summary>
    public void ResetCooldown(StateMachineContext context);

    ///<summary>Start Cooldown Timer</summary>
    public void StartCooldown(StateMachineContext context);

    ///<summary>Get cast range</summary>
    public float CastRange(StateMachineContext context);
}

public class AbilityInjector : MonoBehaviour, IAbilityInjector {
    [SerializeReference, SubclassSelector] private HealAdaptor _ability = new HealAdaptor();
    [SerializeField] private float _range = 2.0f;

    private int _abilityTimerID;

    public float CastRange(StateMachineContext context) {
        return _range;
    }

    public void ContextInit(StateMachineContext context) {
        _abilityTimerID = context.CooldownManager.CreateCooldown(2.0f, "Ability", true);
    }

    public bool CooldownFinished(StateMachineContext context) {
        return false;
    }

    public HealAdaptor GetAbility(StateMachineContext context) {
        return _ability;
    }

    public void ResetCooldown(StateMachineContext context) {
        // Reset cooldown timer using context.CooldownManager.Get(_abilityTimerID)
    }

    public void StartCooldown(StateMachineContext context) {
        // Start cooldown timer using context.CooldownManager.Get(_abilityTimerID)
    }

    // NOTE: Unused

    public void Init() { }

    public void OnEnter(StateMachineContext context) { }

    public void OnExit(StateMachineContext context) { }

    public void OnUpdate(StateMachineContext context, float dt) { }
}