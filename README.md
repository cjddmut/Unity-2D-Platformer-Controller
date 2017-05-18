# Unity 2D Platformer Controller
=======================

<!---%=description%-->

A customizable 2D platformer motor that handles mechanics such as double jumps, wall jumps, and corner grabs. Includes a player controlled prefab that can be dropped into any scene for immediate support.

<!---%=obtain%-->

#### Obtain! ####
[Releases](https://github.com/cjddmut/Unity-2D-Platformer-Controller/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/cjddmut/Unity-2D-Platformer-Controller/archive/develop.zip) and copy the files in Assets to your project's Assets folder.

<!---%=docrest%-->

## NOTE!!!!
Unity 5.3 appears to change the behavior of ray/boxcasts which PC2D relies heavily on. Make sure Environment Check Distance and Minimum Distance From Env need to be increased from what they were in Unity 5.2 (default is 0.08 and 0.04). This isn't a perfect solution but allows it to work within Unity's physics system.

Also I am no longer using the platformer controller in my own personal projects so it's not getting the personal testing it deserves. Please open up an issue if there are any problems.

## Setup

For immediate player support, drop the Basic Player Controller prefab into the scene and set the Static Environment Layer Mask field. For more complicated interaction, interface with PlatformerMotor2D's members and methods.

## Upgrading

When upgrading take a note of your current motor values before upgrading. There is no guarantee made that variables names won't change and if they do then the old values will be lost in the inspector.

## Requirements of PlatformerMotor2D

### BoxCollider2D ###

The motor requires that a Collider2D be present on the GameObject. The motor uses the bounds of the Collider2D to understand where to check for contact with surfaces.

## PlatformerMotor2D Inspector Properties

### General ###

**Static Environment Layer Mask** - This tells the motor what layer collisions to consider the environment (to determine if on the ground, wall, or corner). These are environments that shouldn't be able to move into the player.

**Environment Check Distance** - This is how far out the motor will check for the environment.  

**Minimum Distance From Env** - This is the minimum separation between the motor and surrounding environnment. This is used to prevent catching matching edges of box colliders (or tiles). Half distance of the Environment Check Distance is probably pretty good.

**Number of Iterations** - The number of iterations the motor can take to resolve the final position during a tick. Lower numbers mean more performant but at a cost of potential position loss. 2 iterations will likely cover most cases (unless the time step is really long).

**Enable for One Way Platforms** - Should the motor check for one way platforms? Uncheck this if there aren't any, the motor will be more efficient. This will only have an effect if the motor's collider can't collide with its own layer. If it can then setting this to false won't help, one way platforms or not.

**Moving Platform Layer Mask** - What layer contains moving platforms. The motor uses this knowledge to grab a MovingPlatformMotor2D component from the platforms. This requires that 'Raycasts Start in Colliders' is true in the Physics 2D settings. If there aren't any moving platforms then make sure this value is 'Nothing' (or 0). The motor will be more efficient.

**Additional Raycasts Per Side** - By default the motor will raycast through the corners to separate itself from moving platforms (or static environment if moved into one by a moving platform). The raycasts can miss if the environment is small enough. Increasing the number of additional raycasts can fix this issue. A good way to think about it is if the smaller environment is at least the size of the motor then this can be 0. Half the size of the motor then it should be 1. A third then 2 and so on. Caution as more raycasts are more expensive.

### Movement ###

**Ground Speed** - Maximum ground speed.

**Time to Ground Speed** - The time, in seconds, it will take to reach ground speed. This is used to calculate acceleration. A value of 0 mean instantaneous movement.

**Ground Stop Distance** - If at full speed, how far will the motor skid to a stop.

**Allow Direction Change In Air** - If true, then the motor's x velocity can be changed while in air. If false, then the motor's x velocity cannot be changed when in the air.

**Air Speed** - Maximum speed the motor will move horizontally while in the air.

**Time to Air Speed** - The time, in seconds, it will take to reach air speed. This is used to calculate acceleration.

**Air Stop Distance** - If at full air speed, how far will the motor 'skid' to a stop.

**Fall Speed** - Maximum fall speed (only y axis when negative).

**Gravity Multiplier** - Gravity multiplier to the Physics2D.gravity setting. Works like RigidBody2D's gravityScale.

**Fast Fall Speed** - Maximum fall speed when falling fast.

**Fast Fall Gravity Multiplier** - Gravity multiplier when falling fast.

### Jumping ###

**Jump Height** - The height, in Unity units, that the motor will jump to.

**Extra Jump Height** - If the motor is informed that the jump is held then this is the additional height the character will jump.

**Air Jumps Allowed** - This sets the number of air jumps the character is allowed to perform. Setting it to 0 will disable air jumping altogether.

**Jump Window When Falling** - The time in seconds that a jump will be allowed after it become invalid. This would be like allowing a jump even though the player has technically walked off the edge. Setting this to a low value (such as 0.1) can feel like a better experience for the player. If undesired then just set to 0.

**Jump Window When Activated** - How long the motor should remember that Jump() was called and activate a jump if it becomes valid in that time. This means that a player could press jump before they actually hit the ground and the motor will allow it to occur.

### Slopes ###

**Enable Slopes** - Should the motor check for slopes? If there aren't any slopes to walk/up down or slide down then uncheck this. The motor will be more efficient.

**Angle (Degrees) Allowed For Moving** - This is the degree of the slope the motor can walk on. 0 means only on flat ground where as 50 would mean any slope up to and including 50 degrees.

**Change Speed on Slopes** - Should the motor slow down on steeper slopes. The speed is based off of Max Ground Speed and the angle of the slope. If false then the motor always moves at Max Ground Speed on any allowed slope.

**Speed Multiplier on Slopes** - Multiplier against the speed on slopes. This will heavily emphasize slow downs on slopes.

**Stick to Ground** - This tells the motor to try to always stay on the ground when moving down slopes or up over slopes. For example, if this is false and the motor moves forward on a plane that then slopes down, the motor will fall onto the slope. If this is true then the motor will stay connected to the ground.

**Ground Check Distance for Sticking** - The motor ray casts down to see if there is ground to stick to. This value tells the motor how far to check. Increase this if the motor isn't sticking properly. Be cautious having this value too large as it may make the motor stick to grounds that aren't intended.

### Wall Interactions ###

**Enable Wall Jumps** - If jumping off the wall is allowed.

**Wall Jump Multiplier** - The base jump speed is calculated from Base Jump and Extra Jump Height. The multiplier multiplies the result. Leave at 1 for no change.

**Wall Jump Angle (Degrees)** - The angle (in degrees) of the wall jump. 0 is along the wall normal and 90 is straight up.

**Enable Wall Sticks** - If the motor should stick to walls.

**Wall Stick Duration** - The time, in seconds, that the motor will stick to walls. A large value (say 1000000) is effectively infinite.

**Enable Wall Slides** - If the motor should consider any wall sliding calculations. Wall sliding is when the character would slow down while 'sliding' down the wall.

**Wall Slide Speed** - The speed that the character will slide down the wall.

**Time to Wall Slide Speed** - The time, in seconds, it takes for the character to go from 0 to Wall Slide Speed.

**Enable Corner Grabs** - If corner grabbing is allowed. 

**Corner Grab Duration** - The time, in seconds, that the motor will stick to corners.

**Corner Jump Multiplier** - The multiplier on a corner jump from the calculated speed.

**Corner Distance Check** - A corner is considered grabbed if the upper corners of the collider do not intersect with the environment but the sides do. The value changes the consideration for box checks dimensions.

**Ignore Movement After Jump Duration** - How long the motor should ignore horizontal input movement after a wall jump or corner jump. This can allow better feeling wall jumps.

**Valid Normalized Interaction Area** - This is the size of a valid check (normalized to collider height) that will consider wall interactions valid. Starts from the top of the collider and moves down.

**Wall Interaction Threshold** - The input threshold for wall clings, corner grabs, and slides. Could be set to higher to prevent unwanted sticking to walls.

**Wall Interaction Cooldown** - Cooldown for allowing slides, clings, and corner grabs. This may be necessary if the motor can slide down a vertical moving platform. If they don't exist then this can be 0.

### Dashing ###

**Enable Dashes** - Is dashing allowed?

**Dash Distance** - The distance covered by the dash.

**Dash Duration** - The duration of the dash.

**Dash Cooldown** - How long, in seconds, before the motor will allow dash again?

**Dash Easing Function** - The easing function of the dash. For a dash that movement with a consistent speed pick linear.

**Gravity Delay After Dash** - The delay, in seconds, before gravity is reenabled after a dash. This can allow a short pause after an air dash before falling.

### PlatformerMotor2D Members ###

```csharp
float normalizedXMovement
```

Set the x movement direction. This is multiplied by the max speed. -1 is full left, 1 is full right. Higher numbers will result in faster acceleration.


```csharp
float timeScale
```

Set the time scale for the motor. This is independent of the global time scale. Negative values are not supported.


```csharp
Vector2 Velocity
```

The velocity of the motor. This should be queried instead of the rigidbody's velocity. Setting this during a dash doesn't have any meaning. NOTE: Setting rigidbody2D.velocity can have unexpected results. If you want to let Unity Physics take over then disable the motor first.


```csharp
MotorState motorState // Readonly

enum MotorState
{
    OnGround,
    Jumping,
    Falling,
    FallingFast,
    WallSliding,
    OnCorner,
    WallSticking,
    Dashing,
    Frozen,
    Slipping
}
```

Call this to get state information about the motor. This will be information such as if the object is in the air or on the ground. This can be used to set the appropriate animations.

```csharp
CollidedSurface collidingAgainst // Readonly

[Flags]
public enum CollidedSurface
{
    None = 0x0,
    Ground = 0x1,
    LeftWall = 0x2,
    RightWall = 0x4,
    Ceiling = 0x8,
    SlopeLeft = 0x10,
    SlopeRight = 0x20
}

```

State information on what the motor believes itself to be colliding against. These are flags so any number may be turned on.

```csharp
bool facingLeft
```

The direction the motor is facing. 

```csharp
bool onSlope // Readonly
```

If the motor is currently on a slope. This will be true for walking up/down slopes as well as slipping down a slope.

```csharp
bool slopeNormal // Readyonly
```

The normal of the slope the motor is on. This only has value if onSlope is true. This value could be used to calculate up/down the slope for movements such as dashes.

```csharp
bool fallFast
```

Set this true to have the motor fall faster. Set to false to fall at normal speeds.

```csharp
float amountFallen // Readonly
```

Returns the amount of distance the motor has fallen. Includes fallen fast distance.

```csharp
float amountFastFallen // Readonly
```

Returns the amount of distance the motor has fallen fast.

```csharp
float amountJumpedFor // Readonly
```

Returns the amount the motor has jumped. This ceases to keep calculating after the motor starts to come down.

```csharp
Vector2 dashDirection // Readonly
```

Returns the direction of the current dash. If not dashing then returns Vector2.zero.

```csharp
float distanceDashed // Readonly
```

Returns the amount of distance dashed. If not dashing then returns 0.

```csharp
bool canDash // Readonly
```

If the motor is currently able to dash.

```csharp
bool jumpingHeld
```

If jumpingHeld is set to true then the motor will jump further. Set to false if jumping isn't 'held'.

```csharp
bool frozen
```

Setting frozen to true will put the motor in a 'frozen' state. All information will be saved and set once unfrozen (the motor also reduce gravity to 0).

Note: This isn't a way to turn off the motor. To turn off the motor, simply set the script to disabled.

```csharp
MovingPlatformMotor2D connectedPlatform

```

Returns the moving platform that the motor is coupled with. If null then no moving platform.

```csharp
Action onDash
Action onDashEnd
Action onJump
Action onAirJump
Action onWallJump
Action onCornerJump
Action onLanded
```

Attach to these delegates to receive notifications for dash, dash end, jumping, and landing events.

### PlatformerMotor2D Methods ###

```csharp
void Jump()
```

Call this to have the GameObject try to jump, once called it will be handled in the FixedUpdate tick. The y axis is considered jump.

```csharp
void Jump(float customHeight)
```

Jump that allows a custom height. The extraJumpHeight is still applicable.

```csharp
void ForceJump()
```

This will force a jump to occur even if the motor doesn't think a jump is valid. This function will not work if the motor is dashing.

```csharp
void ForceJump(float customHeight)
```

Force a jump with a custom height. The extraJumpHeight is still applicable.

```csharp
void EndJump()
```

Call to end a jump. Causes the motor to stop calculated held speed for a jump.

```csharp
void ResetAirJump()
```

Resets the state for the jump counter to 0. This doesn't do anything if air jumps is set to 0.

```csharp
void Dash()
```

Call this to have the GameObject try to dash, once called it will be handled in the FixedUpdate tick. This casues the object to dash along their facing (if left or right for side scrollers).

```csharp
void ForceDash()
```

Forces the motor to dash even if dash isn't available yet.

```csharp
void Dash(Vector2 dir)
```

Send a direction vector to dash allow dashing in a specific direction.

```csharp
void ForceDash(Vector2 dir)
```

forces the motor to dash along a specified direction.

```csharp
void EndDash()
```


Call to end dash immediately.

```csharp
void ResetDashCooldown()
```

Resets the current dash cooldown.

```csharp
void DisconnectFromPlatform()
```

Decouples the motor from the platform. This could be useful for a platform that throw the motor in the air. Call this when when the motor should disconnect then set the appropriate velocity.

## PlayerController2D

The PlayerController2D script is a simple script that connects player input to the motor. This is set up as an example and it is encourage to write your own script that interacts with the motor.

## Moving Platforms

Given the complexity of the Unity 2D Physics engine, moving platforms have a few special rules in order to work. Each moving platform is required to have a MovingPlatformMotor2D attached to it and it is **IMPORTANT** that the MovingPlatformMotor2D script and any other script that moves the platforms is executed before the PlatformerMotor2D script in the Script Execution Order Settings (Edit -> Project Settings -> Script Execution Settings). The platform should update its position in FixedUpdate() and can leverage the velocity/position from MovingPlatformMotor2D.

See the Moving Platform scene for examples.

### MovingPlatformMotor2D Members ###

```csharp
Vector2 velocity
```

Set this to drive the platform by speed. This velocity is used to determine if the player should fall off the platform if the platform is falling too fast. Updates the transform position in FixedUpdate.

```csharp
Vector2 position
```

If you plan to update the position manually then use this field instead of transform.position. If this is set then the velocity field is zeroed out.

```csharp
Action<PlatformerMotor2D> onMotorContact
```

Invoked when a motor makes contact with a moving platform and is considered 'attached' to it.

### One Way Platforms ###

To acheive one way platforms with the motor. Have a environment piece with a collider and a PlatformEffector2D component attached. Be sure to check Use One Way and to check Used By Effector on the edge collider. The direction of the one way platform is the local up Vector of the platform (same as how the Unity Physics engine considers it). The motor will collide with the platform if it's velocity points away from the 'up' of the platform.

See the One Way Platforms scene for examples.

## FAQs

**The motor isn't separating correctly from my super awesome fast moving platform!**
The separation code for the motor is really simple and only really separates well if there isn't a ton of penetration by the moving platform. This means that really fast platforms may penetrate too deeply (or even pass completely through the motor without it knowing). A better solution may be implemented in the future but if you need fast moving platforms then consider decreasing the time step for Physics in the Time settings.

**Can the GameObject have a Rigidbody2D attached?**
This is fine. The motor will turn on isKinematic when enabled and set it back to whatever the default was when disabled. This can be useful if you want to disable the motor and allow the Physics engine to take over.

**OMG?! I'm not getting the collision/trigger messages!**
Attached a Rigidbody2D to the motor. See above.

**Can I use PlatformerMotor2D for controlling AI movements?**
Sure can. PlatformerMotor2D doesn't know anything about inputs, it just acts on information passed to it. An AI script can interface with the motor similarly how a player controller script could. A very simple example is included in the SimpleAI scene.

**I let go of the joystick and my GameObject isn't sliding the distance it is supposed to!**
If you're using the supplied PlayerController2D script or one of your own in which you use Input.GetAxis() then there's a built in deceleration in what Input.GetAxis() returns. This can definitely be impacting the distance the GameObject skids to a stop! To see a true skid to stop, set normalizedXMovement to zero.

**Something isn't working right!**
Well, this happens. Please open up an [issue](https://github.com/cjddmut/Unity-2D-Platformer-Controller/issues)!

## Games using the Unity 2D Platformer Controller

[Bit Ball](http://bitballgame.com) - Be a Bit Baller. The universe's top athletes in its most beloved pixelated sport, Bit Ball.

[Beep Boop](http://cjkimberlin.com/games/LD32/WebGL/index.html) - Help Beep Boop navigate past dangers using its unconventional weapon to send it flying. Created solo in 72 hours for Ludumdare 32.

If you are using the motor for your game, add it and set up a pull request or let me know at [@cjkimberlin](https://twitter.com/cjkimberlin)!
<!---%title=Unity 2D Platformer Controller%-->
<!---%download=https://github.com/cjddmut/Unity-2D-Platformer-Controller/releases/download/v0.2.0/PC2D_v0.2.0b.unitypackage%-->
<!---%github=https://github.com/cjddmut/Unity-2D-Platformer-Controller%-->
