using UnityEngine;

namespace AI.Examples {
    [RequireComponent(typeof(Rigidbody))]
    public class RangedProjectile : MonoBehaviour {
        private Rigidbody _rb;

        [SerializeField] private float _lifeTime = 5.0f;
        [SerializeField] private float _damage = 5.0f;
        [SerializeField] private LayerMask _layer;

        private GameObject _owner;

        private void OnValidate() {
            _rb = GetComponent<Rigidbody>();
        }

        Rigidbody InitRigidbody() {
            if (!_rb) {
                _rb = GetComponent<Rigidbody>();
            }

            return _rb;
        }

        public void Fire(Vector3 direction, float lifeTime, float speed, float damage, GameObject owner = null) {
            Fire(direction, lifeTime, speed, damage, -1, owner);
        }

        public void Fire(Vector3 direction, float lifeTime, float speed, float damage, LayerMask mask, GameObject owner = null) {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            _owner = owner;
            InitRigidbody().linearVelocity = transform.forward * speed;
            _damage = damage;
            _lifeTime = lifeTime;
            _layer = mask;
        }

        private void FixedUpdate() {
            _lifeTime -= Time.fixedDeltaTime;

            if (_lifeTime <= 0) {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider hit) {
            if (hit.gameObject != _owner && hit.TryGetComponent(out IDamageable damageable)) {
                damageable.Damage(new DamageSource(_damage, _owner, hit.gameObject));
                Destroy(gameObject);
            }
        }
    }
}
