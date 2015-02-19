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

[CustomEditor(typeof(FilledRenderer3D))]
public class FilledRenderer3DInspector : EnergyBar3DInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================

    private SerializedProperty textureBar;
    private SerializedProperty atlasTextureBarGUID;
    
    private SerializedProperty textureBarColorType;
    private SerializedProperty textureBarColor;
    private SerializedProperty textureBarGradient;
    
    private SerializedProperty growDirection;
    
    private SerializedProperty radialOffset;
    private SerializedProperty radialLength;
    
    private SerializedProperty effectBurn;
    private SerializedProperty effectBurnTextureBar;
    private SerializedProperty atlasEffectBurnTextureBarGUID;
    private SerializedProperty effectBurnTextureBarColor;
    
    private SerializedProperty effectBlink;
    private SerializedProperty effectBlinkValue;
    private SerializedProperty effectBlinkRatePerSecond;
    private SerializedProperty effectBlinkColor;
    
    private SerializedProperty effectFollow;
    private SerializedProperty effectFollowObject;
    private SerializedProperty effectFollowScaleX;
    private SerializedProperty effectFollowScaleY;
    private SerializedProperty effectFollowScaleZ;
    private SerializedProperty effectFollowRotation;
    private SerializedProperty effectFollowColor;
    
    private FilledRenderer3D script;

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
        
        textureBar = serializedObject.FindProperty("textureBar");
        atlasTextureBarGUID = serializedObject.FindProperty("atlasTextureBarGUID");
        
        textureBarColorType = serializedObject.FindProperty("textureBarColorType");
        textureBarColor = serializedObject.FindProperty("textureBarColor");
        textureBarGradient = serializedObject.FindProperty("textureBarGradient");
        
        growDirection = serializedObject.FindProperty("growDirection");
        
        radialOffset = serializedObject.FindProperty("radialOffset");
        radialLength = serializedObject.FindProperty("radialLength");
        
        effectBurn = serializedObject.FindProperty("effectBurn");
        effectBurnTextureBar = serializedObject.FindProperty("effectBurnTextureBar");
        atlasEffectBurnTextureBarGUID = serializedObject.FindProperty("atlasEffectBurnTextureBarGUID");
        effectBurnTextureBarColor = serializedObject.FindProperty("effectBurnTextureBarColor");
        
        effectBlink = serializedObject.FindProperty("effectBlink");
        effectBlinkValue = serializedObject.FindProperty("effectBlinkValue");
        effectBlinkRatePerSecond = serializedObject.FindProperty("effectBlinkRatePerSecond");
        effectBlinkColor = serializedObject.FindProperty("effectBlinkColor");
        
        effectFollow = serializedObject.FindProperty("effectFollow");
        effectFollowObject = serializedObject.FindProperty("effectFollowObject");
        effectFollowScaleX = serializedObject.FindProperty("effectFollowScaleX");
        effectFollowScaleY = serializedObject.FindProperty("effectFollowScaleY");
        effectFollowScaleZ = serializedObject.FindProperty("effectFollowScaleZ");
        effectFollowRotation = serializedObject.FindProperty("effectFollowRotation");
        effectFollowColor = serializedObject.FindProperty("effectFollowColor");

        script = target as FilledRenderer3D;
    }
    
    public override void OnCustomInspector() {
        serializedObject.Update();

        Header();
        
        Section("Textures", () => {
            FieldTextureMode();

            EditorGUILayout.Space();

            if (script.textureMode != EnergyBar3DBase.TextureMode.TextureAtlas || script.atlas != null) {
                FieldBackgroundTextures();

                EditorGUILayout.Space();

                FieldSprite(textureBar, atlasTextureBarGUID, "Bar Texture");

                EditorGUILayout.Space();
                
                CheckTextureIsReadable(script.textureBar);
                CheckTextureFilterTypeNotPoint(script.textureBar);

                FieldForegroundTextures();

                FieldPremultipliedAlpha();
            }
        });

        if (showPositionAndSize) {
            SectionPositionAndSize();
        }

        Section("Appearance", () => {
            var dir = (EnergyBarRenderer.GrowDirection) growDirection.enumValueIndex;

            if (dir == EnergyBarRenderer.GrowDirection.ColorChange) {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(textureBarColorType, new GUIContent("Bar Color Type"));
            EditorGUI.indentLevel++;
            switch ((EnergyBarRenderer.ColorType) textureBarColorType.enumValueIndex) {
                case EnergyBarRenderer.ColorType.Solid:
                    EditorGUILayout.PropertyField(textureBarColor, new GUIContent("Bar Color"));
                    break;

                case EnergyBarRenderer.ColorType.Gradient:
                    EditorGUILayout.PropertyField(textureBarGradient, new GUIContent("Bar Gradient"));
                    break;
            }

            EditorGUI.indentLevel--;

            GUI.enabled = true;

            MadGUI.PropertyFieldEnumPopup(growDirection, "Grow Direction");

            if (dir == EnergyBarRenderer.GrowDirection.RadialCW || dir == EnergyBarRenderer.GrowDirection.RadialCCW) {
                MadGUI.Indent(() => {
                    EditorGUILayout.Slider(radialOffset, -1, 1, "Offset");
                    EditorGUILayout.Slider(radialLength, 0, 1, "Length");
                });
            } else if (dir == EnergyBarRenderer.GrowDirection.ColorChange) {
                EditorGUILayout.PropertyField(textureBarGradient, new GUIContent("Bar Gradient"));
            }

            FieldLabel();
        });

        Section("Effects", () => {
            FieldSmoothEffect();

            MadGUI.PropertyFieldToggleGroup2(effectBurn, "Burn Out", () => {
                MadGUI.Indent(() => {
                    FieldSprite(effectBurnTextureBar, atlasEffectBurnTextureBarGUID, "Burn Texture Bar");
                    MadGUI.PropertyField(effectBurnTextureBarColor, "Burn Color");
                });
            });

            MadGUI.PropertyFieldToggleGroup2(effectBlink, "Blink", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(effectBlinkValue, "Value");
                    MadGUI.PropertyField(effectBlinkRatePerSecond, "Rate (per second)");
                    MadGUI.PropertyField(effectBlinkColor, "Color");
                });
            });

            MadGUI.PropertyFieldToggleGroup2(effectFollow, "Sprite Follow", () => {
                MadGUI.Indent(() => {
                    if (GUI.enabled && !GrowDirectionSupportedByFollowEffect()) {
                        MadGUI.Error("This effect cannot be used with selected grow direction. "
                            + "Please read manual for more information.");
                    }

                    MadGUI.PropertyField(effectFollowObject, "Texture or GameObject",
                        "This can be Texture, MadSprite or any other GameObject.");

                    if (!VerifyFollowObject()) {
                        MadGUI.Error("You can put here only Texture2D or GameObject.");
                    }

                    MadGUI.PropertyField(effectFollowColor, "Color");
                    MadGUI.PropertyField(effectFollowRotation, "Rotation");
                    if (MadGUI.Foldout("Scale", false)) {
                        MadGUI.Indent(() => {
                            MadGUI.PropertyField(effectFollowScaleX, "X");
                            MadGUI.PropertyField(effectFollowScaleY, "Y");
                            MadGUI.PropertyField(effectFollowScaleZ, "Z");
                        });
                    }


                });
            });
        });
        
        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
    }

    // ===========================================================
    // Methods
    // ===========================================================

    private bool GrowDirectionSupportedByFollowEffect() {
        EnergyBarBase.GrowDirection gd = (EnergyBarBase.GrowDirection) growDirection.enumValueIndex;
        return script.GrowDirectionSupportedByFollowEffect(gd);
    }
    
    private bool VerifyFollowObject() {
        var obj = effectFollowObject.objectReferenceValue;
        return obj == null || obj is Texture2D || obj is GameObject || obj is MadSprite;
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace