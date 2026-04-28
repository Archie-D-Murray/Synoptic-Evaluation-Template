using UnityEngine;

namespace AI.Examples {
    ///<summary>Class to handle applying projectile damage to IDamageable objects and move itself</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class RangedProjectile : MonoBehaviour {
        ///<summary>Rigidbody to use to fire when spawned</summary>
        private Rigidbody _rb;

        ///<summary>Lifetime before destroying self</summary>
        [SerializeField] private float _lifeTime = 5.0f;

        ///<summary>Damage to apply</summary>
        [SerializeField] private float _damage = 5.0f;

        ///<summary>Valid layers to hit objects on</summary>
        [SerializeField] private LayerMask _layer;

        ///<summary>Who spawned the projectile</summary>
        private GameObject _owner;

        private void OnValidate() {
            _rb = GetComponent<Rigidbody>();
        }

        ///<summary>Gets and caches Rigidbody reference</summary>
        private Rigidbody InitRigidbody() {
            if (!_rb) {
                _rb = GetComponent<Rigidbody>();
            }

            return _rb;
        }

        ///<summary>Fires projectile configuring all field parameters</summary>
        ///<param name="direction">Direction to set initial velocity</param>
        ///<param name="lifeTime">Projectile lifetime</param>
        ///<param name="speed">Projectile speed (not multiplied by deltaTime or fixedDeltaTime)</param>
        ///<param name="owner">Projectile spawner</param>
        public void Fire(Vector3 direction, float lifeTime, float speed, float damage, GameObject owner = null) {
            Fire(direction, lifeTime, speed, damage, Physics.AllLayers, owner);
        }

        ///<summary>Fires projectile configuring all field parameters</summary>
        ///<param name="direction">Direction to set initial velocity</param>
        ///<param name="lifeTime">Projectile lifetime</param>
        ///<param name="speed">Projectile speed (not multiplied by deltaTime or fixedDeltaTime)</param>
        ///<param name="mask">Valid layers for colliding</param>
        ///<param name="owner">Projectile spawner</param>
        public void Fire(Vector3 direction, float lifeTime, float speed, float damage, LayerMask mask, GameObject owner = null) {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            _owner = owner;
            InitRigidbody().linearVelocity = transform.forward * speed;
            _damage = damage;
            _lifeTime = lifeTime;
            _layer = mask;

            Destroy(gameObject, _lifeTime);
        }

        ///<summary>On trigger collides with object before checking to it being on the specified layer</summary>
        ///<param name="collision">Collision data</param>
        private void OnCollisionEnter(Collision collision) {
            Collider hit = collision.collider;
            if (hit.gameObject != _owner && (_layer & (1 << hit.gameObject.layer)) != 0 && hit.TryGetComponent(out IDamageable damageable)) {
                damageable.Damage(new DamageSource(_damage, _owner, hit.gameObject));
                Destroy(gameObject);
            }
        }
    }
}