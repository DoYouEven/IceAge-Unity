/*
* Energy Bar Toolkit by Mad Pixel Machine
* http://www.madpixelmachine.com
*/

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EnergyBarToolkit {

[CustomEditor(typeof(SequenceRenderer3D))]
public class SequenceRenderer3DInspector : EnergyBar3DInspectorBase {

    // ===========================================================
    // Constants
    // ===========================================================

    // ===========================================================
    // Fields
    // ===========================================================
    
    SerializedProperty renderingMethod;
    
    SerializedProperty gridTexture;
    SerializedProperty gridAtlasTextureGUID;
    SerializedProperty gridWidth;
    SerializedProperty gridHeight;
    SerializedProperty gridFrameCountManual;
    SerializedProperty gridFrameCount;
    SerializedProperty gridTint;
    
    SerializedProperty sequenceTextures;
    SerializedProperty sequenceAtlasTexturesGUID;
    
    SequenceRenderer3D script;

    // ===========================================================
    // Methods for/from SuperClass/Interfaces
    // ===========================================================

    // ===========================================================
    // Methods
    // ===========================================================

    public override void OnEnable() {
        base.OnEnable();
    
        script = target as SequenceRenderer3D;

        renderingMethod = serializedObject.FindProperty("renderingMethod");
        
        gridTexture = serializedObject.FindProperty("gridTexture");
        gridAtlasTextureGUID = serializedObject.FindProperty("gridAtlasTextureGUID");
        gridWidth = serializedObject.FindProperty("gridWidth");
        gridHeight = serializedObject.FindProperty("gridHeight");
        gridFrameCountManual = serializedObject.FindProperty("gridFrameCountManual");
        gridFrameCount = serializedObject.FindProperty("gridFrameCount");
        gridTint = serializedObject.FindProperty("gridTint");
        
        sequenceTextures = serializedObject.FindProperty("sequenceTextures");
        sequenceAtlasTexturesGUID = serializedObject.FindProperty("sequenceAtlasTexturesGUID");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        Header();

        SectionTextures();
        SectionPositionAndSize();
        SectionAppearance();
        SectionEffects();
        
        serializedObject.ApplyModifiedProperties();
    }

    private void SectionTextures() {
        Section("Textures", () => {
            MadGUI.PropertyField(renderingMethod, "Render Method");
            FieldTextureMode();

            EditorGUILayout.Space();

            if (script.textureMode != EnergyBar3DBase.TextureMode.TextureAtlas || script.atlas != null) {
                switch (script.renderingMethod) {
                    case SequenceRenderer3D.Method.Grid:
                        FieldSprite(gridTexture, gridAtlasTextureGUID, "Bar Texture");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Grid Size");
                        GUILayout.FlexibleSpace();
                        MadGUI.LookLikeControls(30, 50);
                        EditorGUILayout.PropertyField(gridWidth, new GUIContent("W"));
                        MadGUI.PropertyField(gridHeight, "H");
                        MadGUI.LookLikeControls(0, 0);
                        EditorGUILayout.EndHorizontal();

                        MadGUI.PropertyField(gridFrameCountManual, "Manual Frame Count");
                        MadGUI.ConditionallyEnabled(gridFrameCountManual.boolValue, () => {
                            MadGUI.PropertyField(gridFrameCount, "Frame Count");
                        });
                        break;

                    case SequenceRenderer3D.Method.Sequence:
                        FieldSequenceTextures();
                        break;
                }

                EditorGUILayout.Space();

                MadGUI.PropertyField(gridTint, "Tint");

                EditorGUILayout.Space();

                FieldBackgroundTextures();
                FieldForegroundTextures();
                FieldPremultipliedAlpha();
            }
        });
    }

    private void SectionAppearance() {
        Section("Appearance", () => {
            FieldLabel();
        });
    }

    private void SectionEffects() {
        Section("Effects", () => {
            FieldSmoothEffect();
        });
    }
    
    void FieldSequenceTextures() {
        if (UseAtlas()) {
            int size = script.sequenceAtlasTexturesGUID.Length;
            int nSize = FieldTextureCount();

            EditorGUILayout.Space();

            FieldBatchTextureSet();

            EditorGUILayout.Space();

            MadGUI.Indent(() => { 
            
                if (size != nSize) {
                    MadUndo.RecordObject2(script, "Texture Count Resize");
                    System.Array.Resize(ref script.sequenceAtlasTexturesGUID, nSize);
                    EditorUtility.SetDirty(script);
                }

                if (sequenceAtlasTexturesGUID.arraySize == 0) {
                    ZeroTexturesWarning();
                }

                for (int i = 0; i < sequenceAtlasTexturesGUID.arraySize; ++i) {
                    var element = sequenceAtlasTexturesGUID.GetArrayElementAtIndex(i);
                    FieldAtlasSprite(element, "Texture " + i);
                }
            });
        } else {
            int size = script.sequenceTextures.Length;
            int nSize = FieldTextureCount();

            EditorGUILayout.Space();

            MadGUI.Indent(() => {

                if (size != nSize) {
                    MadUndo.RecordObject2(script, "Texture Count Resize");
                    System.Array.Resize(ref script.sequenceTextures, nSize);
                    EditorUtility.SetDirty(script);
                }

                if (sequenceTextures.arraySize == 0) {
                    ZeroTexturesWarning();
                }

                for (int i = 0; i < sequenceTextures.arraySize; ++i) {
                    var element = sequenceTextures.GetArrayElementAtIndex(i);
                    MadGUI.PropertyField(element, "Texture " + i);
                }

            });
        }
    }

    private static void ZeroTexturesWarning() {
        MadGUI.Warning("No Textures Count set. Please use the Textures Count field above, and then press the 'Set' button to set the number of textures.");
    }

    private int textureCount = -1;

    private int FieldTextureCount() {
        int currentTextureCount;
        if (UseAtlas()) {
            currentTextureCount = script.sequenceAtlasTexturesGUID.Length;
        } else {
            currentTextureCount = script.sequenceTextures.Length;
        }

        if (textureCount == -1) {
            textureCount = currentTextureCount;
        }

        EditorGUILayout.BeginHorizontal();
        textureCount = EditorGUILayout.IntField("Texture Count", textureCount);
        if (GUILayout.Button("Set", GUILayout.Width(50))) {
            currentTextureCount = textureCount;
        }
        EditorGUILayout.EndHorizontal();

        return currentTextureCount;
    }

    private string nameTemplate = "";

    private void FieldBatchTextureSet() {
        MadGUI.Info("If your atlas textures have regular names (like part_01, part_02) then you can add them all at once. "
            + "Just enter here something like 'part_??' where ?? are placeholders for digits, and then press the button below.");

        nameTemplate = EditorGUILayout.TextField("Name Template", nameTemplate);

        if (MadGUI.Button("Batch Texture Set", Color.yellow)) {
            if (EditorUtility.DisplayDialog("Batch Texture Set", "Are you sure that you want to overwrite your texture settings?", "Yes", "No")) {
                MadUndo.RecordObject2(script, "Batch Texture Set");

                var regex = "^" + nameTemplate.Replace("?", "[0-9]") + "$";
                var filtered = from i in script.atlas.items where Regex.Match(i.name, regex).Success orderby i.name select i;
                var filteredArr = filtered.ToArray();

                for (int i = 0; i < script.sequenceAtlasTexturesGUID.Length && i < filteredArr.Length; ++i) {
                    script.sequenceAtlasTexturesGUID[i] = filteredArr[i].textureGUID;
                }
            }
        }
    }
    
    // ===========================================================
    // Static Methods
    // ===========================================================

    // ===========================================================
    // Inner and Anonymous Classes
    // ===========================================================

}

} // namespace