using System;
using PC2D;
using UnityEngine;

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
    /// Gravity multiplier to the Physics2D.gravity setting. Works like RigidBody2D's gravityMultiplier.
    /// </summary>
    public float gravityMultiplier;

    /// <summary>
    /// If the motor is in 'fast fall' then the gravityMultiplier is multiplied by the value. Higher number means
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
    /// The amount of time once the motor has left an environment that a jump will be allowed.
    /// </summary>
    public float jumpAllowedGrace;

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

    [Range(0f, 90f)]
    public float wallJumpDegree;

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
    /// Cooldown for allowing slides, clings, and corner grabs. This may be necessary if the motor can slide down a vertical
    /// moving platform. If they don't exist then this can be 0.
    /// </summary>
    public float slideClingCooldown;

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
    /// The distance the motor will separate itself from environment. This is useful to prevent the motor from catching on edges.
    /// </summary>
    public float distanceFromEnvironment = 0.025f;

    public bool changeSpeedOnSlopes;
    public float degreeAllowedForSlopes;
    public float speedMultiplierOnSlope = 0.5f;
    public bool stickToGround;
    public float distanceToCheckToStick;

    /// <summary>
    /// The layer that contains moving platforms.
    /// </summary>
    public LayerMask movingPlatformLayerMask;

    public bool debug;

    /// <summary>
    /// The states the motor can be in.
    /// </summary>
    public enum MotorState
    {
        OnGround,
        Jumping,
        Falling,
        FallingFast,
        WallSliding,
        OnCorner,
        Clinging,
        Dashing,
        Frozen,
        Slipping
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
                    _prevState = motorState;
                    motorState = MotorState.Frozen;
                }
                else
                {
                    motorState = _prevState;
                }
            }
        }
    }

    /// <summary>
    /// Returns the moving platform that the motor is coupled with. If null then no moving platform.
    /// </summary>
    public MovingPlatformMotor2D connectedPlatform
    {
        get { return _movingPlatformState.platform; }
    }

    /// <summary>
    /// Set this to use a specific collider for checks instead of grabbing the collider from gameObject.collider2D.
    /// </summary>
    public Collider2D colliderToUse
    {
        get
        {
            return _colliderToUse;
        }
        set
        {
            _colliderToUse = value;
            BuildCollisionMask();
        }
    }

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
        if (motorState == MotorState.Jumping)
        {
            _jumping.pressed = false;
            motorState = MotorState.Falling;
            _jumping.timePressed = 0;
            _jumping.numAirJumps = 0;
        }
    }

    /// <summary>
    /// Resets the state for air jumps by setting the counter to 0.
    /// </summary>
    public void ResetAirJump()
    {
        _jumping.numAirJumps = 0;
    }

    /// <summary>
    /// Decouples the motor from the platform. This could be useful for a platform that throw the motor in the air. Call this
    /// when when the motor should disconnect then set the appropriate velocity.
    /// </summary>
    public void DisconnectFromPlatform()
    {
        _movingPlatformState.platform = null;
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

            _velocity = _dashing.dashDir * GetDashSpeed();

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

    private Vector2 _wallJumpVector;
    private float _ignoreMovementUntil;
    private bool _frozen;
    private bool _originalKinematic;
    private float _timeScale = 1;
    private Vector3 _previousLoc;
    private Collider2D[] _collidersUpAgainst = new Collider2D[DIRECTIONS_CHECKED];
    private Vector2[] _collidedNormals = new Vector2[DIRECTIONS_CHECKED];
    private MotorState _prevState;
    private Bounds _prevColliderBounds;
    private int _collisionMask;
    private Collider2D _colliderToUse;
    private float _dotAllowedForSlopes;

    // This is the unconverted motor velocity. This ignores slopes. It is converted into the appropriate vector before
    // moving.
    private Vector2 _velocity;

    // The function is cached to avoid unnecessary memory allocation.
    private EasingFunctions.EasingFunc _dashFunction;
    private EasingFunctions.EasingFunc _dashDerivativeFunction;

    // This is stored to notice if the public field changes during runtime.
    private EasingFunctions.Functions _currentDashEasingFunction;
    private float _currentWallJumpDegree;
    private float _currentSlopeDegreeAllowed;

    // Debug
    private Vector3 _point;
    private Vector3 _point2;
    private Bounds _prevPosPlat;
    private Bounds _startPosMotor;
    private Bounds _movedPosMotor;

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

        public float jumpGraceTime;
        public bool jumpTypeChanged;

        public JumpType lastValidJump
        {
            get { return _lastValidJump; }
            set
            {
                if (value != JumpType.None)
                {
                    jumpTypeChanged = true;
                }
                else
                {
                    jumpGraceTime = 0;
                }

                _lastValidJump = value;
            }
        }

        public enum JumpType
        {
            None,
            Normal,
            RightWall,
            LeftWall,
            Corner
        }

        private JumpType _lastValidJump;

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
    }
    private DashState _dashing = new DashState();

    // Contains information for wall clings, slides, and corner grabs.
    private class WallState
    {
        public float cornerHangTime;
        public float clingTime;

        public float timeUntilHang;
        public bool canHangAgain = true;
    }
    private WallState _wallInfo = new WallState();

    private class MovingPlatformState
    {
        public MovingPlatformMotor2D platform;

        public Vector2 previousPos;
        public CollidedSurface stuckToWall;
        public bool isOnPlatform { get { return platform != null; } }
    }
    private MovingPlatformState _movingPlatformState = new MovingPlatformState();

    // Used for environment checks and one way platforms
    private static RaycastHit2D[] _hits = new RaycastHit2D[STARTING_ARRAY_SIZE];
    private static RaycastHit2D[] _hitsNoDistance = new RaycastHit2D[STARTING_ARRAY_SIZE];

    private const float ONE_WAY_DOT_CHECK = 0.0001f;

    private const int STARTING_ARRAY_SIZE = 4;
    private const float INCREASE_ARRAY_SIZE_MULTIPLIER = 2;


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

    private Collider2D _collider2D
    {
        get
        {
            if (colliderToUse != null)
            {
                return colliderToUse;
            }

            return GetComponent<Collider2D>();
        }
    }

    private void Awake()
    {
        SetDashFunctions();
    }

    private void Start()
    {
        _previousLoc = transform.position;
        motorState = MotorState.Falling;
        _wallJumpVector = Quaternion.AngleAxis(wallJumpDegree, Vector3.forward) * Vector3.right;
        _currentWallJumpDegree = wallJumpDegree;

        BuildCollisionMask();
        SetSlopeDegreeAllowed();
    }

    private void OnEnable()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            _velocity = rb.velocity;
            _originalKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }
    }

    private void OnDisable()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = _velocity;
            rb.isKinematic = _originalKinematic;
        }
    }

    private void SetSlopeDegreeAllowed()
    {
        Vector3 v = Quaternion.AngleAxis(degreeAllowedForSlopes, Vector3.back) * Vector3.up;
        _dotAllowedForSlopes = Vector3.Dot(Vector3.up, v);
        _currentSlopeDegreeAllowed = degreeAllowedForSlopes;
    }

    private void BuildCollisionMask()
    {
        _collisionMask = 0;

        for (int i = 0; i < 32; i++)
        {
            if (!Physics2D.GetIgnoreLayerCollision(_collider2D.gameObject.layer, i))
            {
                _collisionMask |= (1 << i);
            }
        }
    }

    private static Vector2 GetPointOnBounds(Bounds bounds, Vector3 origin, Vector3 toPoint)
    {
        // From http://stackoverflow.com/questions/4061576/finding-points-on-a-rectangle-at-a-given-angle
        float angle = Vector3.Angle(Vector3.right, toPoint);

        if (toPoint.y < 0)
        {
            angle = 360f - angle;
        }

        float multiplier = 1f;

        if ((angle >= 0f && angle < 45f) ||
            angle > 315f ||
            (angle >= 135f && angle < 225f))
        {

            if (angle >= 135f && angle < 225f)
            {
                multiplier = -1f;
            }

            return new Vector2(
                multiplier * bounds.size.x / 2 + origin.x,
                origin.y + multiplier * ((bounds.size.x / 2) * Mathf.Tan(angle * Mathf.Deg2Rad)));
        }

        if (angle >= 225f)
        {
            multiplier = -1f;
        }

        return new Vector2(
            origin.x + multiplier * bounds.size.y / (2 * Mathf.Tan(angle * Mathf.Deg2Rad)),
            multiplier * bounds.size.y / 2 + origin.y);
    }

    private void UpdateTimersAndProperties()
    {
        if (degreeAllowedForSlopes != _currentSlopeDegreeAllowed)
        {
            SetSlopeDegreeAllowed();
        }

        // All timers in the motor are countdowns and are considered valid so long as the timer is > 0
        _dashing.cooldownTimer -= GetDeltaTime();
        _dashing.gravityEnabledTimer -= GetDeltaTime();
        _wallInfo.cornerHangTime -= GetDeltaTime();
        _wallInfo.clingTime -= GetDeltaTime();
        _wallInfo.timeUntilHang -= GetDeltaTime();
    }

    private void UpdateVelocity()
    {
        // First, are we trying to dash?
        if (allowDash &&
            _dashing.pressed &&
            (_dashing.cooldownTimer <= 0 || _dashing.force) &&
            motorState != MotorState.Dashing)
        {
            StartDash();
        }

        _dashing.pressed = false;

        if (motorState != MotorState.Dashing)
        {
            // If we have standard control then facing can change any frame.
            SetFacing();

            // Apply movement if we're not ignoring it.
            if (Time.time >= _ignoreMovementUntil)
            {
                ApplyMovement();
            }

            if (IsSlipping())
            {
                Vector2 slopeDir = GetDownSlopeDir();

                if (fallFast)
                {
                    float increaseBy = Vector2.Dot(Vector3.down, slopeDir) * fastFallGravityMultiplier *
                        Mathf.Abs(Physics2D.gravity.y) * GetDeltaTime();

                    _velocity += slopeDir * increaseBy;

                    if (Vector2.Dot(GetMovementDir(_velocity.x), slopeDir) > 0)
                    {
                        _velocity = Vector2.ClampMagnitude(_velocity, Vector2.Dot(Vector3.down, slopeDir) * maxFastFallSpeed);
                    }
                }
                else
                {
                    float increaseBy = Vector2.Dot(Vector3.down, slopeDir) * gravityMultiplier *
                        Mathf.Abs(Physics2D.gravity.y) * GetDeltaTime();

                    _velocity += slopeDir * increaseBy;

                    if (Vector2.Dot(GetMovementDir(_velocity.x), slopeDir) > 0)
                    {
                        _velocity = Vector2.ClampMagnitude(_velocity, Vector2.Dot(Vector3.down, slopeDir) * maxFallSpeed);
                    }
                }
            }

            // Handle jumping.
            HandlePreJumping();

            // Finally, any wall interactions.
            HandlePreWallInteraction();

            // If we are falling fast then multiply the gravityMultiplier.
            if (IsInAir() && !_jumping.ignoreGravity)
            {
                if (fallFast)
                {
                    _velocity.y += 
                        fastFallGravityMultiplier *
                        GetDeltaTime() *
                       Physics2D.gravity.y;

                    if (_velocity.y <= 0)
                    {
                        motorState = MotorState.FallingFast;
                    }
                }
                else
                {
                    if (_dashing.gravityEnabledTimer <= 0)
                    {
                        _velocity.y += 
                            gravityMultiplier *
                            GetDeltaTime() *
                            Physics2D.gravity.y;
                    }

                    if (_velocity.y <= 0)
                    {
                        motorState = MotorState.Falling;
                    }
                }
            }

            // Check speeds.
            ClampFallSpeed();
        }
    }

    private void MoveMotor()
    {
        if (motorState == MotorState.Dashing)
        {
            _dashing.timeDashed = Mathf.Clamp(_dashing.timeDashed + GetDeltaTime(), 0f, dashDuration);
            float normalizedTime = _dashing.timeDashed / dashDuration;

            if (_currentDashEasingFunction != dashEasingFunction)
            {
                // This allows the easing function to change during runtime and cut down on unnecessary allocations.
                SetDashFunctions();
            }

            float distance = _dashFunction(0, dashDistance, normalizedTime);

            _velocity = _dashing.dashDir * GetDashSpeed();
            MovePosition(transform.position + (Vector3)_dashing.dashDir * (distance - _dashing.distanceCalculated));
            _dashing.distanceCalculated = distance;

        }
        else
        {
            MovePosition(transform.position + (Vector3)_velocity * GetDeltaTime());
        }
    }

    private void UpdateState()
    {
        collidingAgainst = CheckSurroundings();

        if (motorState == MotorState.Dashing && _dashing.timeDashed >= dashDuration)
        {
            EndDash();
        }

        if (motorState == MotorState.Dashing)
        {
            // Still dashing, nothing else matters.
            _dashing.distanceDashed += (transform.position - _previousLoc).magnitude;
            return;
        }

        if (stickToGround &&
            (motorState == MotorState.OnGround || motorState == MotorState.Slipping) && 
            (collidingAgainst == CollidedSurface.None || collidingAgainst == CollidedSurface.Ceiling))
        {
            // We were on the ground but now we are not, we'll see if we should put ourselves back on the ground. We don't bother
            // rechecking surrounding areas. This will probably be fine in most cases. If weird cases start to pop then recall
            // CheckSurroundings.
            collidingAgainst |= CheckGround(distanceToCheckToStick, true);
        }

        if (HasFlag(CollidedSurface.Ground))
        {
            if ((motorState == MotorState.Falling ||
                motorState == MotorState.FallingFast) &&
                onLanded != null)
            {
                _velocity.y = 0;
                onLanded();
            }
        }

        AttachToMovingPlatforms();

        UpdateInformationFromMovement();

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
            Vector3 slopeDir = GetDownSlopeDir();

            if (IsSlipping() && _velocity != Vector2.zero && Mathf.Sign(_velocity.x) == Mathf.Sign(slopeDir.x))
            {
                float speed = _velocity.magnitude;

                _velocity = slopeDir * Vector2.Dot(_velocity / speed, slopeDir) * speed;
                motorState = MotorState.Slipping;
            }
            else
            {
                motorState = MotorState.OnGround;
            }
        }
        else if (motorState == MotorState.OnGround || motorState == MotorState.Slipping)
        {
            motorState = MotorState.Falling;
        }

        if (_movingPlatformState.isOnPlatform)
        {
            _movingPlatformState.previousPos = _movingPlatformState.platform.position;
        }

        SetLastJumpType();
    }

    private void FixedUpdate()
    {
        // Frozen?
        if (frozen)
        {
            return;
        }

        // The update phase is broken up into five phases each with certain responsibilities.

        // Phase One: Update internal state if something forced the motor into an unexpected state. This can be because
        //            a moving platform moved into us or our collider changed.

        if (UpdateMovingPlatform() || _prevColliderBounds != _collider2D.bounds)
        {
            UpdateState();
        }

        // Phase Two: Update all timers
        UpdateTimersAndProperties();

        // Phase Three: Update internal representation of velocity
        UpdateVelocity();

        // Phase Four: Move the motor to the new location
        MoveMotor();

        // Phase Five: Update internal state so it can be accurately queried and ready for next step
        UpdateState();

        _prevColliderBounds = _collider2D.bounds;
    }

    private bool UpdateMovingPlatform()
    {
        if (movingPlatformLayerMask == 0)
        {
            // No moving platforms, early bail.
            return false;
        }

        if (debug)
        {
            _point = new Vector3();
            _point2 = new Vector3();
            _prevPosPlat = new Bounds();
            _movedPosMotor = new Bounds();
            _startPosMotor = new Bounds();
        }

        // Update location if on a moving platform.
        if (motorState != MotorState.Dashing && _movingPlatformState.isOnPlatform)
        {
            Vector3 toNewPos = _movingPlatformState.platform.position - _movingPlatformState.previousPos;
            transform.position += toNewPos;
        }

        RaycastHit2D hit = Physics2D.BoxCast(_collider2D.bounds.center, _collider2D.bounds.size, 0f, Vector3.up, 0f,
            movingPlatformLayerMask);

        if (hit.collider != null)
        {
            if (debug)
            {
                _point = hit.point;
            }

            // Intersecting with a platform, we separate along the velocity unless we would have landed on it then it's only up.
            MovingPlatformMotor2D mpMotor = hit.collider.GetComponent<MovingPlatformMotor2D>();
            bool separateAlongMovement = true;

            Vector3 curLoc;

            if (debug)
            {
                curLoc = mpMotor.position;
                mpMotor.transform.position = mpMotor.previousPosition;
                _prevPosPlat = mpMotor.GetComponent<Collider2D>().bounds;
                mpMotor.transform.position = curLoc;
                _startPosMotor = _collider2D.bounds;
            }

            if (mpMotor.position.y > mpMotor.previousPosition.y)
            {
                // Can only have landed on it if it is moving up.
                curLoc = mpMotor.position;
                mpMotor.transform.position = mpMotor.previousPosition;

                float distance = curLoc.y - mpMotor.previousPosition.y;

                int num = GetNearbyHits(Vector3.down, distance, _collider2D.bounds, false);

                for (int i = 0; i < num; i++)
                {
                    if (_hits[i].collider == hit.collider)
                    {
                        // Coming from underneath, just separate up.
                        mpMotor.transform.position = curLoc;

                        hit = Physics2D.Raycast(
                            hit.point + Vector2.up * _collider2D.bounds.size.y, 
                            Vector3.down, 
                            _collider2D.bounds.size.y + checkDistance,
                            movingPlatformLayerMask);

                        if (hit.collider != null)
                        {
                            if (debug)
                            {
                                _point2 = hit.point;
                            }

                            float moveUpBy = hit.point.y - _collider2D.bounds.min.y;
                            transform.position += Vector3.up * (moveUpBy + distanceFromEnvironment);
                        }

                        separateAlongMovement = false;
                        break;
                    }
                }

                if (separateAlongMovement)
                {
                    mpMotor.transform.position = curLoc;
                }
            }

            if (separateAlongMovement)
            {
                Vector2 toNewPos = mpMotor.position - mpMotor.previousPosition;
                Vector2 toNewPosNorm = toNewPos.normalized;

                float distance = ((Vector3)GetPointOnBounds(_collider2D.bounds, _collider2D.bounds.center, toNewPosNorm) - 
                    _collider2D.bounds.center).magnitude * 2;

                RaycastHit2D hit2 = Physics2D.Raycast(
                    hit.point + toNewPosNorm * distance,
                    -toNewPosNorm,
                    distance + checkDistance,
                    movingPlatformLayerMask);

                if (hit2.collider != null)
                {
                    transform.position += (Vector3)toNewPosNorm * ((hit2.point - hit.point).magnitude + distanceFromEnvironment);

                    if (debug)
                    {
                        _point2 = hit2.point;
                    }
                }
            }

            if (debug)
            {
                _movedPosMotor = _collider2D.bounds;
            }

            return true;
        }

        return false;
    }

    private void SetLastJumpType()
    {
        if (motorState == MotorState.OnGround)
        {
            _jumping.lastValidJump = JumpState.JumpType.Normal;
        }
        else if (allowWallJump)
        {
            if (PressingIntoLeftWall())
            {
                _jumping.lastValidJump = JumpState.JumpType.LeftWall;
            }
            else if (PressingIntoRightWall())
            {
                _jumping.lastValidJump = JumpState.JumpType.RightWall;
            }
        }
        else if (motorState == MotorState.OnCorner)
        {
            _jumping.lastValidJump = JumpState.JumpType.Corner;
        }

        // We don't track air jumps as they are always valid in the air.
    }

    private void UpdateInformationFromMovement()
    {
        float diffInPositions = Mathf.Abs(transform.position.y - _previousLoc.y);

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
        MovingPlatformMotor2D previous = _movingPlatformState.platform;
        _movingPlatformState.platform = null;
        _movingPlatformState.stuckToWall = CollidedSurface.None;

        if (HasFlag(CollidedSurface.Ground) && IsMovingPlatform(_collidersUpAgainst[DIRECTION_DOWN].gameObject))
        {
            _movingPlatformState.platform = _collidersUpAgainst[DIRECTION_DOWN].GetComponent<MovingPlatformMotor2D>();

            if (fallFast)
            {
                if (_movingPlatformState.platform.velocity.y < -maxFastFallSpeed)
                {
                    _movingPlatformState.platform = null;
                    _velocity.y = -maxFastFallSpeed;
                }
            }
            else
            {
                if (_movingPlatformState.platform.velocity.y < -maxFallSpeed)
                {
                    _movingPlatformState.platform = null;
                    _velocity.y = -maxFallSpeed;
                }
            }
        }
        else if (PressingIntoLeftWall() && IsMovingPlatform(_collidersUpAgainst[DIRECTION_LEFT].gameObject))
        {
            _movingPlatformState.platform = _collidersUpAgainst[DIRECTION_LEFT].GetComponent<MovingPlatformMotor2D>();
            _movingPlatformState.stuckToWall = CollidedSurface.LeftWall;
        }
        else if (PressingIntoRightWall() && IsMovingPlatform(_collidersUpAgainst[DIRECTION_RIGHT].gameObject))
        {
            _movingPlatformState.platform = _collidersUpAgainst[DIRECTION_RIGHT].GetComponent<MovingPlatformMotor2D>();
            _movingPlatformState.stuckToWall = CollidedSurface.RightWall;
        }

        if (_movingPlatformState.platform != null && _movingPlatformState.platform != previous)
        {
            if (_movingPlatformState.platform.onPlatformerMotorContact != null)
            {
                _movingPlatformState.platform.onPlatformerMotorContact.Invoke(this);
            }
        }
    }

    private bool IsMovingPlatform(GameObject obj)
    {
        return ((0x1 << obj.layer) & movingPlatformLayerMask) != 0;
    }

    private void HandlePreJumping()
    {
        if (_jumping.jumpTypeChanged && _jumping.lastValidJump != JumpState.JumpType.None)
        {
            _jumping.jumpTypeChanged = false;
            _jumping.jumpGraceTime = Time.time + jumpAllowedGrace;
        }

        // This is something that the default Unity Controller script does, allows the player to press jump button
        // earlier than would normally be allowed. They say it leads to a more pleasant experience for the
        // user. I'll assume they're on to something.
        if (Time.time > _jumping.timePressed + JumpState.TIME_BUFFER)
        {
            _jumping.pressed = false;
        }

        _jumping.ignoreGravity = false;

        if (_currentWallJumpDegree != wallJumpDegree)
        {
            _wallJumpVector = Quaternion.AngleAxis(wallJumpDegree, Vector3.forward) * Vector3.right;
            _currentWallJumpDegree = wallJumpDegree;
        }

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
            if ((_jumping.lastValidJump == JumpState.JumpType.Normal && Time.time <= _jumping.jumpGraceTime) ||
                _jumping.force)
            {
                // Normal jump.
                if (IsSlipping())
                {
                    _velocity += _collidedNormals[DIRECTION_DOWN] * CalculateSpeedNeeded(_jumping.height);
                }
                else
                {
                    _velocity.y = CalculateSpeedNeeded(_jumping.height);
                }
            }
            else if (motorState == MotorState.OnCorner ||
                     _jumping.lastValidJump == JumpState.JumpType.Corner && Time.time <= _jumping.jumpGraceTime)
            {
                // If we are on a corner then jump up.
                _velocity = Vector2.up * CalculateSpeedNeeded(_jumping.height) * cornerJumpMultiplier;
                _ignoreMovementUntil = Time.time + IGNORE_INPUT_TIME;

                if (onCornerJump != null)
                {
                    onCornerJump();
                }
            }
            else if (_jumping.lastValidJump == JumpState.JumpType.LeftWall &&
                     Time.time <= _jumping.jumpGraceTime)
            {
                // If jump was pressed as we or before we entered the wall then just jump away.
                _velocity = _wallJumpVector * CalculateSpeedNeeded(_jumping.height) * wallJumpMultiplier;

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
            else if (_jumping.lastValidJump == JumpState.JumpType.RightWall &&
                     Time.time <= _jumping.jumpGraceTime)
            {

                _velocity = _wallJumpVector * CalculateSpeedNeeded(_jumping.height) * wallJumpMultiplier;
                _velocity.x *= -1;

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
                _jumping.lastValidJump = JumpState.JumpType.None;

                if (onJump != null)
                {
                    onJump();
                }
            }
        }
    }

    private bool PressingIntoLeftWall()
    {
        if (_movingPlatformState.isOnPlatform && _movingPlatformState.stuckToWall == CollidedSurface.LeftWall && normalizedXMovement < -wallInteractionThreshold)
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
            if (!_wallInfo.canHangAgain)
            {
                // Debounce time, mostly for platforms that are moving down and constantly triggering clings
                _wallInfo.timeUntilHang = slideClingCooldown;
            }

            _wallInfo.canHangAgain = true;

            if (motorState == MotorState.OnCorner ||
                motorState == MotorState.Clinging ||
                motorState == MotorState.WallSliding)
            {
                motorState = MotorState.Falling;
            }
        }
        else
        {
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
        }


        if (HasFlag(CollidedSurface.Ground))
        {
            if (motorState == MotorState.WallSliding)
            {
                _velocity.y = 0;
            }

            motorState = MotorState.OnGround;
        }
    }

    /// <summary>
    /// HandlePreWalInteraction is responsible for saying we are now on a wall and setting velocity to 0.
    /// </summary>
    private void HandlePreWallInteraction()
    {
        if (motorState == MotorState.Jumping || _wallInfo.timeUntilHang > 0 || HasFlag(CollidedSurface.Ground))
        {
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
        if (allowWallSlide && 
            motorState != MotorState.Clinging && 
            motorState != MotorState.OnCorner)
        {
            if (_velocity.y <= 0 && (PressingIntoLeftWall() || PressingIntoRightWall()) && !IsGrounded())
            {
                motorState = MotorState.WallSliding;
            }
        }

        if (motorState == MotorState.WallSliding)
        {
            _velocity = -Vector2.up * wallSlideSpeed;
        }
    }

    private void ApplyMovement()
    {
        float speed;
        float maxSpeed;

        // PERF: Optimal math out the window in favor of ease of figuring out, can resolve later if a problem.
        if (Mathf.Abs(normalizedXMovement) > 0)
        {
            if (IsGrounded())
            {
                if (IsSlipping() && Vector2.Dot(GetMovementDir(normalizedXMovement), GetDownSlopeDir()) <= 0)
                {
                    // Don't allow walking up a slope that we slide down.
                    return;
                }

                GetSpeedAndMaxSpeedOnGround(out speed, out maxSpeed);

                if (timeToMaxGroundSpeed > 0)
                {
                    // If we're moving faster than our normalizedXMovement * maxGroundSpeed then decelerate rather than 
                    // accelerate.
                    //
                    // Or if we are trying to move in the direction opposite of where we are facing.

                    if (speed > 0 &&
                        normalizedXMovement > 0 &&
                        speed > normalizedXMovement * maxSpeed ||
                        speed < 0 &&
                        normalizedXMovement < 0 &&
                        speed < normalizedXMovement * maxSpeed ||
                        speed < 0 &&
                        normalizedXMovement > 0 ||
                        speed > 0 &&
                        normalizedXMovement < 0)
                    {
                        speed = Decelerate(
                            speed,
                            (maxSpeed * maxSpeed) / (2 * groundStopDistance),
                            normalizedXMovement * maxSpeed);
                    }
                    else
                    {
                        speed = Accelerate(
                            speed,
                            normalizedXMovement * (maxSpeed / timeToMaxGroundSpeed),
                            normalizedXMovement * maxSpeed);
                    }
                }
                else
                {
                    // We can overwrite y if we're on the ground, it's fine.
                    speed = normalizedXMovement * maxSpeed;
                }

                _velocity = GetMovementDir(speed) * Mathf.Abs(speed);
            }
            else if (changeDirectionInAir)
            {
                // Air doesn't have to change how it represents speed.
                if (timeToMaxAirSpeed > 0)
                {
                    if (_velocity.x > 0 &&
                        normalizedXMovement > 0 &&
                        _velocity.x > normalizedXMovement * maxAirSpeed ||
                        _velocity.x < 0 &&
                        normalizedXMovement < 0 &&
                        _velocity.x < normalizedXMovement * maxAirSpeed)
                    {
                        speed = Decelerate(
                            _velocity.x,
                            (maxAirSpeed * maxAirSpeed) / (2 * airStopDistance),
                            normalizedXMovement * maxAirSpeed);
                    }
                    else
                    {
                        speed = Accelerate(
                            _velocity.x,
                            normalizedXMovement * (maxAirSpeed / timeToMaxAirSpeed),
                            normalizedXMovement * maxAirSpeed);
                    }
                }
                else
                {
                    speed = normalizedXMovement * maxAirSpeed;
                }

                _velocity.x = speed;
            }
        }
        else if (_velocity.x != 0)
        {
            if (IsGrounded())
            {
                if (motorState != MotorState.Slipping)
                {
                    GetSpeedAndMaxSpeedOnGround(out speed, out maxSpeed);

                    if (groundStopDistance > 0)
                    {
                        speed = Decelerate(speed, (maxGroundSpeed * maxGroundSpeed) / (2 * groundStopDistance), 0);
                    }
                    else
                    {
                        speed = 0;
                    }

                    _velocity = GetMovementDir(speed) * Mathf.Abs(speed);
                }
            }
            else
            {
                if (airStopDistance > 0)
                {
                    speed = Decelerate(_velocity.x, (maxAirSpeed * maxAirSpeed) / (2 * airStopDistance), 0);
                }
                else
                {
                    speed = 0;
                }

                _velocity.x = speed;
            }
        }

        if (HasFlag(CollidedSurface.LeftWall) && _collidedNormals[DIRECTION_LEFT] == Vector2.right ||
            HasFlag(CollidedSurface.RightWall) && _collidedNormals[DIRECTION_RIGHT] == -Vector2.right)
        {
            _velocity.x = 0;
        }
    }

    private void GetSpeedAndMaxSpeedOnGround(out float speed, out float maxSpeed)
    {
        Vector3 moveDir = GetMovementDir(_velocity.x);

        if (IsOnSlope())
        {
            speed = Vector3.Project(_velocity, moveDir).magnitude * Mathf.Sign(_velocity.x);
            Vector3 slopeDir = GetDownSlopeDir();

            if (IsSlipping() && Vector2.Dot(moveDir, slopeDir) > 0)
            {
                if (!changeSpeedOnSlopes)
                {
                    maxSpeed = fallFast ? maxFastFallSpeed : maxFallSpeed;
                }
                else
                {
                    maxSpeed = Vector2.Dot(Vector3.down, slopeDir) * (fallFast ? maxFastFallSpeed : maxFallSpeed);
                }
            }

            else if (changeSpeedOnSlopes)
            {
                if (moveDir.y > 0)
                {
                    maxSpeed = maxGroundSpeed * Vector3.Dot(Vector3.right * Mathf.Sign(moveDir.x), moveDir) * speedMultiplierOnSlope;
                }
                else
                {
                    maxSpeed = maxGroundSpeed * (2f - Vector3.Dot(Vector3.right * Mathf.Sign(moveDir.x), moveDir) * speedMultiplierOnSlope);
                }
            }
            else
            {
                maxSpeed = maxGroundSpeed;
            }
        }
        else
        {
            speed = _velocity.x;
            maxSpeed = maxGroundSpeed;
        }
    }

    private float Accelerate(float speed, float acceleration, float limit)
    {
        // acceleration can be negative or positive to note acceleration in that direction.
        speed += acceleration * GetDeltaTime();

        if (acceleration > 0)
        {
            if (speed > limit)
            {
                speed = limit;
            }
        }
        else
        {
            if (speed < limit)
            {
                speed = limit;
            }
        }

        return speed;
    }

    private float Decelerate(float speed, float deceleration, float limit)
    {
        // deceleration is always positive but assumed to take the velocity backwards.
        if (speed < 0)
        {
            speed += deceleration * GetDeltaTime();

            if (speed > limit)
            {
                speed = limit;
            }
        }
        else if (speed > 0)
        {
            speed -= deceleration * GetDeltaTime();

            if (speed < limit)
            {
                speed = limit;
            }
        }

        return speed;
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
        _previousLoc = transform.position;

        // This will begin the dash this frame.
        _dashing.timeDashed = GetDeltaTime();

        motorState = MotorState.Dashing;

        if (onDash != null)
        {
            onDash();
        }
    }

    private float GetDashSpeed()
    {
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

        return speed;
    }

    private void MovePosition(Vector3 newPos)
    {
        if (newPos == transform.position)
        {
            return;
        }

        Vector3 toNewPos = newPos - transform.position;
        float distance = toNewPos.magnitude;

        RaycastHit2D hit = GetClosestHit(toNewPos / distance, distance);

        _previousLoc = transform.position;

        if (hit.collider != null)
        {
            transform.position = (Vector3)hit.centroid - (toNewPos / distance) * distanceFromEnvironment;
        }
        else
        {
            transform.position = newPos;
        }

    }

    private Vector3 GetMovementDir(float speed)
    {
        Vector3 moveDir = Vector3.zero;

        float multiplier = Mathf.Sign(speed);

        if (speed == 0)
        {
            multiplier = Mathf.Sign(normalizedXMovement);
            speed = normalizedXMovement;
        }

        moveDir.x = multiplier * _collidedNormals[DIRECTION_DOWN].y;

        if (_collidedNormals[DIRECTION_DOWN].x * speed > 0)
        {
            moveDir.y = -Mathf.Abs(_collidedNormals[DIRECTION_DOWN].x);
        }
        else
        {
            moveDir.y = Mathf.Abs(_collidedNormals[DIRECTION_DOWN].x);
        }

        return moveDir;
    }

    private Vector3 GetDownSlopeDir()
    {
        Vector3 downDir = Vector3.zero;
        downDir.x = Mathf.Sign(_collidedNormals[DIRECTION_DOWN].x) * _collidedNormals[DIRECTION_DOWN].y;
        downDir.y = -Math.Abs(_collidedNormals[DIRECTION_DOWN].x);

        return downDir;
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

        Collider2D col = Physics2D.OverlapArea(min, max, _collisionMask);

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
        return HasFlag(CollidedSurface.Ground) && motorState != MotorState.Jumping;
    }

    private bool IsOnSlope()
    {
        return IsGrounded() && Vector3.Dot(Vector3.up, _collidedNormals[DIRECTION_DOWN]) < 1f;
    }

    private bool IsSlipping()
    {
        return IsGrounded() &&
            Vector3.Dot(Vector3.up, _collidedNormals[DIRECTION_DOWN]) < _dotAllowedForSlopes;
    }

    private int GetNearbyHits(
        Vector2 direction,
        float distance,
        Bounds motorBounds,
        bool useExternalHits)
    {
        int num = Physics2D.BoxCastNonAlloc(
            motorBounds.center,
            motorBounds.size,
            0f,
            direction,
            useExternalHits ? _hits : _hitsNoDistance,
            distance,
            _collisionMask);

        if (num > _hits.Length)
        {
            if (useExternalHits)
            {
                _hits = new RaycastHit2D[(int)(INCREASE_ARRAY_SIZE_MULTIPLIER * _hits.Length)];
            }
            else
            {
                _hitsNoDistance = new RaycastHit2D[(int)(INCREASE_ARRAY_SIZE_MULTIPLIER * _hitsNoDistance.Length)];
            }
        }

        num = Physics2D.BoxCastNonAlloc(
            motorBounds.center,
            motorBounds.size,
            0f,
            direction,
            useExternalHits ? _hits : _hitsNoDistance,
            distance,
            _collisionMask);

        return num;
    }

    private RaycastHit2D GetClosestHit(
        Vector3 direction,
        float distance)
    {
        int numOfHits = GetNearbyHits(
            direction,
            distance,
            _collider2D.bounds,
            true);

        RaycastHit2D closestHit = new RaycastHit2D();
        float closeBy = float.MaxValue;

        for (int i = 0; i < numOfHits; i++)
        {
            if (_hits[i].collider == _collider2D)
            {
                continue;
            }

            if (_hits[i].collider.usedByEffector &&
                _hits[i].collider.GetComponent<PlatformEffector2D>().useOneWay)
            {
                bool isTouching = false;

                // You'd think OverlapArea would be sufficient but doesn't
                // appear to necessarily reliably return the expected colliders
                // So we box cast a distance of 0 instead.
                int numOfNoDistanceHits = GetNearbyHits(
                    direction,
                    0f,
                    _collider2D.bounds,
                    false);

                for (int j = 0; j < numOfNoDistanceHits; j++)
                {
                    if (_hitsNoDistance[j].collider == _hits[i].collider)
                    {
                        isTouching = true;
                        break;
                    }
                }

                if (isTouching)
                {
                    continue;
                }

                if (_velocity != Vector2.zero)
                {
                    Vector3 oneWayPlatformForward = _hits[i].collider.transform.TransformDirection(Vector3.up);

                    float dot = Vector3.Dot(
                        oneWayPlatformForward,
                        _velocity);

                    // We check to see if the effector will play a role.
                    if (dot > ONE_WAY_DOT_CHECK)
                    {
                        // ignore
                        continue;
                    }
                }
            }

            Vector3 toHit = _collider2D.bounds.center - (Vector3)_hits[i].centroid;

            if (toHit.sqrMagnitude < closeBy)
            {
                closeBy = toHit.sqrMagnitude;
                closestHit = _hits[i];
            }
        }

        return closestHit;
    }

    private CollidedSurface CheckGround(float distance, bool forceDistance = false, bool ignoreIfFlat = false)
    {
        CollidedSurface surfaces = CollidedSurface.None;
        RaycastHit2D closestHit = GetClosestHit(Vector3.down, distance);

        if (ignoreIfFlat && closestHit.collider != null && closestHit.normal == Vector2.up)
        {
            return surfaces;
        }

        _collidersUpAgainst[DIRECTION_DOWN] = closestHit.collider;
        _collidedNormals[DIRECTION_DOWN] = closestHit.normal;

        if (closestHit.collider != null)
        {

            surfaces |= CollidedSurface.Ground;

            if (transform.position.y - closestHit.centroid.y < distanceFromEnvironment || forceDistance)
            {
                transform.position += (distanceFromEnvironment - (transform.position.y - closestHit.centroid.y)) * Vector3.up;
            }
        }

        return surfaces;
    }

    private CollidedSurface CheckSurroundings()
    {
        CollidedSurface surfaces = CollidedSurface.None;

        // Left
        RaycastHit2D closestHit = GetClosestHit(Vector3.left, checkDistance);

        _collidersUpAgainst[DIRECTION_LEFT] = closestHit.collider;
        _collidedNormals[DIRECTION_LEFT] = closestHit.normal;

        if (closestHit.collider != null)
        {
            surfaces |= CollidedSurface.LeftWall;

            if (transform.position.x - closestHit.centroid.x < distanceFromEnvironment)
            {
                transform.position += (distanceFromEnvironment - (transform.position.x - closestHit.centroid.x)) * Vector3.right;
            }
        }

        // Ceiling
        closestHit = GetClosestHit(Vector3.up, checkDistance);

        _collidersUpAgainst[DIRECTION_UP] = closestHit.collider;
        _collidedNormals[DIRECTION_UP] = closestHit.normal;

        if (closestHit.collider != null)
        {
            surfaces |= CollidedSurface.Ceiling;

            if (closestHit.centroid.y - transform.position.y < distanceFromEnvironment)
            {
                transform.position += (distanceFromEnvironment - (closestHit.centroid.y - transform.position.y)) * Vector3.down;
            }
        }

        // Right
        closestHit = GetClosestHit(Vector3.right, checkDistance);

        _collidersUpAgainst[DIRECTION_RIGHT] = closestHit.collider;
        _collidedNormals[DIRECTION_RIGHT] = closestHit.normal;

        if (closestHit.collider != null)
        {
            surfaces |= CollidedSurface.RightWall;

            if (closestHit.centroid.x - transform.position.x < distanceFromEnvironment)
            {
                transform.position += (distanceFromEnvironment - (closestHit.centroid.x - transform.position.x)) * Vector3.left;
            }
        }

        // Ground
        surfaces |= CheckGround(checkDistance);

        return surfaces;
    }

    private float CalculateSpeedNeeded(float height)
    {
        return Mathf.Sqrt(-2 * height * gravityMultiplier * Physics2D.gravity.y);
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
        min.y -= checkDistance;
        max.y = box.min.y;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

        // Left check box
        min = box.min;
        max = box.max;
        min.x -= checkDistance;
        max.x = box.min.x;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

        // Right check box
        min = box.min;
        max = box.max;
        min.x = box.max.x;
        max.x += checkDistance;
        Gizmos.DrawWireCube(new Vector2((min.x + max.x) / 2, (min.y + max.y) / 2), new Vector2(max.x - min.x, min.y - max.y));

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

        if (debug)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_point, 0.01f);
            Gizmos.DrawWireCube(_prevPosPlat.center, _prevPosPlat.size);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(_startPosMotor.center, _startPosMotor.size);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_point2, 0.01f);
            Gizmos.DrawWireCube(_movedPosMotor.center, _movedPosMotor.size);
        }
    }

    #endregion
}
