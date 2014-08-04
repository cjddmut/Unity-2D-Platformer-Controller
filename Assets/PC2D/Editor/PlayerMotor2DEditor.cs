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
    SerializedProperty baseJumpProp;
    SerializedProperty extraJumpHeightProp;
    SerializedProperty allowDoubleJumpProp;
    SerializedProperty allowWallJumpProp;
    SerializedProperty allowWallSlideProp;
    SerializedProperty wallSlideSpeedProp;
    SerializedProperty allowCornerGrabProp;
    SerializedProperty cornerHeightCheckProp;
    SerializedProperty cornerWidthCheckProp;
    SerializedProperty canDashProp;
    SerializedProperty dashCooldownProp;
    SerializedProperty dashSpeedProp;
    SerializedProperty dashDurationProp;
    SerializedProperty changeLayerDuringDashProp;
    SerializedProperty dashLayerProp;
    SerializedProperty checkMaskProp;
    SerializedProperty checkDistanceProp;
    SerializedProperty drawGizmosProp;

    void OnEnable()
    {
        alwaysOnGroundProp = serializedObject.FindProperty("alwaysOnGround");
        accelerateProp = serializedObject.FindProperty("accelerate");
        groundAccelerationProp = serializedObject.FindProperty("groundAcceleration");
        airAccelerationProp = serializedObject.FindProperty("airAcceleration");
        maxGroundSpeedProp = serializedObject.FindProperty("maxGroundSpeed");
        maxAirSpeedProp = serializedObject.FindProperty("maxAirSpeed");
        maxFallSpeedProp = serializedObject.FindProperty("maxFallSpeed");
        baseJumpProp = serializedObject.FindProperty("baseJump");
        extraJumpHeightProp = serializedObject.FindProperty("extraJumpHeight");
        allowDoubleJumpProp = serializedObject.FindProperty("allowDoubleJump");
        allowWallJumpProp = serializedObject.FindProperty("allowWallJump");
        allowWallSlideProp = serializedObject.FindProperty("allowWallSlide");
        wallSlideSpeedProp = serializedObject.FindProperty("wallSlideSpeed");
        allowCornerGrabProp = serializedObject.FindProperty("allowCornerGrab");
        cornerHeightCheckProp = serializedObject.FindProperty("cornerHeightCheck");
        cornerWidthCheckProp = serializedObject.FindProperty("cornerWidthCheck");
        canDashProp = serializedObject.FindProperty("canDash");
        dashCooldownProp = serializedObject.FindProperty("dashCooldown");
        dashSpeedProp = serializedObject.FindProperty("dashSpeed");
        dashDurationProp = serializedObject.FindProperty("dashDuration");
        changeLayerDuringDashProp = serializedObject.FindProperty("changeLayerDuringDash");
        dashLayerProp = serializedObject.FindProperty("dashLayer");
        checkMaskProp = serializedObject.FindProperty("checkMask");
        checkDistanceProp = serializedObject.FindProperty("checkDistance");
        drawGizmosProp = serializedObject.FindProperty("drawGizmos");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(alwaysOnGroundProp, new GUIContent("Always On Ground"));

        if (alwaysOnGroundProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue)
        {
            EditorGUILayout.PropertyField(checkMaskProp, new GUIContent("Environment Check Mask"));
            EditorGUILayout.PropertyField(checkDistanceProp, new GUIContent("Ground Check Distance"));
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(accelerateProp, new GUIContent("Acceleration"));

        if (accelerateProp.hasMultipleDifferentValues || accelerateProp.boolValue)
        {
            EditorGUILayout.PropertyField(groundAccelerationProp, new GUIContent("Ground Acceleration"));

            if (accelerateProp.hasMultipleDifferentValues || alwaysOnGroundProp.boolValue)
            {
                EditorGUILayout.PropertyField(airAccelerationProp, new GUIContent("Air Acceleration"));
            }
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(maxGroundSpeedProp, new GUIContent("Max Ground Speed"));

        if (alwaysOnGroundProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue)
        {
            EditorGUILayout.PropertyField(maxAirSpeedProp, new GUIContent("Max Air Speed"));
            EditorGUILayout.PropertyField(maxFallSpeedProp, new GUIContent("Max Fall Speed"));
        }

        EditorGUILayout.Separator();

        if (alwaysOnGroundProp.hasMultipleDifferentValues || !alwaysOnGroundProp.boolValue)
        {
            EditorGUILayout.PropertyField(baseJumpProp, new GUIContent("Base Jump Height"));
            EditorGUILayout.PropertyField(extraJumpHeightProp, new GUIContent("Held Extra Jump Height"));
            EditorGUILayout.PropertyField(allowDoubleJumpProp, new GUIContent("Allow Double Jump"));
            EditorGUILayout.PropertyField(allowWallJumpProp, new GUIContent("Allow Wall Jump"));

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
                EditorGUILayout.PropertyField(cornerHeightCheckProp, new GUIContent("Corner Check Height"));
                EditorGUILayout.PropertyField(cornerWidthCheckProp, new GUIContent("Corner Check Width"));
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
                EditorGUILayout.PropertyField(dashLayerProp, new GUIContent("Dash Layer"));
                //dashLayerProp.intValue = EditorGUILayout.LayerField("Layer", dashLayerProp.intValue);
            }
        }

        EditorGUILayout.Separator();

        EditorGUILayout.PropertyField(drawGizmosProp, new GUIContent("Draw Gizmos"));

        serializedObject.ApplyModifiedProperties();
    }
}
