using System;
using System.Collections;
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
    /// If true, then the player can change x direction while jumping. If false, then
    /// the x velocity when leaving the ground will be held while in the air 
    /// </summary>
    public bool changeDirectionInAir = true;

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
    /// Number of air jumps allowed.
    /// </summary>
    public int numAirJumps = 0;

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
    /// The duration, in seconds, that the motor will stick to a corner.
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
    /// Delay (in seconds) before gravity is turned back on after a dash.
    /// </summary>
    public float endDashDelay;

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
    /// Delegate to attach to when the motor jumps (ALL JUMPS!).
    /// </summary>
    public Action onJump;

    /// <summary>
    /// Delegate to attach to when the motor air jumps (called before onJump).
    /// </summary>
    public Action onAirJump;

    /// <summary>
    /// Delegate to attach to when the motor walls jumps (called before onJump). The vector passed is the normal of the wall.
    /// </summary>
    public Action<Vector2> onWallJump;

    /// <summary>
    /// Delegate to attach to when the motor corner jumps (called before onJump).
    /// </summary>
    public Action onCornerJump;

    /// <summary>
    /// Delegate that notifies that the motor has landed. amountFallen can be queried for distance fallen.
    /// </summary>
    public Action onLanded;

    /// <summary>
    /// How far out the motor will check for the environment mask. This value can be tweaked if jump checks are not firing when 
    /// wanted.
    /// </summary>
    public float checkDistance = 0.025f;

    /// <summary>
    /// The environment check mask. The motor doesn't know what to consider is an environment so this mask tells it.
    /// </summary>
    public LayerMask staticEnvironmentLayerMask;

    /// <summary>
    /// The layer that contains moving platforms.
    /// </summary>
    public LayerMask movingPlatformLayerMask;

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
    /// The surfaces the motor may be colliding against.
    /// </summary>
    [Flags]
    public enum CollidedSurface
    {
        None = 0x0,
        Ground = 0x1,
        LeftWall = 0x2,
        RightWall = 0x4,
        Ceiling = 0x8
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
    /// The surfaces the motor is currently colliding against.
    /// </summary>
    public CollidedSurface collidingAgainst { get; private set; }

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
    /// If the motor is currently able to dash.
    /// </summary>
    public bool canDash
    {
        get { return _dashing.cooldownTimer <= 0; }
    }

    /// <summary>
    /// Returns the amount of distance the motor has fallen fast.
    /// </summary>
    public float amountFastFallen { get; private set; }

    /// <summary>
    /// Returns the amount of distance the motor has fallen. Includes fallen fast distance.
    /// </summary>
    public float amountFallen { get; private set; }

    /// <summary>
    /// Returns the amount the motor has jumped. This ceases to keep calculating after the motor starts to come down.
    /// </summary>
    public float amountJumpedFor { get; private set; }

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
    public void Jump()
    {
        _jumping.pressed = true;
        _jumping.timePressed = Time.time;
        _jumping.height = baseJumpHeight;

        // Consider jumping held in case there are multiple fixed ticks before the next update tick.
        // This is useful as jumpingHeld may not be set to true with a GetButton() call.
        _jumping.held = true;
    }

    /// <summary>
    /// Jump that allows a custom height.
    /// </summary>
    /// <param name="customHeight">The height the motor should jump to. The extraJumpHeight is still applicable.</param>
    public void Jump(float customHeight)
    {
        Jump();
        _jumping.height = customHeight;
    }

    /// <summary>
    /// This will force a jump to occur even if the motor doesn't think a jump is valid. This function will not work if the motor
    /// is dashing.
    /// </summary>
    public void ForceJump()
    {
        Jump();
        _jumping.force = true;
    }

    /// <summary>
    /// Force a jump with a custom height.
    /// </summary>
    /// <param name="customHeight">The height the motor should jump to. The extraJumpHeight is still applicable.</param>
    public void ForceJump(float customHeight)
    {
        ForceJump();
        _jumping.height = customHeight;
    }

    /// <summary>
    /// Call to end a jump. Causes the motor to stop calculated held speed for a jump.
    /// </summary>
    public void EndJump()
    {
        _jumping.pressed = false;
        motorState = MotorState.Falling;
        _jumping.timePressed = 0;
        _jumping.numAirJumps = 0;
    }

    /// <summary>
    /// Resets the state for air jumps by setting the counter to 0.
    /// </summary>
    public void ResetAirJump()
    {
        _jumping.numAirJumps = 0;
    }

    /// <summary>
    /// Call this to have the motor try to dash, once called it will be handled in the FixedUpdate tick.
    /// This causes the object to dash along their facing (if left or right for side scrollers).
    /// </summary>
    public void Dash()
    {
        _dashing.pressed = true;
        _dashing.dashWithDirection = false;
    }

    /// <summary>
    /// Forces the motor to dash regardless if the motor thinks it is valid or not.
    /// </summary>
    public void ForceDash()
    {
        Dash();
        _dashing.force = true;
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
    /// Forces a dash along a specified direction.
    /// </summary>
    /// <param name="dir">The normalized direction of the dash.</param>
    public void ForceDash(Vector2 dir)
    {
        Dash(dir);
        _dashing.force = true;
    }

    /// <summary>
    /// Call to end dash immediately.
    /// </summary>
    public void EndDash()
    {
        // If dashing then end now.
        if (motorState == MotorState.Dashing)
        {
            _dashing.cooldownTimer = dashCooldown;
            _dashing.pressed = false;
            _dashing.gravityEnabledTimer = endDashDelay;

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

            if (IsGrounded())
            {
                motorState = MotorState.OnGround;
            }
            else
            {
                motorState = MotorState.Falling;
            }

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
    private Vector2 _previousLoc;
    private Collider2D[] _collidersUpAgainst = new Collider2D[DIRECTIONS_CHECKED];

    // The function is cached to avoid unnecessary memory allocation.
    private EasingFunctions.EasingFunc _dashFunction;
    private EasingFunctions.EasingFunc _dashDerivativeFunction;

    // This is stored to notice if the public field changes during runtime.
    private EasingFunctions.Functions _currentDashEasingFunction;

    // Contains the various jump variables, this is for organization.
    private class JumpState
    {
        public bool pressed;
        public bool held;
        public int numAirJumps;

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
        public bool pressed;
        public float cooldownTimer;
        public float timeDashed;
        public bool dashWithDirection;
        public Vector2 dashDir = Vector2.zero;
        public float distanceCalculated;
        public float distanceDashed;
        public bool force;
        public float gravityEnabledTimer;
        public Vector2 previousLoc;
    }
    private DashState _dashing = new DashState();

    // Contains information for wall clings, slides, and corner grabs.
    private class WallState
    {
        public float cornerHangTime;
        public float clingTime;

        public bool canHangAgain = true;
    }
    private WallState _wallInfo = new WallState();

    private class MovingPlatformState
    {
        public Rigidbody2D platform;
        public Vector2 previousPos;
        public CollidedSurface stuckToWall;
        public bool isOnPlatform { get { return platform != null; } }
    }
    private MovingPlatformState _movingPlatformState = new MovingPlatformState();

    // This seems to be the magic number where you won't collide with the "ground" by being on the wall and not be able to sit on a corner
    // without colliding with the ground.
    private const float TRIM_STUCKTO_NUM = 0.01425f;

    // When jumping off of a wall, this is the amount of time that movement input is ignored.
    private const float IGNORE_INPUT_TIME = 0.2f;

    private const int DIRECTIONS_CHECKED = 4;
    private const int DIRECTION_DOWN = 0;
    private const int DIRECTION_UP = 1;
    private const int DIRECTION_LEFT = 2;
    private const int DIRECTION_RIGHT = 3;

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
        if (staticEnvironmentLayerMask == 0)
        {
            Debug.LogError(CHECK_MASK_NOT_SET);
        }

        _previousLoc = _rigidbody2D.position;
        motorState = MotorState.Falling;
        StartCoroutine(AfterPhysicsTick());
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

    private void Update()
    {
        // Update allows the motor to update information based on what actually happened in the update tick. If you want up to
        // date information before render then it is safest to query in LateUpdate.

        if (motorState == MotorState.Dashing)
        {
            _dashing.distanceDashed += (_rigidbody2D.position - _dashing.previousLoc).magnitude;
            _dashing.previousLoc = _rigidbody2D.position;
        }
    }

    private IEnumerator AfterPhysicsTick()
    {
        while (true)
        {
            collidingAgainst = CheckSurroundings();

            if (motorState == MotorState.Dashing && _dashing.timeDashed >= dashDuration)
            {
                EndDash();
            }

            if (HasFlag(CollidedSurface.Ground))
            {
                if ((motorState == MotorState.Falling ||
                    motorState == MotorState.FallingFast) &&
                    onLanded != null)
                {
                    onLanded();
                }

                _velocity.y = 0;
            }

            UpdateDistancesAndJump();

            HandlePostWallInteraction();

            // If our state is not in the air then open up the possibility of air jumps (we need to be able to air jump if
            // we walk off an edge so it can't be based of when a jump occurred).
            if (!IsInAir())
            {
                _jumping.numAirJumps = 0;
            }

            if (_velocity.y > 0 && HasFlag(CollidedSurface.Ceiling))
            {
                _velocity.y = 0;
            }

            if (IsGrounded())
            {
                motorState = MotorState.OnGround;
            }
            else if (motorState == MotorState.OnGround)
            {
                motorState = MotorState.Falling;
            }

            AttachToMovingPlatforms();

            if (_movingPlatformState.isOnPlatform)
            {
                _movingPlatformState.previousPos = _movingPlatformState.platform.position;
            }

            yield return new WaitForFixedUpdate();
        }
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
        _dashing.gravityEnabledTimer -= GetDeltaTime();
        _wallInfo.cornerHangTime -= GetDeltaTime();
        _wallInfo.clingTime -= GetDeltaTime();

        // First, are we trying to dash?
        if (allowDash &&
            _dashing.pressed &&
            (_dashing.cooldownTimer <= 0 || _dashing.force) &&
            motorState != MotorState.Dashing)
        {
            StartDash();
        }

        _dashing.pressed = false;

        if (motorState == MotorState.Dashing)
        {
            // We are dashing.
            HandleDash();
        }
        else
        {
            // Update location if on a moving platform.
            if (_movingPlatformState.isOnPlatform)
            {
                Vector2 toNewPos = _movingPlatformState.platform.position - _movingPlatformState.previousPos;
                _rigidbody2D.position += toNewPos;
            }

            // If we have standard control then facing can change any frame.
            SetFacing();

            // Apply movement if we're not ignoring it.
            if (Time.time >= _ignoreMovementUntil)
            {
                ApplyMovement();
            }

            // Handle jumping.
            HandlePreJumping();

            // Finally, any wall interactions.
            HandlePreWallInteraction();

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
                    if (_dashing.gravityEnabledTimer <= 0)
                    {
                        _velocity.y += _originalGravity *
                                       GetDeltaTime() *
                                       Physics2D.gravity.y;
                    }
                }
            }

            // Check speeds.
            ClampFallSpeed();

            _previousLoc = _rigidbody2D.position;
            _rigidbody2D.MovePosition(_rigidbody2D.position + _velocity * GetDeltaTime());
        }
    }

    private void UpdateDistancesAndJump()
    {
        float diffInPositions = (_rigidbody2D.position - _previousLoc).magnitude;

        if (motorState == MotorState.Falling ||
            motorState == MotorState.FallingFast)
        {
            amountFallen += diffInPositions;

            if (motorState == MotorState.FallingFast && _velocity.y <= -maxFallSpeed)
            {
                amountFastFallen += diffInPositions;
            }
            else
            {
                amountFastFallen = 0;
            }
        }
        else
        {
            amountFallen = 0;
        }

        // Jumps
        if (motorState == MotorState.Jumping && _velocity.y <= 0)
        {
            motorState = MotorState.Falling;
        }

        if (motorState == MotorState.Jumping)
        {
            amountJumpedFor += diffInPositions;
        }
    }

    private bool HasFlag(CollidedSurface cs)
    {
        return (collidingAgainst & cs) != CollidedSurface.None;
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

    private void AttachToMovingPlatforms()
    {
        _movingPlatformState.platform = null;
        _movingPlatformState.stuckToWall = CollidedSurface.None;

        if (HasFlag(CollidedSurface.Ground) && IsMovingPlatform(_collidersUpAgainst[DIRECTION_DOWN].gameObject))
        {
            _movingPlatformState.platform = _collidersUpAgainst[DIRECTION_DOWN].GetComponent<Rigidbody2D>();
        }
        else if (PressingIntoLeftWall() && IsMovingPlatform(_collidersUpAgainst[DIRECTION_LEFT].gameObject))
        {
            _movingPlatformState.platform = _collidersUpAgainst[DIRECTION_LEFT].GetComponent<Rigidbody2D>();
            _movingPlatformState.stuckToWall = CollidedSurface.LeftWall;
        }
        else if (PressingIntoRightWall() && IsMovingPlatform(_collidersUpAgainst[DIRECTION_RIGHT].gameObject))
        {
            _movingPlatformState.platform = _collidersUpAgainst[DIRECTION_RIGHT].GetComponent<Rigidbody2D>();
            _movingPlatformState.stuckToWall = CollidedSurface.RightWall;
        }
    }

    private bool IsMovingPlatform(GameObject obj)
    {
        return ((0x1 << obj.layer) & movingPlatformLayerMask) != 0;
    }

    private void HandlePreJumping()
    {
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
        if (motorState == MotorState.Jumping && _jumping.held)
        {
            if (_jumping.allowExtraDuration > 0)
            {
                _jumping.allowExtraDuration -= GetDeltaTime();
                _jumping.ignoreGravity = true;
            }
        }

        // Jump?
        if (_jumping.pressed)
        {
            bool jumped = true;

            // Jump might mean different things depending on the state.
            if (IsGrounded() || _jumping.force)
            {
                // Normal jump.
                _velocity.y = CalculateSpeedNeeded(_jumping.height);
            }
            else if (motorState == MotorState.OnCorner)
            {
                // If we are on a corner then jump up.
                _velocity = Vector2.up * CalculateSpeedNeeded(_jumping.height) * cornerJumpMultiplier;
                _ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;

                if (onCornerJump != null)
                {
                    onCornerJump();
                }
            }
            else if (allowWallJump && PressingIntoLeftWall())
            {
                // If jump was pressed as we or before we entered the wall then just jump away.
                _velocity = _upRight * CalculateSpeedNeeded(_jumping.height) * wallJumpMultiplier;

                if (Mathf.Abs(_velocity.x) >= maxAirSpeed)
                {
                    // We'll limit our x to the maxAirSpeed.
                    _velocity.x = Mathf.Sign(_velocity.x) * maxAirSpeed;
                }

                // It's likely the player is still pressing into the wall, ignore movement for a little amount of time.
                // TODO: Only ignore left movement?
                _ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;

                // If wall jump is allowed but not wall slide then double jump will not be allowed earlier, allow it now.
                _jumping.numAirJumps = 0;

                if (onWallJump != null)
                {
                    onWallJump(Vector2.right);
                }
            }
            else if (allowWallJump && PressingIntoRightWall())
            {
                _velocity = _upLeft * CalculateSpeedNeeded(_jumping.height) * wallJumpMultiplier;

                if (Mathf.Abs(_velocity.x) >= maxAirSpeed)
                {
                    // We'll limit our x to the maxAirSpeed.
                    _velocity.x = Mathf.Sign(_velocity.x) * maxAirSpeed;
                }

                _ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;
                _jumping.numAirJumps = 0;

                if (onWallJump != null)
                {
                    onWallJump(-Vector2.right);
                }
            }
            else if (_jumping.numAirJumps < numAirJumps)
            {
                _velocity.y = CalculateSpeedNeeded(_jumping.height);
                _jumping.numAirJumps++;

                if (onAirJump != null)
                {
                    onAirJump();
                }
            }
            else
            {
                // Guess we aren't jumping!
                jumped = false;
            }

            if (jumped)
            {
                _jumping.pressed = false;
                _jumping.force = false;
                _jumping.allowExtraDuration = extraJumpHeight / CalculateSpeedNeeded(_jumping.height);
                amountJumpedFor = 0;
                motorState = MotorState.Jumping;
                _movingPlatformState.platform = null;

                if (onJump != null)
                {
                    onJump();
                }
            }
        }
    }

    private bool PressingIntoLeftWall()
    {
        if (_movingPlatformState.isOnPlatform && _movingPlatformState.stuckToWall == CollidedSurface.LeftWall && normalizedXMovement < wallInteractionThreshold)
        {
            return true;
        }

        return HasFlag(CollidedSurface.LeftWall) && normalizedXMovement < -wallInteractionThreshold;
    }

    private bool PressingIntoRightWall()
    {
        if (_movingPlatformState.isOnPlatform && _movingPlatformState.stuckToWall == CollidedSurface.RightWall && normalizedXMovement > wallInteractionThreshold)
        {
            return true;
        }

        return HasFlag(CollidedSurface.RightWall) && normalizedXMovement > wallInteractionThreshold;
    }

    private void HandlePostWallInteraction()
    {
        // We can grab corners or walls again.
        if (!PressingIntoLeftWall() && !PressingIntoRightWall())
        {
            _wallInfo.canHangAgain = true;
        }

        // Kill off horizontal velocity if we are by a wall.
        if (HasFlag(CollidedSurface.LeftWall) && _velocity.x < 0 ||
            HasFlag(CollidedSurface.RightWall) && _velocity.x > 0)
        {
            _velocity.x = 0;
        }

        if (motorState == MotorState.OnCorner)
        {
            if (!CheckIfAtCorner() || _wallInfo.cornerHangTime < 0)
            {
                motorState = MotorState.Falling;
            }
        }

        if (motorState == MotorState.Clinging)
        {
            if (!(PressingIntoLeftWall() || PressingIntoRightWall()) || _wallInfo.clingTime < 0)
            {
                motorState = MotorState.Falling;
            }
        }

        if (motorState == MotorState.Falling && allowWallSlide)
        {
            motorState = MotorState.Sliding;
        }

        if (motorState == MotorState.Sliding)
        {
            if (!(PressingIntoLeftWall() || PressingIntoRightWall()))
            {
                motorState = MotorState.Falling;
            }
        }

        if (HasFlag(CollidedSurface.Ground))
        {
            motorState = MotorState.OnGround;
        }
    }

    /// <summary>
    /// HandlePreWalInteraction is responsible for saying we are now on a wall and setting velocity to 0.
    /// </summary>
    private void HandlePreWallInteraction()
    {
        if (fallFast || motorState == MotorState.Jumping)
        {
            // If we're being told to fall fast then we won't allow any wall interactions. Ditto on if we're jumping.
            return;
        }

        // Corner grab?
        if (allowCornerGrab)
        {
            if (_velocity.y <= 0 && CheckIfAtCorner() && _wallInfo.canHangAgain)
            {
                _wallInfo.cornerHangTime = cornerGrabDuration;
                _wallInfo.canHangAgain = false;
                _velocity = Vector2.zero;
                motorState = MotorState.OnCorner;
                return;
            }
        }

        // Wall Cling
        if (allowWallCling)
        {
            if (_velocity.y <= 0 && (PressingIntoLeftWall() || PressingIntoRightWall()) && _wallInfo.canHangAgain)
            {
                _wallInfo.clingTime = wallClingDuration;
                _velocity = Vector2.zero;
                motorState = MotorState.Clinging;
                _wallInfo.canHangAgain = false;
                return;
            }
        }

        // Wall slide?
        if (allowWallSlide && motorState != MotorState.Clinging && motorState != MotorState.OnCorner)
        {
            if (_velocity.y <= 0 && (PressingIntoLeftWall() || PressingIntoRightWall()))
            {
                _velocity = -Vector2.up * wallSlideSpeed;
                motorState = MotorState.Sliding;
            }
        }
    }

    private void ApplyMovement()
    {
        if (Mathf.Abs(normalizedXMovement) > 0)
        {
            if (IsGrounded())
            {
                if (timeToMaxGroundSpeed > 0)
                {
                    // If we're moving faster than our normalizedXMovement * maxGroundSpeed then decelerate rather than 
                    // accelerate.
                    //
                    // Or if we are trying to move in the direction opposite of where we are facing.
                    if (_velocity.x > 0 &&
                        normalizedXMovement > 0 &&
                        _velocity.x > normalizedXMovement * maxGroundSpeed ||
                        _velocity.x < 0 &&
                        normalizedXMovement < 0 &&
                        _velocity.x < normalizedXMovement * maxGroundSpeed ||
                        _velocity.x < 0 &&
                        normalizedXMovement > 0 ||
                        _velocity.x > 0 &&
                        normalizedXMovement < 0)
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
            else if (changeDirectionInAir)
            {
                // Don't apply the force if we're already on the wall.
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
        else if (_velocity.x != 0)
        {
            if (IsGrounded())
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
        // Set facing now and it won't be set again during dash.
        SetFacing();

        if (!_dashing.dashWithDirection)
        {
            // We dash depending on our direction.
            _dashing.dashDir = facingLeft ? -Vector2.right : Vector2.right;
        }

        _dashing.distanceDashed = 0;
        _dashing.distanceCalculated = 0;
        _dashing.previousLoc = _rigidbody2D.position;

        // This will begin the dash this frame.
        _dashing.timeDashed = GetDeltaTime();

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

        _rigidbody2D.MovePosition(_rigidbody2D.position + _dashing.dashDir * (distance - _dashing.distanceCalculated));

        _dashing.distanceCalculated = distance;
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

        if (PressingIntoLeftWall())
        {
            max.x = min.x;
            min.x -= cornerDistanceCheck;
        }
        else if (PressingIntoRightWall())
        {
            min.x = max.x;
            max.x += cornerDistanceCheck;
        }
        else
        {
            return false;
        }

        int layerMask = staticEnvironmentLayerMask | movingPlatformLayerMask;
        Collider2D col = Physics2D.OverlapArea(min, max, layerMask);

        return col == null;
    }

    private void ClampFallSpeed()
    {
        if (!IsGrounded())
        {
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

    private bool IsGrounded()
    {
        return HasFlag(CollidedSurface.Ground) && _velocity.y <= 0;
    }

    private CollidedSurface CheckSurroundings(
        CollidedSurface checkAgainst = CollidedSurface.Ceiling | 
        CollidedSurface.Ground | 
        CollidedSurface.LeftWall | 
        CollidedSurface.RightWall)
    {
        Bounds box;
        CollidedSurface surfaces;

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

        int layerMask = staticEnvironmentLayerMask | movingPlatformLayerMask;

        // TODO: This requires that a ground layer is set up to work. Consider moving to a set up that will consider all
        //       collisions but ignore the player's collider.

        surfaces = CollidedSurface.None;

        if ((checkAgainst & CollidedSurface.Ground) != 0)
        {
            min.x += TRIM_STUCKTO_NUM;
            max.x -= TRIM_STUCKTO_NUM;

            min.y -= checkDistance;
            max.y = transform.position.y; // Go ahead and bring the maximum y down.

            _collidersUpAgainst[DIRECTION_DOWN] = Physics2D.OverlapArea(min, max, layerMask);

            if (_collidersUpAgainst[DIRECTION_DOWN] != null)
            {
                surfaces |= CollidedSurface.Ground;
            }
        }

        if ((checkAgainst & CollidedSurface.Ceiling) != 0)
        {
            min = box.min;
            max = box.max;

            min.x += TRIM_STUCKTO_NUM;
            max.x -= TRIM_STUCKTO_NUM;

            max.y += checkDistance;
            min.y = transform.position.y;

            _collidersUpAgainst[DIRECTION_UP] = Physics2D.OverlapArea(min, max, layerMask);


            if (_collidersUpAgainst[DIRECTION_UP] != null)
            {
                surfaces |= CollidedSurface.Ceiling;
            }
        }

        if ((checkAgainst & CollidedSurface.LeftWall) != 0)
        {
            min = box.min;
            max = box.max;

            min.y += TRIM_STUCKTO_NUM;
            max.y -= TRIM_STUCKTO_NUM;

            min.x -= checkDistance;
            max.x = transform.position.x;

            _collidersUpAgainst[DIRECTION_LEFT] = Physics2D.OverlapArea(min, max, layerMask);

            if (_collidersUpAgainst[DIRECTION_LEFT] != null)
            {
                surfaces |= CollidedSurface.LeftWall;
            }
        }

        if ((checkAgainst & CollidedSurface.RightWall) != 0)
        {
            min = box.min;
            max = box.max;

            min.y += TRIM_STUCKTO_NUM;
            max.y -= TRIM_STUCKTO_NUM;

            min.x = transform.position.x;
            max.x += checkDistance;

            _collidersUpAgainst[DIRECTION_RIGHT] = Physics2D.OverlapArea(min, max, layerMask);

            if (_collidersUpAgainst[DIRECTION_RIGHT] != null)
            {
                surfaces |= CollidedSurface.RightWall;
            }
        }

        return surfaces;
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
