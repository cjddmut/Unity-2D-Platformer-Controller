using PC2D;
using UnityEngine;
using UnityEditor;

/**
 * The motor has quite a lot of customizations that can be messed with to change its behavior. However, not
 * every option will do something depending on what is allowed. So this editor script will hide fields that
 * are no longer useful.
 **/
[CustomEditor(typeof(PlatformerMotor2D))]
[CanEditMultipleObjects]
public class PlatformerMotor2DEditor : Editor
{
    // Gah! This is so unwieldy! I wanted to use reflection but I suspect then serializedObject.Update() and
    // serializedObject.ApplyModifiedProperties() wouldn't work right. Though, I didn't verify.

    private SerializedProperty _maxGroundSpeedProp;
    private SerializedProperty _timeToMaxGroundSpeedProp;
    private SerializedProperty _groundStopDistanceProp;

    private SerializedProperty _maxAirSpeedProp;
    private SerializedProperty _timeToMaxAirSpeedProp;
    private SerializedProperty _airStopDistanceProp;
    private SerializedProperty _canChangeDirInAirProp;

    private SerializedProperty _maxFallSpeedProp;
    private SerializedProperty _maxFastFallSpeedProp;
    private SerializedProperty _fastFallGravityMultiplierProp;

    private SerializedProperty _baseJumpHeightProp;
    private SerializedProperty _extraJumpHeightProp;
    private SerializedProperty _jumpAllowedGraceProp;
    private SerializedProperty _numAirJumpsProp;

    private SerializedProperty _allowWallJumpProp;
    private SerializedProperty _wallJumpMultiplierProp;

    private SerializedProperty _allowWallClingProp;
    private SerializedProperty _wallClingDurationProp;

    private SerializedProperty _allowWallSlideProp;
    private SerializedProperty _wallSlideSpeedProp;

    private SerializedProperty _allowCornerGrabProp;
    private SerializedProperty _cornerJumpMultiplierProp;
    private SerializedProperty _cornerGrabDurationProp;
    private SerializedProperty _cornerDistanceCheckProp;

    private SerializedProperty _allowDashProp;
    private SerializedProperty _dashCooldownProp;
    private SerializedProperty _dashDistanceProp;
    private SerializedProperty _dashDurationProp;
    private SerializedProperty _dashEasingFunctionProp;
    private SerializedProperty _endDashDelay;

    private SerializedProperty _checkMaskProp;
    private SerializedProperty _movingPlatformMaskProp;
    private SerializedProperty _checkDistanceProp;
    private SerializedProperty _distanceFromEnvironmentProp;
    private SerializedProperty _wallInteractionThresholdProp;

    private bool _showGeneral;
    private bool _showMovement;
    private bool _showJumping;
    private bool _showWallInteractions;
    private bool _showDashing;
    private bool _showInformation;

    void OnEnable()
    {
        _maxGroundSpeedProp = serializedObject.FindProperty("maxGroundSpeed");
        _timeToMaxGroundSpeedProp = serializedObject.FindProperty("timeToMaxGroundSpeed");
        _groundStopDistanceProp = serializedObject.FindProperty("groundStopDistance");

        _maxAirSpeedProp = serializedObject.FindProperty("maxAirSpeed");
        _timeToMaxAirSpeedProp = serializedObject.FindProperty("timeToMaxAirSpeed");
        _airStopDistanceProp = serializedObject.FindProperty("airStopDistance");

        _maxFallSpeedProp = serializedObject.FindProperty("maxFallSpeed");
        _maxFastFallSpeedProp = serializedObject.FindProperty("maxFastFallSpeed");
        _fastFallGravityMultiplierProp = serializedObject.FindProperty("fastFallGravityMultiplier");

        _baseJumpHeightProp = serializedObject.FindProperty("baseJumpHeight");
        _extraJumpHeightProp = serializedObject.FindProperty("extraJumpHeight");
        _jumpAllowedGraceProp = serializedObject.FindProperty("jumpAllowedGrace");
        _numAirJumpsProp = serializedObject.FindProperty("numAirJumps");
        _canChangeDirInAirProp = serializedObject.FindProperty("changeDirectionInAir");

        _allowWallJumpProp = serializedObject.FindProperty("allowWallJump");
        _wallJumpMultiplierProp = serializedObject.FindProperty("wallJumpMultiplier");

        _allowWallClingProp = serializedObject.FindProperty("allowWallCling");
        _wallClingDurationProp = serializedObject.FindProperty("wallClingDuration");

        _allowWallSlideProp = serializedObject.FindProperty("allowWallSlide");
        _wallSlideSpeedProp = serializedObject.FindProperty("wallSlideSpeed");

        _allowCornerGrabProp = serializedObject.FindProperty("allowCornerGrab");
        _cornerJumpMultiplierProp = serializedObject.FindProperty("cornerJumpMultiplier");
        _cornerGrabDurationProp = serializedObject.FindProperty("cornerGrabDuration");
        _cornerDistanceCheckProp = serializedObject.FindProperty("cornerDistanceCheck");

        _allowDashProp = serializedObject.FindProperty("allowDash");
        _dashCooldownProp = serializedObject.FindProperty("dashCooldown");
        _dashDistanceProp = serializedObject.FindProperty("dashDistance");
        _dashDurationProp = serializedObject.FindProperty("dashDuration");
        _dashEasingFunctionProp = serializedObject.FindProperty("dashEasingFunction");
        _endDashDelay = serializedObject.FindProperty("endDashDelay");

        _checkMaskProp = serializedObject.FindProperty("staticEnvironmentLayerMask");
        _movingPlatformMaskProp = serializedObject.FindProperty("movingPlatformLayerMask");
        _checkDistanceProp = serializedObject.FindProperty("checkDistance");
        _distanceFromEnvironmentProp = serializedObject.FindProperty("distanceFromEnvironment");

        _wallInteractionThresholdProp = serializedObject.FindProperty("wallInteractionThreshold");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUIStyle boldStyle = new GUIStyle();
        boldStyle.fontStyle = FontStyle.Bold;

        EditorGUILayout.Separator();
        _showGeneral = EditorGUILayout.Foldout(_showGeneral, "General");

        if (_showGeneral)
        {
            EditorGUILayout.PropertyField(_checkMaskProp, new GUIContent("Static Environment Layer Mask"));
            EditorGUILayout.PropertyField(_movingPlatformMaskProp, new GUIContent("Moving Platform Layer Mask"));
            EditorGUILayout.PropertyField(_checkDistanceProp, new GUIContent("Environment Check Distance"));
            EditorGUILayout.PropertyField(_distanceFromEnvironmentProp, new GUIContent("Minimum Distance From Env"));
            EditorGUILayout.Separator();
        }

        _showMovement = EditorGUILayout.Foldout(_showMovement, "Movement");

        if (_showMovement)
        {
            EditorGUILayout.PropertyField(_maxGroundSpeedProp, new GUIContent("Ground Speed"));
            EditorGUILayout.PropertyField(_timeToMaxGroundSpeedProp, new GUIContent("Time To Ground Speed"));
            EditorGUILayout.PropertyField(_groundStopDistanceProp, new GUIContent("Ground Stop Distance"));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_maxAirSpeedProp, new GUIContent("Horizontal Air Speed"));
            EditorGUILayout.PropertyField(_canChangeDirInAirProp, new GUIContent("Allow Direction Change In Air"));

            if (_canChangeDirInAirProp.boolValue)
            {
                EditorGUILayout.PropertyField(_timeToMaxAirSpeedProp, new GUIContent("Time To Air Speed"));
                EditorGUILayout.PropertyField(_airStopDistanceProp, new GUIContent("Air Stop Distance"));
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_maxFallSpeedProp, new GUIContent("Max Fall Speed"));
            EditorGUILayout.PropertyField(_maxFastFallSpeedProp, new GUIContent("Max Fast Fall Speed"));
            EditorGUILayout.PropertyField(_fastFallGravityMultiplierProp, new GUIContent("Fast Fall Gravity Multiplier"));
            EditorGUILayout.Separator();
        }

        _showJumping = EditorGUILayout.Foldout(_showJumping, "Jumping");

        if (_showJumping)
        {
            EditorGUILayout.PropertyField(_baseJumpHeightProp, new GUIContent("Base Jump Height"));
            EditorGUILayout.PropertyField(_extraJumpHeightProp, new GUIContent("Held Extra Jump Height"));
            EditorGUILayout.PropertyField(_jumpAllowedGraceProp, new GUIContent("Grace For Jump"));
            EditorGUILayout.PropertyField(_numAirJumpsProp, new GUIContent("Air Jumps Allowed"));
            EditorGUILayout.Separator();
        }

        _showWallInteractions = EditorGUILayout.Foldout(_showWallInteractions, "Wall Interactions");

        if (_showWallInteractions)
        {
            EditorGUILayout.PropertyField(_allowWallJumpProp, new GUIContent("Allow Wall Jump"));

            if (_allowWallJumpProp.hasMultipleDifferentValues || _allowWallJumpProp.boolValue)
            {
                EditorGUILayout.PropertyField(_wallJumpMultiplierProp, new GUIContent("Wall Jump Multiplier"));
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_allowWallClingProp, new GUIContent("Allow Wall Cling"));

            if (_allowWallClingProp.hasMultipleDifferentValues || _allowWallClingProp.boolValue)
            {
                EditorGUILayout.PropertyField(_wallClingDurationProp, new GUIContent("Wall Cling Duration"));
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_allowWallSlideProp, new GUIContent("Allow Wall Slide"));

            if (_allowWallSlideProp.hasMultipleDifferentValues || _allowWallSlideProp.boolValue)
            {
                EditorGUILayout.PropertyField(_wallSlideSpeedProp, new GUIContent("Wall Slide Speed"));
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(_allowCornerGrabProp, new GUIContent("Allow Corner Grab"));

            if (_allowCornerGrabProp.hasMultipleDifferentValues || _allowCornerGrabProp.boolValue)
            {
                EditorGUILayout.PropertyField(_cornerGrabDurationProp, new GUIContent("Corner Grab Duration"));
                EditorGUILayout.PropertyField(_cornerJumpMultiplierProp, new GUIContent("Corner Jump Multiplier"));
                EditorGUILayout.PropertyField(_cornerDistanceCheckProp, new GUIContent("Corner Distance Check"));
            }

            EditorGUILayout.Separator();

            if ((_allowWallClingProp.hasMultipleDifferentValues || _allowWallClingProp.boolValue) ||
                (_allowCornerGrabProp.hasMultipleDifferentValues || _allowCornerGrabProp.boolValue) ||
                (_allowWallSlideProp.hasMultipleDifferentValues || _allowWallSlideProp.boolValue))
            {
                EditorGUILayout.PropertyField(_wallInteractionThresholdProp, new GUIContent("Wall Interaction Threshold"));
            }

            EditorGUILayout.Separator();
        }

        _showDashing = EditorGUILayout.Foldout(_showDashing, "Dashing");

        if (_showDashing)
        {
            EditorGUILayout.PropertyField(_allowDashProp, new GUIContent("Allow Dashing"));

            if (_allowDashProp.hasMultipleDifferentValues || _allowDashProp.boolValue)
            {
                EditorGUILayout.PropertyField(_dashDistanceProp, new GUIContent("Dash Distance"));
                EditorGUILayout.PropertyField(_dashDurationProp, new GUIContent("Dash Duration"));
                EditorGUILayout.PropertyField(_dashCooldownProp, new GUIContent("Dash Cooldown"));
                EditorGUILayout.PropertyField(_dashEasingFunctionProp, new GUIContent("Dash Easing Function"));
                EditorGUILayout.PropertyField(_endDashDelay, new GUIContent("Gravity Delay After Dash"));
            }

            EditorGUILayout.Separator();
        }

        if (!serializedObject.isEditingMultipleObjects)
        {
            _showInformation = EditorGUILayout.Foldout(_showInformation, "Information");

            if (_showInformation)
            {
                EditorGUILayout.HelpBox(
                    "Approx Jump Distance: " + _maxAirSpeedProp.floatValue * 2 *
                        Mathf.Sqrt(2 *
                        (_baseJumpHeightProp.floatValue + _extraJumpHeightProp.floatValue) /
                        (((PlatformerMotor2D)target).GetComponent<Rigidbody2D>().gravityScale * Mathf.Abs(Physics2D.gravity.y))), 
                    MessageType.Info, 
                    true);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
