using UnityEngine;
using System.Collections;
using PC2D;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMotor2D : MonoBehaviour
{
    public float GroundAcceleration = 30;
    public float AirAcceleration = 10;

    public float MaxGroundSpeed = 3f;
    public float MaxAirSpeed = 2f;
    public float MaxFallSpeed = 5f;

    public float BaseJump = 0.5f;
    public float ExtraJumpHeight = 0.5f;
    public bool AllowDoubleJump = false;

    public bool AllowWallSlide = false;
    public float WallSlideSpeed = 1;

    public bool AllowWallJump = false;

    public bool AllowCornerGrab = false;
    public float CornerHeightCheck = 0.1f;
    public float CornerWidthCheck = 0.1f;

    public bool CanDash = false;
    public float DashCooldown = 1;
    public float DashSpeed = 5;
    public float DashDuration = 0.2f;
    public bool ChangeLayerDuringDash = false;
    public int DashLayer = 0;

    [HideInInspector]
    // Set this to use a specific collider for checks instead of grabbing the collider from gameObject.collider.
    public Collider2D ColliderToUse;

    // Delegates, attach to these to get event calls.
    public Notification OnDash;
    public Notification OnDashEnd;
    public Notification OnJump;

    // If this value is increased then unexpected behavior can occur when wall jumping (basically a wall check will occur after a jump). If
    // this needs to be fixed then add a debounce time to when a wall check can happen after a jump (really short is fine).
    public float CheckDistance = 0.025f;

    // This is the layer mask checked by the motor to determine if the character has landed.
    public LayerMask CheckMask;

    public bool DrawGizmos = false;

    public enum MotorState
    {
        OnGround,
        InAir,
        Sliding,
        OnCorner,
        Dashing
    }

    private MotorState _MotorState;

    private Surface StuckTo = Surface.None;
    private enum Surface
    {
        None,
        Ground,
        LeftWall,
        RightWall,
    }

    private Vector2 _MovementDir;

    private Vector2 _UpRight;
    private Vector2 _UpLeft;
    private float _InitialDrag;
    private float _InitialGravity;
    private float _IgnoreMovementUntil = 0;
    private bool _OnCorner = false;
    
    private bool _FacingLeft = false;

    // Contains the various jump variables, this is for organization.
    private class JumpState
    {
        public bool IsJumping = false;
        public bool Pressed = false;
        public bool Held = false;
        public bool DoubleJumped = false;

        public float Time = 0;
        public float TimeBuffer = 0.2f; // Amount of time that a jump can be triggered, same as the default unity controller script.

        public bool Force = false;
        public float ExtraSpeed;
    }
    private JumpState _Jumping = new JumpState();

    // Contains the various dash variables.
    private class DashState
    {
        public bool IsDashing = false;
        public bool Pressed = false;
        public float CanDashAgain;
        public float EndDash;
        public bool DashWithDirection;
        public Vector2 DashDir = Vector2.zero;
        public int OriginalLayer;
        public bool ForceEnd = false;
    }
    private DashState _Dashing = new DashState();

    // This seems to be the magic number where you won't collide with the "ground" by being on the wall and not be able to sit on a corner
    // without colliding with the ground.
    private const float TRIM_STUCKTO_NUM = 0.01f;

    // Input threshold in order to take effect. Arbitarily set.
    private const float INPUT_THRESHOLD = 0.1f;

    // When jumping off of a wall, this is the amount of time that movement input is ignored.
    private const float IGNORE_INPUT_TIME = 0.2f;

    private const float IGNORE_STICKINESS_TIME = 0.2f;

    void Start()
    {
        _UpRight = Vector2.up + Vector2.right;
        _UpRight.Normalize();
        _UpLeft = new Vector2(-_UpRight.x, _UpRight.y);

        _InitialDrag = rigidbody2D.drag;
        _InitialGravity = rigidbody2D.gravityScale;
        _Dashing.OriginalLayer = gameObject.layer;
    }

    void OnDestroy()
    {
        OnDash = null;
        OnDashEnd = null;
    }

    /**
     * Call this to have the GameObject try to jump, once called it will be handled in the FixedUpdate tick. The y axis is considered jump.
     **/
    public void Jump(float extraSpeed = 0)
    {
        _Jumping.Pressed = true;
        _Jumping.Time = Time.time;
        _Jumping.IsJumping = false;
        _Jumping.ExtraSpeed = extraSpeed;
    }

    public void ForceJump(float extraSpeed = 0)
    {
        Jump(extraSpeed);
        _Jumping.Force = true;
    }

    /**
     * Call this function if the jump button is considered held (not on the initial detection but others (OnButton)).
     **/
    public void JumpHeld()
    {
        if (_Jumping.IsJumping)
        {
            _Jumping.Held = true;
        }
    }

    /**
     * Call this to have the GameObject try to dash, once called it will be handled in the FixedUpdate tick.
     * This casues the object to dash along their facing (if left or right for side scrollers).
     **/
    public void Dash()
    {
        _Dashing.Pressed = true;
        _Dashing.DashWithDirection = false;
    }

    /**
     * Send a direction vector to dash allow dashing in a specific direction. 
     **/
    public void Dash(Vector2 dir)
    {
        _Dashing.Pressed = true;
        _Dashing.DashWithDirection = true;
        _Dashing.DashDir = dir * DashSpeed;
    }

    public void EndDash()
    {
        // If dashing then end now.
        _Dashing.ForceEnd = true;
    }

    /**
     * Set the movement direction. Ideally this should be a normalized vector but could be larger for faster speeds.
     **/
    public void SetMovementDirection(Vector2 dir)
    {
        _MovementDir = dir;
    }

    public void SetFacingOffAxis(float axis)
    {
        if (axis < - INPUT_THRESHOLD)
        {
            _FacingLeft = true;
        }
        else if (axis > INPUT_THRESHOLD)
        {
            _FacingLeft = false;
        }
    }


    /**
     * Call this to get state information about the motor. This will be information such as if the object is in the air or on the ground. This can be used
     * to set the appropriate animations.
     **/
    public MotorState GetMotorState()
    {
        return _MotorState;
    }

    /**
     * Since the motor needs to know the facing of the object, this information is made available to anyone else who might need it.
     **/
    public bool IsFacingLeft()
    {
        return _FacingLeft;
    }

    void FixedUpdate()
    {
        if (_MovementDir.x < -INPUT_THRESHOLD)
        {
            _FacingLeft = true;
        }
        else if (_MovementDir.x > INPUT_THRESHOLD)
        {
            _FacingLeft = false;
        }

        // First, are we trying to dash?
        if (CanDash && _Dashing.Pressed && Time.time >= _Dashing.CanDashAgain)
        {
            _Dashing.IsDashing = true;
            _Dashing.CanDashAgain = Time.time + DashCooldown;
            _Dashing.EndDash = Time.time + DashDuration;
            rigidbody2D.drag = 0;
            rigidbody2D.gravityScale = 0;
            _MotorState = MotorState.Dashing;

            if (ChangeLayerDuringDash)
            {
                gameObject.layer = DashLayer;
                // Changed layers so we dirty the physics with this hack.
                transform.localScale = transform.localScale;
            }

            if (!_Dashing.DashWithDirection)
            {
                if (_FacingLeft)
                {
                    _Dashing.DashDir = -Vector2.right * DashSpeed;
                }
                else
                {
                    _Dashing.DashDir = Vector2.right * DashSpeed;
                }
            }

            if (OnDash != null)
            {
                OnDash();
            }
        }

        _Dashing.Pressed = false;

        if (_Dashing.IsDashing)
        {
            // Dashing is special!
            if (!_Dashing.ForceEnd)
            {
                rigidbody2D.velocity = _Dashing.DashDir;
            }

            if (Time.time >= _Dashing.EndDash || _Dashing.ForceEnd)
            {
                // Done dashing, back to normals.
                _Dashing.ForceEnd = false;
                _Dashing.IsDashing = false;
                rigidbody2D.gravityScale = _InitialGravity;

                if (StuckTo == Surface.Ground)
                {
                    rigidbody2D.drag = _InitialDrag;
                }

                if (ChangeLayerDuringDash)
                {
                    gameObject.layer = _Dashing.OriginalLayer;

                    // Changed layers so we dirty the physics with this hack.
                    transform.localScale = transform.localScale;
                }

                if (OnDashEnd != null)
                {
                    OnDashEnd();
                }
            }
        }
        else
        {
            // Are we grounded?
            SetStuckTo();

            if (StuckTo != Surface.None && rigidbody2D.velocity.y < 0)
            {
                // If we're grounded then we are not jumping.
                _Jumping.IsJumping = false;
            }

            if (StuckTo == Surface.Ground)
            {
                _MotorState = MotorState.OnGround;
            }
            else
            {
                _MotorState = MotorState.InAir;
            }

            // Apply movement if we're not ignoring it.
            if (Time.time >= _IgnoreMovementUntil)
            {
                if (StuckTo == Surface.Ground)
                {
                    // Not in air.
                    rigidbody2D.drag = _InitialDrag;
                    //rigidbody2D.AddForce(_MovementDir * GroundAcceleration);
                    rigidbody2D.velocity += _MovementDir * GroundAcceleration * Time.fixedDeltaTime;
                }
                else
                {
                    rigidbody2D.drag = 0;

                    // Don't apply the force if we're already on the wall.
                    if (_MovementDir.x > 0 && StuckTo == Surface.LeftWall ||
                        _MovementDir.x < 0 && StuckTo == Surface.RightWall ||
                        StuckTo == Surface.None)
                    {
                        rigidbody2D.AddForce(_MovementDir * AirAcceleration);
                    }
                }
            }

            // Wall hug?
            if (AllowWallSlide)
            {
                // Only if we're currently falling.
                if (rigidbody2D.velocity.y < 0 &&
                    (StuckTo == Surface.LeftWall && _MovementDir.x < -INPUT_THRESHOLD ||
                     StuckTo == Surface.RightWall && _MovementDir.x > INPUT_THRESHOLD))
                {
                    // Sticky!
                    Vector2 v = rigidbody2D.velocity;

                    // Set the y to one acceleration tick upwards against gravity.
                    v.y = -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime - WallSlideSpeed;
                    rigidbody2D.velocity = v;
                    _MotorState = MotorState.Sliding;
                }
            }

            // Corner grab?
            if (AllowCornerGrab)
            {
                if (rigidbody2D.velocity.y < 0 || _OnCorner)
                {
                    _OnCorner = false;

                    if (StuckTo == Surface.LeftWall && _MovementDir.x < -INPUT_THRESHOLD ||
                        StuckTo == Surface.RightWall && _MovementDir.x > INPUT_THRESHOLD)
                    {
                        if (CheckIfAtCorner())
                        {
                            // Stick completely!
                            Vector2 v = rigidbody2D.velocity;

                            // Set the y to one acceleration tick upwards against gravity.
                            v.y = -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime;
                            rigidbody2D.velocity = v;
                            _OnCorner = true;
                            _MotorState = MotorState.OnCorner;
                            _Jumping.DoubleJumped = false;
                        }
                    }
                }
            }

            // This is something that the default Unity Controller script does, allows the player to press jump button
            // earlier than would normally be allowed. They say it leads to a more pleasant experience for the
            // user. I'll assume they're on to something.
            if (Time.time > _Jumping.Time + _Jumping.TimeBuffer)
            {
                _Jumping.Pressed = false;
            }

            // If we're currently jumping and the jump button is still held down ignore gravity to allow us to acheive the extra height.
            if (_Jumping.IsJumping && _Jumping.Held)
            {
                if (Time.time < _Jumping.Time + ExtraJumpHeight / CalculateJumpSpeed())
                {
                    // TODO: This might want to be based off of the jump direction (in case of wall jumps) which might give a better
                    //       feel.
                    rigidbody2D.velocity += new Vector2(0, -1 * rigidbody2D.gravityScale * Physics2D.gravity.y * Time.fixedDeltaTime);
                }
            }

            // If our state is not in the air then open up the possibility of double jump (we need to be able to double jump if
            // we walk off an edge so it can't be based of when a jump occured).
            if (_MotorState != MotorState.InAir)
            {
                _Jumping.DoubleJumped = false;
            }

            // Jump?
            if (_Jumping.Pressed)
            {
                bool jumped = true;

                // Jump might mean different things depending on the state.
                if (StuckTo == Surface.Ground || _Jumping.Force)
                {
                    // Normal jump.
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, CalculateJumpSpeed() + _Jumping.ExtraSpeed);
                }
                else if (_OnCorner)
                {
                    // If we are on a corner then jump up.
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, CalculateJumpSpeed());
                    _IgnoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
                }
                else if (AllowWallJump && StuckTo == Surface.LeftWall)
                {
                    // If jump was pressed as we or before we entered the wall then just jump away.
                    rigidbody2D.velocity = _UpRight * CalculateJumpSpeed();

                    // It's likely the player is still pressing into the wall, ignore movement for a little amount of time.
                    // TODO: Only ignore left movement?
                    _IgnoreMovementUntil = Time.time + IGNORE_INPUT_TIME;

                    // If wall jump is allowed but not wall slide then double jump will not be allowed earlier, allow it now.
                    _Jumping.DoubleJumped = false;
                }
                else if (AllowWallJump && StuckTo == Surface.RightWall)
                {
                        rigidbody2D.velocity = _UpLeft * CalculateJumpSpeed();
                        _IgnoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
                        _Jumping.DoubleJumped = false;
                }
                else if (AllowDoubleJump && StuckTo == Surface.None && !_Jumping.DoubleJumped)
                {
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, CalculateJumpSpeed());
                    _Jumping.DoubleJumped = true;
                }
                else
                {
                    // Guess we aren't jumping!
                    jumped = false;
                }

                if (jumped)
                {
                    _Jumping.IsJumping = true;
                    rigidbody2D.drag = 0;
                    _Jumping.Pressed = false;
                    _OnCorner = false;
                    _Jumping.Force = false;

                    if (OnJump != null)
                    {
                        OnJump();
                    }
                }
            }

            // Remove left or right movements if trying to movement into the wall, this 

            // Check speeds.
            ClampVelocity();
        }

        // Reset some things.
        _MovementDir = Vector2.zero;
        _Jumping.Held = false;
    }

    private bool CheckIfAtCorner()
    {
        Bounds box = renderer.bounds;
        Vector2 min = box.min;
        Vector2 max = box.max;

        // New min y is always at the current max y.
        min.y = max.y;
        max.y += CornerHeightCheck;

        if (StuckTo == Surface.LeftWall)
        {
            max.x = min.x;
            min.x -= CornerWidthCheck;
        }
        else if (StuckTo == Surface.RightWall)
        {
            min.x = max.x;
            max.x += CornerWidthCheck;
        }

        Collider2D col = Physics2D.OverlapArea(min, max, CheckMask);

        return col == null;
    }

    private void ClampVelocity()
    {
        Vector2 checkedSpeed = rigidbody2D.velocity;

        if (StuckTo == Surface.Ground)
        {
            checkedSpeed.x = Mathf.Clamp(checkedSpeed.x, -MaxGroundSpeed, MaxGroundSpeed);
        }
        else
        {
            // Check both horizontal air speed and fall speed.
            checkedSpeed.x = Mathf.Clamp(checkedSpeed.x, -MaxAirSpeed, MaxAirSpeed);

            if (checkedSpeed.y < -MaxFallSpeed)
            {
                checkedSpeed.y = -MaxFallSpeed;
            }
        }

        rigidbody2D.velocity = checkedSpeed;
    }

    private void SetStuckTo()
    {
        Bounds box = renderer.bounds;
        Vector2 min = box.min;
        Vector2 max = box.max;

        min.x += TRIM_STUCKTO_NUM;
        max.x -= TRIM_STUCKTO_NUM;

        min.y -= CheckDistance;
        max.y = transform.position.y; // Go ahead and bring the maximum y down.

        // TODO: This requires that a ground layer is set up to work. Consider moving to a set up that will consider all
        //       collisions but ignore the player's collider.

        Collider2D col = Physics2D.OverlapArea(min, max, CheckMask);

        StuckTo = col != null ? Surface.Ground : Surface.None;

        if (StuckTo == Surface.None)
        {
            // Consider possible stuck to left wall if we're pressing into it.
            if (_MovementDir.x < -INPUT_THRESHOLD)
            {
                // How about on the walls for wall jump? Left wall first.
                min = box.min;
                max = box.max;

                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;

                min.x -= CheckDistance;
                max.x = transform.position.x;

                col = Physics2D.OverlapArea(min, max, CheckMask);

                if (col != null)
                {
                    StuckTo = Surface.LeftWall;
                }
            }
            else if (_MovementDir.x > INPUT_THRESHOLD)
            {
                // Now right wall.
                min = box.min;
                max = box.max;

                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;

                min.x = transform.position.x;
                max.x += CheckDistance;

                col = Physics2D.OverlapArea(min, max, CheckMask);

                if (col != null)
                {
                    StuckTo = Surface.RightWall;
                }
            }
        }
    }

    private float CalculateJumpSpeed()
    {
        return Mathf.Sqrt(-2 * BaseJump * rigidbody2D.gravityScale * Physics2D.gravity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (!DrawGizmos)
            return;

        // Ground check.
        Bounds box = renderer.bounds;

        // Ground check box
        Vector2 min = box.min;
        Vector2 max = box.max;
        min.x += TRIM_STUCKTO_NUM;
        max.x -= TRIM_STUCKTO_NUM;
        min.y -= CheckDistance;
        max.y = transform.position.y;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

        // Left check box
        min = box.min;
        max = box.max;
        min.y += TRIM_STUCKTO_NUM;
        max.y -= TRIM_STUCKTO_NUM;
        min.x -= CheckDistance;
        max.x = transform.position.x;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

        // Right check box
        min = box.min;
        max = box.max;
        min.y += TRIM_STUCKTO_NUM;
        max.y -= TRIM_STUCKTO_NUM;
        min.x = transform.position.x;
        max.x += CheckDistance;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

        min = box.min;
        max = box.max;
        min.y = max.y;
        max.y += CornerHeightCheck;
        max.x = min.x;
        min.x -= CornerWidthCheck;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

        min = box.min;
        max = box.max;
        min.y = max.y;
        max.y += CornerHeightCheck;
        min.x = max.x;
        max.x += CornerWidthCheck;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));
    }
}
