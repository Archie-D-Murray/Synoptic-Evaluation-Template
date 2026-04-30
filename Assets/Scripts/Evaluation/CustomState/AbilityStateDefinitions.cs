using System.Collections.Generic;

using AI;
using AI.Examples;
using AI.HSM;

public class AbilityStateDefinition : IStateDefinition {

    ///<summary>Adds ability state to factory definitions</summary>
    ///<param name="factory">Factory to add definition to</param>
    public void InitFactory(StateFactory factory) {
        factory.AddStateDefinition(new StateFactoryDefinition(AIState.Ability, AbilityState.Create));
    }

    ///<summary>All required states for Flag Bearer</summary>
    public IEnumerable<AIState> RequiredStates() {
        return new AIState[] { AIState.Root, AIState.Idle, AIState.Wander, AIState.Patrol, AIState.Chase, AIState.Ability };
    }

    ///<summary>Initialise all injectors with this context</summary>
    ///<param name="ctx">Context to initialise</param>
    public void InitInjectors(StateMachineContext ctx) {
        // Set filter to only apply to allies that have taken damage
        ctx.Detector.SetFilter((obj) => {
            if (obj.TryGetComponent(out Health health)) {
                return health.CurHealth < health.MaxHealth;
            }

            return false;
        });

        ctx.IdleInjector.ContextInit(ctx);
        ctx.WanderInjector.ContextInit(ctx);
        ctx.PatrolInjector.ContextInit(ctx);
        ctx.ChaseInjector.ContextInit(ctx);

        // Ability Injector Context Init Here
    }

    ///<summary>Initialise all transitions for the context</summary>
    ///<param name="ctx">Context to initialise</param>
    public void InitTransitions(StateMachineContext ctx) {
        // Init default state
        // Init transitions - ability state is similar to an attack state, detector just targets allies
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
        // Needs to be adapted to move to ability if target is valid and in cast range

        ctx.StateMachine.AddStateTransition(
            ctx[AIState.Chase],
            ctx[AIState.Idle],
            new LambdaPredicate(() => ctx.ChaseInjector.LostTarget(ctx)));

        // Ability
        // Needs a transition back to chase if no longer in range to cast but still has target
    }
}