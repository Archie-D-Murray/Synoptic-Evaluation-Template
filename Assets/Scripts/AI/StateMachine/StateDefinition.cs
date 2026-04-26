using System;
using System.Collections.Generic;

using AI.Injectors;

namespace AI.HSM {
    public static class StateDefinitions {
        public static AIState[] BasicStates = new AIState[] { AIState.Root, AIState.Idle, AIState.Wander, AIState.Patrol, AIState.Chase, AIState.Attack };
    }

    public interface IStateDefinition {
        public IEnumerable<AIState> RequiredStates() { return StateDefinitions.BasicStates; }
        public void InitFactory(StateFactory factory) { }
        public void InitInjectors(StateMachineContext context);
        public void InitTransitions(StateMachineContext context);
    }

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
            // Idle
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Wander],
                new AndPredicate(new LambdaPredicate(() => ctx.IdleInjector.DoneIdling(ctx)), new RandomChancePredicate(0.5f)));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Idle],
                ctx[AIState.Patrol],
                new AndPredicate(new LambdaPredicate(() => ctx.IdleInjector.DoneIdling(ctx)), new RandomChancePredicate(0.5f)));

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