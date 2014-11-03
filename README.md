# Unity 2D PlayerController
=======================

<!---%=description%-->

A customizable player controller asset that can be dropped into any scene for immediate 2D motion support.

<!---%=obtain%-->

####Obtain!####
[Releases](https://github.com/cjddmut/Unity-2D-Player-Controller/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/cjddmut/Unity-2D-Player-Controller/archive/develop.zip) and copy the files in Assets to your project's Assets folder.

<!---%=docrest%-->

## Setup

Simply drag either the PlatformContoller2D or the TopdownController2D script on an object for immediate control support. 

## Overview of Public Properties of Motor

### General ###

**Always On Ground** - Should the motor always consider the player on the ground? Turn this on for top down motor control. Note, don't tell the motor to jump if this is true.

**Environment Check Mask** - Note: **This has to be set if 'Always On Ground' is false.** This tells the motor what layer collisions to consider the environment (to determine if on the ground, wall, or corner).

**Environment Check Distance** - This is how far out the motor will check for the environment.  

### Movement ###

**Acceleration** - If the motor will accelerate the game object or immediately set it to the maximum speed.

**Ground Acceleration** - The acceleration of the Motor in Unity units when considered on the ground. This value is applied by the movement dir set by the Controller.

**Air Acceleration** - The acceleration of the Motor when considered in the air in Unity units.

**Max Ground Speed** - Clamped maximum ground speed.

**Max Air Speed** - Clamped maximum air speed, only considers x axis.

**Max Fall Speed** - Clamped fall speed (only y axis when negative).

**Max Fast Fall Speed** - Clamped fall speed when falling fast.

**Horizontal Air Drag** - The drag on the x axis while in the air. Only applies if there isn't movement supplied to the motor is zero.

### Jumping ###

**Base Jump** - The height, in Unity units, that the Motor will jump to.

**Extra Jump Height** - If the Motor is informed that the jump button is pressed down then this is the additional height the character will jump.

**Allow Double Jump** - If the Motor should allow double jump. A double jump is considered when the Motor is in the air and a double jump has not occurred. 

**Allow Wall Jump** - If jumping off the wall is allowed.

**Wall Jump Multiplier** - The base jump speed is calculated from Base Jump and Extra Jump Height. The multiplier multiplies the result. Leave at 1 for no change.

### Wall Cling ###

**Allow Wall Cling** - If the motor should cling to the walls (sticking in place).

**Wall Cling Duration** - The time, in seconds, that the motor will stick to walls.

### Wall Slide ###

**Allow Wall Slide** - If the Motor should consider any wall sliding calculations. Wall sliding is when the character would slow down while 'sliding' down the wall.

**Wall Slide Speed** - The speed that the character will slide down the wall.

### Corner Grabs ###

**Allow Corner Grab** - If corner grabbing is allowed. 

**Corner Grab Duration** - The time, in seconds, that the motor will stick to corners.

**Corner Jump Multiplier** - The multiplier on a corner jump from the calculated speed.

**Corner Distance Check** - A corner is considered grabbed if the upper corners of the collider do not intersect with the environment but the sides do. The value changes the consideration for box checks dimensions.

### Dashing ###

**Allow Dashing** - Is dashing allowed?

**Dash Distance** - The distance covered by the dash.

**Dash Duration** - The duration of the dash.

**Dash Cooldown** - How long, in seconds, before the Motor will allow dash again?

**Change Layer During Dash** - If the layer of the Motor should change when dashing. This is useful if the character wants to ignore certain colliders when dashing.

**Dash Layer** - If the layer changes then this is the layer that the Motor will change the Game Object to.

### Input ###

**Input Threshold** - The input threshold that has to be overcome for the motor to consider motion.

**Heavy Input Theshold** - The input threshold for wall clings, corner grabs, and slides. Could be set to higher to prevent unwanted sticking to walls.

### PlayerMotor2D Members ###

```csharp
Vector2 movementDir
```

The direction that the motor should move the GameObject. The magnitude of this vector should be between 0 and 1 (can be taken directly from Input.GetAxis()) and will be multiplied by the acceleration or ground speed.

```csharp
MotorState motorState // Readonly

enum MotorState
{
    OnGround,
    InAir,
    Sliding,
    OnCorner,
    Clinging,
    Dashing,
    Frozen
}
```

The state that the motor is in.

```csharp
bool facingLeft
```

If the motor is considering itself to be facing left.

```csharp
bool fallFast
```

If the motor shoudl allow the GameObject to move faster. This can be set to true if the player is holding down and false otherwise.

```csharp
bool jumpingHeld
```

If the extra jump height should be used in the jump calculation. Could be set to true if the jump button is held and false otherwise.

```csharp
bool frozen
```

Setting frozen to true will put the motor in a 'frozen' state. All information will be saved and set once unfrozen (the motor also reduce gravity to 0).

Note: This isn't a way to turn off the motor. To turn off the motor, simply set the script to disabled.

```csharp
bool clampVelocity
```

Should the motor clamp the velocity of the GameObject? Set to true by default.

```csharp
bool changeDrag
```

Should the motor change drag of the rigidbody2D. The motor commonly changes the drag depending on the situation, if this conflicts with your own manipulation of rigidbody2D's drag then set this to false.

If this is false then the horizontal air drag is also ignored.

```csharp
Collider2D colliderToUse
```
Set this to use a specific collider for checks instead of grabbing the collider from gameObject.collider2D.

```csharp
delegate void Notification()

Notification onDash
Notification onDashEnd
Notification onJump
```

Attach to these delegates to receive notifications for dash, dash end, and jump events.

### PlayerMotor2D Methods ###

```csharp
void Jump(float extraSpeed = 0)
```

Call this to have the GameObject try to jump, once called it will be handled in the FixedUpdate tick. The y axis is considered jump. The parameter extraSpeed adds additional speed to the jump.

```csharp
void ForceJump(float extraSpeed = 0)
```

This works similar to Jump() except it will jump even if the motor doesn't detect a valid jump scenario. Note, this is ignore while the motor is dashing.

```csharp
void ResetDoubleJump()
```

Resets the motor's double jump state. If the motor has already double jumped and this is called then the motor will allow an additional jump. This method has no use if double jumps aren't allowed.

```csharp
void Dash()
```

Call this to have the GameObject try to dash, once called it will be handled in the FixedUpdate tick. This casues the object to dash along their facing (if left or right for side scrollers).

```csharp
void Dash(Vector2 dir)
```

Send a direction vector to dash allow dashing in a specific direction. 

```csharp
void EndDash()
```

Ends dash early.

## Overview of Public Properties of Controller

Most likely this file will need to be modified to suit the needs of the game.

**Can Control** - If input should be given to the Motor. If false then the character will not respond to input (will react to gravity and other forces).

<!---%title=Unity 2D Player Controller%-->
<!---%download=https://github.com/cjddmut/Unity-2D-Player-Controller/releases/download/v0.2.0/PC2D_v0.2.0b.unitypackage%-->
<!---%github=https://github.com/cjddmut/Unity-2D-Player-Controller%-->
