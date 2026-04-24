using System;

using UnityEngine;

namespace AI.Examples {

    ///<summary>Respresentation of damage instance - extend as needed</summary>
    [Serializable]
    public class DamageSource {
        public float Amount;
        public GameObject Dealer;
        public GameObject Target;

        public DamageSource(float amount, GameObject dealer, GameObject target) {
            Amount = amount;
            Dealer = dealer;
            Target = target;
        }
    }

    ///<summary>Representation of result of application of damage instance - extend as needed</summary>
    [Serializable]
    public class DamageResult {
        public float Dealt;
        public bool Killed;

        public DamageResult() {
            Dealt = 0.0f;
            Killed = false;
        }

        public DamageResult(float dealt, bool killed) {
            Dealt = dealt;
            Killed = killed;
        }
    }

    ///<summary>Interface for dealing damage to anything</summary>
    ///<summary>
    ///<c>Examples could include:</c>
    ///</summary>
    ///<summary>
    ///<example>
    ///Breakable boxes, Players, Enemies, Bosses
    ///</example>
    ///</summary>
    public interface IDamageable {
        ///<summary>General damage interface</summary>
        ///<param name="source">Data about damage being dealt. Ex: amount, dealer, target, etc</param>
        ///<returns>Data about damage after application - did it kill the target, how much was dealt, etc</returns>
        public DamageResult Damage(DamageSource source);
    }
}
