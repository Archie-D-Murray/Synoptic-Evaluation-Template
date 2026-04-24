using UnityEngine;

using AI.Adapters;

namespace AI.Examples {

    [System.Serializable]
    public class PhysicsCastAdaptor : AttackAdaptor {
        [SerializeField] private float _range;
        [SerializeField] private LayerMask _layer;
        [SerializeField] private float _damage = 5.0f;

        public PhysicsCastAdaptor() {
            _normalizedTime = 0.5f;
            _range = 5.0f;
            _layer = -1;
        }

        public PhysicsCastAdaptor(float normalizedTime, float range, LayerMask layer) {
            _normalizedTime = normalizedTime;
            _range = range;
            _layer = layer;
        }

        public override void OnEvent(AttackContext context) {
            foreach (Collider hit in Physics.OverlapSphere(context.Origin, _range, _layer)) {
                if (hit.transform == context.Entity.transform) { continue; }
                if (hit.TryGetComponent(out IDamageable damageable)) {
                    damageable.Damage(new DamageSource(_damage, context.Entity.gameObject, hit.gameObject));
                }
            }
        }
    }
}
