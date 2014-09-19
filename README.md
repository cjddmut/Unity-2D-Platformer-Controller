# UnityPlayerController2D
=======================

A customizable player controller asset that can be dropped into any scene for immediate 2D motion support.

####Obtain!####
[Releases](https://github.com/cjddmut/Unity-2D-Player-Controller/releases)

If you'd like the most up to date version (which is the most cool), then pull the repo or download it [here](https://github.com/cjddmut/Unity-2D-Player-Controller/archive/develop.zip) and copy the files in Assets to your project's Assets folder.

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

## Overview of Public Properties of Controller

Most likely this file will need to be modified to suit the needs of the game.

**Can Control** - If input should be given to the Motor. If false then the character will not respond to input (will react to gravity and other forces).
