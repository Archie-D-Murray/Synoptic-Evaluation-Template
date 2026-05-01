using AI.Injectors;
using AI.HSM;
using System.Collections.Generic;
using UnityEngine;
using AI;

[System.Serializable]
public class Route {

    ///<summary> Array of all patrol points</summary>
    public Vector3[] Points = new Vector3[3];

    ///<summary>Current number of entities patrolling this route</summary>
    public int CurrentCount = 0;

    ///<summary>Constructs route with all children as patrol points</summary>
    public Route(Transform root) {
        Points = new Vector3[root.childCount];

        for (int i = 0; i < root.childCount; i++) {
            Points[i] = root.GetChild(i).position;
        }

        CurrentCount = 0;
    }
}

///<summary>Patrol injector allowing for multiple patrol routes adding enemy to lowest patrol count route</summary>
public class RoutePatrolInjector : MonoBehaviour, IPatrolInjector {

    ///<summary>Provides index into _routes for which Route a Context is on</summary>
    private Dictionary<StateMachineContext, int> _contextToRoute = new Dictionary<StateMachineContext, int>();

    ///<summary>Patrol routes</summary>
    [SerializeField] private Route[] _routes;

    ///<summary>Min distance to be considered at patrol point</summary>
    [SerializeField] private float _targetDistance = 0.5f;

    ///<summary>Common parent of all patrol points for a route</summary>
    [SerializeField] private Transform[] _roots;

    ///<summary>Patrol timer ID</summary>
    private int _timePerPointID = AICooldownManager.GetHash("TimePerPoint");

    ///<summary>Is close enough to patrol point to begin idling</summary>
    ///<param name="context">Entity context</param>
    ///<param name="index">Patrol index</param>
    ///<returns>True if close enough to patrol point</returns>
    public bool AtPatrolPoint(StateMachineContext context, Vector3 position, int index) {
        return Vector3.Distance(position, GetPatrolTarget(context, index)) <= _targetDistance;
    }

    ///<summary>Ensures TimePerPoint timer exists with time</summary>
    ///<param name="context">Entity context</param>
    public void ContextInit(StateMachineContext context) { 
        _timePerPointID = context.CooldownManager.CreateCooldown(2.0f, "TimePerPoint", false);
    }

    ///<summary>Used to find if entity needs to advance to new patrol point</summary>
    ///<param name="context">Entity context</param>
    ///<param name="index">Patrol index</param>
    ///<returns>Has finished idling at patrol point</returns>
    public bool FinishedPatrolPoint(StateMachineContext context, int index) {
        // Return if patrol timer is finished using context.CooldownManager.Get(_timePerPointID)
        return true;
    }

    ///<summary>Get target patrol point</summary>
    ///<param name="context">Entity context</param>
    ///<param name="index">Patrol index</param>
    ///<returns>Target patrol destination</returns>
    public Vector3 GetPatrolTarget(StateMachineContext context, int index) {
        // Use _contextToRoute to get destination using index for Route.Points
        return context.Position;
    }

    ///<summary>Get starting patrol point for current route</summary>
    ///<param name="context">Entity context</param>
    ///<param name="index">Patrol index</param>
    ///<returns>New patrol index</returns>
    public int GetStartIndex(StateMachineContext context, int index) {
        // Find route index with lowest CurrentCount and add to route, incrementing Route.CurrentCount also add context to map, then start on first index of patrol route

        return 0; // Does not need to be changed, logic is needed above
    }

    public void Init() {
        int i = 0;
        _routes = new Route[_roots.Length];
        foreach (Transform root in _roots) {
            _routes[i] = new Route(root);
            i++;
        }
    }

    ///<summary>Get next patrol index wrapped to patrol points length for route</summary>
    ///<param name="context">Entity context</param>
    ///<param name="index">Patrol index</param>
    ///<returns>Next patrol index</returns>
    public int Next(StateMachineContext context, int index) {
        // Return next index, using _contextToRoute to provide _routes length
        return 0;
    }

    ///<summary>OnEnter call propagated from state</summary>
    ///<param name="context">Entity context</param>
    public void OnEnter(StateMachineContext context) {
        // Resume patrol timer using context.CooldownManager.Get(_timePerPointID)
    }

    ///<summary>OnExit call propagated from state</summary>
    ///<param name="context">Entity context</param>
    public void OnExit(StateMachineContext context) {
        // Reduce patrol count
        // Remove from patrol
    }

    ///<summary>Called upon finishing idling at patrol point</summary>
    ///<param name="context">Entity context</param>
    public void OnPatrolPointFinish(StateMachineContext context) {
        // Start cooldown timer using context.CooldownManager.Get(_timePerPointID)
    }

    ///<summary>OnUpdate call propagated from state</summary>
    ///<param name="context">Entity context</param>
    ///<param name="dt">Time since last state machine update</param>
    public void OnUpdate(StateMachineContext context, float dt) { }

    ///<summary>Get previous patrol index wrapped to patrol points length for route</summary>
    ///<param name="context">Entity context</param>
    ///<param name="index">Patrol index</param>
    ///<returns>Previous patrol index</returns>
    public int Prev(StateMachineContext context, int index) {
        return 0;
    }

    ///<summary>Update wait timer when close enough to patrol point</summary>
    ///<param name="context">Entity context</param>
    ///<param name="dt">Time to tick timer by</param>
    public void TickPatrolPoint(StateMachineContext context, float dt) {
        // Update Patrol Timer using context.CooldownManager.Get(_timePerPointID)
    }
}