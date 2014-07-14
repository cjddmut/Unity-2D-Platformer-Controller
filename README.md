# UnityPlayerController2D
=======================

A customizable player controller asset that can be dropped into any scene for immediate 2D motion support.

## Overview of Public Properties of Motor

### Movement ###

**Ground Acceleration** - The acceleration of the Motor in Unity units when considered on the ground. This value is applied by the movement dir set by the Controller.

**Air Acceleration** - The acceleration of the Motor when considered in the air in Unity units.

**Max Ground Speed** - Clamped maximum ground speed.

**Max Air Speed** - Clamped maximum air speed, only considers x axis.

**Max Fall Speed** - Clamped fall speed (only y axis when negative).

### Jumping ###

**Base Jump** - The height, in Unity units, that the Motor will jump to.

**Extra Jump Height** - If the Motor is informed that the jump button is pressed down then this is the additional height the character will jump.

**Allow Double Jump** - If the Motor should allow double jump. A double jump is considered when the Motor is in the air and a double jump has not occurred. 

**Allow Wall Jump** - If jumping off the wall is allowed.

### Wall Slide ###

**Allow Wall Slide** - If the Motor should consider any wall sliding calculations. Wall sliding is when the character would slow down while 'sliding' down the wall.

**Wall Slide Speed** - The speed that the character will slide down the wall.

### Corner Grabs ###

**Allow Corner Grab** - If corner grabbing is allowed. 

**Corner Height/Width Check** - A corner is considered grabbed if the upper corners of the collider do not intersect with the environment but the sides do. These values change the consideration for box checks dimensions.

### Dashing ###

**Can Dash** - Is dashing allowed?

**Dash Cooldown** - How long, in seconds, before the Motor will allow dash again?

**Dash Speed** - The speed of the dash.

**Dash Duration** - The duration of the dash.

**Change Layer During Dash** - If the layer of the Motor should change when dashing. This is useful if the character wants to ignore certain colliders when dashing.

**Dash Layer** - If the layer changes then this is the layer that the Motor will change the Game Object to.

### Other ###

**Check Distance** - The distance out of the character's collider that the motor will check to determine if the character is grounded or on a wall.

**Check Mask** - Layer mask that the Motor checks against for ground/wall checks. **This needs to be set**.


## Overview of Public Properties of Controller

Most likely this file will need to be modified to suit the needs of the game.

**Can Control** - If input should be given to the Motor. If false then the character will not respond to input (will react to gravity and other forces).