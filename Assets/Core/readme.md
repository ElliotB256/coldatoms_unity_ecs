
# The time-integration loop

We use a velocity-verlet time integration method. The simulation loop is as described in the table below:

| System | Description |
|--------|------|
| UpdatePositionSystem | Updates the `Translation` of atoms using their `Velocity` and the previous frames acceleration (via `PrevForce` and `Mass`). |
| ClearForceSystem | Sets `Force` to `float3(0f,0f,0f)` for each atom. |
| CollisionSystem | Calculates collisions between atoms. |
| ForceCalculationSystems | Simulation group for systems that calculate atomic forces. Modifies the `Force` component on each atom. |
| UpdateVelocitySystem | Updates velocity using the velocity-verlet method, using the mean of `Force` and `PrevForce` from the previous frame. |
| SavePrevForceSystem | Sets `PrevForce` equal to `Force`, thereby saving current frames force for use in the next frame. |


# Collisions

Atoms are sorted into boxes by hashing their positions and saving them to a hashmap.