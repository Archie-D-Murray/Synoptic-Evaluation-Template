using System;
using System.Collections.Generic;

using AI.Examples;
using AI.Injectors;

namespace AI.HSM {

    ///<summary>Static collection of commonly reused state lists</summary>
    public static class StateDefinitions {
        public static AIState[] BasicStates = new AIState[] { AIState.Root, AIState.Idle, AIState.Wander, AIState.Patrol, AIState.Chase, AIState.Attack };
    }

    ///<summary>Representation of state definitions for a state machine</summary>
    public interface IStateDefinition {

        ///<summary>Gets the collection of required states for a state definition</summary>
        ///<returns>Collection of AIState objects that can be used to create AIStateViews from contents</returns>
        public IEnumerable<AIState> RequiredStates() { return StateDefinitions.BasicStates; }

        ///<summary>Allows for adding extra factory definitions</summary>
        ///<param name="factory">Factory to modify</param>
        public void InitFactory(StateFactory factory) { }

        ///<summary>Allows for assigning references to injectors in context</summary>
        ///<param name="context">Context to initialise injectors for</param>
        ///<remarks>It is recommended to call injector.ContextInit() here</remarks>
        public void InitInjectors(StateMachineContext context);

        ///<summary>Sets up transitions for state machine - make sure to also pass initial states for any parent states</summary>
        ///<param name="context">Context to initialise transitions for</param>
        public void InitTransitions(StateMachineContext context);
    }

    ///<summary>Basic definition for an entity that just idles - useful as testing value only</summary>
    [Serializable]
    public class BasicStateDefintion : IStateDefinition {
        public IEnumerable<AIState> RequiredStates() { return new AIState[] { AIState.Root, AIState.Idle }; }
        public void InitFactory(StateFactory factory) { }
        public void InitInjectors(StateMachineContext context) {
            context.IdleInjector = InjectorManager.Instance.Idle;
        }
        public void InitTransitions(StateMachineContext context) {
            context.StateMachine.AddInitialState(context[AIState.Root], context[AIState.Idle]);
        }
    }

    ///<summary>Default definition for an entity that idles then either wanders or patrols and chases enemies attacking if in range</summary>
    [Serializable]
    public class DefaultStateDefinitions : IStateDefinition {
        public void InitInjectors(StateMachineContext ctx) {
            ctx.IdleInjector = ctx.GetComponentInChildren<IIdleInjector>();
            ctx.WanderInjector = ctx.GetComponentInChildren<IWanderInjector>();
            ctx.PatrolInjector = ctx.GetComponentInChildren<IPatrolInjector>();
            ctx.ChaseInjector = ctx.GetComponentInChildren<IChaseInjector>();
            ctx.AttackInjector = ctx.GetComponentInChildren<IAttackInjector>();
            ctx.IdleInjector.Init();
            ctx.WanderInjector.Init();
            ctx.PatrolInjector.Init();
            ctx.ChaseInjector.Init();
            ctx.AttackInjector.Init();
            ctx.IdleInjector.ContextInit(ctx);
            ctx.WanderInjector.ContextInit(ctx);
            ctx.PatrolInjector.ContextInit(ctx);
            ctx.ChaseInjector.ContextInit(ctx);
            ctx.AttackInjector.ContextInit(ctx);
        }

        public void InitTransitions(StateMachineContext ctx) {
            ctx.StateMachine.AddInitialState(ctx[AIState.Root], ctx[AIState.Idle]);

            StableChangePredicate wanderOrPatrol = new StableChangePredicate(0.5f);

            // Idle
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Wander],
                new AndPredicate(new LambdaPredicate(() => ctx.IdleInjector.DoneIdling(ctx)), wanderOrPatrol));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Patrol],
                new AndPredicate(new LambdaPredicate(() => ctx.IdleInjector.DoneIdling(ctx)), new NotPredicate(wanderOrPatrol)));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Chase],
                new LambdaPredicate(ctx.Detector.HasTarget));

            // Wander
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Wander],
                ctx[AIState.Chase],
                new LambdaPredicate(ctx.Detector.HasTarget));

            // Patrol
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Patrol],
                ctx[AIState.Chase],
                new LambdaPredicate(ctx.Detector.HasTarget));

            // Chase
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Chase],
                ctx[AIState.Attack],
                new LambdaPredicate(() => ctx.ChaseInjector.InAttackRange(ctx, ctx.AttackInjector.AttackRange(ctx))));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Chase],
                ctx[AIState.Idle],
                new LambdaPredicate(() => ctx.ChaseInjector.LostTarget(ctx)));

            // Attack
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Attack],
                ctx[AIState.Chase],
                new LambdaPredicate(() => ctx.AttackInjector.UnableToAttack(ctx)));

        }
    }

    ///<summary>Stationary enemy that attack when in range and otherwise idles</summary>
    [Serializable]
    public class StationaryEnemyDefinition : IStateDefinition {

        public void InitInjectors(StateMachineContext ctx) {
            ctx.IdleInjector = ctx.GetComponentInChildren<IIdleInjector>();
            ctx.AttackInjector = ctx.GetComponentInChildren<IAttackInjector>();
            ctx.IdleInjector.Init();
            ctx.AttackInjector.Init();
            ctx.IdleInjector.ContextInit(ctx);
            ctx.AttackInjector.ContextInit(ctx);
        }

        public void InitTransitions(StateMachineContext ctx) {
            ctx.StateMachine.AddInitialState(ctx[AIState.Root], ctx[AIState.Idle]);
            // Idle
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Attack],
                new LambdaPredicate(() => ctx.Detector.HasTarget()));

            // Attack
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Attack],
                ctx[AIState.Idle],
                new LambdaPredicate(() => ctx.Detector.JustLostTarget));

        }
    }
}