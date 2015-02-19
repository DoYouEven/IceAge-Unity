/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {
 
[CustomEditor(typeof(EnergyBarTransformRenderer))]   
public class EnergyBarTransformRendererInspector : EnergyBarInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    SerializedProperty textureObjectTexture;
    SerializedProperty textureObjectColor;
    
    SerializedProperty screenPosition;
    SerializedProperty screenPositionNormalized;
    SerializedProperty size;
    
    SerializedProperty transformAnchor;
    
    SerializedProperty transformTranslate;
    SerializedProperty transformRotate;
    SerializedProperty transformScale;
    
    SerializedProperty translateFunctionStart;
    SerializedProperty translateFunctionEnd;
    SerializedProperty rotateFunctionStart;
    SerializedProperty rotateFunctionEnd;
    SerializedProperty scaleFunctionStart;
    SerializedProperty scaleFunctionEnd;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================

    public override void OnEnable() {
        base.OnEnable();
        
        var textureObject = serializedObject.FindProperty("textureObject");
        textureObjectTexture = textureObject.FindPropertyRelative("texture");
        textureObjectColor = textureObject.FindPropertyRelative("color");
        
        screenPosition = serializedObject.FindProperty("screenPosition");
        screenPositionNormalized = serializedObject.FindProperty("screenPositionNormalized");
        size = serializedObject.FindProperty("size");
        
        transformAnchor = serializedObject.FindProperty("transformAnchor");
        
        transformTranslate = serializedObject.FindProperty("transformTranslate");
        transformRotate = serializedObject.FindProperty("transformRotate");
        transformScale = serializedObject.FindProperty("transformScale");
        
        translateFunctionStart = serializedObject.FindProperty("translateFunction")
            .FindPropertyRelative("startPosition");
        translateFunctionEnd = serializedObject.FindProperty("translateFunction")
            .FindPropertyRelative("endPosition");
        rotateFunctionStart = serializedObject.FindProperty("rotateFunction")
            .FindPropertyRelative("startAngle");
        rotateFunctionEnd = serializedObject.FindProperty("rotateFunction")
            .FindPropertyRelative("endAngle");
        scaleFunctionStart = serializedObject.FindProperty("scaleFunction")
            .FindPropertyRelative("startScale");
        scaleFunctionEnd = serializedObject.FindProperty("scaleFunction")
            .FindPropertyRelative("endScale");
    }
    
    public override void OnInspectorGUI() {
        ClearActionQueue();
        serializedObject.Update();
        
        var t = target as EnergyBarTransformRenderer;
        
        if (MadGUI.Foldout("Textures", true)) {
            MadGUI.BeginBox();
            FieldBackgroundTextures();
            
            MadGUI.Indent(() => {
                EditorGUILayout.LabelField("Object Texture");
                GUITexture(textureObjectTexture, textureObjectColor);
                
                MadGUI.PropertyFieldVector2(transformAnchor, "Texture Anchor");
            });
            
            EditorGUILayout.Space();
            
            CheckTextureIsReadable(t.textureObject.texture);
            CheckTextureFilterTypeNotPoint(t.textureObject.texture);
            
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
        
        if (MadGUI.Foldout("Object Transform", true)) {
            MadGUI.BeginBox();
            MadGUI.PropertyFieldToggleGroup2(transformTranslate, "Translate", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyFieldVector2(translateFunctionStart, "Start Point");
                    MadGUI.PropertyFieldVector2(translateFunctionEnd, "End Point");
                });
            });
            
            MadGUI.PropertyFieldToggleGroup2(transformRotate, "Rotate", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyField(rotateFunctionStart, "Start Angle");
                    MadGUI.PropertyField(rotateFunctionEnd, "End Angle");
                });
            });
            
            MadGUI.PropertyFieldToggleGroup2(transformScale, "Scale", () => {
                MadGUI.Indent(() => {
                    MadGUI.PropertyFieldVector2(scaleFunctionStart, "Start Scale");
                    MadGUI.PropertyFieldVector2(scaleFunctionEnd, "End Scale");
                });
            });
            
            MadGUI.EndBox();
        }
        
        if (MadGUI.Foldout("Appearance", false)) {
            MadGUI.BeginBox();
            FieldLabel();
            MadGUI.EndBox();
        }
        
        serializedObject.ApplyModifiedProperties();
        ExecuteActionQueue();
    }

    protected override List<Texture2D> AllTextures() {
        var result = new List<Texture2D>();
        result.AddRange(BackgroundTextures());
        result.AddRange(ForegroundTextures());
        result.Add(textureObjectTexture.objectReferenceValue as Texture2D);
        return result;
    }

    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace
