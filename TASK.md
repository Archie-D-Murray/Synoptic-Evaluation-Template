# Custom Injector

## Summary
Make a custom injector that handles distributing entities between multiple different patrol routes when they enter the patrol state. This will likely need to cache all currently patrolling entities.

## Setup
In the scene in `Assets/Scenes/Evaluation Tasks/Custom Injector.unity` has been setup for this with two example patrol routes defined and a new injector manager that does not have a patrol injector. This will need to be added once complete - feel free to edit the terrain to make routes more interesting, equally if there is somewhat challenging terrain for entities to navigate, they may need to be moved to the NavMeshAgent movement adaptor as the Rigidbody version only has a very basic obstacle avoidance implementation

# Custom State

## Summary
Add a new state for a specifically marked 'Flag Bearer' enemy that will not attack and instead provides an area wide temporary buff to nearby entities increasing their damage and speed. The state will represent the activation and cast of the ability, while no visuals are present, an animation has been provided for this along with the relevant AnimationType. This state should be transitioned to when the cooldown for the ability is finished and the ability duration is has less than 25% of its duration remaining. 

## Setup
There is a starter scene in `Assets/Scenes/Evaluation Tasks/Custom State.unity` with a flag bearer enemy already set up. A new state definition will be required along with some way of implementing the ability, any ability parameters like the speed boost or damage boost may be defined in the state machine but ideally would be part of a custom ability injector that handles activating the ability.
