using System.Collections.Generic;

using AI.Adapters;
using AI.HSM;
using AI.Injectors;

using UnityEngine;

public interface IAbilityInjector : IStateInjector {

    ///<summary>Returns attack adaptor as ability for player to 
    public List<AttackAdaptor> GetAbilities(StateMachineContext context);

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
    [SerializeReference, SubclassSelector] private List<AttackAdaptor> _abilities = new List<AttackAdaptor>() { new HealAdaptor() };
    [SerializeField] private float _range = 2.0f;
    [SerializeField] private float _cooldown = 2.0f;

    private int _abilityTimerID;

    ///<summary>Get cast range</summary>
    public float CastRange(StateMachineContext context) {
        return _range;
    }

    ///<summary>Ensure ability timer is created</summary>
    public void ContextInit(StateMachineContext context) {
        _abilityTimerID = context.CooldownManager.CreateCooldown(_cooldown, "Ability", true);
    }

    ///<summary>Cooldown finished</summary>
    public bool CooldownFinished(StateMachineContext context) {
        return false;
    }

    ///<summary>Return all attacks - should only contain HealAdaptor</summary>
    public List<AttackAdaptor> GetAbilities(StateMachineContext context) {
        return _abilities;
    }

    ///<summary>Restart cooldown timer</summary>
    public void ResetCooldown(StateMachineContext context) {
        // Reset cooldown timer using context.CooldownManager.Get(_abilityTimerID)
    }

    ///<summary>Start Cooldown Timer</summary>
    public void StartCooldown(StateMachineContext context) {
        // Start cooldown timer using context.CooldownManager.Get(_abilityTimerID)
    }

    // NOTE: Unused

    public void Init() { }

    public void OnEnter(StateMachineContext context) { }

    public void OnExit(StateMachineContext context) { }

    public void OnUpdate(StateMachineContext context, float dt) { }
}