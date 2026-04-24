
using System;

using UnityEngine;

namespace AI.Examples {
    ///<summary>A basic resprentation of health for things like players and enemies</summary>
    public class Health : MonoBehaviour, IDamageable {

        [Header("Health")]
        [SerializeField] private float _maxHealth = 100.0f;
        [SerializeField] private float _curHealth = 0.0f;

        [Space, Header("Behaviour")]
        ///<summary>If true, set current health to max health in Awake</summary>
        [SerializeField] private bool _maxHealthOnAwake = true;

        ///<summary>Current health (clamped to max health)</summary>
        public float CurHealth => _curHealth;
        ///<summary>Maximum Health</summary>
        public float MaxHealth => _maxHealth;

        ///<summary>Is health 0 and non 0 max health</summary>
        public bool IsDead => _curHealth == 0.0f && _maxHealth != 0.0f;

        ///<summary>Percent health in range [[0, 1]] - clamped</summary>
        public float PercentHealth => Mathf.Clamp01(_curHealth / _maxHealth);

        ///<summary>Invoked each time Damage is called and is valid to damage (including kill hit)</summary>
        public Action<DamageSource, DamageResult> OnDamage = delegate { };

        ///<summary>Invoked once when current health reaches 0</summary>
        public Action<DamageSource> OnDeath = delegate { };

        private void Awake() {
            if (_maxHealthOnAwake) {
                _curHealth = _maxHealth;
            }
        }

        ///<summary>Deals damage by removing from current health (clamped)</summary>
        ///<remarks>Will not damage if health is 0 or amount is 0 or below</remarks>
        ///<param name="source">Source of damage</param>
        ///<returns>Data about what damage was applied</returns>
        public DamageResult Damage(DamageSource source) {
            if (_curHealth == 0.0f) {
                return default;
            } else if (source.Amount <= 0) {
                return default;
            }

            float damage = Mathf.Min(_curHealth, source.Amount);

            _curHealth -= damage;

            DamageResult result = new DamageResult(damage, IsDead);

            OnDamage.Invoke(source, result);

            if (IsDead) {
                OnDeath.Invoke(source);
            }

            return result;
        }
    }
}
