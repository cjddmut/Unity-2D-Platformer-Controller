using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;

/**
 * The motor has quite a lot of customizations that can be messed with to change its behavior. However, not
 * every option will do something depending on what is allowed. So this editor script will hide fields that
 * are no longer useful.
 **/ 
[CustomEditor(typeof(PlayerMotor2D))]
[CanEditMultipleObjects]
public class PlayerMotor2DEditor : Editor
{
    // Gah! This is so unwieldy! I wanted to use reflection but I suspect then serializedObject.Update() and
    // serializedObject.ApplyModifiedProperties() wouldn't work right. Though, I didn't verify.

    SerializedProperty alwaysOnGroundProp;
    SerializedProperty accelerateProp;
    SerializedProperty groundAccelerationProp;
    SerializedProperty airAccelerationProp;
    
    SerializedProperty maxGroundSpeedProp;
    SerializedProperty maxAirSpeedProp;
    SerializedProperty maxFallSpeedProp;
    SerializedProperty maxFastFallSpeedProp;

    SerializedProperty airDragProp;

    SerializedProperty baseJumpProp;
    SerializedProperty extraJumpHeightProp;
    SerializedProperty allowDoubleJumpProp;

    SerializedProperty allowWallJumpProp;

    SerializedProperty allowWallClingProp;
    SerializedProperty wallClingDurationProp;

    SerializedProperty allowWallSlideProp;
    SerializedProperty wallSlideSpeedProp;

    SerializedProperty allowCornerGrabProp;
    SerializedProperty cornerGrabDurationProp;
    SerializedProperty cornerDistanceCheckProp;

    SerializedProperty canDashProp;
    SerializedProperty dashCooldownProp;
    SerializedProperty dashSpeedProp;
    SerializedProperty dashDurationProp;
    SerializedProperty changeLayerDuringDashProp;
    SerializedProperty dashLayerProp;
    SerializedProperty checkMaskProp;
    SerializedProperty checkDistanceProp;

    SerializedProperty inputThresholdProp;
    SerializedProperty heavyInputThresholdProp;

    void OnEnable()
    {
        alwaysOnGroundProp = serializedObject.FindProperty("alwaysOnGround");
        accelerateProp = serializedObject.FindProperty("accelerate");
        groundAccelerationProp = serializedObject.FindProperty("groundAcceleration");
        airAccelerationProp = serializedObject.FindProperty("airAcceleration");

        maxGroundSpeedProp = serializedObject.FindProperty("maxGroundSpeed");
        maxAirSpeedProp = serializedObject.FindProperty("maxAirSpeed");
        maxFallSpeedProp = serializedObject.FindProperty("maxFallSpeed");
        maxFastFallSpeedProp = serializedObject.FindProperty("maxFastFallSpeed");

        airDragProp = serializedObject.FindProperty("airDrag");

        baseJumpProp = serializedObject.FindProperty("baseJump");
        extraJumpHeightProp = serializedObject.FindProperty("extraJumpHeight");
        allowDoubleJumpProp = serializedObject.FindProperty("allowDoubleJump");

        allowWallJumpProp = serializedObject.FindProperty("allowWallJump");

        allowWallClingProp = serializedObject.FindProperty("allowWallCling");
        wallClingDurationProp = serializedObject.FindProperty("wallClingDuration");

        allowWallSlideProp = serializedObject.FindProperty("allowWallSlide");
        wallSlideSpeedProp = serializedObject.FindProperty("wallSlideSpeed");

        allowCornerGrabProp = serializedObject.FindProperty("allowCornerGrab");
        cornerGrabDurationProp = serializedObject.FindProperty("cornerGrabDuration");
        cornerDistanceCheckProp = serializedObject.FindProperty("cornerDistanceCheck");

        canDashProp = serializedObject.FindProperty("canDash");
        dashCooldownProp = serializedObject.FindProperty("dashCooldown");
        dashSpeedProp = serializedObject.FindProperty("dashSpeed");
        dashDurationProp = serializedObject.FindProperty("dashDuration");
        changeLayerDuringDashProp = serializedObject.FindProperty("changeLayerDuringDash");
        dashLayerProp = serializedObject.FindProperty("dashLayer");
        checkMaskProp = serializedObject.FindProperty("checkMask");
        checkDistanceProp = serializedObject.FindProperty("checkDistance");

        inputThresholdProp = serializedObject.FindProperty("inputThreshold");
        heavyInputThresholdProp = serializedObject.FindProperty("heavyInputThreshold");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(alwaysOnGroundProp, new GUIContent("Always On Ground"));

        if (alwaysOnGroundProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue)
        {
            EditorGUILayout.PropertyField(checkMaskProp, new GUIContent("Environment Check Mask"));
            EditorGUILayout.PropertyField(checkDistanceProp, new GUIContent("Environment Check Distance"));
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(accelerateProp, new GUIContent("Acceleration"));

        if (accelerateProp.hasMultipleDifferentValues || accelerateProp.boolValue)
        {
            EditorGUILayout.PropertyField(groundAccelerationProp, new GUIContent("Ground Acceleration"));

            if (accelerateProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue)
            {
                EditorGUILayout.PropertyField(airAccelerationProp, new GUIContent("Air Acceleration"));
            }
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(maxGroundSpeedProp, new GUIContent("Max Ground Speed"));

        if (alwaysOnGroundProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue)
        {
            EditorGUILayout.PropertyField(maxAirSpeedProp, new GUIContent("Max Horizontal Air Speed"));
            EditorGUILayout.PropertyField(maxFallSpeedProp, new GUIContent("Max Fall Speed"));
            EditorGUILayout.PropertyField(maxFastFallSpeedProp, new GUIContent("Max Fast Fall Speed"));
            EditorGUILayout.PropertyField(airDragProp, new GUIContent("Horizontal Air Drag"));

        }

        EditorGUILayout.Separator();

        if (alwaysOnGroundProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue)
        {
            EditorGUILayout.PropertyField(baseJumpProp, new GUIContent("Base Jump Height"));
            EditorGUILayout.PropertyField(extraJumpHeightProp, new GUIContent("Held Extra Jump Height"));
            EditorGUILayout.PropertyField(allowDoubleJumpProp, new GUIContent("Allow Double Jump"));
            EditorGUILayout.PropertyField(allowWallJumpProp, new GUIContent("Allow Wall Jump"));

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
                EditorGUILayout.PropertyField(cornerDistanceCheckProp, new GUIContent("Corner Distance Check"));
            }

            EditorGUILayout.Separator(); 
        }

        EditorGUILayout.PropertyField(canDashProp, new GUIContent("Allow Dashing"));

        if (canDashProp.hasMultipleDifferentValues || canDashProp.boolValue)
        {
            EditorGUILayout.PropertyField(dashCooldownProp, new GUIContent("Dash Cooldown"));
            EditorGUILayout.PropertyField(dashSpeedProp, new GUIContent("Dash Speed"));
            EditorGUILayout.PropertyField(dashDurationProp, new GUIContent("Dash Duration"));
            EditorGUILayout.PropertyField(changeLayerDuringDashProp, new GUIContent("Change Layer During Dash"));

            if (changeLayerDuringDashProp.hasMultipleDifferentValues || changeLayerDuringDashProp.boolValue)
            {
                if (!dashLayerProp.hasMultipleDifferentValues)
                {
                    dashLayerProp.intValue = EditorGUILayout.LayerField("Dash Layer", dashLayerProp.intValue);
                }
                else
                {
                    // I can't find a way to work with enumerated layers and instead if there are different multiple values
                    // then will just display the integer. (I might be able to construct my own list and display that, will
                    // look into later).
                    EditorGUILayout.PropertyField(dashLayerProp, new GUIContent("Dash Layer"));
                }
            }
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(inputThresholdProp, new GUIContent("Input Threshold"));

        if ((alwaysOnGroundProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue) &&
            ((allowWallClingProp.hasMultipleDifferentValues || allowWallClingProp.boolValue) ||
            (allowCornerGrabProp.hasMultipleDifferentValues || allowCornerGrabProp.boolValue) ||
            (allowWallSlideProp.hasMultipleDifferentValues || allowWallSlideProp.boolValue)))
        {
            EditorGUILayout.PropertyField(heavyInputThresholdProp, new GUIContent("Heavy Input Threshold"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
