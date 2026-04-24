using System.Collections.Generic;

using AI.Adapters;
using AI.HSM;

using UnityEngine;

namespace AI.Injectors {
    public interface IStateInjector {

        ///<summary>Used for first time initialisation</summary>
        public void Init();

        ///<summary>Used for first initialisation per object using the injector</summary>
        ///<param name="context">Entity context</param>
        public void ContextInit(StateMachineContext context);

        ///<summary>OnEnter call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnEnter(StateMachineContext context);

        ///<summary>OnUpdate call propagated from state</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time since last state machine update</param>
        public void OnUpdate(StateMachineContext context, float dt);

        ///<summary>OnExit call propagated from state</summary>
        ///<param name="context">Entity context</param>
        public void OnExit(StateMachineContext context);
    }

    public interface IIdleInjector : IStateInjector {

        ///<summary>Signals if entity is finished idling</summary>
        ///<param name="context">Entity context</param>
        ///<returns>True if finished idling</returns>
        public bool DoneIdling(StateMachineContext context);
    }

    public interface IWanderInjector : IStateInjector {

        ///<summary>Gets a random wander point within specified ranges</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Target destination with y = 0</returns>
        public Vector3 GetWanderPoint(StateMachineContext context);

        ///<summary>Returns if entity needs a new wander point as it has finished idling at it</summary>
        ///<param name="context">Entity context</param>
        ///<returns>True if new wander point is needed</returns>
        public bool NextWanderPoint(StateMachineContext context);
    }

    public interface IPatrolInjector : IStateInjector {

        ///<summary>Get starting patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>New patrol index</returns>
        public int GetStartIndex(StateMachineContext context, int index);

        ///<summary>Get target patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Target patrol destination</returns>
        public Vector3 GetPatrolTarget(StateMachineContext context, int index);

        ///<summary>Get next patrol index wrapped to patrol points length</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Next patrol index</returns>
        public int Next(StateMachineContext context, int index);

        ///<summary>Get previous patrol index wrapped to patrol points length</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Previous patrol index</returns>
        public int Prev(StateMachineContext context, int index);

        ///<summary>Called upon finishing idling at patrol point</summary>
        ///<param name="context">Entity context</param>
        public void OnPatrolPointFinish(StateMachineContext context);

        ///<summary>Update wait timer when close enough to patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="dt">Time to tick timer by</param>
        public void TickPatrolPoint(StateMachineContext context, float dt);

        ///<summary>Used to find if entity needs to advance to new patrol point</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>Has finished idling at patrol point</returns>
        public bool FinishedPatrolPoint(StateMachineContext context, int index);

        ///<summary>Is close enough to patrol point to begin idling</summary>
        ///<param name="context">Entity context</param>
        ///<param name="index">Patrol index</param>
        ///<returns>True if close enough to patrol point</returns>
        public bool AtPatrolPoint(StateMachineContext context, Vector3 position, int index);
    }

    public interface IChaseInjector : IStateInjector {

        ///<summary>Completely lost target and needs to give up chasing</summary>
        ///<param name="context">Entity context</param>
        ///<returns>No valid target and did not find new target within time limit</returns>
        public bool LostTarget(StateMachineContext context);

        ///<summary>Starts window to resume chase upon finding new target</summary>
        ///<param name="context">Entity context</param>
        public void StartLostTimer(StateMachineContext context);

        ///<summary>Whether entity is in range to start attacking</summary>
        ///<param name="context">Entity context</param>
        ///<param name="attackRange">Attack range</param>
        ///<returns>Target is within attack range specified</returns>
        public bool InAttackRange(StateMachineContext context, float attackRange);
    }

    public interface IAttackInjector : IStateInjector {

        ///<summary>Is attack timer finished</summary>
        ///<param name="context">Entity context</param>
        ///<returns>IsFinished value of attack timer cooldown</returns>
        public bool CanAttack(StateMachineContext context);

        ///<summary>Is the target too far to attack or no target found</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Signal to transition back to different state</returns>
        public bool UnableToAttack(StateMachineContext context);

        ///<summary>Gets attack time of entity associated with context</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Attack Time of entity associated with context</returns>
        public float AttackTime(StateMachineContext context);

        ///<summary>Gets attack range of entity associated with context</summary>
        ///<param name="context">Entity context</param>
        ///<returns>Attack range of entity associated with context</returns>
        public float AttackRange(StateMachineContext context);

        ///<summary>Gets attacks for entities using injector</summary>
        ///<param name="context">Entity context</param>
        ///<returns>List of attack adapters to be queued into attack state</returns>
        public List<AttackAdaptor> GetAttacks(StateMachineContext context);

        ///<summary>Restarts attack cooldown as attack has occurred</summary>
        ///<param name="context">Entity context</param>
        public void RestartAttackCooldown(StateMachineContext context);
    }
}