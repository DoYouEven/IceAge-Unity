/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

[CustomEditor(typeof(EnergyBarRepeatRenderer))]
public class EnergyBarRepeatRendererInspector : EnergyBarInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    private SerializedProperty icon;
    private SerializedProperty iconColor;
    private SerializedProperty iconSlot;
    private SerializedProperty iconSlotColor;
    
    private SerializedProperty iconSize;
    
    private SerializedProperty startPosition;
    private SerializedProperty startPositionNormalized;
    private SerializedProperty repeatCount;
    private SerializedProperty positionDelta;
    
    private SerializedProperty effect;
    private SerializedProperty cutDirection;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================
    
    public override void OnEnable() {
        base.OnEnable();
        
        icon = serializedObject.FindProperty("icon");
        iconColor = serializedObject.FindProperty("iconColor");
        iconSlot = serializedObject.FindProperty("iconSlot");
        iconSlotColor = serializedObject.FindProperty("iconSlotColor");
        
        iconSize = serializedObject.FindProperty("iconSize");
        
        startPosition = serializedObject.FindProperty("startPosition");
        startPositionNormalized = serializedObject.FindProperty("startPositionNormalized");
        repeatCount = serializedObject.FindProperty("repeatCount");
        positionDelta = serializedObject.FindProperty("positionDelta");
        
        effect = serializedObject.FindProperty("effect");
        cutDirection = serializedObject.FindProperty("cutDirection");
        
    }
    
    public override void OnInspectorGUI() {
        ClearActionQueue();
        serializedObject.Update();
        
        if (MadGUI.Foldout("Textures", true)) {
            MadGUI.BeginBox();
            MadGUI.PropertyField(repeatCount, "Repeat Count");
            MadGUI.PropertyField(icon, "Icon");
            CheckTextureIsGUI(icon.objectReferenceValue as Texture2D);
            CheckTextureFilterTypeNotPoint(icon.objectReferenceValue as Texture2D);
            MadGUI.Indent(() => { MadGUI.PropertyField(iconColor, "Icon Tint"); });
            
            MadGUI.PropertyField(iconSlot, "Slot Icon");
            CheckTextureIsGUI(iconSlot.objectReferenceValue as Texture2D);
            CheckTextureFilterTypeNotPoint(iconSlot.objectReferenceValue as Texture2D);
            MadGUI.Indent(() => { MadGUI.PropertyField(iconSlotColor, "Slot Icon Tint"); });
            FieldPremultipliedAlpha();
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Position & Size", true)) {
            MadGUI.BeginBox();
            MadGUI.PropertyFieldVector2(startPosition, "Start Position");
            EditorGUI.indentLevel++;
            PropertySpecialNormalized(startPosition, startPositionNormalized);
            MadGUI.PropertyField(pivot, "Pivot");
            MadGUI.PropertyField(anchorObject, "Anchor");
            EditorGUI.indentLevel--;
            MadGUI.PropertyField(guiDepth, "GUI Depth");
            
            PropertySpecialResizeMode(iconSize, resizeMode, "Icon ");
            
//            switch ((EnergyBarOnGUIBase.ResizeMode) resizeMode.enumValueIndex) {
//                case EnergyBarOnGUIBase.ResizeMode.Constant:
//                    PropertyFieldToggleGroupInv2(iconSizeCalculate, "Manual Size", () => {
//                        Indent(() => {
//                            PropertyFieldVector2(iconSize, "Icon Size");
//                        });
//                    });
//                    break;
//                
//                case EnergyBarOnGUIBase.ResizeMode.Stretch:
//                    PropertyFieldVector2(iconSize, "Icon Size");
//                    break;
//                
//                case EnergyBarOnGUIBase.ResizeMode.KeepRatio:
//                    EditorGUI.BeginChangeCheck();
//
//                    float y = EditorGUILayout.FloatField("Icon Height", iconSize.vector2Value.y);
//                    if (EditorGUI.EndChangeCheck()) {
//                        var vec = new Vector2(iconSize.vector2Value.x, y);
//                        iconSize.vector2Value = vec;
//                    }
//                break;
//            }
            
            MadGUI.PropertyFieldVector2(positionDelta, "Icons Distance");
            
            FieldRelativeToTransform();
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Appearance", false)) {
            MadGUI.BeginBox();
            FieldLabel();
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Effects", false)) {
            MadGUI.BeginBox();
            MadGUI.PropertyField(effect, "Grow Effect");
            if (effect.enumValueIndex == (int) EnergyBarRepeatRenderer.Effect.Cut) {
                MadGUI.PropertyField(cutDirection, "Cut Direction");
            }
            
            FieldSmoothEffect();
            MadGUI.EndBox();
        }
        
        serializedObject.ApplyModifiedProperties();
        ExecuteActionQueue();
    }

    protected override List<Texture2D> AllTextures() {
        var result = new List<Texture2D>();
        result.Add(icon.objectReferenceValue as Texture2D);
        result.Add(iconSlot.objectReferenceValue as Texture2D);
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
