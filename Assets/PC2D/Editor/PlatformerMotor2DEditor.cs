using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

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

    SerializedProperty maxGroundSpeedProp;
    SerializedProperty timeToMaxGroundSpeedProp;
    SerializedProperty groundStopDistanceProp;

    SerializedProperty maxAirSpeedProp;
    SerializedProperty timeToMaxAirSpeedProp;
    SerializedProperty airStopDistanceProp;
    
    SerializedProperty maxFallSpeedProp;
    SerializedProperty maxFastFallSpeedProp;
    SerializedProperty fastFallGravityMultiplierProp;
    SerializedProperty preserveHorizontalMomentumOnLandingProp;

    SerializedProperty baseJumpHeightProp;
    SerializedProperty extraJumpHeightProp;
    SerializedProperty allowDoubleJumpProp;

    SerializedProperty allowWallJumpProp;
    SerializedProperty wallJumpMultiplierProp;

    SerializedProperty allowWallClingProp;
    SerializedProperty wallClingDurationProp;

    SerializedProperty allowWallSlideProp;
    SerializedProperty wallSlideSpeedProp;

    SerializedProperty allowCornerGrabProp;
    SerializedProperty cornerJumpMultiplierProp;
    SerializedProperty cornerGrabDurationProp;
    SerializedProperty cornerDistanceCheckProp;

    SerializedProperty allowDashProp;
    SerializedProperty dashCooldownProp;
    SerializedProperty dashDistanceProp;
    SerializedProperty dashDurationProp;
    SerializedProperty dashEasingFunctionProp;
    SerializedProperty dashLayerProp;
    SerializedProperty checkMaskProp;
    SerializedProperty checkDistanceProp;

    SerializedProperty wallInteractionThresholdProp;

    void OnEnable()
    {
        maxGroundSpeedProp = serializedObject.FindProperty("maxGroundSpeed");
        timeToMaxGroundSpeedProp = serializedObject.FindProperty("timeToMaxGroundSpeed");
        groundStopDistanceProp = serializedObject.FindProperty("groundStopDistance");

        maxAirSpeedProp = serializedObject.FindProperty("maxAirSpeed");
        timeToMaxAirSpeedProp = serializedObject.FindProperty("timeToMaxAirSpeed");
        airStopDistanceProp = serializedObject.FindProperty("airStopDistance");

        maxFallSpeedProp = serializedObject.FindProperty("maxFallSpeed");
        maxFastFallSpeedProp = serializedObject.FindProperty("maxFastFallSpeed");
        fastFallGravityMultiplierProp = serializedObject.FindProperty("fastFallGravityMultiplier");
        preserveHorizontalMomentumOnLandingProp = serializedObject.FindProperty("preserveHorizontalMomentumOnLanding");

        baseJumpHeightProp = serializedObject.FindProperty("baseJumpHeight");
        extraJumpHeightProp = serializedObject.FindProperty("extraJumpHeight");
        allowDoubleJumpProp = serializedObject.FindProperty("allowDoubleJump");

        allowWallJumpProp = serializedObject.FindProperty("allowWallJump");
        wallJumpMultiplierProp = serializedObject.FindProperty("wallJumpMultiplier");

        allowWallClingProp = serializedObject.FindProperty("allowWallCling");
        wallClingDurationProp = serializedObject.FindProperty("wallClingDuration");

        allowWallSlideProp = serializedObject.FindProperty("allowWallSlide");
        wallSlideSpeedProp = serializedObject.FindProperty("wallSlideSpeed");

        allowCornerGrabProp = serializedObject.FindProperty("allowCornerGrab");
        cornerJumpMultiplierProp = serializedObject.FindProperty("cornerJumpMultiplier");
        cornerGrabDurationProp = serializedObject.FindProperty("cornerGrabDuration");
        cornerDistanceCheckProp = serializedObject.FindProperty("cornerDistanceCheck");

        allowDashProp = serializedObject.FindProperty("allowDash");
        dashCooldownProp = serializedObject.FindProperty("dashCooldown");
        dashDistanceProp = serializedObject.FindProperty("dashDistance");
        dashDurationProp = serializedObject.FindProperty("dashDuration");
        dashEasingFunctionProp = serializedObject.FindProperty("dashEasingFunction");
        dashLayerProp = serializedObject.FindProperty("dashLayer");

        checkMaskProp = serializedObject.FindProperty("checkMask");
        checkDistanceProp = serializedObject.FindProperty("checkDistance");

        wallInteractionThresholdProp = serializedObject.FindProperty("wallInteractionThreshold");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(checkMaskProp, new GUIContent("Environment Check Mask"));
        EditorGUILayout.PropertyField(checkDistanceProp, new GUIContent("Environment Check Distance"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(maxGroundSpeedProp, new GUIContent("Ground Speed"));
        EditorGUILayout.PropertyField(timeToMaxGroundSpeedProp, new GUIContent("Time To Ground Speed"));
        EditorGUILayout.PropertyField(groundStopDistanceProp, new GUIContent("Ground Stop Distance"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(maxAirSpeedProp, new GUIContent("Horizontal Air Speed"));
        EditorGUILayout.PropertyField(timeToMaxAirSpeedProp, new GUIContent("Time To Air Speed"));
        EditorGUILayout.PropertyField(airStopDistanceProp, new GUIContent("Air Stop Distance"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(maxFallSpeedProp, new GUIContent("Max Fall Speed"));
        EditorGUILayout.PropertyField(maxFastFallSpeedProp, new GUIContent("Max Fast Fall Speed"));
        EditorGUILayout.PropertyField(fastFallGravityMultiplierProp, new GUIContent("Fast Fall Gravity Multiplier"));
        EditorGUILayout.PropertyField(preserveHorizontalMomentumOnLandingProp, new GUIContent("Preserve Momentum When Landing"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(baseJumpHeightProp, new GUIContent("Base Jump Height"));
        EditorGUILayout.PropertyField(extraJumpHeightProp, new GUIContent("Held Extra Jump Height"));
        EditorGUILayout.PropertyField(allowDoubleJumpProp, new GUIContent("Allow Double Jump"));

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(allowWallJumpProp, new GUIContent("Allow Wall Jump"));

        if (allowWallJumpProp.hasMultipleDifferentValues || allowWallJumpProp.boolValue)
        {
            EditorGUILayout.PropertyField(wallJumpMultiplierProp, new GUIContent("Wall Jump Multiplier"));
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(allowWallClingProp, new GUIContent("Allow Wall Cling"));

        if (allowWallClingProp.hasMultipleDifferentValues || allowWallClingProp.boolValue)
        {
            EditorGUILayout.PropertyField(wallClingDurationProp, new GUIContent("Wall Cling Duration"));
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(allowWallSlideProp, new GUIContent("Allow Wall Slide"));

        if (allowWallSlideProp.hasMultipleDifferentValues || allowWallSlideProp.boolValue)
        {
            EditorGUILayout.PropertyField(wallSlideSpeedProp, new GUIContent("Wall Slide Speed"));
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(allowCornerGrabProp, new GUIContent("Allow Corner Grab"));

        if (allowCornerGrabProp.hasMultipleDifferentValues || allowCornerGrabProp.boolValue)
        {
            EditorGUILayout.PropertyField(cornerGrabDurationProp, new GUIContent("Corner Grab Duration"));
            EditorGUILayout.PropertyField(cornerJumpMultiplierProp, new GUIContent("Corner Jump Multiplier"));
            EditorGUILayout.PropertyField(cornerDistanceCheckProp, new GUIContent("Corner Distance Check"));
        }

        EditorGUILayout.Separator();

        if ((allowWallClingProp.hasMultipleDifferentValues || allowWallClingProp.boolValue) ||
            (allowCornerGrabProp.hasMultipleDifferentValues || allowCornerGrabProp.boolValue) ||
            (allowWallSlideProp.hasMultipleDifferentValues || allowWallSlideProp.boolValue))
        {
            EditorGUILayout.PropertyField(wallInteractionThresholdProp, new GUIContent("Wall Interaction Threshold"));
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(allowDashProp, new GUIContent("Allow Dashing"));

        if (allowDashProp.hasMultipleDifferentValues || allowDashProp.boolValue)
        {
            EditorGUILayout.PropertyField(dashDistanceProp, new GUIContent("Dash Distance"));
            EditorGUILayout.PropertyField(dashDurationProp, new GUIContent("Dash Duration"));
            EditorGUILayout.PropertyField(dashCooldownProp, new GUIContent("Dash Cooldown"));
            EditorGUILayout.PropertyField(dashEasingFunctionProp, new GUIContent("Dash Easing Function"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
