/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

[CustomEditor(typeof(EnergyBarRenderer))]
public class EnergyBarRendererInspector : EnergyBarInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================
    
    // ===========================================================
    // Fields
    // ===========================================================
    
    private SerializedProperty textureBar;
    
    private SerializedProperty textureBarColorType;
    private SerializedProperty textureBarColor;
    private SerializedProperty textureBarGradient;
    
    private SerializedProperty screenPosition;
    private SerializedProperty screenPositionNormalized;
    private SerializedProperty size;
    
    private SerializedProperty growDirection;
    
    private SerializedProperty radialOffset;
    private SerializedProperty radialLength;
    
    private SerializedProperty effectBurn;
    private SerializedProperty effectBurnTextureBar;
    private SerializedProperty effectBurnTextureBarColor;
    
    private SerializedProperty effectBlink;
    private SerializedProperty effectBlinkValue;
    private SerializedProperty effectBlinkRatePerSecond;
    private SerializedProperty effectBlinkColor;
    
#if MAD_DEBUG
    SerializedProperty effectEdge;
    SerializedProperty effectEdgeTexture;
    SerializedProperty effectEdgeIn;
    SerializedProperty effectEdgeOut;
    SerializedProperty effectEdgeRotateAngle;
#endif

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
        
        textureBarColorType = serializedObject.FindProperty("textureBarColorType");
        textureBarColor = serializedObject.FindProperty("textureBarColor");
        textureBarGradient = serializedObject.FindProperty("textureBarGradient");
        
        screenPosition = serializedObject.FindProperty("screenPosition");
        screenPositionNormalized = serializedObject.FindProperty("screenPositionNormalized");
        size = serializedObject.FindProperty("size");
        
        growDirection = serializedObject.FindProperty("growDirection");
        
        radialOffset = serializedObject.FindProperty("radialOffset");
        radialLength = serializedObject.FindProperty("radialLength");
        
        effectBurn = serializedObject.FindProperty("effectBurn");
        effectBurnTextureBar = serializedObject.FindProperty("effectBurnTextureBar");
        effectBurnTextureBarColor = serializedObject.FindProperty("effectBurnTextureBarColor");
        
        effectBlink = serializedObject.FindProperty("effectBlink");
        effectBlinkValue = serializedObject.FindProperty("effectBlinkValue");
        effectBlinkRatePerSecond = serializedObject.FindProperty("effectBlinkRatePerSecond");
        effectBlinkColor = serializedObject.FindProperty("effectBlinkColor");
      
#if MAD_DEBUG  
        effectEdge = serializedObject.FindProperty("effectEdge");
        effectEdgeTexture = serializedObject.FindProperty("effectEdgeTexture");
        effectEdgeIn = serializedObject.FindProperty("effectEdgeIn");
        effectEdgeOut = serializedObject.FindProperty("effectEdgeOut");
        effectEdgeRotateAngle = serializedObject.FindProperty("effectEdgeRotateAngle");
#endif
        
    }
    
    public override void OnInspectorGUI() {
        ClearActionQueue();
        serializedObject.Update();
        
        var t = target as EnergyBarRenderer;
        
        if (MadGUI.Foldout("Textures", true)) {
            MadGUI.BeginBox();
            FieldBackgroundTextures();
            
            EditorGUILayout.PropertyField(textureBar, new GUIContent("Bar Texture"));         
            CheckTextureIsReadable(t.textureBar);
            CheckTextureFilterTypeNotPoint(t.textureBar);
            
            FieldForegroundTextures();
            
            FieldPremultipliedAlpha();
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Position & Size", true)) {
            MadGUI.BeginBox();
            
            MadGUI.PropertyFieldVector2(screenPosition, "Position");
            
            EditorGUI.indentLevel++;
            PropertySpecialNormalized(screenPosition, screenPositionNormalized);
            MadGUI.PropertyField(pivot, "Pivot");
            MadGUI.PropertyField(anchorObject, "Anchor");
            MadGUI.PropertyField(anchorCamera, "Anchor Camera", "Camera on which world coordinates will be translated to "
                + "screen coordinates.");
            EditorGUI.indentLevel--;
            
            MadGUI.PropertyField(guiDepth, "GUI Depth");
            
            PropertySpecialResizeMode(size, resizeMode);
            
            FieldRelativeToTransform();
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Appearance", false)) {
            MadGUI.BeginBox();
            
            var dir = (EnergyBarRenderer.GrowDirection) growDirection.enumValueIndex;
        
            if (dir == EnergyBarRenderer.GrowDirection.ColorChange) {
                GUI.enabled = false;
            }
            EditorGUILayout.PropertyField(textureBarColorType, new GUIContent("Bar Color Type"));
            EditorGUI.indentLevel++;
                switch ((EnergyBarRenderer.ColorType)textureBarColorType.enumValueIndex) {
                    case EnergyBarRenderer.ColorType.Solid:
                        EditorGUILayout.PropertyField(textureBarColor, new GUIContent("Bar Color"));
                        break;
                        
                    case EnergyBarRenderer.ColorType.Gradient:
                        EditorGUILayout.PropertyField(textureBarGradient, new GUIContent("Bar Gradient"));
                        break;
                }
                
            EditorGUI.indentLevel--;
            
            GUI.enabled = true;
            
            EditorGUILayout.PropertyField(growDirection, new GUIContent("Grow Direction"));
            
            if (dir == EnergyBarRenderer.GrowDirection.RadialCW || dir == EnergyBarRenderer.GrowDirection.RadialCCW) {
                MadGUI.Indent(() => {
                    EditorGUILayout.Slider(radialOffset, -1, 1, "Offset");
                    EditorGUILayout.Slider(radialLength, 0, 1, "Length");
                });
            } else if (dir == EnergyBarRenderer.GrowDirection.ColorChange) {
                EditorGUILayout.PropertyField(textureBarGradient, new GUIContent("Bar Gradient"));
            }
            
            FieldLabel();
            
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Effects", false)) {
            MadGUI.BeginBox();
            
            FieldSmoothEffect();
            
            MadGUI.PropertyFieldToggleGroup2(effectBurn, "Burn Out Effect", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(effectBurnTextureBar, "Burn Texture Bar");
                    MadGUI.PropertyField(effectBurnTextureBarColor, "Burn Color");
                });
            });
            
            MadGUI.PropertyFieldToggleGroup2(effectBlink, "Blink Effect", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(effectBlinkValue, "Value");
                    MadGUI.PropertyField(effectBlinkRatePerSecond, "Rate (per second)");
                    MadGUI.PropertyField(effectBlinkColor, "Color");
                });
            });
            
#if MAD_DEBUG
            MadGUI.PropertyFieldToggleGroup2(effectEdge, "Edge Effect", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(effectEdgeTexture, "Edge Texture");
                    MadGUI.PropertyField(effectEdgeIn, "Fade In Buffer");
                    MadGUI.PropertyField(effectEdgeOut, "Fade Out Buffer");
                    MadGUI.PropertyField(effectEdgeRotateAngle, "Rotation");
                });
            });
#endif
            
            MadGUI.EndBox();
        }
        
        EditorGUILayout.Space();
        
        serializedObject.ApplyModifiedProperties();
        ExecuteActionQueue();
    }
    
    protected override List<Texture2D> AllTextures() {
        var result = new List<Texture2D>();
        result.AddRange(BackgroundTextures());
        result.AddRange(ForegroundTextures());
        result.Add(textureBar.objectReferenceValue as Texture2D);
        return result;
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
