/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using EnergyBarToolkit;

namespace EnergyBarToolkit {

[CustomEditor(typeof(RepeatedRenderer3D))]
public class RepeatRenderer3DInspector : EnergyBar3DInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================

    private SerializedProperty textureIcon;
    private SerializedProperty textureSlot;

    private SerializedProperty atlasTextureIconGUID;
    private SerializedProperty atlasTextureSlotGUID;
    
    private SerializedProperty tintIcon;
    private SerializedProperty tintSlot;
    
    private SerializedProperty repeatCount;
    private SerializedProperty repeatPositionDelta;
    
    private SerializedProperty growType;
    private SerializedProperty fillDirection;
    
//    private SerializedProperty effectBlink;
//    private SerializedProperty effectBlinkValue;
//    private SerializedProperty effectBlinkRatePerSecond;
//    private SerializedProperty effectBlinkColor;

    // ===========================================================
    // Constructors (Including Static Constructors)
    // ===========================================================

    // ===========================================================
    // Getters / Setters
    // ===========================================================

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override void OnEnable() {
        base.OnEnable();
        
        textureIcon = serializedObject.FindProperty("textureIcon");
        textureSlot = serializedObject.FindProperty("textureSlot");

        atlasTextureIconGUID = serializedObject.FindProperty("atlasTextureIconGUID");
        atlasTextureSlotGUID = serializedObject.FindProperty("atlasTextureSlotGUID");
        
        tintIcon = serializedObject.FindProperty("tintIcon");
        tintSlot = serializedObject.FindProperty("tintSlot");
        
        repeatCount = serializedObject.FindProperty("repeatCount");
        repeatPositionDelta = serializedObject.FindProperty("repeatPositionDelta");
        
        growType = serializedObject.FindProperty("growType");
        fillDirection = serializedObject.FindProperty("fillDirection");
        
    }
    
    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        var t = target as RepeatedRenderer3D;

        Header();

        Section("Textures", () => {

            FieldTextureMode();

            EditorGUILayout.BeginHorizontal();
            FieldSprite(textureIcon, atlasTextureIconGUID, "Icon");
            //MadGUI.PropertyField(textureIcon, "Icon");
            EditorGUILayout.PropertyField(tintIcon, new GUIContent(""), new GUILayoutOption[] { GUILayout.Width(50) });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            FieldSprite(textureSlot, atlasTextureSlotGUID, "Slot");
            //MadGUI.PropertyField(textureSlot, "Slot");
            EditorGUILayout.PropertyField(tintSlot, new GUIContent(""), new GUILayoutOption[] { GUILayout.Width(50) });
            EditorGUILayout.EndHorizontal();

            FieldPremultipliedAlpha();

            CheckTextureIsReadable(t.textureIcon);
            CheckTextureIsReadable(t.textureSlot);
        });

        SectionPositionAndSize();

        Section("Appearance", () => {
            MadGUI.PropertyField(repeatCount, "Repeat Count");
            MadGUI.PropertyFieldVector2(repeatPositionDelta, "Icon Distance");

            MadGUI.PropertyField(growType, "Grow Type");
            MadGUI.ConditionallyEnabled(growType.enumValueIndex == (int) RepeatedRenderer3D.GrowType.Fill, () => {
                MadGUI.PropertyField(fillDirection, "Fill Direction");
            });

            FieldLabel();
        });

        Section("Effects", () => {
            FieldSmoothEffect();
        });
        
        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
    }

    // ===========================================================
    // Methods
    // ===========================================================

    

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace