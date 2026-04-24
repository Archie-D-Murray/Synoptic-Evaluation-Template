# Synoptic Project Evaluation

## State Machine
The core of the state machine handles updating and transitioning states handling proper exit of states before entering the new state resolving the path and resolving the states across the path. States contain basic methods like `OnUpdate()`, `OnEnter()` and `OnExit()` which can be implemented in custom state types, with the state machine calling the methods for it. Data flows in a couple of different ways:
 - Entity specific state data that does not need to accessed via anything other than the state and maybe an injector should stay in the state type
 - State data shared across multiple enemies like attack definitions and wander ranges are part of an injector that provides data to multiple entities
 - Entity specific data that multiple states reference is added to the `StateMachineContext` allowing it to be passed around between states

## State Machine Context
The state machine context is a large context object holding all base references states may need and all the states, transitions and any extra references the states may need. Most states take a reference in the constructor allowing states to do things like move the entity and control its animations. This should be extended with any reference an entity may need no matter the type, while seemingly wasteful, it was elected to go this route where a small amount of unique data will be wasted rather than have to store some inherited type and to a large amount of casting up and down.

## Transitions
Transitions are defined inside a class that provides a set of methods that are called upon initialisation of the state machine. The `IStateDefinition` interface provides methods like `InitInjectors` and `InitTransitions` providing the `StateMachineContext` for the states to be defined for. It also provides a method `InitFactory` for adding new custom states to the state factories and how they are constructed. Transitions are defined using a function that is evaluated to determine if the transition should happen, transitions can either be independent of the current state or specific to the current state. Some examples transition situations have below have been provided:
 - When health drops to 0: Here the state machine just needs to go to a dead state to stop any actions, this is not state specific and so it can be classified as an 'any' state transitions
 - When chasing a target and getting into attack range: Specific to the chase state and so is a state specific transition from the chasing to attacking

## Injectors
The final piece of the puzzle is to add injectors to supply states with relevant data, injectors provide data that would be shared across multiple entities - usually the same type but in some cases like patrol points or wander settings you may be able to share across all! This allows for configuring data in a central way and having the changes update for all entities using those injectors - it is very important to not store state specific data in these as other entities may access the injector. You may either want to store them on the entity for very specific cases or use some kind of manager/singleton to provide a way for entities to gain a reference to their injectors - this can be overriden in the `StateDefinition` implementation in the `InitInjectors` method.

# Template Project Walkthrough

## Player Setup:
The root player has a `PlayerInputs` component along with a `PlayerController` to handle movement with an underlying `Rigidbody` and `CapsuleCollider` providing a valid physics settings. It will also have a `Health` component allowing the player to be attacked by enemies. This is available as a prefab in the Assets/Prefabs folder for easy setup.

The hierarchy of the player is expected to look something like this - `GameObjects` are connected via the lines with the dashes under them respresenting components they have:
```
Root:
 │ - PlayerController
 │ - PlayerInputs
 │ - Rigidbody
 │ - CapsuleCollider
 │ - Health
 │ - AnimationAdapator
 │
 └── Model:
        - Animator
```

![Player Setup](./Share/Player_Setup.png)

## Enemy Setup
Enemies are a little more complicated with a few extra components providing adaptors allowing functionality to be overriden. They have a `StateMachineContext` providing the state machine references and states, this must have it's states field configured with all desired states - make sure to configure these! The state machine also uses a state definition to allow the user to define its transitions conditions and add any custom states to the state factory which is used to build the state tree at runtime.

![State Machine Context](./Share/States_Machine_Context.png)

```
Root:
 │ - StateMachineContext
 │ - RangedDetectorAdaptor    (Detector)
 │ - RigidbodyMovementAdaptor (Movement)
 │ - AnimationAdaptor         (Animation)
 │ - StateMachineContext
 │ - Rigidbody
 │ - CapsuleCollider
 │ - Health
 │
 └── Model:
        - AnimationController
        - Animator
```

## Injector Setup
Injectors all sit on a single `InjectorManager` singleton that provides an easy way to supply entities with injector references, this would likely need to be extracted to provide per enemy type injectors but it simplifies setup for the example massively.

![Injectors](./Share/Injectors.png)
