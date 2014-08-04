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

    public float baseJump = 0.5f;
    public float extraJumpHeight = 0.5f;
    public bool allowDoubleJump = false;
    public bool allowWallJump = false;

    public bool allowWallSlide = false;
    public float wallSlideSpeed = 1;

    public bool allowCornerGrab = false;

    // These might need to change depending on the scale of sprites in Unity units.
    public float cornerHeightCheck = 0.1f;
    public float cornerWidthCheck = 0.1f;

    public bool canDash = false;
    public float dashCooldown = 1;
    public float dashSpeed = 5;
    public float dashDuration = 0.2f;
    public bool changeLayerDuringDash = false;
    public int dashLayer = 0;

    // Set this to use a specific collider for checks instead of grabbing the collider from gameObject.collider.
    [HideInInspector]
    public Collider2D colliderToUse;

    // Delegates, attach to these to get event calls.
    public Notification onDash;
    public Notification onDashEnd;
    public Notification onJump;

    // If this value is increased then unexpected behavior can occur when wall jumping (basically a wall check will occur after a jump). If
    // this needs to be fixed then add a debounce time to when a wall check can happen after a jump (really short is fine).
    public float checkDistance = 0.025f;

    // This is the layer mask checked by the motor to determine if the character has landed.
    public LayerMask checkMask;

    public bool drawGizmos = true;

    public enum MotorState
    {
        OnGround,
        InAir,
        Sliding,
        OnCorner,
        Dashing
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
    private bool onCorner = false;
    
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

    // This seems to be the magic number where you won't collide with the "ground" by being on the wall and not be able to sit on a corner
    // without colliding with the ground.
    private const float TRIM_STUCKTO_NUM = 0.01f;

    // When jumping off of a wall, this is the amount of time that movement input is ignored.
    private const float IGNORE_INPUT_TIME = 0.2f;

    private const float IGNORE_STICKINESS_TIME = 0.2f;

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
     * Call this function if the jump button is considered held (not on the initial detection but others (OnButton)).
     **/
    public void JumpHeld()
    {
        if (jumping.isJumping)
        {
            jumping.held = true;
        }
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
        dashing.dashDir = dir * dashSpeed;
    }

    public void EndDash()
    {
        // If dashing then end now.
        dashing.forceEnd = true;
    }

    public void SetFacingOffAxis(float axis)
    {
        if (axis < -PC2D.Globals.INPUT_THRESHOLD)
        {
            facingLeft = true;
        }
        else if (axis > PC2D.Globals.INPUT_THRESHOLD)
        {
            facingLeft = false;
        }
    }

    void FixedUpdate()
    {
        if (movementDir.x < -PC2D.Globals.INPUT_THRESHOLD)
        {
            facingLeft = true;
        }
        else if (movementDir.x > PC2D.Globals.INPUT_THRESHOLD)
        {
            facingLeft = false;
        }

        // First, are we trying to dash?
        if (canDash && dashing.pressed && Time.time >= dashing.canDashAgain)
        {
            dashing.isDashing = true;
            dashing.canDashAgain = Time.time + dashCooldown;
            dashing.endDash = Time.time + dashDuration;
            rigidbody2D.drag = 0;
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
                    dashing.dashDir = -Vector2.right * dashSpeed;
                }
                else
                {
                    dashing.dashDir = Vector2.right * dashSpeed;
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

                if (stuckTo == Surface.Ground)
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
                    rigidbody2D.drag = initialDrag;

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
                    rigidbody2D.drag = 0;

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
                            rigidbody2D.velocity = movementDir * maxAirSpeed;
                        }
                    }
                }
            }

            // Wall hug?
            if (allowWallSlide)
            {
                // Only if we're currently falling.
                if (rigidbody2D.velocity.y < 0 &&
                    (stuckTo == Surface.LeftWall && movementDir.x < -PC2D.Globals.INPUT_THRESHOLD ||
                     stuckTo == Surface.RightWall && movementDir.x > PC2D.Globals.INPUT_THRESHOLD))
                {
                    // Sticky!
                    Vector2 v = rigidbody2D.velocity;

                    // Set the y to one acceleration tick upwards against gravity.
                    v.y = -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime - wallSlideSpeed;
                    rigidbody2D.velocity = v;
                    motorState = MotorState.Sliding;
                }
            }

            // Corner grab?
            if (allowCornerGrab)
            {
                if (rigidbody2D.velocity.y < 0 || onCorner)
                {
                    onCorner = false;

                    if (stuckTo == Surface.LeftWall && movementDir.x < -PC2D.Globals.INPUT_THRESHOLD ||
                        stuckTo == Surface.RightWall && movementDir.x > PC2D.Globals.INPUT_THRESHOLD)
                    {
                        if (CheckIfAtCorner())
                        {
                            // Stick completely!
                            Vector2 v = rigidbody2D.velocity;

                            // Set the y to one acceleration tick upwards against gravity.
                            v.y = -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime;
                            rigidbody2D.velocity = v;
                            onCorner = true;
                            motorState = MotorState.OnCorner;
                            jumping.doubleJumped = false;
                        }
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
                else if (onCorner)
                {
                    // If we are on a corner then jump up.
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, CalculateJumpSpeed());
                    ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
                }
                else if (allowWallJump && stuckTo == Surface.LeftWall)
                {
                    // If jump was pressed as we or before we entered the wall then just jump away.
                    rigidbody2D.velocity = upRight * CalculateJumpSpeed();

                    // It's likely the player is still pressing into the wall, ignore movement for a little amount of time.
                    // TODO: Only ignore left movement?
                    ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;

                    // If wall jump is allowed but not wall slide then double jump will not be allowed earlier, allow it now.
                    jumping.doubleJumped = false;
                }
                else if (allowWallJump && stuckTo == Surface.RightWall)
                {
                        rigidbody2D.velocity = upLeft * CalculateJumpSpeed();
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
                    rigidbody2D.drag = 0;
                    jumping.pressed = false;
                    onCorner = false;
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

        // Reset some things.
        movementDir = Vector2.zero;
        jumping.held = false;
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
        max.y += cornerHeightCheck;

        if (stuckTo == Surface.LeftWall)
        {
            max.x = min.x;
            min.x -= cornerWidthCheck;
        }
        else if (stuckTo == Surface.RightWall)
        {
            min.x = max.x;
            max.x += cornerWidthCheck;
        }

        Collider2D col = Physics2D.OverlapArea(min, max, checkMask);

        return col == null;
    }

    private void ClampVelocity()
    {
        Vector2 checkedSpeed = rigidbody2D.velocity;

        if (stuckTo == Surface.Ground)
        {
            checkedSpeed.x = Mathf.Clamp(checkedSpeed.x, -maxGroundSpeed, maxGroundSpeed);
        }
        else
        {
            // Check both horizontal air speed and fall speed.
            checkedSpeed.x = Mathf.Clamp(checkedSpeed.x, -maxAirSpeed, maxAirSpeed);

            if (checkedSpeed.y < -maxFallSpeed)
            {
                checkedSpeed.y = -maxFallSpeed;
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
            if (movementDir.x < -PC2D.Globals.INPUT_THRESHOLD)
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
            else if (movementDir.x > PC2D.Globals.INPUT_THRESHOLD)
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
    public Vector2 movementDir { set; private get; }

    /**
     * Call this to get state information about the motor. This will be information such as if the object is in the air or on the ground. This can be used
     * to set the appropriate animations.
     **/
    public MotorState motorState { get; private set; }

    /**
     * Since the motor needs to know the facing of the object, this information is made available to anyone else who might need it.
     **/
    public bool facingLeft { get; private set; }

    //
    // Debug
    //

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

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
                max.y += cornerHeightCheck;
                max.x = min.x;
                min.x -= cornerWidthCheck;
                Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

                min = box.min;
                max = box.max;
                min.y = max.y;
                max.y += cornerHeightCheck;
                min.x = max.x;
                max.x += cornerWidthCheck;
                Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));
            }
        }
    }
}
