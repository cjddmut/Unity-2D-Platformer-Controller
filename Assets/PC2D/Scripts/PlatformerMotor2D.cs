using System;
using PC2D;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerMotor2D : MonoBehaviour
{

    #region Public

    /// <summary>
    /// The maximum speed the motor will move on the ground, only effects horizontal speed.
    /// </summary>
    public float maxGroundSpeed = 3f;
    
    /// <summary>
    /// How much time does it take for the motor to get from zero speed to max speed. This value
    /// is used to calculate the acceleration.
    /// </summary>
    public float timeToMaxGroundSpeed;

    /// <summary>
    /// The distance the motor will slide to a stop from full speed while on the ground.
    /// </summary>
    public float groundStopDistance;

    /// <summary>
    /// The maximum horizontal speed of the motor in the air.
    /// </summary>
    public float maxAirSpeed = 2f;

    /// <summary>
    /// The time it takes to move from zero horizontal speed to the maximum speed. This value is
    /// used to calculate the acceleration.
    /// </summary>
    public float timeToMaxAirSpeed;

    /// <summary>
    /// The distance the motor will 'slide' to a stop while in the air. Only effects horizontal
    /// movement.
    /// </summary>
    public float airStopDistance;

    /// <summary>
    /// The maximum speed that the motor will fall. Only effects vertical speed when falling.
    /// </summary>
    public float maxFallSpeed = 5f;

    /// <summary>
    /// The maximum speed that the motor will fall during 'fast fall'.
    /// </summary>
    public float maxFastFallSpeed = 5f;

    /// <summary>
    /// If the motor is in 'fast fall' then the gravityScale is multiplied by the value. Higher number means
    /// faster acceleration while falling.
    /// </summary>
    public float fastFallGravityMultiplier = 1f;

    /// <summary>
    /// The height the motor will jump when a jump command is issued.
    /// </summary>
    public float baseJumpHeight = 0.5f;

    /// <summary>
    /// The extra height the motor will jump if jump is 'held' down.
    /// </summary>
    public float extraJumpHeight = 0.5f;

    /// <summary>
    /// If double jumps are allowed.
    /// </summary>
    public bool allowDoubleJump = false;

    /// <summary>
    /// If wall jumps are allowed.
    /// </summary>
    public bool allowWallJump = false;

    /// <summary>
    /// The jump speed multiplier when wall jumping. This is useful to force bigger jumps off of the wall.
    /// </summary>
    public float wallJumpMultiplier = 1f;

    /// <summary>
    /// If wall clinging is allowed. A wall cling is when a motor will 'stick' to a wall.
    /// </summary>
    public bool allowWallCling = false;

    /// <summary>
    /// The duration of the wall cling in seconds. Set to a very large number to effectively allow permanent clings.
    /// </summary>
    public float wallClingDuration = 0.5f;

    /// <summary>
    /// If wall slides are allowed. A wall slide is when a motor slides down a wall. This will only take in effect
    /// once the cling is over.
    /// </summary>
    public bool allowWallSlide = false;

    /// <summary>
    /// The speed that the motor will slide down the wall.
    /// </summary>
    public float wallSlideSpeed = 1;

    /// <summary>
    /// Are corner grabs allowed? A corner grab is when the motor sticks to a corner.
    /// </summary>
    public bool allowCornerGrab = false;

    /// <summary>
    /// The jump speed multiplier when jumping from a corner grab. Useful to forcing bigger jumps.
    /// </summary>
    public float cornerJumpMultiplier = 1f;

    /// <summary>
    /// The duraction, in seconds, that the motor will stick to a corner.
    /// </summary>
    public float cornerGrabDuration = 0.5f;

    /// <summary>
    /// This is the size of the corner check. This can be tweaked with if corner grabs are not working correctly.
    /// </summary>
    public float cornerDistanceCheck = 0.1f;

    /// <summary>
    /// Is dashing allowed?
    /// </summary>
    public bool allowDash = false;

    /// <summary>
    /// When the motor will be allowed to dash again after dashing. The cooldown begins at the end of a dash.
    /// </summary>
    public float dashCooldown = 1;

    /// <summary>
    /// How far the motor will dash.
    /// </summary>
    public float dashDistance = 1;

    /// <summary>
    /// How long the dash lasts in seconds.
    /// </summary>
    public float dashDuration = 0.2f;

    /// <summary>
    /// The easing function used during the dash. Pick 'Linear' for just a set speed.
    /// </summary>
    public EasingFunctions.Functions dashEasingFunction = EasingFunctions.Functions.EaseOutQuad;

    /// <summary>
    /// The threshold that normalizedXMovement will have to be higher than to consider wall clings, wall slides, wall jumps,
    /// and corner grabs.
    /// </summary>
    [Range(0f, 1f)]
    public float wallInteractionThreshold = 0.5f;

    /// <summary>
    /// Delegate to attach to when the motor dashes.
    /// </summary>
    public Action onDash;

    /// <summary>
    /// Delegate to attach to when the motor's dash ends.
    /// </summary>
    public Action onDashEnd;

    /// <summary>
    /// Delegate to attach to when the motor jumps.
    /// </summary>
    public Action onJump;

    /// <summary>
    /// How far out the motor will check for the environment mask. This value can be tweaked if jump checks are not firing when 
    /// wanted.
    /// </summary>
    public float checkDistance = 0.025f;

    /// <summary>
    /// The environment check mask. The motor doesn't know what to consider is an environment so this mask tells it.
    /// </summary>
    public LayerMask checkMask;

    /// <summary>
    /// The states the motor can be in.
    /// </summary>
    public enum MotorState
    {
        OnGround,
        Jumping,
        Falling,
        FallingFast,
        Sliding,
        OnCorner,
        Clinging,
        Dashing,
        Frozen
    }

    /// <summary>
    /// Set the x movement direction. This is multiplied by the max speed. -1 is full left, 1 is full right. Higher numbers will 
    /// result in faster speed.
    /// </summary>
    public float normalizedXMovement { get; set; }

    /// <summary>
    /// Set the time scale for the motor. This is independent of the global time scale. Negative values are not supported.
    /// </summary>
    public float timeScale
    {
        get
        {
            return _timeScale;
        }
        set
        {
            _timeScale = value;

            if (_timeScale < 0)
            {
                _timeScale = 0;
            }
        }
    }

    /// <summary>
    /// The velocity of the motor. This should be queried instead of the rigidbody's velocity. Setting this during a dash doesn't
    /// have any meaning.
    /// </summary>
    public Vector2 velocity
    {
        get
        {
            if (motorState == MotorState.Dashing)
            {
                float normalizedTime = _dashing.timeDashed / dashDuration;
                float speed = _dashDerivativeFunction(0, dashDistance, normalizedTime) / dashDuration;
                return _dashing.dashDir * speed;
            }

            return _velocity;
        }
        set
        {
            _velocity = value;
        }
    }

    /// <summary>
    /// Call this to get state information about the motor. This will be information such as if the object is in the air or on the
    /// ground. This can be used to set the appropriate animations.
    /// </summary>
    public MotorState motorState { get; private set; }

    /// <summary>
    /// Since the motor needs to know the facing of the object, this information is made available to anyone else who might need 
    /// it.
    /// </summary>
    public bool facingLeft { get; private set; }

    /// <summary>
    /// Returns the direction of the current dash. If not dashing then returns Vector2.zero.
    /// </summary>
    public Vector2 dashDirection
    {
        get
        {
            if (motorState == MotorState.Dashing)
            {
                return _dashing.dashDir;
            }

            return Vector2.zero;
        }
    }

    /// <summary>
    /// Returns the amount of distance dashed. If not dashing then returns 0.
    /// </summary>
    public float distanceDashed
    {
        get
        {
            if (motorState == MotorState.Dashing)
            {
                return _dashing.distanceDashed;
            }

            return 0;
        }
    }

    /// <summary>
    /// Set this true to have the motor fall faster. Set to false to fall at normal speeds.
    /// </summary>
    public bool fallFast { get; set; }

    /// <summary>
    /// If jumpingHeld is set to true then the motor will jump further. Set to false if jumping isn't 'held'.
    /// </summary>
    public bool jumpingHeld
    {
        get
        {
            return _jumping.held;
        }

        set
        {
            // Since we set held to true on pressed, we only set to false here. This prevent held from being set after a release.
            if (!value)
            {
                _jumping.held = false;
            }

        }
    }

    /// <summary>
    /// Setting frozen to true will put the motor in a 'frozen' state. All information will be saved and set once unfrozen 
    /// (the motor also reduces gravity to 0).
    /// 
    /// Note: This isn't a way to turn off the motor. To turn off the motor, simply set the script to disabled.
    /// </summary>
    public bool frozen
    {
        get
        {
            return _frozen;
        }
        set
        {
            if (!enabled)
            {
                Debug.LogWarning(FROZEN_SET_WHILE_DISABLED);
                return;
            }

            if (_frozen != value)
            {
                _frozen = value;

                if (_frozen)
                {
                    motorState = MotorState.Frozen;
                }
            }
        }
    }

    /// <summary>
    /// Set this to use a specific collider for checks instead of grabbing the collider from gameObject.collider2D.
    /// </summary>
    public Collider2D colliderToUse { get; set; }

    /// <summary>
    /// Call this to have the GameObject try to jump, once called it will be handled in the FixedUpdate tick. The y axis is 
    /// considered jump.
    /// </summary>
    /// <param name="extraHeight">Extra height added to the jump.</param>
    public void Jump(float extraHeight = 0)
    {
        _jumping.pressed = true;
        _jumping.timePressed = Time.time;
        _jumping.isJumping = false;
        _jumping.height = baseJumpHeight + extraHeight;

        // Consider jumping held in case there are multiple fixed ticks before the next update tick.
        // This is useful as jumpingHeld may not be set to true with a GetButton() call.
        _jumping.held = true;
    }

    /// <summary>
    /// This will force a jump to occur even if the motor doesn't think a jump is valid. This function will not work if the motor
    /// is dashing.
    /// </summary>
    /// <param name="extraHeight">Extra height added to the jump.</param>
    public void ForceJump(float extraHeight = 0)
    {
        Jump(extraHeight);
        _jumping.force = true;
    }

    /// <summary>
    /// Call to end a jump. Causes the motor to stop calculated held speed for a jump.
    /// </summary>
    public void EndJump()
    {
        _jumping.pressed = false;
        _jumping.isJumping = false;
        _jumping.timePressed = 0;
    }

    /// <summary>
    /// Resets the state for the a double jump allowing another jump. This doesn't do anything if double jumps aren't allowed.
    /// </summary>
    public void ResetDoubleJump()
    {
        _jumping.doubleJumped = false;
    }

    /// <summary>
    /// Call this to have the GameObject try to dash, once called it will be handled in the FixedUpdate tick.
    /// This causes the object to dash along their facing (if left or right for side scrollers).
    /// </summary>
    public void Dash()
    {
        _dashing.pressed = true;
        _dashing.dashWithDirection = false;
    }

    /// <summary>
    /// Send a direction vector to dash allow dashing in a specific direction.
    /// </summary>
    /// <param name="dir">The normalized direction of the dash.</param>
    public void Dash(Vector2 dir)
    {
        _dashing.pressed = true;
        _dashing.dashWithDirection = true;
        _dashing.dashDir = dir;
    }

    /// <summary>
    /// Call to end dash immediately.
    /// </summary>
    public void EndDash()
    {
        // If dashing then end now.
        if (_dashing.isDashing)
        {
            _dashing.isDashing = false;
            _dashing.cooldownTimer = dashCooldown;
            _dashing.pressed = false;

            float normalizedTime = _dashing.timeDashed / dashDuration;
            float speed = _dashDerivativeFunction(0, dashDistance, normalizedTime) / dashDuration;

            // Some of the easing functions may result in infinity, we'll uh, lower our expectations and make it maxfloat.
            // This will almost certainly be clamped.
            if (float.IsNegativeInfinity(speed))
            {
                speed = float.MinValue;
            }
            else if (float.IsPositiveInfinity(speed))
            {
                speed = float.MaxValue;
            }

            _velocity = _dashing.dashDir * speed;

            if (onDashEnd != null)
            {
                onDashEnd();
            }
        }
    }

    #endregion

    #region Private

    private Vector2 _upRight;
    private Vector2 _upLeft;
    private float _originalDrag;
    private float _originalGravity;
    private float _ignoreMovementUntil;
    private bool _frozen;
    private Rigidbody2D _rigidbody2D;
    private Vector2 _velocity;
    private float _timeScale = 1;
    private Collider2D _collider2D;

    private CollidedSurface _collidingAgainst = CollidedSurface.None;
    private enum CollidedSurface
    {
        None,
        Ground,
        LeftWall,
        RightWall
    }

    // The function is cached to avoid unnecessary memory allocation.
    private EasingFunctions.EasingFunc _dashFunction;
    private EasingFunctions.EasingFunc _dashDerivativeFunction;

    // This is stored to notice if the public field changes during runtime.
    private EasingFunctions.Functions _currentDashEasingFunction;

    // Contains the various jump variables, this is for organization.
    private class JumpState
    {
        public bool isJumping;
        public bool pressed;
        public bool held;
        public bool doubleJumped;

        public float timePressed;
        public float allowExtraDuration;

        public bool force;
        public float height;

        public bool ignoreGravity;

        // Amount of time that a jump can be triggered, same as the default unity controller script.
        public const float TIME_BUFFER = 0.2f;
    }
    private JumpState _jumping = new JumpState();

    // Contains the various dash variables.
    private class DashState
    {
        public bool isDashing;
        public bool pressed;
        public float cooldownTimer;
        public float timeDashed;
        public bool dashWithDirection;
        public Vector2 dashDir = Vector2.zero;
        public float distanceDashed;
    }
    private DashState _dashing = new DashState();

    // Contains information for wall clings, slides, and corner grabs.
    private class WallState
    {
        public bool onCorner;
        public float cornerHangTime;

        public bool sliding;

        public bool clinging;
        public float clingTime;

        public bool canHangAgain = true;
    }
    private WallState _wallInfo = new WallState();

    // This seems to be the magic number where you won't collide with the "ground" by being on the wall and not be able to sit on a corner
    // without colliding with the ground.
    private const float TRIM_STUCKTO_NUM = 0.01425f;

    // When jumping off of a wall, this is the amount of time that movement input is ignored.
    private const float IGNORE_INPUT_TIME = 0.2f;

    private const string CHECK_MASK_NOT_SET = 
        "PC2D: Environment Check Mask not set! This is needed to know what to collide against!";

    private const string FROZEN_SET_WHILE_DISABLED = "PC2D: PlatformerMotor2D.frozen set when motor is disabled, ignoring.";

    private void Awake()
    {
        _upRight = Vector2.up + Vector2.right;
        _upRight.Normalize();
        _upLeft = new Vector2(-_upRight.x, _upRight.y);

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();
        _originalDrag = _rigidbody2D.drag;
        _originalGravity = _rigidbody2D.gravityScale;

        SetDashFunctions();
    }

    private void Start()
    {
        if (checkMask == 0)
        {
            Debug.LogError(CHECK_MASK_NOT_SET);
        }
    }

    private void OnDestroy()
    {
        onDash = null;
        onDashEnd = null;
        onJump = null;
    }

    private void OnEnable()
    {
        // The motor manually moves the rigidbody and does not rely on gravity, drag, or its velocity.
        _rigidbody2D.drag = 0;
        _rigidbody2D.gravityScale = 0;
        _velocity = _rigidbody2D.velocity;
        _rigidbody2D.velocity = Vector2.zero;
    }

    private void OnDisable()
    {
        _rigidbody2D.gravityScale = _originalGravity;
        _rigidbody2D.drag = _originalDrag;
        _rigidbody2D.velocity = _velocity;
    }

    private void FixedUpdate()
    {
        // Frozen?
        if (frozen)
        {
            return;
        }

        // Timers
        _dashing.cooldownTimer -= GetDeltaTime();

        // First, are we trying to dash?
        if (allowDash && 
            _dashing.pressed && 
            _dashing.cooldownTimer <= 0 &&
            !_dashing.isDashing)
        {
            StartDash();
        }

        _dashing.pressed = false;

        if (_dashing.isDashing)
        {
            // We are dashing.
            HandleDash();
        }
        else
        {
            // If we have standard control then facing can change any frame.
            SetFacing();

            // Are we grounded?
            CheckSurroundings();

            if (_collidingAgainst == CollidedSurface.Ground)
            {
                motorState = MotorState.OnGround;

                // Turn off gravity, this prevents losing some velocity on every tick.
                _velocity.y = 0;
            }
            else if (_velocity.y <= 0)
            {
                motorState = MotorState.Falling;
            }

            // Apply movement if we're not ignoring it.
            if (Time.time >= _ignoreMovementUntil)
            {
                ApplyMovement();
            }

            // Handle jumping.
            HandleJumping();

            // Finally, any wall interactions.
            HandleWallInteraction();

            // If we are falling fast then multiply the gravityScale.
            if (IsInAir() && !_jumping.ignoreGravity)
            {
                if (fallFast)
                {
                    _velocity.y += _originalGravity *
                                   fastFallGravityMultiplier *
                                   GetDeltaTime() *
                                   Physics2D.gravity.y;

                    motorState = MotorState.FallingFast;
                }
                else
                {
                    _velocity.y += _originalGravity *
                                   GetDeltaTime() *
                                   Physics2D.gravity.y;
                }
            }

            if (_velocity.y > 0 && IsTouchingCeiling())
            {
                _velocity.y = 0;
                Bounds b = _collider2D.bounds;
                Debug.DrawLine(b.min, new Vector3(b.min.x, b.max.y), Color.red, 34234, false);
            }

            _rigidbody2D.MovePosition(_rigidbody2D.position + _velocity * GetDeltaTime());

            // Check speeds.
            ClampVelocity();
        }
    }

    private bool IsTouchingCeiling()
    {
        Bounds box;

        if (colliderToUse != null)
        {
            box = colliderToUse.bounds;
        }
        else
        {
            box = _collider2D.bounds;
        }

        Vector2 min = box.min;
        Vector2 max = box.max;

        min.x += TRIM_STUCKTO_NUM;
        max.x -= TRIM_STUCKTO_NUM;

        max.y += checkDistance;
        min.y = transform.position.y;

        return Physics2D.OverlapArea(min, max, checkMask) != null;
    }

    private bool IsInAir()
    {
        return motorState == MotorState.Jumping || 
            motorState == MotorState.Falling ||
               motorState == MotorState.FallingFast;
    }


    private float GetDeltaTime()
    {
        return Time.fixedDeltaTime * timeScale;
    }

    private void SetDashFunctions()
    {
        _dashFunction = EasingFunctions.GetEasingFunction(dashEasingFunction);
        _dashDerivativeFunction = EasingFunctions.GetEasingFunctionDerivative(dashEasingFunction);
        _currentDashEasingFunction = dashEasingFunction;
    }

    private void HandleJumping()
    {
        if (_collidingAgainst != CollidedSurface.None && _velocity.y <= 0)
        {
            // If we're grounded then we are not jumping.
            _jumping.isJumping = false;
        }

        // This is something that the default Unity Controller script does, allows the player to press jump button
        // earlier than would normally be allowed. They say it leads to a more pleasant experience for the
        // user. I'll assume they're on to something.
        if (Time.time > _jumping.timePressed + JumpState.TIME_BUFFER)
        {
            _jumping.pressed = false;
        }

        _jumping.ignoreGravity = false;

        // If we're currently jumping and the jump button is still held down ignore gravity to allow us to achieve the extra 
        // height.
        if (_jumping.isJumping && _jumping.held)
        {
            if (_jumping.allowExtraDuration > 0)
            {
                _jumping.allowExtraDuration -= GetDeltaTime();
                _jumping.ignoreGravity = true;
            }
        }

        // If our state is not in the air then open up the possibility of double jump (we need to be able to double jump if
        // we walk off an edge so it can't be based of when a jump occurred).
        if (!IsInAir())
        {
            _jumping.doubleJumped = false;
        }

        // Jump?
        if (_jumping.pressed)
        {
            bool jumped = true;

            // Jump might mean different things depending on the state.
            if (_collidingAgainst == CollidedSurface.Ground || _jumping.force)
            {
                // Normal jump.
                _velocity.y = CalculateSpeedNeeded(_jumping.height);
            }
            else if (_wallInfo.onCorner)
            {
                // If we are on a corner then jump up.
                _velocity = Vector2.up * CalculateSpeedNeeded(_jumping.height) * cornerJumpMultiplier;
                _ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
            }
            else if (allowWallJump && _collidingAgainst == CollidedSurface.LeftWall)
            {
                // If jump was pressed as we or before we entered the wall then just jump away.
                _velocity = _upRight * CalculateSpeedNeeded(_jumping.height) * wallJumpMultiplier;

                // It's likely the player is still pressing into the wall, ignore movement for a little amount of time.
                // TODO: Only ignore left movement?
                _ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;

                // If wall jump is allowed but not wall slide then double jump will not be allowed earlier, allow it now.
                _jumping.doubleJumped = false;
            }
            else if (allowWallJump && _collidingAgainst == CollidedSurface.RightWall)
            {
                _velocity = _upLeft * CalculateSpeedNeeded(_jumping.height) * wallJumpMultiplier;
                _ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
                _jumping.doubleJumped = false;
            }
            else if (allowDoubleJump && _collidingAgainst == CollidedSurface.None && !_jumping.doubleJumped)
            {
                _velocity.y = CalculateSpeedNeeded(_jumping.height);
                _jumping.doubleJumped = true;
            }
            else
            {
                // Guess we aren't jumping!
                jumped = false;
            }

            if (jumped)
            {
                _jumping.isJumping = true;
                _jumping.pressed = false;
                _wallInfo.onCorner = false;
                _wallInfo.sliding = false;
                _wallInfo.clinging = false;
                _jumping.force = false;
                _jumping.allowExtraDuration = extraJumpHeight / CalculateSpeedNeeded(_jumping.height);
                motorState = MotorState.Jumping;

                if (onJump != null)
                {
                    onJump();
                }
            }
        }
    }

    private void HandleWallInteraction()
    {
        // We can grab corners or walls again.
        if (_collidingAgainst != CollidedSurface.RightWall && _collidingAgainst != CollidedSurface.LeftWall)
        {
            _wallInfo.canHangAgain = true;
        }

        if (fallFast || _jumping.isJumping)
        {
            // If we're being told to fall fast then we won't allow any wall interactions. Ditto on if we're jumping.
            return;
        }

        // Corner grab?
        if (allowCornerGrab)
        {
            if (_velocity.y < 0 || _wallInfo.onCorner)
            {
                if ((_collidingAgainst == CollidedSurface.LeftWall && normalizedXMovement < -wallInteractionThreshold ||
                    _collidingAgainst == CollidedSurface.RightWall && normalizedXMovement > wallInteractionThreshold) &&
                    CheckIfAtCorner())
                {
                    if (!_wallInfo.onCorner && _wallInfo.canHangAgain)
                    {
                        _wallInfo.onCorner = true;
                        _wallInfo.canHangAgain = false;
                        _wallInfo.cornerHangTime = Time.time + cornerGrabDuration;
                    }

                    if (_wallInfo.onCorner && Time.time >= _wallInfo.cornerHangTime)
                    {
                        _wallInfo.onCorner = false;
                    }

                    if (_wallInfo.onCorner)
                    {
                        // We're stuck!
                        _velocity = Vector2.zero;
                        motorState = MotorState.OnCorner;
                        return;
                    }
                }
                else
                {
                    _wallInfo.onCorner = false;
                }
            }
        }

        // Wall Cling
        if (allowWallCling)
        {
            if (_velocity.y < 0 || _wallInfo.clinging)
            {
                if (_collidingAgainst == CollidedSurface.LeftWall && normalizedXMovement < -wallInteractionThreshold ||
                _collidingAgainst == CollidedSurface.RightWall && normalizedXMovement > wallInteractionThreshold)
                {
                    if (!_wallInfo.clinging && _wallInfo.canHangAgain)
                    {
                        _wallInfo.clinging = true;
                        _wallInfo.canHangAgain = false;
                        _wallInfo.clingTime = Time.time + wallClingDuration;
                    }

                    if (_wallInfo.clinging && Time.time >= _wallInfo.clingTime)
                    {
                        _wallInfo.clinging = false;
                    }

                    if (_wallInfo.clinging)
                    {
                        // Sticky!
                        _velocity = Vector2.zero;
                        motorState = MotorState.Clinging;
                        return;
                    }
                }
                else
                {
                    _wallInfo.clinging = false;
                }
            }
        }

        // Wall slide?
        if (allowWallSlide)
        {
            if (_velocity.y < 0 || _wallInfo.sliding)
            {
                _wallInfo.sliding = false;

                // Only if we're currently falling.
                if (_collidingAgainst == CollidedSurface.LeftWall && normalizedXMovement < -wallInteractionThreshold ||
                    _collidingAgainst == CollidedSurface.RightWall && normalizedXMovement > wallInteractionThreshold)
                {
                    _velocity = -Vector2.up * wallSlideSpeed;
                    motorState = MotorState.Sliding;
                    _wallInfo.sliding = true;
                }
            }
        }
    }

    private void ApplyMovement()
    {
        if (Mathf.Abs(normalizedXMovement) > 0)
        {
            if (_collidingAgainst == CollidedSurface.Ground)
            {
                if (timeToMaxGroundSpeed > 0)
                {
                    // If we're moving faster than our normalizedXMovement * maxGroundSpeed then decelerate rather than 
                    // accelerate.
                    if (_velocity.x > 0 &&
                        normalizedXMovement > 0 &&
                        _velocity.x > normalizedXMovement * maxGroundSpeed ||
                        _velocity.x < 0 &&
                        normalizedXMovement < 0 &&
                        _velocity.x < normalizedXMovement * maxGroundSpeed)
                    {
                        Decelerate(
                            (maxGroundSpeed * maxGroundSpeed) / (2 * groundStopDistance), 
                            normalizedXMovement * maxGroundSpeed);
                    }
                    else
                    {
                        Accelerate(
                            normalizedXMovement * (maxGroundSpeed / timeToMaxGroundSpeed), 
                            normalizedXMovement * maxGroundSpeed);
                    }
                }
                else
                {
                    // We can overwrite y if we're on the ground, it's fine.
                    _velocity = Vector2.right * normalizedXMovement * maxGroundSpeed;
                }
            }
            else
            {
                // Don't apply the force if we're already on the wall.
                if (normalizedXMovement > 0 && _collidingAgainst == CollidedSurface.LeftWall ||
                    normalizedXMovement < 0 && _collidingAgainst == CollidedSurface.RightWall ||
                    _collidingAgainst == CollidedSurface.None)
                {
                    if (timeToMaxAirSpeed > 0)
                    {
                        if (_velocity.x > 0 &&
                            normalizedXMovement > 0 &&
                            _velocity.x > normalizedXMovement * maxAirSpeed ||
                            _velocity.x < 0 &&
                            normalizedXMovement < 0 &&
                            _velocity.x < normalizedXMovement * maxAirSpeed)
                        {
                            Decelerate(
                                (maxAirSpeed * maxAirSpeed) / (2 * airStopDistance), 
                                normalizedXMovement * maxAirSpeed);
                        }
                        else
                        {
                            Accelerate(
                                normalizedXMovement * (maxAirSpeed / timeToMaxAirSpeed), 
                                normalizedXMovement * maxAirSpeed);
                        }
                    }
                    else
                    {
                        _velocity.x = normalizedXMovement * maxAirSpeed;
                    }
                }
            }
        }
        else if (_velocity.x != 0)
        {
            if (_collidingAgainst == CollidedSurface.Ground)
            {
                if (groundStopDistance > 0)
                {
                    Decelerate((maxGroundSpeed * maxGroundSpeed) / (2 * groundStopDistance), 0);
                }
                else
                {
                    _velocity = Vector2.zero;
                }
            }
            else
            {
                if (airStopDistance > 0)
                {
                    Decelerate((maxAirSpeed * maxAirSpeed) / (2 * airStopDistance), 0);
                }
                else
                {
                    _velocity.x = 0;
                }
            }
        }
    }

    private void Accelerate(float acceleration, float limit)
    {
        // acceleration can be negative or positive to note acceleration in that direction.
        _velocity.x += acceleration * GetDeltaTime();

        if (acceleration > 0)
        {
            if (_velocity.x > limit)
            {
                _velocity.x = limit;
            }
        }
        else
        {
            if (_velocity.x < limit)
            {
                _velocity.x = limit;
            }
        }
    }
    
    private void Decelerate(float deceleration, float limit)
    {
        // deceleration is always positive but assumed to take the velocity backwards.
        if (_velocity.x < 0)
        {
            _velocity.x += deceleration * GetDeltaTime();

            if (_velocity.x > limit)
            {
                _velocity.x = limit;
            }
        }
        else if (_velocity.x > 0)
        {
            _velocity.x -= deceleration * GetDeltaTime();

            if (_velocity.x < limit)
            {
                _velocity.x = limit;
            }
        }
    }

    private void StartDash()
    {
        _dashing.isDashing = true;

        // Set facing now and it won't be set again during dash.
        SetFacing();

        if (!_dashing.dashWithDirection)
        {
            // We dash depending on our direction.
            _dashing.dashDir = facingLeft ? -Vector2.right : Vector2.right;
        }

        // This will begin the dash this frame.
        _dashing.timeDashed = GetDeltaTime();
        _dashing.distanceDashed = 0;

        motorState = MotorState.Dashing;

        if (onDash != null)
        {
            onDash();
        }
    }

    private void HandleDash()
    {
        _dashing.timeDashed = Mathf.Clamp(_dashing.timeDashed + GetDeltaTime(), 0f, dashDuration);
        float normalizedTime = _dashing.timeDashed / dashDuration;

        if (_currentDashEasingFunction != dashEasingFunction)
        {
            // This allows the easing function to change during runtime and cut down on unnecessary allocations.
            SetDashFunctions();
        }

        float distance = _dashFunction(0, dashDistance, normalizedTime);
        _rigidbody2D.MovePosition(_rigidbody2D.position + _dashing.dashDir * (distance - _dashing.distanceDashed));

        _dashing.distanceDashed = distance;

        if (_dashing.timeDashed >= dashDuration)
        {
            EndDash();
        }
    }

    private void SetFacing()
    {
        if (normalizedXMovement < 0)
        {
            facingLeft = true;
        }
        else if (normalizedXMovement > 0)
        {
            facingLeft = false;
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
            box = GetComponent<Collider2D>().bounds;
        }

        Vector2 min = box.min;
        Vector2 max = box.max;

        // New min y is always at the current max y.
        min.y = max.y;
        max.y += cornerDistanceCheck;

        if (_collidingAgainst == CollidedSurface.LeftWall)
        {
            max.x = min.x;
            min.x -= cornerDistanceCheck;
        }
        else if (_collidingAgainst == CollidedSurface.RightWall)
        {
            min.x = max.x;
            max.x += cornerDistanceCheck;
        }

        Collider2D col = Physics2D.OverlapArea(min, max, checkMask);

        return col == null;
    }

    private void ClampVelocity()
    {
        if (_collidingAgainst == CollidedSurface.Ground)
        {
            _velocity.x = Mathf.Clamp(_velocity.x, -maxGroundSpeed, maxGroundSpeed);
        }
        else
        {
            // Check both horizontal air speed and fall speed.
            _velocity.x = Mathf.Clamp(_velocity.x, -maxAirSpeed, maxAirSpeed);

            float cappedFallSpeed;

            if (fallFast)
            {
                cappedFallSpeed = -maxFastFallSpeed;

                if (_velocity.y < cappedFallSpeed)
                {
                    _velocity.y = cappedFallSpeed;
                }
            }
            else
            {
                cappedFallSpeed = -maxFallSpeed;

                if (_velocity.y < cappedFallSpeed)
                {
                    _velocity.y = cappedFallSpeed;
                }
            }
        }
    }

    private void CheckSurroundings()
    {
        Bounds box;

        if (colliderToUse != null)
        {
            box = colliderToUse.bounds;
        }
        else
        {
            box = _collider2D.bounds;
        }

        Vector2 min = box.min;
        Vector2 max = box.max;

        // TODO: This requires that a ground layer is set up to work. Consider moving to a set up that will consider all
        //       collisions but ignore the player's collider.

        _collidingAgainst = CollidedSurface.None;

        if (_velocity.y <= 0)
        {
            min.x += TRIM_STUCKTO_NUM;
            max.x -= TRIM_STUCKTO_NUM;

            min.y -= checkDistance;
            max.y = transform.position.y; // Go ahead and bring the maximum y down.

            if (Physics2D.OverlapArea(min, max, checkMask) != null)
            {
                _collidingAgainst = CollidedSurface.Ground;
            }
        }
        
        if (_collidingAgainst == CollidedSurface.None)
        {
            // Only consider the walls if we're pressing into them.
            if (normalizedXMovement < 0)
            {
                min = box.min;
                max = box.max;

                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;

                min.x -= checkDistance;
                max.x = transform.position.x;

                if (Physics2D.OverlapArea(min, max, checkMask) != null)
                {
                    _collidingAgainst = CollidedSurface.LeftWall;
                }
            }
            else if (normalizedXMovement > 0)
            {
                min = box.min;
                max = box.max;

                min.y += TRIM_STUCKTO_NUM;
                max.y -= TRIM_STUCKTO_NUM;

                min.x = transform.position.x;
                max.x += checkDistance;

                if (Physics2D.OverlapArea(min, max, checkMask) != null)
                {
                    _collidingAgainst = CollidedSurface.RightWall;
                }
            }
        }
    }

    private float CalculateSpeedNeeded(float height)
    {
        return Mathf.Sqrt(-2 * height * _originalGravity * Physics2D.gravity.y);
    }

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
            box = GetComponent<Collider2D>().bounds;
        }

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

        // Show the distance that it will take for the motor to stop on the ground and air.
        Vector2 from = new Vector2(box.max.x, box.min.y);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(from, from + Vector2.right * groundStopDistance);

        from = box.max;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(from, from + Vector2.right * airStopDistance);
    }

    #endregion
}
