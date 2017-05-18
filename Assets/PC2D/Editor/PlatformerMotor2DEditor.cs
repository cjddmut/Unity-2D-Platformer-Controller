using System.Collections.Generic;
using System.Text;
using PC2D;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlatformerMotor2D))]
[CanEditMultipleObjects]
public class PlatformerMotor2DEditor : Editor
{
    private Dictionary<string, SerializedProperty> _properties = new Dictionary<string, SerializedProperty>();
    private List<Property> _timingProperties = new List<Property>();

    private class Property
    {
        public string name;
        public string text;

        public Property(string n, string t)
        {
            name = n;
            text = t;
        }
    }

    private static bool _showGeneral;
    private static bool _showMovement;
    private static bool _showSlopes;
    private static bool _showJumping;
    private static bool _showWallInteractions;
    private static bool _showDashing;
    private static bool _showInformation;

    #region Properties
    private readonly Property STATIC_ENV_LAYER_MASK = new Property("staticEnvLayerMask", "Static Environment Layer Mask");
    private readonly Property ENV_CHECK_DISTANCE = new Property("envCheckDistance", "Environment Check Distance");
    private readonly Property MIN_DISTANCE_FROM_ENV = new Property("minDistanceFromEnv", "Minimum Distance from Environment");
    private readonly Property NUM_OF_ITERATIONS = new Property("numOfIterations", "Number of Iterations");
    private readonly Property ENABLE_ONE_WAY_PLATFORMS = new Property("enableOneWayPlatforms", "Enable One Way Platforms");
    private readonly Property MOVING_PLATFORM_LAYER_MASK = new Property("movingPlatformLayerMask", "Moving Platforms Layer Mask");
    private readonly Property RAYCASTS_PER_SIDE = new Property("additionalRaycastsPerSide", "Additional Raycasts per Side");

    private readonly Property GROUND_SPEED = new Property("groundSpeed", "Ground Speed");
    private readonly Property TIME_TO_GROUND_SPEED = new Property("timeToGroundSpeed", "Time to Ground Speed");
    private readonly Property GROUND_STOP_DISTANCE = new Property("groundStopDistance", "Ground Stop Distance");

    private readonly Property AIR_SPEED = new Property("airSpeed", "Air Speed");
    private readonly Property CHANGE_DIR_IN_AIR = new Property("changeDirectionInAir", "Enable Change Direction in Air");
    private readonly Property TIME_TO_AIR_SPEED = new Property("timeToAirSpeed", "Time to Air Speed");
    private readonly Property AIR_STOP_DISTANCE = new Property("airStopDistance", "Air Stop Distance");

    private readonly Property FALL_SPEED = new Property("fallSpeed", "Fall Speed");
    private readonly Property GRAVITY_MUTLIPLIER = new Property("gravityMultiplier", "Fall Gravity Multiplier");
    private readonly Property FAST_FALL_SPEED = new Property("fastFallSpeed", "Fast Fall Speed");

    private readonly Property FAST_FALL_GRAVITY_MULTIPLIER = new Property(
        "fastFallGravityMultiplier",
        "Fast Fall Gravity Multiplier");

    private readonly Property LADDER_SPEED = new Property(
        "ladderSpeed",
        "Ladder Speed");

    private readonly Property ENABLE_SLOPES = new Property("enableSlopes", "Enable Slopes");

    private readonly Property ANGLE_ALLOWED_FOR_SLOPES = new Property(
        "angleAllowedForMoving",
        "Angle (Degrees) Allowed For Moving");

    private readonly Property CHANGE_SPEED_ON_SLOPES = new Property("changeSpeedOnSlopes", "Change Speed on Slopes");
    private readonly Property MIN_SPEED_TO_MOVE_UP_SLIPPERY_SLOPE = new Property(
        "minimumSpeedToMoveUpSlipperySlope",
        "Minimum Speed to Move Up Slippery Slope");

    private readonly Property SLOPES_SPEED_MULTIPLIER = new Property("speedMultiplierOnSlope", "Speed Multiplier on Slopes");
    private readonly Property SLOPE_NORMAL = new Property("slopeNormal", "Touching slope angle");
    private readonly Property STICK_TO_GROUND = new Property("stickOnGround", "Stick to Ground");
    private readonly Property STICK_CHECK_DISTANCE = new Property("distanceToCheckToStick", "Ground Check Distance to Stick");

    private readonly Property JUMP_HEIGHT = new Property("jumpHeight", "Jump Height");
    private readonly Property EXTRA_JUMP_HEIGHT = new Property("extraJumpHeight", "Extra Jump Height");
    private readonly Property NUM_OF_AIR_JUMPS = new Property("numOfAirJumps", "Number of Air Jumps");
    private readonly Property JUMP_WINDOW_WHEN_FALLING = new Property("jumpWindowWhenFalling", "Jump Window When Falling");
    private readonly Property JUMP_WINDOW_WHEN_ACTIVATED = new Property("jumpWindowWhenActivated", "Jump Window When Activated");

    private readonly Property ENABLE_WALL_JUMPS = new Property("enableWallJumps", "Enable Wall Jumps");
    private readonly Property WALL_JUMP_MULTIPLIER = new Property("wallJumpMultiplier", "Wall Jump Multiplier");
    private readonly Property WALL_JUMP_DEGREE = new Property("wallJumpAngle", "Wall Jump Angle (Degrees)");

    private readonly Property ENABLE_WALL_STICKS = new Property("enableWallSticks", "Enable Wall Sticks");
    private readonly Property WALL_STICK_DURATION = new Property("wallSticksDuration", "Wall Stick Duration");

    private readonly Property ENABLE_WALL_SLIDES = new Property("enableWallSlides", "Enable Wall Slides");
    private readonly Property WALL_SLIDE_SPEED = new Property("wallSlideSpeed", "Wall Slide Speed");
    private readonly Property TIME_TO_WALL_SLIDE_SPEED = new Property("timeToWallSlideSpeed", "Time to Wall Slide Speed");

    private readonly Property ENABLE_CORNER_GRABS = new Property("enableCornerGrabs", "Enable Corner Grabs");
    private readonly Property CORNER_JUMP_MULTIPLIER = new Property("cornerJumpMultiplier", "Corner Jump Multiplier");
    private readonly Property CORNER_GRAB_DURATION = new Property("cornerGrabDuration", "Corner Grab Duration");
    private readonly Property CORNER_DISTANCE_CHECK = new Property("cornerDistanceCheck", "Distance Check for Corner Grab");
    private readonly Property NORMALIZED_VALID_WALL_INTERACTION = new Property("normalizedValidWallInteraction", "Valid Normalized Interaction Area");

    private readonly Property WALL_INTERACTION_IGNORE_MOVEMENT_DURATION = new Property(
        "ignoreMovementAfterJump",
        "Ignore Movement After Jump Duration");

    private readonly Property WALL_INTERACTION_COOLDOWN = new Property("wallInteractionCooldown", "Wall Interaction Cooldown");
    private readonly Property WALL_INTERACTION_THRESHOLD = new Property("wallInteractionThreshold", "Wall Interaction Threshold");

    private readonly Property ENABLE_DASHES = new Property("enableDashes", "Enable Dashes");
    private readonly Property DASH_DISTANCE = new Property("dashDistance", "Dash Distance");
    private readonly Property DASH_EASING_FUNCTION = new Property("dashEasingFunction", "Dash Easing Function");
    private readonly Property DASH_DURATION = new Property("dashDuration", "Dash Duration");
    private readonly Property DASH_COOLDOWN = new Property("dashCooldown", "Dash Cooldown");
    private readonly Property END_DASH_DELAY = new Property("endDashNoGravityDuration", "Gravity Delay After Dash");
    #endregion

    private void OnEnable()
    {
        _properties.Clear();
        SerializedProperty property = serializedObject.GetIterator();

        while (property.NextVisible(true))
        {
            _properties[property.name] = property.Copy();
        }

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _timingProperties.Clear();

        GUIStyle boldStyle = new GUIStyle();
        boldStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.Separator();

        _showGeneral = EditorGUILayout.Foldout(_showGeneral, "General");

        if (_showGeneral)
        {
            DisplayRegularField(STATIC_ENV_LAYER_MASK);
            DisplayRegularField(ENV_CHECK_DISTANCE);
            DisplayRegularField(MIN_DISTANCE_FROM_ENV);
            DisplayRegularField(NUM_OF_ITERATIONS);
            DisplayRegularField(ENABLE_ONE_WAY_PLATFORMS);

            EditorGUILayout.Separator();

            DisplayRegularField(MOVING_PLATFORM_LAYER_MASK);

            if (_properties[MOVING_PLATFORM_LAYER_MASK.name].hasMultipleDifferentValues ||
                _properties[MOVING_PLATFORM_LAYER_MASK.name].intValue != 0)
            {
                DisplayRegularField(RAYCASTS_PER_SIDE);
            }

            EditorGUILayout.Separator();
        }

        _showMovement = EditorGUILayout.Foldout(_showMovement, "Movement");

        if (_showMovement)
        {
            DisplayRateField(GROUND_SPEED);
            DisplayAccelerationField(TIME_TO_GROUND_SPEED);
            DisplayRegularField(GROUND_STOP_DISTANCE);

            EditorGUILayout.Separator();


            DisplayRateField(AIR_SPEED);
            DisplayRegularField(CHANGE_DIR_IN_AIR);

            if (_properties[CHANGE_DIR_IN_AIR.name].hasMultipleDifferentValues || _properties[CHANGE_DIR_IN_AIR.name].boolValue)
            {
                DisplayAccelerationField(TIME_TO_AIR_SPEED);
                DisplayRegularField(AIR_STOP_DISTANCE);
            }

            EditorGUILayout.Separator();

            DisplayRateField(FALL_SPEED);
            DisplayRegularField(GRAVITY_MUTLIPLIER);
            DisplayRateField(FAST_FALL_SPEED);
            DisplayRegularField(FAST_FALL_GRAVITY_MULTIPLIER);

            EditorGUILayout.Separator();

            DisplayRegularField(LADDER_SPEED);

            EditorGUILayout.Separator();
        }

        _showJumping = EditorGUILayout.Foldout(_showJumping, "Jumping");

        if (_showJumping)
        {
            DisplayRegularField(JUMP_HEIGHT);
            DisplayRegularField(EXTRA_JUMP_HEIGHT);
            DisplayRegularField(NUM_OF_AIR_JUMPS);
            DisplayTimingField(JUMP_WINDOW_WHEN_FALLING);
            DisplayTimingField(JUMP_WINDOW_WHEN_ACTIVATED);

            EditorGUILayout.Separator();
        }

        _showSlopes = EditorGUILayout.Foldout(_showSlopes, "Slopes");

        if (_showSlopes)
        {
            DisplayRegularField(ENABLE_SLOPES);

            if (_properties[ENABLE_SLOPES.name].hasMultipleDifferentValues || _properties[ENABLE_SLOPES.name].boolValue)
            {
                DisplayRegularField(ANGLE_ALLOWED_FOR_SLOPES);
                DisplayRegularField(MIN_SPEED_TO_MOVE_UP_SLIPPERY_SLOPE);
                DisplayRegularField(CHANGE_SPEED_ON_SLOPES);

                if (_properties[CHANGE_SPEED_ON_SLOPES.name].hasMultipleDifferentValues ||
                    _properties[CHANGE_SPEED_ON_SLOPES.name].boolValue)
                {
                    DisplayRegularField(SLOPES_SPEED_MULTIPLIER);
                }

                DisplayRegularField(STICK_TO_GROUND);

                if (_properties[STICK_TO_GROUND.name].hasMultipleDifferentValues || _properties[STICK_TO_GROUND.name].boolValue)
                {
                    DisplayRegularField(STICK_CHECK_DISTANCE);
                }
            }

            EditorGUILayout.Separator();
        }

        _showWallInteractions = EditorGUILayout.Foldout(_showWallInteractions, "Wall Interactions");

        if (_showWallInteractions)
        {
            DisplayRegularField(ENABLE_WALL_JUMPS);

            if (_properties[ENABLE_WALL_JUMPS.name].hasMultipleDifferentValues || _properties[ENABLE_WALL_JUMPS.name].boolValue)
            {
                DisplayRegularField(WALL_JUMP_MULTIPLIER);
                DisplayRegularField(WALL_JUMP_DEGREE);
            }

            EditorGUILayout.Separator();

            DisplayRegularField(ENABLE_WALL_STICKS);

            if (_properties[ENABLE_WALL_STICKS.name].hasMultipleDifferentValues || _properties[ENABLE_WALL_STICKS.name].boolValue)
            {
                DisplayTimingField(WALL_STICK_DURATION);
            }

            EditorGUILayout.Separator();

            DisplayRegularField(ENABLE_WALL_SLIDES);

            if (_properties[ENABLE_WALL_SLIDES.name].hasMultipleDifferentValues || _properties[ENABLE_WALL_SLIDES.name].boolValue)
            {
                DisplayRateField(WALL_SLIDE_SPEED);
                DisplayAccelerationField(TIME_TO_WALL_SLIDE_SPEED);
            }

            EditorGUILayout.Separator();

            DisplayRegularField(ENABLE_CORNER_GRABS);

            if (_properties[ENABLE_CORNER_GRABS.name].hasMultipleDifferentValues || _properties[ENABLE_CORNER_GRABS.name].boolValue)
            {
                DisplayTimingField(CORNER_GRAB_DURATION);
                DisplayRegularField(CORNER_JUMP_MULTIPLIER);
                DisplayRegularField(CORNER_DISTANCE_CHECK);
            }

            EditorGUILayout.Separator();

            if ((_properties[ENABLE_WALL_JUMPS.name].hasMultipleDifferentValues ||
                    _properties[ENABLE_WALL_JUMPS.name].boolValue) ||
                (_properties[ENABLE_CORNER_GRABS.name].hasMultipleDifferentValues ||
                    _properties[ENABLE_CORNER_GRABS.name].boolValue))
            {
                DisplayTimingField(WALL_INTERACTION_IGNORE_MOVEMENT_DURATION);
            }

            EditorGUILayout.Separator();

            if ((_properties[ENABLE_WALL_STICKS.name].hasMultipleDifferentValues ||
                    _properties[ENABLE_WALL_STICKS.name].boolValue) ||
                (_properties[ENABLE_CORNER_GRABS.name].hasMultipleDifferentValues ||
                    _properties[ENABLE_CORNER_GRABS.name].boolValue) ||
                (_properties[ENABLE_WALL_SLIDES.name].hasMultipleDifferentValues ||
                    _properties[ENABLE_WALL_SLIDES.name].boolValue))
            {
                DisplayRegularField(NORMALIZED_VALID_WALL_INTERACTION);
                DisplayTimingField(WALL_INTERACTION_COOLDOWN);
                DisplayRegularField(WALL_INTERACTION_THRESHOLD);
            }

            EditorGUILayout.Separator();
        }

        _showDashing = EditorGUILayout.Foldout(_showDashing, "Dashing");

        if (_showDashing)
        {
            DisplayRegularField(ENABLE_DASHES);

            if (_properties[ENABLE_DASHES.name].hasMultipleDifferentValues || _properties[ENABLE_DASHES.name].boolValue)
            {
                DisplayRegularField(DASH_DISTANCE);

                DisplayTimingField(DASH_DURATION);
                DisplayTimingField(DASH_COOLDOWN);

                DisplayRegularField(DASH_EASING_FUNCTION);
                DisplayTimingField(END_DASH_DELAY);
            }

            EditorGUILayout.Separator();
        }

        if (!serializedObject.isEditingMultipleObjects)
        {
            _showInformation = EditorGUILayout.Foldout(_showInformation, "Information");

            if (_showInformation)
            {
                EditorGUILayout.HelpBox(
                    GetInformation(),
                    MessageType.Info,
                    true);
            }
        }

        CheckValues();

        CheckAndDisplayInfo();

        serializedObject.ApplyModifiedProperties();
    }

    private void CheckAndDisplayInfo()
    {
        if (!Physics2D.queriesStartInColliders &&
            (_properties[MOVING_PLATFORM_LAYER_MASK.name].hasMultipleDifferentValues ||
            _properties[MOVING_PLATFORM_LAYER_MASK.name].intValue != 0))
        {
            EditorGUILayout.HelpBox(
                "'Raycasts Start in Colliders' in the Physics 2D settings needs to be checked on for moving platforms",
                MessageType.Error,
                true);
        }

        if (!_properties[STATIC_ENV_LAYER_MASK.name].hasMultipleDifferentValues &&
            _properties[STATIC_ENV_LAYER_MASK.name].intValue == 0)
        {
            EditorGUILayout.HelpBox(
                "Static Environment Layer Mask is required to be set!",
                MessageType.Error,
                true);
        }

        if (!_properties[STATIC_ENV_LAYER_MASK.name].hasMultipleDifferentValues &&
            (_properties[STATIC_ENV_LAYER_MASK.name].intValue & (1 << ((PlatformerMotor2D)target).gameObject.layer)) != 0)
        {
            EditorGUILayout.HelpBox(
                "The Static Environment Layer Mask can not include the layer the motor is on!",
                MessageType.Error,
                true);
        }

        for (int i = 0; i < _timingProperties.Count; i++)
        {
            CheckAndDisplayTimingWarnings(_timingProperties[i]);
        }
    }


    private void CheckAndDisplayTimingWarnings(Property property)
    {
        if (!_properties[property.name].hasMultipleDifferentValues &&
            !Mathf.Approximately(_properties[property.name].floatValue / Time.fixedDeltaTime,
                Mathf.Round(_properties[property.name].floatValue / Time.fixedDeltaTime)))
        {
            string msg = string.Format(
                "'{0}' is not a multiple of the fixed time step ({1}). This results in an extra frame effectively making '{0}' {2} instead of {3}",
                property.text,
                Time.fixedDeltaTime,
                Globals.GetFrameCount(_properties[property.name].floatValue) * Time.fixedDeltaTime,
                _properties[property.name].floatValue);

            EditorGUILayout.HelpBox(
                msg,
                MessageType.Warning,
                true);
        }
    }

    private void DisplayRegularField(Property property)
    {
        EditorGUILayout.PropertyField(_properties[property.name], new GUIContent(property.text));
    }

    private void DisplayRateField(Property property)
    {
        string frameRate = "-";

        if (!_properties[property.name].hasMultipleDifferentValues)
        {
            frameRate = (_properties[property.name].floatValue * Time.fixedDeltaTime).ToString();
        }

        EditorGUILayout.PropertyField(_properties[property.name],
            new GUIContent(string.Format("{0} ({1} Distance/Frame)", property.text, frameRate)));
    }

    private void DisplayTimingField(Property property)
    {
        _timingProperties.Add(property);

        string frameCount = "-";

        if (!_properties[property.name].hasMultipleDifferentValues)
        {
            frameCount = Globals.GetFrameCount(_properties[property.name].floatValue).ToString();
        }

        EditorGUILayout.PropertyField(_properties[property.name],
            new GUIContent(string.Format("{0} ({1} Frames)", property.text, frameCount)));
    }

    private void DisplayAccelerationField(Property property)
    {
        string frameCount = "-";

        if (!_properties[property.name].hasMultipleDifferentValues)
        {
            frameCount = (_properties[property.name].floatValue / Time.fixedDeltaTime).ToString();
        }

        EditorGUILayout.PropertyField(_properties[property.name],
            new GUIContent(string.Format("{0} ({1} Frames)", property.text, frameCount)));
    }

    private void CheckValues()
    {
        if (!_properties[ENV_CHECK_DISTANCE.name].hasMultipleDifferentValues &&
            _properties[ENV_CHECK_DISTANCE.name].floatValue <= Globals.MINIMUM_DISTANCE_CHECK * 2)
        {
            _properties[ENV_CHECK_DISTANCE.name].floatValue = Globals.MINIMUM_DISTANCE_CHECK * 2;
        }

        if (!_properties[MIN_DISTANCE_FROM_ENV.name].hasMultipleDifferentValues &&
            _properties[MIN_DISTANCE_FROM_ENV.name].floatValue <= Globals.MINIMUM_DISTANCE_CHECK)
        {
            _properties[MIN_DISTANCE_FROM_ENV.name].floatValue = Globals.MINIMUM_DISTANCE_CHECK;
        }

        if (!_properties[NUM_OF_ITERATIONS.name].hasMultipleDifferentValues &&
            _properties[NUM_OF_ITERATIONS.name].intValue < 0)
        {
            _properties[NUM_OF_ITERATIONS.name].intValue = 1;
        }
    }

    private string GetInformation()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendFormat("Approx jump distance: {0}", GetJumpDistance());

        if (_properties[TIME_TO_GROUND_SPEED.name].floatValue != 0)
        {
            sb.AppendFormat(
                "\nGround acceleration: {0}",
                _properties[GROUND_SPEED.name].floatValue / _properties[TIME_TO_GROUND_SPEED.name].floatValue);
        }

        if (_properties[GROUND_STOP_DISTANCE.name].floatValue != 0)
        {
            sb.AppendFormat(
                "\nTime to stop on ground: {0}",
                GetTimeToDistance(
                    _properties[GROUND_STOP_DISTANCE.name].floatValue,
                    _properties[GROUND_SPEED.name].floatValue));
        }

        if (_properties[TIME_TO_AIR_SPEED.name].floatValue != 0 && _properties[CHANGE_DIR_IN_AIR.name].boolValue)
        {
            sb.AppendFormat(
                "\nMax air acceleration: {0}",
                _properties[AIR_SPEED.name].floatValue / _properties[TIME_TO_AIR_SPEED.name].floatValue);
        }

        if (_properties[AIR_STOP_DISTANCE.name].floatValue != 0 && _properties[CHANGE_DIR_IN_AIR.name].boolValue)
        {
            sb.AppendFormat(
                "\nTime to stop on ground: {0}",
                GetTimeToDistance(
                    _properties[AIR_STOP_DISTANCE.name].floatValue,
                    _properties[AIR_SPEED.name].floatValue));
        }

        sb.AppendFormat("\nApprox single jump duration (up & down): {0}",
            Mathf.Sqrt(-8 * (_properties[JUMP_HEIGHT.name].floatValue + _properties[EXTRA_JUMP_HEIGHT.name].floatValue) /
                (_properties[GRAVITY_MUTLIPLIER.name].floatValue * Physics2D.gravity.y)));

        sb.AppendFormat(
            "\nWill hit fall speed cap during jump: {0}",
            (GetTotalJumpSpeed() >= _properties[FALL_SPEED.name].floatValue));

        sb.AppendFormat(
            "\nFrames needed to get reach extra jump height: {0}",
            (_properties[EXTRA_JUMP_HEIGHT.name].floatValue / GetBaseJumpSpeed()) / Time.fixedDeltaTime);

        if (_properties[TIME_TO_WALL_SLIDE_SPEED.name].floatValue != 0 && _properties[ENABLE_WALL_SLIDES.name].boolValue)
        {
            sb.AppendFormat(
                "\nWall slide acceleration: {0}",
                _properties[WALL_SLIDE_SPEED.name].floatValue / _properties[TIME_TO_WALL_SLIDE_SPEED.name].floatValue);
        }

        if (_properties[ENABLE_SLOPES.name].boolValue)
        {
            sb.AppendFormat(
                "\nColliding slope angle: {0}",
                (Vector2.Angle(_properties[SLOPE_NORMAL.name].vector2Value, Vector2.up)));
        }
        
        return sb.ToString();
    }

    private float GetJumpDistance()
    {
        return _properties[AIR_SPEED.name].floatValue * 2 *
               Mathf.Sqrt(2 *
                    (_properties[JUMP_HEIGHT.name].floatValue +
                    _properties[EXTRA_JUMP_HEIGHT.name].floatValue) /
                    (((PlatformerMotor2D)target).gravityMultiplier *
                    Mathf.Abs(Physics2D.gravity.y)));
    }

    private float GetBaseJumpSpeed()
    {
        return Mathf.Sqrt(-2 * ((_properties[JUMP_HEIGHT.name].floatValue) *
            _properties[GRAVITY_MUTLIPLIER.name].floatValue * Physics2D.gravity.y));
    }

    private float GetTotalJumpSpeed()
    {
        return Mathf.Sqrt(-2 * ((_properties[JUMP_HEIGHT.name].floatValue + _properties[EXTRA_JUMP_HEIGHT.name].floatValue) *
            _properties[GRAVITY_MUTLIPLIER.name].floatValue * Physics2D.gravity.y));
    }

    private float GetTimeToDistance(float distance, float maxSpeed)
    {
        float deceleration = (maxSpeed * maxSpeed) / (2 * distance);

        return Mathf.Sqrt(2 * distance / deceleration);
    }
}
