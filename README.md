# Synoptic Project Evaluation

## Player Setup:
The root player has a `PlayerInputs` component along with a `PlayerController` to handle movement with an underlying `Rigidbody` and `CapsuleCollider` providing a valid physics settings. It will also have a `Health` component allowing the player to be attacked by enemies. This is available as a prefab in the Assets/Prefabs folder for easy setup.

The hierarchy of the player is expected to look something like:
```
Root (PlayerController, PlayerInputs, Rigidbody, CapsuleCollider, Health)
 |
 |- Model Root (AnimationController, Animator)
```

![Player Setup](./Share/Player_Setup.png)

## Enemy Setup
Enemies are a little more complicated with a few extra components providing adaptors allowing functionality to be overriden. They have a `StateMachineContext`
