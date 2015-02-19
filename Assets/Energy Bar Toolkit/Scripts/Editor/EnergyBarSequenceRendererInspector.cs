/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace EnergyBarToolkit {

[CustomEditor(typeof(EnergyBarSequenceRenderer))]
public class EnergyBarSequenceRendererInspector : EnergyBarInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    private SerializedProperty method;
    private SerializedProperty position;
    private SerializedProperty positionNormalized;
    private SerializedProperty size;
    
    private SerializedProperty gridTexture;
    private SerializedProperty gridWidth;
    private SerializedProperty gridHeight;
    
    private SerializedProperty color;
    
    private SerializedProperty frameCountManual;
    private SerializedProperty frameCount;
    
//    private SerializedProperty sequence;

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
    
        position = serializedObject.FindProperty("position");
        positionNormalized = serializedObject.FindProperty("positionNormalized");
        method = serializedObject.FindProperty("method");
        size = serializedObject.FindProperty("size");
        
        gridTexture = serializedObject.FindProperty("gridTexture");
        gridWidth = serializedObject.FindProperty("gridWidth");
        gridHeight = serializedObject.FindProperty("gridHeight");
        
        frameCountManual = serializedObject.FindProperty("frameCountManual");
        frameCount = serializedObject.FindProperty("frameCount");
        
        color = serializedObject.FindProperty("color");
    }

    void Update() {
    }
    
    public override void OnInspectorGUI() {
        ClearActionQueue();
        serializedObject.Update();
        
        var t = target as EnergyBarSequenceRenderer;
        
        if (MadGUI.Foldout("Textures", true)) {
            MadGUI.BeginBox();
        
            EditorGUILayout.PropertyField(method, new GUIContent("Render Method"));
            
            switch ((EnergyBarSequenceRenderer.Method) method.enumValueIndex) {
                case EnergyBarSequenceRenderer.Method.Grid:
                    OnGUIGrid();
                    
                    break;
                case EnergyBarSequenceRenderer.Method.Sequence:
                    PropertyFieldWithChildren("sequence");
                    break;
            }
            
            MadGUI.PropertyField(color, "Color Tint");
            
            FieldBackgroundTextures();
            FieldForegroundTextures();
            FieldPremultipliedAlpha();
            MadGUI.EndBox();
        }
    
        if (MadGUI.Foldout("Position & Size", true)) {
            MadGUI.BeginBox();
            MadGUI.PropertyFieldVector2(position, "Position");
            EditorGUI.indentLevel++;
            PropertySpecialNormalized(position, positionNormalized);
            MadGUI.PropertyField(pivot, "Pivot");
            MadGUI.PropertyField(anchorObject, "Anchor");
            EditorGUI.indentLevel--;
            
            MadGUI.PropertyField(guiDepth, "GUI Depth");
            
            PropertySpecialResizeMode(size, resizeMode);
            
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
            FieldSmoothEffect();
            MadGUI.EndBox();
        }
        
        serializedObject.ApplyModifiedProperties();
        
        ExecuteActionQueue();
        
        EditorGUILayout.PrefixLabel("Preview:");
        MadGUI.BeginBox(); {
            var rect = EditorGUILayout.BeginVertical();
            GUILayout.Space(100);
            
            if (t.IsValid()) {
                Rect texCoords;
                var texture = t.GetTexture(out texCoords);
                if (texture != null) {
                    GUI.DrawTextureWithTexCoords(PreserveAspect(rect), texture, texCoords);
                }
            }
            EditorGUILayout.EndVertical();
        } MadGUI.EndBox();
    }
    
    // ===========================================================
    // Methods
    // ===========================================================
    
    private void OnGUIGrid() {
        EditorGUILayout.PropertyField(gridTexture, new GUIContent("Grid Texture"));
        CheckTextureIsGUI(gridTexture.objectReferenceValue as Texture2D);
        
        EditorGUILayout.PropertyField(gridWidth, new GUIContent("Grid Width"));
        EditorGUILayout.PropertyField(gridHeight, new GUIContent("Grid Height"));
        
        
        MadGUI.PropertyFieldToggleGroup2(frameCountManual, "Manual Frame Count", () => {
            MadGUI.PropertyField(frameCount, "Frame Count");
        });
//        frameCountManual.boolValue = EditorGUILayout.BeginToggleGroup("Manual Frame Count", frameCountManual.boolValue); {
            
//        EditorGUILayout.EndToggleGroup();
    }
    
    // returns new rect that preserves aspect of bar size
    private Rect PreserveAspect(Rect rect) {
        if (rect.height == 0) {
            return rect;
        }
    
        var t = target as EnergyBarSequenceRenderer;
        Vector2 size = t.size;
        
        float sourceAspect = size.x / size.y;
        float targetAspect = rect.width / rect.height;
        
        float width, height;
        
        if (sourceAspect < targetAspect) {
            height = rect.height;
            width = rect.width * sourceAspect / targetAspect;
        } else {
            width = rect.width;
            height = rect.height * targetAspect / sourceAspect;
        }
        
        return new Rect(rect.x + (rect.width - width) / 2, rect.y + (rect.height - height) / 2, width, height);
        
    }

    protected override List<Texture2D> AllTextures() {
        var result = new List<Texture2D>();
        result.AddRange(BackgroundTextures());
        result.AddRange(ForegroundTextures());
        
        var t = target as EnergyBarSequenceRenderer;
        
        result.Add(t.gridTexture);
        if (t.sequence != null) {
            foreach (var tex in t.sequence) {
                result.Add(tex);
            }
        }
        
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
