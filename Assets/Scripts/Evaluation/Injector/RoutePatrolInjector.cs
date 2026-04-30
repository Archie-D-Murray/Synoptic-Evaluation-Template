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

    private static int _timePerPointID = AICooldownManager.GetHash("TimePerPoint");

    public bool AtPatrolPoint(StateMachineContext context, Vector3 position, int index) {
        return Vector3.Distance(position, GetPatrolTarget(context, index)) <= _targetDistance;
    }

    public void ContextInit(StateMachineContext context) { }

    public bool FinishedPatrolPoint(StateMachineContext context, int index) {
        // Return if patrol timer is finished using context.CooldownManager.Get(_timePerPointID)
        return true;
    }

    public Vector3 GetPatrolTarget(StateMachineContext context, int index) {
        // Use _contextToRoute to get destination using index for Route.Points
        return context.Position; // Replace this
    }

    public int GetStartIndex(StateMachineContext context, int index) {
        // Find route index with lowest CurrentCount

        return 0;
    }

    public void Init() {
        int i = 0;
        _routes = new Route[_roots.Length];
        foreach (Transform root in _roots) {
            _routes[i] = new Route(root);
            i++;
        }
    }

    public int Next(StateMachineContext context, int index) {
        // Return next index, using _contextToRoute to provide _routes index
        return 0;
    }

    public void OnEnter(StateMachineContext context) {
        // Resume patrol timer using context.CooldownManager.Get(_timePerPointID)
    }

    public void OnExit(StateMachineContext context) {
        // Reduce patrol count
        // Remove from patrol
    }

    public void OnPatrolPointFinish(StateMachineContext context) {
        // Start cooldown timer using context.CooldownManager.Get(_timePerPointID)
    }

    public void OnUpdate(StateMachineContext context, float dt) { }

    public int Prev(StateMachineContext context, int index) {
        return 0;
    }

    public void TickPatrolPoint(StateMachineContext context, float dt) {
        // Update Patrol Timer using context.CooldownManager.Get(_timePerPointID)
    }
}