using UnityEngine;
using System.Collections;
using PC2D;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor2D : MonoBehaviour
{
    public bool alwaysOnGround = false;

    public bool accelerate = true;
    public float groundAcceleration = 30;
    public float airAcceleration = 10;

    public float maxGroundSpeed = 3f;
    public float maxAirSpeed = 2f;
    public float maxFallSpeed = 5f;
    public float maxFastFallSpeed = 5f;

    public float airDrag = 0;

    public float baseJump = 0.5f;
    public float extraJumpHeight = 0.5f;
    public bool allowDoubleJump = false;

    public bool allowWallJump = false;
    public float wallJumpMultiplier = 1f;

    public bool allowWallCling = false;
    public float wallClingDuration = 0.5f;

    public bool allowWallSlide = false;
    public float wallSlideSpeed = 1;

    public bool allowCornerGrab = false;
    public float cornerJumpMultiplier = 1f;
    public float cornerGrabDuration = 0.5f;

    // These might need to change depending on the scale of sprites in Unity units.
    public float cornerDistanceCheck = 0.1f;

    public bool canDash = false;
    public float dashCooldown = 1;
    public float dashDistance = 1;
    public float dashDuration = 0.2f;
    public bool changeLayerDuringDash = false;
    public int dashLayer = 0;

    // Input threshold for the motor.
    [Range(0f, 1f)]
    public float inputThreshold = 0.2f;

    [Range(0f, 1f)]
    public float heavyInputThreshold = 0.5f;

    // Delegates, attach to these to get event calls.
    public Notification onDash;
    public Notification onDashEnd;
    public Notification onJump;

    // If this value is increased then unexpected behavior can occur when wall jumping (basically a wall check will occur after a jump). If
    // this needs to be fixed then add a debounce time to when a wall check can happen after a jump (really short is fine).
    public float checkDistance = 0.025f;

    // This is the layer mask checked by the motor to determine if the character has landed.
    public LayerMask checkMask;

    public enum MotorState
    {
        OnGround,
        InAir,
        Sliding,
        OnCorner,
        Clinging,
        Dashing,
        Frozen
    }

    private Surface stuckTo = Surface.None;
    private enum Surface
    {
        None,
        Ground,
        LeftWall,
        RightWall,
    }

    private Vector2 upRight;
    private Vector2 upLeft;
    private float initialDrag;
    private float initialGravity;
    private float ignoreMovementUntil = 0;
    
    // Contains the various jump variables, this is for organization.
    private class JumpState
    {
        public bool isJumping = false;
        public bool pressed = false;
        public bool held = false;
        public bool doubleJumped = false;

        public float time = 0;
        public float timeBuffer = 0.2f; // Amount of time that a jump can be triggered, same as the default unity controller script.

        public bool force = false;
        public float extraSpeed;
    }
    private JumpState jumping = new JumpState();

    // Contains the various dash variables.
    private class DashState
    {
        public bool isDashing = false;
        public bool pressed = false;
        public float canDashAgain;
        public float endDash;
        public bool dashWithDirection;
        public Vector2 dashDir = Vector2.zero;
        public int originalLayer;
        public bool forceEnd = false;
    }
    private DashState dashing = new DashState();

    // Contains information for wall clings, slides, and corner grabs.
    private class WallState
    {
        public bool onCorner = false;
        public float cornerHangTime = 0;

        public bool sliding = false;

        public bool clinging = false;
        public float clingTime = 0;

        public bool canHangAgain = true;
    }
    private WallState wallInfo = new WallState();

    // This seems to be the magic number where you won't collide with the "ground" by being on the wall and not be able to sit on a corner
    // without colliding with the ground.
    private const float TRIM_STUCKTO_NUM = 0.01f;

    // When jumping off of a wall, this is the amount of time that movement input is ignored.
    private const float IGNORE_INPUT_TIME = 0.2f;

    private const float IGNORE_STICKINESS_TIME = 0.2f;

    void Awake()
    {
        // Placed in awake so some other script can change it in Start().
        clampVelocity = true;
        changeDrag = true;
    }

    void Start()
    {
        upRight = Vector2.up + Vector2.right;
        upRight.Normalize();
        upLeft = new Vector2(-upRight.x, upRight.y);

        initialDrag = rigidbody2D.drag;
        initialGravity = rigidbody2D.gravityScale;
        dashing.originalLayer = gameObject.layer;
    }

    void OnDestroy()
    {
        onDash = null;
        onDashEnd = null;
    }

    /**
     * Call this to have the GameObject try to jump, once called it will be handled in the FixedUpdate tick. The y axis is considered jump.
     **/
    public void Jump(float extraSpeed = 0)
    {
        jumping.pressed = true;
        jumping.time = Time.time;
        jumping.isJumping = false;
        jumping.extraSpeed = extraSpeed;
    }

    public void ForceJump(float extraSpeed = 0)
    {
        Jump(extraSpeed);
        jumping.force = true;
    }

    /**
     * Resets a double jump to occur. This only has effect if double jumps are allowed. 
     * */
    public void ResetDoubleJump()
    {
        jumping.doubleJumped = false;
    }

    /**
     * Call this to have the GameObject try to dash, once called it will be handled in the FixedUpdate tick.
     * This casues the object to dash along their facing (if left or right for side scrollers).
     **/
    public void Dash()
    {
        dashing.pressed = true;
        dashing.dashWithDirection = false;
    }

    /**
     * Send a direction vector to dash allow dashing in a specific direction. 
     **/
    public void Dash(Vector2 dir)
    {
        dashing.pressed = true;
        dashing.dashWithDirection = true;
        dashing.dashDir = dir * (dashDistance / dashDuration);
    }

    public void EndDash()
    {
        // If dashing then end now.
        dashing.forceEnd = true;
    }

    void FixedUpdate()
    {
        // Frozen?
        if (frozen)
        {
            return;
        }

        if (movementDir.x < -inputThreshold)
        {
            facingLeft = true;
        }
        else if (movementDir.x > inputThreshold)
        {
            facingLeft = false;
        }

        // First, are we trying to dash?
        if (canDash && dashing.pressed && Time.time >= dashing.canDashAgain)
        {
            dashing.isDashing = true;
            dashing.canDashAgain = Time.time + dashCooldown;
            dashing.endDash = Time.time + dashDuration;

            if (changeDrag)
            {
                rigidbody2D.drag = 0;
            }

            rigidbody2D.gravityScale = 0;
            motorState = MotorState.Dashing;

            if (changeLayerDuringDash)
            {
                gameObject.layer = dashLayer;
                // Changed layers so we dirty the physics with this hack.
                transform.localScale = transform.localScale;
            }

            if (!dashing.dashWithDirection)
            {
                if (facingLeft)
                {
                    dashing.dashDir = -Vector2.right * (dashDistance / dashDuration);
                }
                else
                {
                    dashing.dashDir = Vector2.right * (dashDistance / dashDuration);
                }
            }

            if (onDash != null)
            {
                onDash();
            }
        }

        dashing.pressed = false;

        if (dashing.isDashing)
        {
            // Dashing is special!
            if (!dashing.forceEnd)
            {
                rigidbody2D.velocity = dashing.dashDir;
            }

            if (Time.time >= dashing.endDash || dashing.forceEnd)
            {
                // Done dashing, back to normals.
                dashing.forceEnd = false;
                dashing.isDashing = false;
                rigidbody2D.gravityScale = initialGravity;

                if (stuckTo == Surface.Ground && changeDrag)
                {
                    rigidbody2D.drag = initialDrag;
                }

                if (changeLayerDuringDash)
                {
                    gameObject.layer = dashing.originalLayer;

                    // Changed layers so we dirty the physics with this hack.
                    transform.localScale = transform.localScale;
                }

                if (onDashEnd != null)
                {
                    onDashEnd();
                }
            }
        }
        else
        {
            // Are we grounded?
            SetStuckTo();

            if (stuckTo != Surface.None && rigidbody2D.velocity.y < 0)
            {
                // If we're grounded then we are not jumping.
                jumping.isJumping = false;
            }

            if (stuckTo != Surface.RightWall && stuckTo != Surface.LeftWall)
            {
                wallInfo.canHangAgain = true;
            }

            if (stuckTo == Surface.Ground)
            {
                motorState = MotorState.OnGround;
            }
            else
            {
                motorState = MotorState.InAir;
            }

            // Apply movement if we're not ignoring it.
            if (Time.time >= ignoreMovementUntil)
            {
                if (stuckTo == Surface.Ground)
                {
                    // Not in air.
                    if (changeDrag)
                    {
                        rigidbody2D.drag = initialDrag;
                    }

                    if (accelerate)
                    {
                        rigidbody2D.velocity += movementDir * groundAcceleration * Time.fixedDeltaTime;
                    }
                    else
                    {
                        rigidbody2D.velocity = movementDir * maxGroundSpeed;
                    }
                }
                else
                {
                    if (changeDrag)
                    {
                        rigidbody2D.drag = 0;
                    }

                    if (changeDrag && airDrag > 0 && Mathf.Abs(movementDir.x) < inputThreshold)
                    {
                        // slow down horizontal.
                        Vector3 vel = rigidbody2D.velocity;

                        if (vel.x < 0)
                        {
                            vel.x += airDrag * Time.fixedDeltaTime;

                            if (vel.x > 0)
                            {
                                vel.x = 0;
                            }
                        }
                        else if (vel.x > 0)
                        {
                            vel.x -= airDrag * Time.fixedDeltaTime;

                            if (vel.x < 0)
                            {
                                vel.x = 0;
                            }
                        }

                        rigidbody2D.velocity = vel;
                    }
                    else
                    {
                        // Don't apply the force if we're already on the wall.
                        if (movementDir.x > 0 && stuckTo == Surface.LeftWall ||
                            movementDir.x < 0 && stuckTo == Surface.RightWall ||
                            stuckTo == Surface.None)
                        {
                            if (accelerate)
                            {
                                rigidbody2D.velocity += movementDir * airAcceleration * Time.fixedDeltaTime;
                            }
                            else
                            {
                                // In air so we set the x but add the y.

                                rigidbody2D.velocity = new Vector2(
                                    movementDir.x * maxAirSpeed,
                                    rigidbody2D.velocity.y + movementDir.y * maxAirSpeed);
                            }
                        }
                    }
                }
            }

            // Corner grab?
            if (allowCornerGrab)
            {
                if (rigidbody2D.velocity.y < 0 || wallInfo.onCorner)
                {
                    if ((stuckTo == Surface.LeftWall && movementDir.x < -heavyInputThreshold ||
                        stuckTo == Surface.RightWall && movementDir.x > heavyInputThreshold) &&
                        CheckIfAtCorner())
                    {
                        if (!wallInfo.onCorner && wallInfo.canHangAgain)
                        {
                            wallInfo.onCorner = true;
                            wallInfo.canHangAgain = false;
                            wallInfo.cornerHangTime = Time.time + cornerGrabDuration;
                        }

                        if (wallInfo.onCorner && Time.time >= wallInfo.cornerHangTime)
                        {
                            wallInfo.onCorner = false;
                        }

                        if (wallInfo.onCorner)
                        {
                            // Stick completely!
                            Vector2 v = rigidbody2D.velocity;

                            // Set the y to one acceleration tick upwards against gravity.
                            v.y = -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime;
                            rigidbody2D.velocity = v;
                            motorState = MotorState.OnCorner;
                        }
                    }
                    else
                    {
                        wallInfo.onCorner = false;
                    }
                }
            }

            // Wall Cling
            if (allowWallCling)
            {
                if (rigidbody2D.velocity.y < 0 || wallInfo.clinging)
                {
                    if (stuckTo == Surface.LeftWall && movementDir.x < -heavyInputThreshold ||
                    stuckTo == Surface.RightWall && movementDir.x > heavyInputThreshold)
                    {
                        if (!wallInfo.clinging && wallInfo.canHangAgain)
                        {
                            wallInfo.clinging = true;
                            wallInfo.canHangAgain = false;
                            wallInfo.clingTime = Time.time + wallClingDuration;
                        }

                        if (wallInfo.clinging && Time.time >= wallInfo.clingTime)
                        {
                            wallInfo.clinging = false;
                        }

                        if (wallInfo.clinging)
                        {
                            // Sticky!
                            Vector2 v = rigidbody2D.velocity;

                            // Set the y to one acceleration tick upwards against gravity.
                            v.y = -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime;
                            rigidbody2D.velocity = v;
                            motorState = MotorState.Clinging;
                        }
                    }
                    else
                    {
                        wallInfo.clinging = false;
                    }
                }
            }

            // Wall slide?
            if (allowWallSlide)
            {
                if (rigidbody2D.velocity.y < 0 || wallInfo.sliding)
                {
                    wallInfo.sliding = false;

                    // Only if we're currently falling.
                    if (stuckTo == Surface.LeftWall && movementDir.x < -heavyInputThreshold ||
                        stuckTo == Surface.RightWall && movementDir.x > heavyInputThreshold)
                    {
                        // Sticky!
                        Vector2 v = rigidbody2D.velocity;

                        // Set the y to one acceleration tick upwards against gravity.
                        v.y = -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime - wallSlideSpeed;
                        rigidbody2D.velocity = v;
                        motorState = MotorState.Sliding;
                        wallInfo.sliding = true;
                    }
                }
            }
            // This is something that the default Unity Controller script does, allows the player to press jump button
            // earlier than would normally be allowed. They say it leads to a more pleasant experience for the
            // user. I'll assume they're on to something.
            if (Time.time > jumping.time + jumping.timeBuffer)
            {
                jumping.pressed = false;
            }

            // If we're currently jumping and the jump button is still held down ignore gravity to allow us to acheive the extra height.
            if (jumping.isJumping && jumping.held)
            {
                if (Time.time < jumping.time + extraJumpHeight / CalculateJumpSpeed())
                {
                    // TODO: This might want to be based off of the jump direction (in case of wall jumps) which might give a better
                    //       feel.
                    rigidbody2D.velocity += new Vector2(0, -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime);
                }
            }

            // If our state is not in the air then open up the possibility of double jump (we need to be able to double jump if
            // we walk off an edge so it can't be based of when a jump occured).
            if (motorState != MotorState.InAir)
            {
                jumping.doubleJumped = false;
            }

            // Jump?
            if (jumping.pressed)
            {
                bool jumped = true;

                // Jump might mean different things depending on the state.
                if (stuckTo == Surface.Ground || jumping.force)
                {
                    // Normal jump.
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, CalculateJumpSpeed() + jumping.extraSpeed);
                }
                else if (wallInfo.onCorner)
                {
                    // If we are on a corner then jump up.
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, CalculateJumpSpeed() * cornerJumpMultiplier);
                    ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
                }
                else if (allowWallJump && stuckTo == Surface.LeftWall)
                {
                    // If jump was pressed as we or before we entered the wall then just jump away.
                    rigidbody2D.velocity = upRight * CalculateJumpSpeed() * wallJumpMultiplier;

                    // It's likely the player is still pressing into the wall, ignore movement for a little amount of time.
                    // TODO: Only ignore left movement?
                    ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;

                    // If wall jump is allowed but not wall slide then double jump will not be allowed earlier, allow it now.
                    jumping.doubleJumped = false;
                }
                else if (allowWallJump && stuckTo == Surface.RightWall)
                {
                    rigidbody2D.velocity = upLeft * CalculateJumpSpeed() * wallJumpMultiplier;
                    ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
                    jumping.doubleJumped = false;
                }
                else if (allowDoubleJump && stuckTo == Surface.None && !jumping.doubleJumped)
                {
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, CalculateJumpSpeed());
                    jumping.doubleJumped = true;
                }
                else
                {
                    // Guess we aren't jumping!
                    jumped = false;
                }

                if (jumped)
                {
                    jumping.isJumping = true;

                    if (changeDrag)
                    {
                        rigidbody2D.drag = 0;
                    }

                    jumping.pressed = false;
                    wallInfo.onCorner = false;
                    wallInfo.sliding = false;
                    wallInfo.clinging = false;
                    jumping.force = false;

                    if (onJump != null)
                    {
                        onJump();
                    }
                }
            }

            // Remove left or right movements if trying to movement into the wall, this 

            // Check speeds.
            ClampVelocity();
        }

        if (Physics2D.gravity != Vector2.zero && rigidbody2D.gravityScale != 0 && motorState == MotorState.InAir)
        {
            // The rigidbody might go to sleep if clinged onto a wall.
            rigidbody2D.WakeUp();
        }
    }

    private bool CheckIfAtCorner()
    {
        Bounds box;

        if (colliderToUse != null)
        {
            box = colliderToUse.bounds;
        }
        else
        {
            box = collider2D.bounds;
        }

        Vector2 min = box.min;
        Vector2 max = box.max;

        // New min y is always at the current max y.
        min.y = max.y;
        max.y += cornerDistanceCheck;

        if (stuckTo == Surface.LeftWall)
        {
            max.x = min.x;
            min.x -= cornerDistanceCheck;
        }
        else if (stuckTo == Surface.RightWall)
        {
            min.x = max.x;
            max.x += cornerDistanceCheck;
        }

        Collider2D col = Physics2D.OverlapArea(min, max, checkMask);

        return col == null;
    }

    private void ClampVelocity()
    {
        if (!clampVelocity)
        {
            return;
        }

        Vector2 checkedSpeed = rigidbody2D.velocity;

        if (stuckTo == Surface.Ground)
        {
            checkedSpeed.x = Mathf.Clamp(checkedSpeed.x, -maxGroundSpeed, maxGroundSpeed);
        }
        else
        {
            // Check both horizontal air speed and fall speed.
            checkedSpeed.x = Mathf.Clamp(checkedSpeed.x, -maxAirSpeed, maxAirSpeed);

            if (fallFast)
            {
                if (checkedSpeed.y < -maxFastFallSpeed)
                {
                    checkedSpeed.y = -maxFastFallSpeed;
                }
            }
            else
            {
                if (checkedSpeed.y < -maxFallSpeed)
                {
                    checkedSpeed.y = -maxFallSpeed;
                }
            }
        }

        rigidbody2D.velocity = checkedSpeed;
    }

    private void SetStuckTo()
    {
        if (alwaysOnGround)
        {
            stuckTo = Surface.Ground;
            return;
        }

        Bounds box;
        
        if (colliderToUse != null)
        {
            box = colliderToUse.bounds;
        }
        else
        {
            box = collider2D.bounds;
        }

        Vector2 min = box.min;
        Vector2 max = box.max;

        min.x += TRIM_STUCKTO_NUM;
        max.x -= TRIM_STUCKTO_NUM;

        min.y -= checkDistance;
        max.y = transform.position.y; // Go ahead and bring the maximum y down.

        // TODO: This requires that a ground layer is set up to work. Consider moving to a set up that will consider all
        //       collisions but ignore the player's collider.

        Collider2D col = Physics2D.OverlapArea(min, max, checkMask);

        stuckTo = col != null ? Surface.Ground : Surface.None;

        if (stuckTo == Surface.None)
        {
            // Consider possible stuck to left wall if we're pressing into it.
            if (movementDir.x < -inputThreshold)
            {
                // How about on the walls for wall jump? Left wall first.
                min = box.min;
                max = box.max;

                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;

                min.x -= checkDistance;
                max.x = transform.position.x;

                col = Physics2D.OverlapArea(min, max, checkMask);

                if (col != null)
                {
                    stuckTo = Surface.LeftWall;
                }
            }
            else if (movementDir.x > inputThreshold)
            {
                // Now right wall.
                min = box.min;
                max = box.max;

                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;

                min.x = transform.position.x;
                max.x += checkDistance;

                col = Physics2D.OverlapArea(min, max, checkMask);

                if (col != null)
                {
                    stuckTo = Surface.RightWall;
                }
            }
        }
    }

    private float CalculateJumpSpeed()
    {
        return Mathf.Sqrt(-2 * baseJump * rigidbody2D.gravityScale * Physics2D.gravity.y);
    }

    //
    // Getters/Setters
    //

    /**
     * Set the movement direction. Ideally this should be a normalized vector but could be larger for faster speeds. READONLY
     **/
    public Vector2 movementDir { get; set; }

    /**
     * Call this to get state information about the motor. This will be information such as if the object is in the air or on the ground. This can be used
     * to set the appropriate animations.
     **/
    public MotorState motorState { get; private set; }

    /**
     * Since the motor needs to know the facing of the object, this information is made available to anyone else who might need it.
     **/
    public bool facingLeft { get; set; }

    /**
     * Set this true to have the motor fall faster. Set to false to fall at normal speeds.
     * */
    public bool fallFast { get; set; }

    /**
     * If jumpingHeld is set to true then the motor will jump further. Set to false if jumping isn't 'held'.
     * */
    public bool jumpingHeld
    {
        get
        {
            return jumping.held;
        }

        set
        {
            jumping.held = value;
        }
    }

    /**
     * Setting frozen to true will put the motor in a 'frozen' state. All information will be saved and set once unfrozen (the motor also
     * reduce gravity to 0).
     * 
     * Note: This isn't a way to turn off the motor. To turn off the motor, simply set the script to disabled.
     **/
    public bool frozen
    {
        get
        {
            return _frozen;
        }
        set
        {
            if (_frozen != value)
            {
                _frozen = value;

                if (_frozen)
                {
                    frozenTime = Time.time;
                    frozenVelocity = rigidbody2D.velocity;
                    frozenGravity = rigidbody2D.gravityScale;

                    rigidbody2D.velocity = Vector2.zero;
                    rigidbody2D.gravityScale = 0;
                    motorState = MotorState.Frozen;
                }
                else
                {
                    // Any cooldown or time events happen here.
                    float delta = Time.time - frozenTime;
                    dashing.canDashAgain += delta;
                    dashing.endDash += delta;

                    rigidbody2D.velocity = frozenVelocity;
                    rigidbody2D.gravityScale = frozenGravity;
                }
            }
        }
    }
    private bool _frozen;
    private float frozenTime;
    private Vector2 frozenVelocity;
    private float frozenGravity;

    /**
     * Should the motor clamp the velocity of the GameObject? Set to true by default.
     * */
    public bool clampVelocity { get; set; }

    /**
     * Should the motor change drag of the rigidbody2D. The motor commonly changes the drag depending on the situation, if this conflicts
     * with your own manipulation of rigidbody2D's drag then set this to false.
     * 
     * If this is false then the horizontal air drag is also ignored.
     * */
    public bool changeDrag { get; set; }

    /**
     * Set this to use a specific collider for checks instead of grabbing the collider from gameObject.collider2D.
     * */
    public Collider2D colliderToUse { get; set; }

    //
    // Debug
    //

    void OnDrawGizmosSelected()
    {
        // Ground check.
        Bounds box;
        Vector2 min;
        Vector2 max;

        if (colliderToUse != null)
        {
            box = colliderToUse.bounds;
        }
        else
        {
            box = collider2D.bounds;
        } 

        if (!alwaysOnGround)
        {
            // Ground check box
            min = box.min;
            max = box.max;
            min.x += TRIM_STUCKTO_NUM;
            max.x -= TRIM_STUCKTO_NUM;
            min.y -= checkDistance;
            max.y = transform.position.y;
            Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

            if (allowCornerGrab || allowWallJump || allowWallSlide)
            {
                // Left check box
                min = box.min;
                max = box.max;
                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;
                min.x -= checkDistance;
                max.x = transform.position.x;
                Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

                // Right check box
                min = box.min;
                max = box.max;
                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;
                min.x = transform.position.x;
                max.x += checkDistance;
                Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));
            }

            if (allowCornerGrab)
            {
                min = box.min;
                max = box.max;
                min.y = max.y;
                max.y += cornerDistanceCheck;
                max.x = min.x;
                min.x -= cornerDistanceCheck;
                Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

                min = box.min;
                max = box.max;
                min.y = max.y;
                max.y += cornerDistanceCheck;
                min.x = max.x;
                max.x += cornerDistanceCheck;
                Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));
            }
        }
    }
}
