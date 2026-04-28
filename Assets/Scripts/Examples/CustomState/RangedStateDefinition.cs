using System.Collections.Generic;

using AI.HSM;

namespace AI.Examples {

    ///<summary>Extended ranged definition for an entity that idles then either wanders or patrols and chases enemies attacking if in range or using ranged attacks if not in melee range</summary>
    public class RangedStateDefinition : IStateDefinition {

        public IEnumerable<AIState> RequiredStates() {
            foreach (AIState state in StateDefinitions.BasicStates) {
                yield return state;
            }

            yield return AIState.Ranged;
        }

        public void InitFactory(StateFactory factory) {
            factory.AddStateDefinition(new StateFactoryDefinition(AIState.Ranged, StateCreators.CreateRanged));
        }

        public void InitInjectors(StateMachineContext ctx) {
            ctx.IdleInjector = InjectorManager.Instance.Idle;
            ctx.WanderInjector = InjectorManager.Instance.Wander;
            ctx.PatrolInjector = InjectorManager.Instance.Patrol;
            ctx.ChaseInjector = InjectorManager.Instance.Chase;
            ctx.AttackInjector = InjectorManager.Instance.Attack;
            ctx.RangedInjector = InjectorManager.Instance.Ranged;
            ctx.IdleInjector.ContextInit(ctx);
            ctx.WanderInjector.ContextInit(ctx);
            ctx.PatrolInjector.ContextInit(ctx);
            ctx.ChaseInjector.ContextInit(ctx);
            ctx.AttackInjector.ContextInit(ctx);
            ctx.RangedInjector.ContextInit(ctx);
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

            // Chase
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Chase],
                ctx[AIState.Ranged],
                new LambdaPredicate(() => ctx.ChaseInjector.InAttackRange(ctx, ctx.RangedInjector.AttackRange(ctx))));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Chase],
                ctx[AIState.Idle],
                new LambdaPredicate(() => ctx.ChaseInjector.LostTarget(ctx)));

            // Attack
            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Attack],
                ctx[AIState.Chase],
                new LambdaPredicate(() => ctx.AttackInjector.UnableToAttack(ctx)));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Attack],
                ctx[AIState.Ranged],
                new LambdaPredicate(() =>
                    ctx.ChaseInjector.InAttackRange(ctx, ctx.RangedInjector.AttackRange(ctx)) &&
                    !ctx.ChaseInjector.InAttackRange(ctx, ctx.AttackInjector.AttackRange(ctx))));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Ranged],
                ctx[AIState.Chase],
                new LambdaPredicate(() => ctx.RangedInjector.UnableToAttack(ctx)));

            ctx.StateMachine.AddStateTransition(
                ctx[AIState.Ranged],
                ctx[AIState.Attack],
                new LambdaPredicate(() => ctx.ChaseInjector.InAttackRange(ctx, ctx.AttackInjector.AttackRange(ctx))));

        }
    }
}