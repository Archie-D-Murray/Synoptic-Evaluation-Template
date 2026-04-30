using AI.Adapters;

using UnityEngine;

[System.Serializable]
public class HealAdaptor : AttackAdaptor {

    [SerializeField] private float _healAmount = 5.0f;
    [SerializeField] private float _range = 5.0f;
    [SerializeField] private LayerMask _mask;

    public override void OnEvent(AttackContext context) {
        foreach (Collider collider in Physics.OverlapSphere(context.Origin, _range, _mask, QueryTriggerInteraction.Ignore)) {
            // Check if collider has StateMachineContext and Health component
        }
    }
}