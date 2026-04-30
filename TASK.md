# Custom Injector

## Summary
Make a custom injector that handles distributing entities between multiple different patrol routes when they enter the patrol state. This will likely need to cache all currently patrolling entities.

## Setup
In the scene in `Assets/Scenes/Evaluation Tasks/Custom Injector.unity` has been setup for this with two example patrol routes defined and a new injector manager that does not have a patrol injector. This will need to be added once complete - feel free to edit the terrain to make routes more interesting, equally if there is somewhat challenging terrain for entities to navigate, they may need to be moved to the NavMeshAgent movement adaptor as the Rigidbody version only has a very basic obstacle avoidance implementation

Some template code has been provided in `[Route Patrol Injector](./Assets/Scripts/Evaluation/Injector/RoutePatrolInjector.cs)` You will need to fill in the methods to complete the task. The injector manager has already been configured with the new injector so all that is needed is for the methods to be filled out

# Custom State

## Summary
Add a new state for a specifically marked 'Flag Bearer' enemy that will not attack and instead provides an area wide heal to nearby entities. The state will represent the activation and cast of the ability, while no visuals are present, an animation has been provided for this along with the relevant AnimationType. This state should be transitioned to when the cooldown for the ability is finished and there are entities nearby

## Setup
There is a starter scene in `Assets/Scenes/Evaluation Tasks/Custom State.unity` with a flag bearer enemy already set up. A new state definition will be required along with some way of implementing the ability, any ability parameters like the speed boost or damage boost may be defined in the state machine but ideally would be part of a custom ability injector that handles activating the ability.

There is skeleton code for state definitions that will require state injector initialisation logic, and transitions for the new ability state. This has also been provided for the injector, ability state and the injector manager has been modified with logic for this, you can find the files you will need to modify in the `Assets/Scripts/Evaluation/CustomState/` folder

The detector filter on the Flag Bearer is set to look for allies with less than max health, they others have had their health set to 50/100 to ensure that they will receive healing, the Flag Bearer should stop healing them once they are full health.
