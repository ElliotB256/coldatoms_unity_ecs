# ECS MD

Molecular dynamics in unity using the ECS framework and a velocity-verlet integrator.

In future, this will replace the evap simulations using the GameObject framework.

Unity Version: 2018.b2 or later

# Notes with the Unity Editor

* Make sure your package manager has correctly loaded the packages required.
* If you want extreme performance, make sure Burst compilation is enabled.
* For fast reloading when entering play mode, turn off domain reload when entering playmode (Editor>Project Settings>Editor>Enter Play Mode Settings).

## Common Problems

* If you receive errors about Burst compilation failing, make sure you have upgraded Burst to the newest version in the package manager. This may require that you fold out the 'all versions' tab and manually select the most recent package - the auto update doesn't always seem to work.